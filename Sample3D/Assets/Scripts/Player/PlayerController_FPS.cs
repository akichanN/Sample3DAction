using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;



/// <summary>
/// プレイヤーの動作処理全般 FPS
/// ※メインプレイヤー
/// </summary>
public class PlayerController_FPS : UnitBase
{
    [Header("基本設定値")]
    public Rigidbody rb = default;
    public CinemachineVirtualCamera fpsCam;
    public Transform followTarget; // カメラの基準になるオブジェクト
    public Transform cameraPos; // 近接攻撃のレイを飛ばす始点
     
    // Singleton
    private WallRan w_Ran;
    private GrapplingGun g_Gun; 
    private GrapplingUI g_UI;
    private PlayerDebug p_Debug;
    private PlayerEffect p_Effect;
    private CameraShake c_Shake;
    private PlayerInputActionHandler p_InputAction;

    [Header("プレイヤー動作処理の設定値")]
    [SerializeField] private LayerMask groundLayer; // 地面のLayerを指定
    [SerializeField] private float gravity = 30f; // 重力
    [SerializeField] private float wallRunGravity = 20f; // 壁走り状態にかかる重力
    [SerializeField] private float moveSpeed = 200f; // 移動値
    [SerializeField] private float moveMultiplier = 0f; // 乗数値の格納　→　移動処理時に使用
    [SerializeField] private float jumpForce = 500f; // ジャンプ倍率
    [SerializeField] private float jumpDelatTime = 0.3f; // ジャンプの間隔
    [SerializeField] private float wallJumpPower = 12f; // 壁ジャンプのパワー
    [SerializeField] private float c_SheckDuration = 0.25f; // ダメージを受けたときカメラを振動させる時間
    [SerializeField] private float c_SheckMagnitude = 0.1f; // ダメージを受けたときカメラの振動の大きさ

    private Vector3 moveDirection = Vector3.zero; // 移動値を格納する変数
    private const float MAX_WALK_SPEED = 6f; // 歩き最大移動速度
    private const float MAX_SPRINT_SPEED = 9f; // ダッシュ時最大移動速度
    private const float MAX_SPEDD_SKY = 30f; // 空中での最大移動速度
    private const float MAX_SLOPEANGLE = 35f; // 地面判定を検知する際に使用する距離の閾値
    private float jumpCooldown = 0.25f; // 次のジャンプまでの間隔
    private Vector2 inputMove; // プレイヤーの移動値を受け取る変数
    private Vector3 normalVector = Vector3.up; // ジャンプ処理に使用する変数

    // Coroutine
    private Coroutine jumpCoroutine;
    private Coroutine wallJumpCoroutine;

    // 読み取り専用
    public LayerMask GroundLayer { get { return groundLayer; } }

    public float MaxWalkSpeed{ get { return MAX_WALK_SPEED; } }
    public float MaxSprintSpeed{ get { return MAX_SPRINT_SPEED; } }
    public Vector3 MoveDirection { get { return moveDirection; } }


    [Header("ダッシュのボタン設定")]
    [Tooltip("Shiftを押すことでダッシュ、歩きの切替する設定 \n（他ダッシュ処理設定と併用不可）")]
    public bool isSwitchSprint; // 移動方法切替値  false →　ボタンが押されている間ダッシュする
    [Tooltip("自動ダッシュ設定（他ダッシュ処理設定と併用不可）")]
    public bool isAutoSprint; // ダッシュ固定

    // 読み取り専用
    public bool IsGround { get; private set; } // 地面感知用のBool値　地面についている時 → true : 離れたとき →　false
    public bool IsJumping { get; private set; }// ジャンプした → true
    public bool IsWallJumpDelay { get; private set; } // 壁ジャンプのディレイが開始されたら

    public bool isSprint; // ダッシュキーが押されたら → true
    [SerializeField] private bool isCancellingGrounded; // 地面についている時 → true
    [SerializeField] private bool isAim; // 構えてる状態なら →　true : 
    [SerializeField] private bool isReadyToJump = true; // ジャンプの準備完了 → true
    [SerializeField] private bool isAudioStart;

