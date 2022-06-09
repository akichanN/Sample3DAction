using UnityEngine;
using System.Linq;

/// <summary>
/// 壁走り処理を管理するクラス
/// </summary>
public class WallRan : SingletonMonoBehaviour<WallRan>
{
    [Header("基本設定値")]
    [SerializeField] private PlayerController_FPS player;
    [SerializeField] private LayerMask wallRunLayer;

    // Singleton
    private GrapplingGun g_Gun;
    private PlayerInputActionHandler p_InputAction;

    [Header("壁走り処理の設定値")]
    [SerializeField] private float minimumHeight = 1.2f; // 地面検知用のレイの長さ
    [SerializeField] private float wallMaxDistance = 1.3f; // レイの最大長さ（壁検知用）
    [SerializeField] private float wallSpeedMultiplier = 10f; // 壁走り時のスピード
    [SerializeField] private float wallGravityDownForce = 20f; // 壁走り時にかかる重力
    [SerializeField] private float wallBouncing = 1f; // 壁ジャンプの計算に使用
    [SerializeField] private float jumpDuration = 0.4f; // ジャンプ間隔
    [Range(0.0f, 1.0f)] [SerializeField] private float v_normalizedAngleThreshold = 0.1f; // 垂直の角度の基礎決め
    [Range(0.0f, 1.0f)] [SerializeField] private float h_normalizedAngleThreshold = 0.35f; // 水平の角度の基礎決め

    private float elapsedTimeSinceJump = 0; // ジャンプ中の経過時間
    private Vector3 lastWallPosition; // 新しい壁の情報を格納する変数
    private Vector3 lastWallNormal; // 新しい壁の面の角度情報を格納する変数
    private Vector3[] directions; // 壁検知をするレイの方向を格納する配列
    private RaycastHit[] hits; // 壁を検知下際にヒットした要素を格納する配列

    // 以下の変数は壁走り継続時間に制限を付けたい場合に使用する
    // ※現在の処理は地面に付くまで壁走りが可能
    //private float elapsedTimeSinceWallAttach = 0; // 壁走りをしている経過時間 
    //private float elapsedTimeSinceWallDetatch = 0; // 壁走りをしていない間の経過時間（常に更新）

    public bool isCanWallCatch; // ジャンプ時壁にくっ付ける状態か返す
    public bool isWallRunning; // 壁走りしている状態ならtrue


    // テンプレート
    void Start()
    {
        // Singleton
        g_Gun = GrapplingGun.Instance;
        p_InputAction = PlayerInputActionHandler.Instance;

        // レイを飛ばす用の配列
        directions = new Vector3[]
        {
             Vector3.right, // 右
             Vector3.right + Vector3.forward, // 右斜め前
             //////Vector3.forward, // 正面
             Vector3.left + Vector3.forward, // 左斜め前
             Vector3.left // 左  
        };
    }

    private void FixedUpdate()
    {

        isWallRunning = false;

        // グラップルをしていない間は壁走り処理を実行
        if (!g_Gun.IsFire)
        {

            // ジャンプで飛びついた瞬間か？
            if (CanAttach())
            {
                // 前方5方向に飛ばしたレイに当たった要素を格納する変数
                hits = new RaycastHit[directions.Length];

                // 配列の要素数分回す
                for (int i = 0; i < directions.Length; i++)
                {
                    // ワールド座標に変更
                    Vector3 dir = transform.TransformDirection(directions[i]);

                    // レイの生成
                    // レイに当たったオブジェクトを配列に格納する
                    // 前方5方向にレイを伸ばす
                    Physics.Raycast(transform.position, dir, out hits[i], wallMaxDistance);

                    // 壁の判定をもらっていたら
                    // デバッグ用の記述？
                    if (hits[i].collider != null)
                    {
                        // プレイヤーの前方にレイを表示
                        Debug.DrawRay(transform.position, dir * hits[i].distance, Color.green);
                    }
                    else
                    {
                        // 前方5方向に表示する
                        Debug.DrawRay(transform.position, dir * wallMaxDistance, Color.red);
                    }
                }

                // 壁走りの条件がクリアなら通る
                if (CanWallRun())
                {
                    // 条件：コライダーのあるオブジェクト且つレイヤーが「wall」のオブジェクト
                    // ソート：壁との距離を昇順に並び替え
                    hits = hits.ToList().Where(h => h.collider != null &&
                           wallRunLayer == (1 << h.transform.gameObject.layer | wallRunLayer)).
                           OrderBy(h => h.distance).
                           ToArray();


                    // 配列がから出なければ実行
                    if (hits.Length > 0)
                    {
                        // ソートした配列の先頭要素を送る
                        OnWall(hits[0]);

                        // 一番新しいポジションを格納
                        lastWallPosition = hits[0].point;

                        // 面の角度を格納
                        lastWallNormal = hits[0].normal;
                             
                    }
                }
            }

            //　壁走り状態なら
            if (isWallRunning)
            {

                // 壁走り時の重力処理
                player.rb.velocity += Vector3.down * wallGravityDownForce * Time.deltaTime;

                // 壁走りしている間の時間を計測する
                //elapsedTimeSinceWallDetatch = 0;
                //elapsedTimeSinceWallAttach += Time.deltaTime;
            }

            //else // 壁走り状態じゃなければ
            //{
            //    //// 初期化
            //    //elapsedTimeSinceWallAttach = 0;
            //    //// 壁走りをしていない間の時間を計測する
            //    //elapsedTimeSinceWallDetatch += Time.deltaTime;
            //}
        }
    }

