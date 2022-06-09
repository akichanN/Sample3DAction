using UnityEngine;

/// <summary>
/// グラップルガンを上下左右に振動させるクラス
/// ※メインの動作処理は「CurveControlledBob」の処理を参照してください
/// </summary>
public class GrappleGunMoveSystem : MonoBehaviour
{

    [Header("基本設定値")]
    [SerializeField] private PlayerController_FPS p_Controller;
    [SerializeField] private GameObject grappleGunObj;

    // Singleton
    private WallRan w_Ran;
    //private GrappleAttack g_Attack;
    private GrapplingGun g_Gun;

    [SerializeField] private CurveControlledBob m_HandBob = new CurveControlledBob();
    [SerializeField] private float m_StepInterval = 5f;
    [SerializeField] private float objMoveSpeed = 0.5f;

    // テンプレート
    void Start()
    {
        // Singleton
        w_Ran = WallRan.Instance;
        //g_Attack = GrappleAttack.Instance;
        g_Gun = GrapplingGun.Instance;

        // グラップルガンの動作の初期設定
        m_HandBob.Setup(grappleGunObj, m_StepInterval);
    }

    private void FixedUpdate()
    {
        // 以下の条件の場合グラップルガンの振動処理を停止(returnする)

        // 壁走り状態でないなら
        if (!w_Ran.isWallRunning)
        {
            // 地面から離れているまたはグラップルガンが元の配置にもどっていない(攻撃処理実行中)なら
            if (!p_Controller.IsGround)
            {
                return;
            }
        }
        
        UpdateGrappleGunBob();

    }

    /// <summary>
    /// グラップルガンを上下左右に振動させる
    /// </summary>
    private void UpdateGrappleGunBob()
    {
       
        grappleGunObj.transform.localPosition = m_HandBob.DoHeadBob(p_Controller.rb.velocity.magnitude +
           p_Controller.MaxWalkSpeed * objMoveSpeed);

        //Debug.Log($"sqrMagnitude: {p_Controller.rb.velocity.sqrMagnitude}");
        //Debug.Log($"magnitude: {p_Controller.rb.velocity.magnitude}");

        // 元処理
        //m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
        //                              (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
    }

}