    [Header("効果音")]
    [SerializeField] private AudioClip grappleMoveSE;
    [SerializeField] private AudioClip damageSE;
    [SerializeField] private float limitSpeed = 350f; // 移動速度がこの値を超えたらSEを再生する


    protected override void Start()
    {
        base.Start();

        // 自動ダッシュ処理が適応されていれば
        if (isAutoSprint)
        {
            isSprint = true;
        }

        // Singleton
        g_Gun = GrapplingGun.Instance;
        w_Ran = WallRan.Instance;
        g_UI = GrapplingUI.Instance;
        p_Debug = PlayerDebug.Instance;
        p_Effect = PlayerEffect.Instance;
        c_Shake = CameraShake.Instance;
        p_InputAction = PlayerInputActionHandler.Instance;

    }

    #region InputActionPerformed

    /// <summary>
    /// ジャンプ
    /// </summary>
    /// <param name="context"></param>
    public void JumpActionPerformed(InputAction.CallbackContext context)
    {
        IsJumping = true;

        if (jumpCoroutine == null)
        {
            // 次のジャンプが行えるように一定時間経過後にfalseに変更する
            jumpCoroutine = StartCoroutine(MyCoroutine.Delay(jumpDelatTime, () =>
            {
                IsJumping = false;
                jumpCoroutine = null;
            }));
        }
    }


    /// <summary>
    /// ダッシュ
    /// </summary>
    /// <param name="context"></param>
    public void SprintActionPerformed(InputAction.CallbackContext context)
    {
        // 自動ダッシュ設定になっていればreturn
        if (!isAutoSprint)
        {
            // ダッシュキーを押すとダッシュと歩きの切替（デバッグモード）
            if (isSwitchSprint)
            {
                isSprint = !isSprint;
            }
            else
            {
                isSprint = true;
            }
        }
    }


    #endregion


    private void LateUpdate()
    {
        if (IsJumping)
        {
            w_Ran.isCanWallCatch = true;
        }
    }

    private void FixedUpdate()
    {
       
        // グラップルの耐久値が0になったら死亡する
        if (g_Gun.IsNoDurabilityVal)
        {
            Death();
            return;
        }

       
        Move();

        #region プレイヤーの重力処理
        // 地面についていないとき
        if (!IsGround)
        {
            if (g_Gun.IsFire)
            {
                // 重力処理無しでリターン
                return;
            }
            else if (w_Ran.isWallRunning)
            {
                // 壁走り時の重力
                rb.velocity += Vector3.down * wallRunGravity * Time.fixedDeltaTime;
                return;
            }
            else
            {
                // 重力
                rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
            }
        }

        // 地面についていれば実行
        if (IsGround)
        {
            // 重力
            rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
            rb.velocity.Scale(new Vector3(0.05f, 1, 0.05f));

            // 最大速度制限
            if (isSprint)
            {
                // ダッシュ
                rb.velocity = VelocityLimitXZ(rb.velocity, MAX_SPRINT_SPEED);

                // プレイヤーの移動値に値が入っていれば実行
                if (!MoveCheck())
                {
                    p_Effect.speedEffectState = SpeedEffectState.DashEffect;
                }
                // 移動値に入力がないなら実行
                else
                {
                    p_Effect.speedEffectState = SpeedEffectState.DefaultEffect;
                }
            }
            else
            {
                // 歩き
                rb.velocity = VelocityLimitXZ(rb.velocity, MAX_WALK_SPEED);
                p_Effect.speedEffectState = SpeedEffectState.DefaultEffect;
            }
        }
        #endregion

        // グラップルしている且つプレイヤーの移動値が0なら実行
        if (g_Gun.IsFire)
        {
            if ((rb.velocity.x != 0) && (rb.velocity.y != 0))
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }

            p_Effect.speedEffectState = SpeedEffectState.GrappleEffect;
        }

