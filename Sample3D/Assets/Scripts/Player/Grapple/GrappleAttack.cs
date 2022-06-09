//using UnityEngine;

///// <summary>
///// グラップルガンの位置を管理するステート
///// </summary>
//public enum GrappleGunMoveState
//{
//    Default, // 通常時
//    DownGunPosState, // 攻撃時-銃を下げる動作
//    ReturnGunPosState, // 銃が下がっている状態から元の位置にもどる動作
//}


///// <summary>
///// グラップルの攻撃処理クラス
///// </summary>
//public class GrappleAttack : SingletonMonoBehaviour<GrappleAttack>
//{
//    [Header("基本設定値")]
//    [SerializeField] private Animator anim;

//    // Singleton
//    private GrapplingGun g_Gun;

//    [Header("近接戦闘用の設定値")]
//    [SerializeField] private LayerMask canAttackLayer; // 近接攻撃の影響を受けるレイヤー
//    [SerializeField] private GrappleGunMoveState moveState; // ステートを管理する変数名
//    [SerializeField] private float attackMaxDistance = 5f; // 基礎攻撃の有効距離（カメラから前方に飛ばすレイの最大値）
//    [SerializeField] private float attackDamage = 1f; // 基礎攻撃のダメージ ※ステータスはUnitBaseに移動する可能性あり
//    [Range(0, 1)][SerializeField] private float attackDelatTime = 0.4f; // 攻撃の間隔

//    public LayerMask CanAttackLayer { get { return canAttackLayer; } }
//    public GrappleGunMoveState MoveState { get { return moveState; } }
//    public float AttackMaxDistance { get { return attackMaxDistance; } }

//    [Header("グラップルガンの挙動処理で使用する設定値")]
//    [SerializeField] private GameObject grappleAttackObj; // 攻撃した際に表示する攻撃エフェクト
//    [SerializeField] private Transform downGunPosition; // グラップルガンを下げる位置
//    [SerializeField] private Transform defaultGunPosition;　// グラップルガンの初期位置
//    [SerializeField] private GameObject grappleGunObj; // 画面に表示しているグラップルガン
//    [SerializeField] private float grappleSwitchDelay = 0.2f; // グラップルガンを移動する速さ
//    [Range(0, 1)] [SerializeField] private float attackSpanTime = 1f; // 連続攻撃の判定が実行される最大の時間

//    private int multiAttackCount = 0; // 一定時間の間に攻撃が連続した回数をカウントする変数
//    private int attackCount = 1; // 攻撃アニメーションように使用するカウント
//    private float attackElapsedTime; // 1度攻撃をしてから経過した時間を一定時間計測した値を保持する
//    private float moveElapsedTime = 0f; // グラップルガンが挙動を開始した時点からの経過時間を保持する

//    // Coroutine
//    private Coroutine attackCoroutine;
//    public GameObject GrappleAttackObj { get { return grappleAttackObj; } }


//    [Header("フラグ(テスト用に表示中)")]
//    [SerializeField] private bool isReadyToAttack = true; // 攻撃の準備完了 → true
//    [SerializeField] private bool isAttack; // 攻撃をした → true
//    [SerializeField] private bool isAttackCombo; // 1回目の攻撃が実行され、次の攻撃の準備が出来たときtrue
//    [SerializeField] private bool isMultiAttack; // 一定時間の間に攻撃が連続して実行されていたらtrue
//    [SerializeField] private bool isMoveGrappleGun; // グラップルガンの挙動処理が実行されていればtrue

//    [Header("効果音")]
//    [SerializeField] private AudioClip attackSE;
//    [SerializeField] private AudioClip attackHitSE;

//    // テンプレート
//    void Start()
//    {
//        // Singleton
//        g_Gun = GrapplingGun.Instance;

//        // stateの初期化
//        moveState = GrappleGunMoveState.Default;

//        // 攻撃エフェクト用のオブジェクトを非表示
//        grappleAttackObj.SetActive(false);

//        // グラップルガンの初期位置設定
//        grappleGunObj.transform.position = defaultGunPosition.transform.position;
//    }

//    private void FixedUpdate()
//    {
//        MoveGrappleGun();

//        // 攻撃処理が実行されたら
//        if (isAttack && isReadyToAttack)
//        {
//            //Debug.Log("攻撃実行");

//            if (attackCoroutine == null)
//            {
//                isReadyToAttack = false;
//                // 一定時間経過したら攻撃ができるようにbool値を変更する
//                attackCoroutine = StartCoroutine(MyCoroutine.Delay(attackDelatTime, () =>
//                {
//                    //Debug.Log("攻撃クールタイム終了");
//                    isReadyToAttack = true;
//                    isAttack = false;

//                    // 攻撃後処理の実行のため使用
//                    isAttackCombo = true;

//                    attackCoroutine = null;
//                }));
//            }
//        }

//        // 攻撃が実行され攻撃準備が完了している状態なら実行
//        if (isAttackCombo)
//        {
//            AttackComboDirection();
//        }
//    }


//    /// <summary>
//    /// 攻撃処理
//    /// </summary>
//    /// <param name="hit">レイで検知した要素</param>
//    public void OnGrappleAttack(RaycastHit hit)
//    {
//        if (!isAttack)
//        {
//            isAttack = true;

