using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


/// <summary>
/// グラップルガン(移動、ワイヤーの発射)処理を管理するクラス
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class GrapplingGun : SingletonMonoBehaviour<GrapplingGun>
{
    [Header("基本設定値")]
    public Rigidbody rb;
    public Transform cameraPos; // レイを飛ばす始点
    public Transform playerPos;
    [SerializeField] private PlayerController_FPS p_Controller;
    //[SerializeField] private PlayerInput playerInput;   
    private UnitBase unit; // 攻撃を当ててきたユニットのUnitBaseを取得する

    // Singleton
    private GrapplingRope g_Rope;
    private GrapplingUI g_UI;
    private PlayerEffect p_Effect;
    //private MeshCreate m_Create;
    //private GrappleAttack g_Attack;
    private CameraShake c_Shake;
    private PlayerInputActionHandler p_InputAction;

    [Header("ワイヤー設定値")]
    [SerializeField] private LayerMask grappleLayer; // グラップルが使用できるレイヤー
    [SerializeField] private Transform gunTip; // グラップルの紐を射出する位置
    [SerializeField] private LineRenderer lr; // グラップルのLineRenderer（紐となる部分）
    [SerializeField] private float grappleDelayTime = 0.2f; // グラップルガンのクールタイム
    [SerializeField] private float maxDistance = 35f; // グラップルの有効距離
    [SerializeField] private float wireDieDistance = 25; // プレイヤーとワイヤーの着弾地点間の距離感でワイヤーを削除する距離
    [SerializeField] private float invalidationDistance = 30; // プレイヤーの移動値が無効化される距離

    private Vector3 grapplePoint; // グラップルの接着地点の格納用変数
    private Vector3 grappleStartPosition; // 紐を射出する位置の格納用変数
    private float distance; // プレイヤーとワイヤーの着弾地点の距離感を格納する変数
    private float distanceFromPoint;

    public LayerMask GrappleLayer { get { return grappleLayer; } }
    public float MaxDistance { get { return maxDistance; } }
    public float GrappleDelayTime { get { return grappleDelayTime; } }

    [Header("グラップルの耐久値関連の項目")]
    //public float durability; // グラップルの耐久値
    //public float maxDurability; // グラップルの最大値
    [SerializeField] private float effectDamage = 0.5f; // グラップルを行った際に減る耐久値
    [SerializeField] private float heal = 2f; //　耐久値が自動回復する際に使用する加算値
    [SerializeField] private float damageTime = 0f; // ダメージの間隔を保持する変数
    [SerializeField] private float healNextTime = 0.3f; // 自動回復の間隔
    [SerializeField] private float damageNextTime = 0.2f; // ダメージの間隔
    [SerializeField] private float healDelayTime = 1f; // 自動回復を開始する前に待機する時間 

    [Header("SpringJointの設定値")]
    [Range(0, 10)] [SerializeField] private float spring = 4f; // グラップルの力
    [Range(0, 10)] [SerializeField] private float damper = 2f; // 減衰させるスプリング力の値 (0に近ければスプリング力が強くなる)
    [Range(0, 50)] [SerializeField] private float massScale = 4f; // グラップル使用時の質量の大きさ
    [Range(0, 10)] [SerializeField] private float jointMaxDistance = 0.1f; // グラップル使用時、プレイヤーとオブジェクトとの距離の最大値
    [Range(0, 1)] [SerializeField] private float jointMinDistance = 0.146f; // グラップル使用時、プレイヤーとオブジェクトとの距離の最小値
   
    private SpringJoint joint; // プレイヤーのSpringJointの格納用変数

    [Header("フラグ(テスト用に表示中)")]
    [SerializeField] private bool isFire; // グラップルが発射されたら
    [SerializeField] private bool isGrapple; // グラップルのボタンがおされたか
    [SerializeField] private bool isGrappleDelay; // グラップルの発射間隔の処理が終了していたらtrue 
    [SerializeField] private bool isInvalidation; // プレイヤーの移動値が無効化されたか
    [SerializeField] private bool isNoDurabilityVal; // グラップルの耐久値が0になったらtrue

    public bool IsFire { get { return isFire; } }
    public bool IsInvalidation { get { return isInvalidation; } }
    public bool IsNoDurabilityVal { get { return isNoDurabilityVal; } }

    // routine
    private IEnumerator grappleRecoveryRoutine; // グラップル回復ルーチン
    private Coroutine grappleCoroutine;

    [Header("デバッグモード設定値")]
    [Tooltip("グラップルガンの耐久値を無限にする設定")]
    public bool isInfiniteDurability; // 耐久値を無限化
    [Tooltip("グラップルガンの耐久値を自動回復する設定")]
    [SerializeField] private bool isAutoHealing; // 耐久値の自動回復の有効可
    [Tooltip("メッシュ生成処理の検証版を適応する設定")]
    [SerializeField] private bool isNewStyleMeshCreate; // 新しいメッシュ作成方法の有効化

    private void Start()
    {
       
        // Singleton
        g_Rope = GrapplingRope.Instance;
        g_UI = GrapplingUI.Instance;
        p_Effect = PlayerEffect.Instance;
        //m_Create = MeshCreate.Instance;
        //g_Attack = GrappleAttack.Instance;
        c_Shake = CameraShake.Instance;
        p_InputAction = PlayerInputActionHandler.Instance;

        // 耐久値の初期化
        InitDurability();

        // カメラがアタッチされていない場合
        if (cameraPos == null)
        {
            cameraPos = Camera.main.transform;
        }

    }

    #region InputActionPerformed

    /// <summary>
    /// グラップル
    /// </summary>
    /// <param name="context"></param>
    public void GrappleActionPerformed(InputAction.CallbackContext context)
    {
        // グラップル処理の実行
        StartGrapple();

        // レイの可視化
        //Debug.DrawRay(cameraPos.transform.position, cameraPos.transform.forward * grapplingGun.maxDistance, Color.red, 1f, false);
    }


    //public void MeshCreateActionPerformed(InputAction.CallbackContext context)
    //{
    //    // メッシュ生成処理を実行
    //    m_Create.CreatTriangleMesh();

    //}

    #endregion

    /// <summary>
    /// 耐久値の初期化
    /// </summary>
    public void InitDurability()
    {
        isNoDurabilityVal = false;

        // UIの初期化
        g_UI.InitializeSilder(p_Controller.unitMaxHp, p_Controller.unitMaxHp);

        // プレイヤーのHPが反映されていなければ実行
        if (p_Controller.unitHp != p_Controller.unitMaxHp)
        {
            p_Controller.unitHp = p_Controller.unitMaxHp;
        }
    }

    private void LateUpdate()
    {
        if (isFire)
        {
            // アニメーションに必要な情報を送り続ける
            g_Rope.UpdateStart(gunTip.position);
            g_Rope.UpdateGrapple();
        }

        //DrawRope();
    }

    private void FixedUpdate()
    {

        // 耐久値が0になったら実行
        if (p_Controller.unitHp <= 0)
        {
            isNoDurabilityVal = true;
        }

        // グラップルボタンの入力検知
        var inputGrapple = p_InputAction.GetGrappleInput();

        // 毎フレーム更新が必要な場合（ダメージ処理）
        //if (!isNoDurabilityVal)
        //{
        //    DamageEffect(durability);
        //}

        // 現在グラップルの状態か？
        if (isFire && !isNoDurabilityVal)
        {

            // グラップルのテスト状態（グラップルの耐久値が無限）でない場合
            if (!isInfiniteDurability)
            {
                // グラップルの耐久値を減らす(この処理内で呼ばれる場合はunitはNULL)
                GrappleDurabilityDown(effectDamage);
                g_UI.DurabilitySilder(p_Controller.unitHp);
                p_Effect.DamageEffect(p_Controller.unitHp);

            }
           
            // プレイヤーの現在位置とワイヤーの着弾地点の差分をsqrMagnitudeした値を格納する
            distance = (playerPos.position - grapplePoint).sqrMagnitude;

            // 2点の距離感が wireDieDistanceで指定した距離より短くなったら実行
            if (distance < invalidationDistance)
            {
                // 動作の無効化
                isInvalidation = true;
            }

            // 2点の距離感が wireDieDistanceで指定した距離より短くなったら実行
            if (distance < wireDieDistance)
            {
                // デバッグモードが有効なら
                if (isNewStyleMeshCreate)
                {
                    // 旧式：グラップルを撃った地点
                    //m_Create.PushPos(grapplePoint);
                }
                
                // グラップルを停止する
                StopGrapple();
            }

            // グラップルの回復routineを停止する
            if (grappleRecoveryRoutine != null)
            {
                //Debug.Log("ルーチン終了");
                StopCoroutine(grappleRecoveryRoutine);
                grappleRecoveryRoutine = null;
            }
        }
        else // グラップルが停止or動作していない間実行
        {
            // デバッグモードが有効なら
            if (isAutoHealing)
            {
                // 変数の初期化
                distance = 0;

                // 自動回復
                // グラップルの回復routineがNULLの時かつ耐久値が減っている状態なら実行
                if (grappleRecoveryRoutine == null &&
                    p_Controller.unitHp < p_Controller.unitMaxHp)
                {
                    //Debug.Log("ルーチン開始");
                    grappleRecoveryRoutine = GrappleRecoveryRoutine();
                    StartCoroutine(grappleRecoveryRoutine);
                }
            }
        }

        // グラップル解除処理
        // グラップル使用時かつグラップルのボタンが離されたとき実行
        if ((inputGrapple == 0) && isGrapple)
        {
            // グラップル状態を停止
            StopGrapple();

            isGrappleDelay = true;

            if(grappleCoroutine == null)
            {
                grappleCoroutine = StartCoroutine(MyCoroutine.Delay(grappleDelayTime, () =>
                {
                    //Debug.Log("グラップルのクールタイム終了");
                    isGrappleDelay = false;
                    grappleCoroutine = null;
                }));
            }
        }
    }

   
    /// <summary>
    /// グラップルの処理（開始）
    /// レイヤーに応じてグラップルか攻撃に処理を分岐する
    /// </summary>
    public void StartGrapple()
    {
        // 耐久値が0になったら
        if (isNoDurabilityVal)
        {
            return;
        }

        RaycastHit hit;

        // レイの表示
        if (Physics.Raycast(cameraPos.transform.position, cameraPos.transform.forward, out hit, maxDistance))
        {

            int layer = hit.transform.gameObject.layer;

            // 攻撃可能レイヤーか？
            //if (g_Attack.CanAttackLayer == (1 << layer | g_Attack.CanAttackLayer))
            //{
            //    //Debug.Log($"①攻撃可能レイヤーを検知");
            //    //Debug.Log($"distanceFromPoint:{distanceFromPoint}\ndistanceFromPoint < attackMaxDistance: {distanceFromPoint < attackMaxDistance}");
            //    distanceFromPoint = (playerPos.position - hit.point).sqrMagnitude;

            //    // 攻撃可能距離だったら実行
            //    if (distanceFromPoint < g_Attack.AttackMaxDistance)
            //    {
            //        //Debug.Log($"②攻撃可能距離なので攻撃開始");
            //        g_Attack.OnGrappleAttack(hit);
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}
            // 攻撃判定以外のレイヤーなら実行
            //else
            //{
                //Debug.Log($"➂攻撃レイヤーではありません");

                // グラップル処理
                // カメラの前方にグラップルの有効距離までの範囲内で、グラップル使用可能なレイヤーがあるか？
                if (grappleLayer == (1 << layer | grappleLayer))
                {
                    //Debug.Log($"④グラップル可能レイヤーだったのでグラップルを開始します");
                    Grapple(hit);
                }
            //}
        }
    }

    /// <summary>
    /// グラップル処理
    /// </summary>
    /// <param name="hit">レイで検知した要素</param>
    public void Grapple(RaycastHit hit)
    {
        // グラップルの遅延が終了していれば実行
        if (!isGrappleDelay)
        {
            // SpringJointがコンポーネントされていないかどうか
            // ※フレームの遅延等でSpringJointが削除されていない場合があるため
            if (playerPos.gameObject.GetComponent<SpringJoint>() != null)
            {
                Destroy(playerPos.gameObject.GetComponent<SpringJoint>());
            }

            isGrapple = true;
            isFire = true;

            //Rayが当たった位置を取得
            grapplePoint = hit.point;

            // アニメーションを再生するために必要な情報を送信
            g_Rope.Grapple(gunTip.position, grapplePoint);

            // デバッグモードが有効で無ければ実行
            if (!isNewStyleMeshCreate)
            {
                // メッシュ生成地点として格納 
                //MeshCreate.Instance.PushPos(gameObject.transform.position);
            }

            // グラップルを射出した時のみにPlayerにSpringJointをコンポーネントする
            // グラップルが終了したら消える
            joint = playerPos.gameObject.AddComponent<SpringJoint>();

            // 以上の設定値を一緒の位置にする
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            // プレイヤーの位置とグラップルの接着地点の間の距離を取得
            float distanceFromPoint = Vector3.Distance(playerPos.position, grapplePoint);

            // ラインの頂点数を２にする
            lr.positionCount = 2;

            // グラップルの紐を射出する位置の格納
            grappleStartPosition = gunTip.position;

            // SpringJointの設定値の反映
            joint.maxDistance = distanceFromPoint * jointMaxDistance;
            joint.minDistance = distanceFromPoint * jointMinDistance;
            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;
        }
        else
        {
            //Debug.Log("グラップルガンはクールタイム中です");
        }
        
    }


    /// <summary>
    /// グラップルの処理（終了）
    /// </summary>
    public void StopGrapple()
    {
        // グラップルが終了時削除
        Destroy(joint);

        // グラップルアニメーション停止
        g_Rope.UnGrapple();

        // 頂点数を0にする
        lr.positionCount = 0;

        // 初期化
        isGrapple = false;
        isFire = false;
        isInvalidation = false;
    }



    /// <summary>
    /// グラップル中に耐久値を減少させる関数
    /// </summary>
    /// <param name="damage">減算値</param>
    public void GrappleDurabilityDown(float damage)
    {

        // 耐久値が0なら
        if (p_Controller.unitHp <= 0)
        {
            // グラップル停止
            StopGrapple();
            return;
        }


        // ダメージを受ける間隔になったら
        if (damageTime >= damageNextTime)
        {
            p_Controller.HitDamage(damage);
            damageTime = 0;

            // 耐久値が0なら
            if (p_Controller.unitHp == 0)
            {
                // グラップル停止
                StopGrapple();
            }
        }

        damageTime += Time.fixedDeltaTime;

    }

    /// <summary>
    /// グラップルの耐久値を回復する関数
    /// </summary>
    /// <param name="addVal">回復値</param>
    public void GrappleDurabilityUp(float addVal)
    {
        //Debug.Log("grapple: 敵を倒したので回復しました");

        p_Controller.unitHp += addVal;
        g_UI.DurabilitySilder(p_Controller.unitHp);
        // 回復後の画面エフェクトの反映
        p_Effect.DamageEffect(p_Controller.unitHp);
    }

    /// <summary>
    /// グラップル回復ルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator GrappleRecoveryRoutine()
    {
        // 自動回復開始までの待機時間
        yield return new WaitForSeconds(healDelayTime);

        do
        { 
            // 自動回復の間隔
            yield return new WaitForSeconds(healNextTime);
            // 耐久値の加算
            p_Controller.unitHp += Math.Min(heal, p_Controller.unitMaxHp);

        } 
        while (p_Controller.unitHp < p_Controller.unitMaxHp); // 耐久値が最大値未満の間はループ

        // 回復ルーチン終了
        grappleRecoveryRoutine = null;
    }


    /// <summary>
    /// グラップル時のメソッド（外部スクリプトで使用中）
    /// </summary>
    /// <returns></returns>
    public bool IsGrappling()
    {
        return joint != null;
    }


    /// <summary>
    /// グラップルのポイント取得（外部スクリプトで使用中）
    /// </summary>
    /// <returns></returns>
    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

   
}