        // ダッシュ処理のデバッグモードと自動ダッシュが適応されていなければ
        if (!isSwitchSprint && !isAutoSprint)
        {
            var inputSprint = p_InputAction.GetSprintInput();

            // ダッシュキーが離されたとき実行
            if ((inputSprint == 0) && isSprint)
            {
                isSprint = false;
            }
        }

        // 移動値が一定値超えたら実行
        if (rb.velocity.sqrMagnitude >= limitSpeed)
        {
            // 以下のSEが今実行されていなければ実行
            if (!isAudioStart)
            {
                isAudioStart = true;
                // SEの再生
                //AudioManager2D.Instance.AudioSE.PlayOneShot(grappleMoveSE);

                //Debug.Log("SEの再生");
            }
        }
        else
        {
            isAudioStart = false;
        }
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    public override void Move()
    {
        
        // 移動値加算
        rb.velocity += MoveDirection * Time.fixedDeltaTime * moveSpeed * moveMultiplier;


        // グラップルがまだ使用できる状態なら実行
        if (!g_Gun.IsInvalidation)
        {
            // 移動値の取得
            inputMove = p_InputAction.GetMoveInput();

            // カメラの向いている方向に対して、移動方向を変換する
            moveDirection = new Vector3(inputMove.x, 0, inputMove.y);
            moveDirection = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * moveDirection;
        }

        // 壁走りをしていなければ実行
        if (!w_Ran.isWallRunning)
        { 
            // 通常時はカメラが向いている方向をプレイヤーの正面として回転する
            // 壁走り中はプレイヤーの回転を固定　→　見渡せるように変更(壁走り中はこの処理を実行しない)
            var angle = Quaternion.LookRotation(cameraPos.transform.forward).eulerAngles;
            transform.eulerAngles = new Vector3(0, angle.y, 0);
            followTarget.eulerAngles = new Vector3(angle.x, angle.y, 0);
        }
        

        // ジャンプ待機状態かつジャンプキーが押されていたら
        if (isReadyToJump && IsJumping && IsGround) 
        {
            if (!w_Ran.isWallRunning)
            {
                //Debug.LogError("地面でジャンプしました");
                rb.AddForce(Vector2.up * jumpForce * 2.0f);
                //rb.AddForce(normalVector * jumpForce * 0.5f);

                //落下中にジャンプできないようにする
                Vector3 vel = rb.velocity;
                if (rb.velocity.y < 0.5f)
                {
                    // y軸の速度を0にする
                    rb.velocity = new Vector3(vel.x, 0, vel.z);
                }
                else if (rb.velocity.y > 0)
                {
                    rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
                }
            }
            // ジャンプの遅延
            Invoke(nameof(ResetJump), jumpCooldown);
            isReadyToJump = false;
        }

        // 初期化
        moveMultiplier = 1;

        // 条件：空中にいる状態の時に呼ばれる処理
        if (!IsGround)
        {
            // グラップル使用中の場合
            if (g_Gun.IsFire)
            {
                moveMultiplier = 0.001f;
                return;
            }
            
            // 空中に滞在中、移動処理に乗算する値
            moveMultiplier = 0.001f;


            // 壁ジャンプ処理
            if (w_Ran.isWallRunning && IsJumping && !IsWallJumpDelay)
            {
                //Debug.Break();
                //Debug.Log("壁ジャンプ");
                IsWallJumpDelay = true;
                rb.velocity += w_Ran.GetWallJumpDirection() * wallJumpPower;

                if(wallJumpCoroutine == null)
                {
                    // 連続で処理が通らないようにディレイをかける
                    wallJumpCoroutine = StartCoroutine(MyCoroutine.Delay(jumpDelatTime, () =>
                    {
                        IsWallJumpDelay = false;
                        wallJumpCoroutine = null;
                    }));
                }
            }
        }
    }

    private void SEPlay()
    {
       
    }