    /// <summary>
    /// 壁走り移行メソッド
    /// </summary>
    /// <param name="hit">壁検知用のレイに触れたオブジェクト</param>
    private void OnWall(RaycastHit hit)
    {
        // 2つの角度の内積を出す(-0~1~0) Vector3.Dot
        float v_dot = Vector3.Dot(hit.normal, Vector3.up);
        float h_dot = Vector3.Dot(hit.normal, transform.forward);

        //Debug.Log($"水平方向の内積{h_dot}");

        // 触れている壁の面からプレイヤーの垂直方向の内積が指定値以内なら実行
        if ((v_dot >= -v_normalizedAngleThreshold) && (v_dot <= v_normalizedAngleThreshold))
        {
            // 触れている壁の面からプレイヤーの水平方向の内積が指定値以内なら実行
            if ((h_dot >= -h_normalizedAngleThreshold) && (h_dot <= h_normalizedAngleThreshold))
            {
                // 前方への入力値
                // 移動ボタンの入力検知
                var inputMove = p_InputAction.GetMoveInput();
                float vertical = inputMove.y;

                // プレイヤーの前方
                Vector3 alongWall = transform.TransformDirection(Vector3.forward);

                // 壁走り中の進行方向に緑色の線を引く(進行方向の視覚化)
                Debug.DrawRay(transform.position, alongWall.normalized * 10, Color.green);
                // 壁走りをしている面の垂直方向に紫色の線を引く
                Debug.DrawRay(transform.position, lastWallNormal * 10, Color.magenta);


                // 壁走りの動作処理
                player.rb.velocity = alongWall * vertical * wallSpeedMultiplier;

                // 壁走り状態
                isWallRunning = true;
            } 
        }
    }


    /// <summary>
    /// 壁走り状態でジャンプしたときのメソッド
    /// </summary>
    /// <returns>壁走り中なら→斜め上方向の数値を返す</returns>
    public Vector3 GetWallJumpDirection()
    {

        if (isWallRunning)
        {
            // 斜め上方向の数値を返す
            return lastWallNormal * wallBouncing + Vector3.up;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// プレイヤーの垂直方向の角度を計算
    /// </summary>
    /// <returns>計算した垂直方向の角度</returns>
    public float CalculateSide()
    {
        if (isWallRunning)
        {
            // 一番最新の壁の場所と現在のプレイヤーの場所の差分
            Vector3 heading = lastWallPosition - transform.position;

            // 垂直な3番目のベクトルを出す
            Vector3 perp = Vector3.Cross(transform.forward, heading);

            //　角度の差分を出す
            float dir = Vector3.Dot(perp, transform.up);

            return dir;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// ジャンプした瞬間すぐ壁走りしないようにディレイさせる処理
    /// </summary>
    /// <returns></returns>
    private bool CanAttach()
    {
        // ジャンプしたなら
        if (isCanWallCatch)
        {
            // ジャンプ中時間を計測する
            elapsedTimeSinceJump += Time.deltaTime;

            // １秒以上になったら
            if (elapsedTimeSinceJump > jumpDuration)
            {
                
                elapsedTimeSinceJump = 0;
                isCanWallCatch = false;
            }
            return false;
        }
        return true;
    }


    /// <summary>
    ///  壁走りの細かい条件判定を行い最終的な判定を返すメソッド
    /// </summary>
    /// <returns></returns>
    private bool CanWallRun()
    {
        // 垂直方向に入力を格納
        // 移動ボタンの入力検知
        var inputMove = p_InputAction.GetMoveInput();
        float verticalAxis = inputMove.y;

        // 壁走りの条件が全てそろったらtrueが変える
        // 空中且つ
        // 前入力がある状態且つ
        // 地面から一定距離離れている且つ
        return !player.IsGround && verticalAxis > 0 && VerticalCheck();
    }

    // プレイヤーの下方向にレイを伸ばす
    // レイがオブジェクトに触れているならfalse,触れていないならtrue
    private bool VerticalCheck()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumHeight);
    }

}
