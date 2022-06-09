using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;


/// <summary>
/// プレイヤーのカメラを制御するクラス
/// </summary>
public class PlayerCameraController : SingletonMonoBehaviour<PlayerCameraController>
{
    [Header("基本設定値")]
    public CinemachineVirtualCamera fpsCam; 
    [SerializeField] private Camera g_OnlyCam; // 武器のみを表示するカメラ(壁に埋まらないようにする)

    // Singleton
    private WallRan w_Ran;
    private PlayerEffect p_Effect;
    //private QuestManager q_Manager;

    private const float MAX_ANGLE_ROLL = 20; // 壁走り状態の時のカメラの傾き
    private float defaultAngle = 0f; // 壁走りを開始したときの角度を保存する変数
    private bool isWallRunStart; // 壁走り処理が実行された最初のフレーム　→　フレーム


    /// <summary>
    /// カーソルのフォーカス切り替え
    /// </summary>
    /// <param name="isFocus"></param>
    private void Focus(bool isFocus)
    {
        Cursor.lockState = isFocus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFocus;
    }


    // テンプレート
    void Start()
    {
        // カーソルの非表示設定
        Focus(true);

        if(fpsCam == null)
        {
            fpsCam = GameObject.Find("FPScam").GetComponent<CinemachineVirtualCamera>();
        }

        if(g_OnlyCam == null)
        {
            g_OnlyCam =  GameObject.Find("GrappleGunOnlyCamera").GetComponent<Camera>();
        }


        // Singleton
        w_Ran = WallRan.Instance;
        p_Effect = PlayerEffect.Instance;
        //q_Manager = QuestManager.Instance;

    }


    void Update()
    {
        // クエストのステートがゲームクリアならマウスカーソルを固定かさせる処理を無効にする
        //if(q_Manager.questEvents[q_Manager.eventsNum].stageStatus ==
        //    QuestEvent.QuestEventStatus.GAMECLEAR)
        //{
        //    Focus(false);
        //    return;
        //}

        // マウスカーソルの表示非表示
        if (Cursor.lockState == CursorLockMode.Locked &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Focus(false);
        }
        else if (Cursor.lockState == CursorLockMode.None &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Focus(true);
        }

        CameraSystem();
    }

    /// <summary>
    ///  カメラ処理
    /// </summary>
    private void CameraSystem()
    {
        float leepDeltaTime = 5f * Time.deltaTime;
        var c_POV = fpsCam.GetCinemachineComponent(CinemachineCore.Stage.Aim).GetComponent<CinemachinePOV>();

        // 壁走り状態にカメラの角度を変える処理
        if (w_Ran.isWallRunning)
        {
            // １度だけ実行される処理
            if (!isWallRunStart)
            {
                isWallRunStart = true;

                // 壁走りを開始した際のプレイヤーの視点角度を格納する
                defaultAngle = c_POV.m_HorizontalAxis.Value;
            }

            c_POV.m_HorizontalAxis.m_Wrap = false;

            // 右の壁に触れているなら
            if (w_Ran.CalculateSide() > 0)
            {
                // 左にカメラを傾ける
                fpsCam.m_Lens.Dutch = Mathf.Lerp(fpsCam.m_Lens.Dutch, MAX_ANGLE_ROLL, leepDeltaTime);

                // 視点を壁走りを開始した時の角度から-90した角度までの範囲(左側に90度)
                c_POV.m_HorizontalAxis.m_MaxValue = defaultAngle;
                c_POV.m_HorizontalAxis.m_MinValue = defaultAngle + -90;

            }
            // 左の壁に触れているなら
            else if (w_Ran.CalculateSide() < 0)
            {
                // 右にカメラを傾ける
                fpsCam.m_Lens.Dutch = Mathf.Lerp(fpsCam.m_Lens.Dutch, -MAX_ANGLE_ROLL, leepDeltaTime);

                // 視点を壁走りを開始した時の角度から＋90した角度までの範囲(右側に90度)
                c_POV.m_HorizontalAxis.m_MaxValue = defaultAngle + 90;
                c_POV.m_HorizontalAxis.m_MinValue = defaultAngle;
            }

            // グラップルで移動している際の画面エフェクト
            p_Effect.speedEffectState = SpeedEffectState.GrappleEffect;
        }
        else // 壁走りが解除されたら
        {
            // カメラの角度が初期値にもどり切っていないなら
            if (fpsCam.m_Lens.Dutch != 0)
            {
                fpsCam.m_Lens.Dutch = Mathf.Lerp(fpsCam.m_Lens.Dutch, 0, leepDeltaTime);
            }

            // プロパティの値を元の値に戻す
            c_POV.m_HorizontalAxis.m_Wrap = true;
            c_POV.m_HorizontalAxis.m_MaxValue = 180;
            c_POV.m_HorizontalAxis.m_MinValue = -180;

            isWallRunStart = false;
            defaultAngle = 0;
        }
    }

}