    /// <summary>
    /// XZ軸の速度制限
    /// </summary>
    /// <param name="velocity">プレイヤーのvelocity</param>
    /// <param name="maxSqrV">プレイヤーの最大速度</param>
    /// <returns></returns>
    private Vector3 VelocityLimitXZ(Vector3 velocity, float maxSqrV)
    {
        Vector3 v = velocity;
        v.y = 0;

        // 超過速度を減算する
        if (maxSqrV < v.magnitude)
        {
            v = v - (v.normalized * maxSqrV);
            v.y = 0f;
            return velocity -= v;
        }
        return velocity;
    }

   
    /// <summary>
    /// 地面判定の取得
    /// </summary>
    /// <param name="other">Collisionに触れているオブジェクト</param>
    private void OnCollisionStay(Collision other)
    {
        // int型でのレイヤーを取得
        int layer = other.gameObject.layer;

        // 取得したレイヤーと地面のレイヤー（Ground）が一致しなかったらreturn
        if (groundLayer != (groundLayer | (1 << layer)))
        {
            return;
        }

        // 接触したGroundレイヤーの設定されたオブジェクトの数だけ繰り返す
        for (int i = 0; i < other.contactCount; i++)
        {
            // 接触したオブジェクトの位置を取得
            Vector3 normal = other.contacts[i].normal;

            // 地面についている間実行
            if (IsFloor(normal))
            {
                IsGround = true;
                isCancellingGrounded = false;
                normalVector = normal;

                // StopGroundedのキャンセル
                CancelInvoke(nameof(StopGrounded));
            }
        }


        float delay = 3f; // StopGrounded()を遅延させる変数

        if (!isCancellingGrounded)
        {
            // 基本的に地面についている間は以下の処理が呼ばれる
            isCancellingGrounded = true;

            // StopGrounded()を一定時間経過していたら実行する
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }


    /// <summary>
    ///  敵からのダメージ処理
    /// </summary>
    /// <param name="damage"></param>
    public void OnDamage(float damage)
    {
        // 現在耐久値が無限なら以下の処理を呼ばない
        if (g_Gun.isInfiniteDurability)
        {
            return;
        }

        // 耐久値が0なら
        if (unitHp <= 0)
        {
            return;
        }

        // 一定値ダメージを受ける処理
        HitDamage(damage);

        // カメラの振動
        c_Shake.Shake(c_SheckDuration, c_SheckMagnitude);
        g_UI.DurabilitySilder(unitHp);

        // 画面エフェクトの反映
        p_Effect.DamageEffect(unitHp);

        //SE
        //AudioManager2D.Instance.AudioSE.PlayOneShot(damageSE);

    }


   
    /// <summary>
    /// 死亡処理
    /// </summary>
    protected override void Death()
    {
        Debug.LogWarning("playerが死亡したためプレイヤーの処理を停止しました\nRキーでリスポーンをしてください");
        rb.velocity = Vector3.zero;
    }

    /// <summary>
    /// 地面から離れたら呼ばれる、ジャンプ時等
    /// </summary>
    private void StopGrounded()
    {
        IsGround = false;
    }


    /// <summary>
    /// ジャンプの遅延処理
    /// </summary>
    private void ResetJump()
    {
        isReadyToJump = true;
    }


    /// <summary>
    /// 地面についているかをboolの値を返すメソッド
    /// ※ 地面との距離間がmaxSlopeAngleの値よりも小さければfalse,大きければtrueを返す
    /// </summary>
    /// <param name="v">触れている地面オブジェクトの座標</param>
    /// <returns></returns>
    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < MAX_SLOPEANGLE;
    }


    /// <summary>
    /// プレイヤーの移動キーが押されているかbool値で返すメソッド
    /// ・true　→　XYどちらのキーも押されていない状態（プレイヤーが静止している状態）
    /// ・false　→　XYどちらかのキーが押されている状態（プレイヤーが移動している状態）
    /// </summary>
    public bool MoveCheck()
    {
        return (0 == inputMove.x) && (0 == inputMove.y);
    }

    public override void Attack()
    {
        throw new NotImplementedException();
    }
}