//            // グラップルガンを画面外に移動させる
//            moveState = GrappleGunMoveState.DownGunPosState;
//            isMoveGrappleGun = true;

//            // 一定時間の間で連続で攻撃が実行された回数のカウント
//            multiAttackCount = Mathf.Clamp(++multiAttackCount, 0, 2);

//            // 一定時間の間で連続で攻撃が実行されたとき実行
//            if (multiAttackCount > 1)
//            {
//                isMultiAttack = true;
//            }


//            // 攻撃エフェクトの表示
//            grappleAttackObj.SetActive(true);

//            // エフェクトのアニメーションを再生
//            anim.SetTrigger($"Attack_{attackCount}");

//            // 攻撃回数が3回以上なら実行
//            if (attackCount >= 3)
//            {
//                attackCount = 1;
//            }
//            // 攻撃の回数に応じて一定の数カウントをする
//            else
//            {
//                attackCount++;
//            }

           
//            // レイの当たったオブジェクトのUnitBaseを取得
//            var targetUnit = hit.transform.gameObject.GetComponent<UnitBase>();
//            Debug.Log(targetUnit);
//            // UnitBaseが取得出来たら
//            if (targetUnit)
//            {
                

//                // 攻撃SE
//                //AudioManager2D.Instance.AudioSE.PlayOneShot(attackSE);
//                //AudioManager2D.Instance.AudioSE.PlayOneShot(attackHitSE);


//                // 取得したユニットに対してダメージを与える
//                targetUnit.HitDamage(attackDamage);
                

//                //Debug.Log(targetUnit + "  " + targetUnit.gameObject.name);
//                //Debug.Log("Player →　Enemy　にダメージを与えました");
//            }
//        }

//    }

//    /// <summary>
//    /// ・攻撃をしたときにグラップルガンを下げる動作
//    /// ・一定時間攻撃が行われなかったら元の位置に戻す処理
//    /// 上記の処理を行うメソッド
//    /// </summary>
//    private void MoveGrappleGun()
//    {
//        // 攻撃（グラップルガンを下げる）処理が呼ばれていない間は返す
//        if (!isMoveGrappleGun)
//        {
//            return;
//        }

//        // 経過時間を過ぎたときの処理
//        if (moveElapsedTime >= grappleSwitchDelay)
//        {

//            isMoveGrappleGun = false;
//            moveElapsedTime = 0;
//            //Debug.Log($"経過時間オーバー\nelapsedTime:{elapsedTime}\nweaponSwitchDelay:{weaponSwitchDelay}");
//            //Debug.Log(weaponSwitchDelay);
//            return;
//        }

//        // 経過時間の加算
//        moveElapsedTime += Time.deltaTime;
//        //Debug.Log($"経過時間 elapsedTime:{elapsedTime}");

//        // 割合計算
//        float rate = Mathf.Clamp01(moveElapsedTime / g_Gun.GrappleDelayTime);

//        // 攻撃開始したときに呼ばれる
//        if (moveState == GrappleGunMoveState.DownGunPosState)
//        {
//            // 現在の位置から指定した位置まで移動させる
//            grappleGunObj.transform.localPosition =
//                Vector3.Lerp(grappleGunObj.transform.localPosition, downGunPosition.localPosition, rate);
//        }
//        // 攻撃が一定時間実行されなかったら実行
//        else if (moveState == GrappleGunMoveState.ReturnGunPosState)
//        {
//            // 現在の位置から元の位置に戻す
//            grappleGunObj.transform.localPosition =
//                Vector3.Lerp(grappleGunObj.transform.localPosition, defaultGunPosition.localPosition, rate);

//            // グラップルガンが元の位置にもどっていたら実行
//            if(grappleGunObj.transform.localPosition == defaultGunPosition.localPosition)
//            {
//                //Debug.Log("元の場所にもどったためステートを変更します");
//                moveState = GrappleGunMoveState.Default;
//            }
//        }

//    }

//    /// <summary>
//    /// 1回目の攻撃が実行されてからその後連続でキーが押されたかによって実行処理を分けるメソッド
//    /// ※攻撃が実行された後にこの関数を呼ぶ
//    /// </summary>
//    private void AttackComboDirection()
//    {
//        // 一定時間の間に複数攻撃が発生したら
//        if (isMultiAttack)
//        {
//            // 時間計測のリセット
//            attackElapsedTime = 0;
//            isMultiAttack = false;

//            //Debug.LogError($"2回目リセット");
//        }

//        // 1回目の攻撃処理が通り且つ次の攻撃の準備ができたタイミングから計測を行う
//        attackElapsedTime += Time.deltaTime;
//        //Debug.Log($"attackElapsedTime: {attackElapsedTime}");

//        // 連続で攻撃を受け付ける時間を過ぎたら実行
//        if (attackElapsedTime > attackSpanTime)
//        {
//            // 元の位置にグラップルガンを戻す
//            moveState = GrappleGunMoveState.ReturnGunPosState;
//            isMoveGrappleGun = true;

//            // 初期化
//            multiAttackCount = 0;
//            attackCount = 1;
//            attackElapsedTime = 0;
//            isAttackCombo = false;

//            //Debug.LogWarning($"attackElapsedTime: {attackElapsedTime}");
//        }
//    }
//}
