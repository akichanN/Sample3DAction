using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


// プレイヤーにデバッグ処理を適応させるクラス
public class PlayerDebug : SingletonMonoBehaviour<PlayerDebug>
{
    [Tooltip("デバッグシステムを適応するプレイヤーを\nアタッチしてください")]
    [SerializeField] private PlayerController_FPS p;
    //[SerializeField] private UnitBase currentUnit;

    // Singleton
    private WallRan w_Ran;
    private GrapplingGun g_Gun;
    private GrapplingUI g_UI;
    private CameraShake c_Shake;
    private PlayerCameraController p_Camera;
    private PlayerInputActionHandler p_InputAction;

    [Header("デバッグ用")]
    [Tooltip("死亡時スポーンする座標")]
    [SerializeField] private Vector3 spawnPos = new Vector3(0, 2, 0); // 生成位置
    [Tooltip("プレイヤーのy座標が設定した値以下になったら死亡するY座標")]
    [SerializeField] private float playerDeath_Y = -10f; // プレイヤーが死亡するY軸の値
    [Tooltip("自動で値をアタッチしてくれる設定")]
    [SerializeField] private bool isAutoSetParam; // true → 自動的にNoneの値を受け取ってくれる

    [SerializeField] private Text log = default; // プレイヤーの設定値を表示する
    //[SerializeField] private GameObject debugWindowPanel = default; // デバッグ時表示するパネル
    //[SerializeField] private int pushCount = 0;
   

    // テンプレート
    void Start()
    {
        // デバックのみ
        AutoSetParam();

        // Singleton
        g_Gun = GrapplingGun.Instance;
        w_Ran = WallRan.Instance;
        g_UI = GrapplingUI.Instance;
        c_Shake = CameraShake.Instance;
        p_Camera = PlayerCameraController.Instance;
        p_InputAction = PlayerInputActionHandler.Instance;
    }

    private void FixedUpdate()
    {
        DebugSystem();


        //log.text = $"isJumping:{p.isJumping}\n" +
        //      $"isWallRuning:{wallRan.isWallRunning}\n" +
        //      $"isWallJumpDelay:{p.isWallJumpDelay};

        // 実行中に変更した場合の処理
        // 自動ダッシュが設定されていてダッシュ状態出なければ実行
        if (p.isAutoSprint && !p.isSprint)
        {
            p.isSprint = true;
        }

        // ダッシュ設定が被らないように制御
        if (p.isAutoSprint)
        {
            p.isSwitchSprint = false;
        }
    }


    /// <summary>
    /// デバッグ環境でのみ適応する処理
    /// ※実装環境では削除する
    /// </summary>
    private void DebugSystem()
    {

        // デバッグ用のキーコマンド ↓

        // プレイヤーのy座標が-10よりも下に落ちた時
        // Rキーが押されたとき
        if ((transform.position.y < playerDeath_Y) ||
            (Keyboard.current.rKey.wasPressedThisFrame))
        {
            // プレイヤーを初期値に戻す
            transform.position = spawnPos;
            g_Gun.InitDurability();

            // 耐久値を初期化する
            g_UI.InitializeSilder(p.unitMaxHp, p.unitMaxHp);
        }

         // 1キー
        if (Keyboard.current.digit1Key.wasReleasedThisFrame)
        {
            // シーンの再ロード
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // 2キー
        // 無限と減少の切り替え
        if (Keyboard.current.digit2Key.wasReleasedThisFrame)
        {
            g_Gun.isInfiniteDurability = !g_Gun.isInfiniteDurability;
        }

        // 3キー
        // 自動ダッシュ切替
        if (Keyboard.current.digit3Key.wasReleasedThisFrame)
        {
            p.isAutoSprint = !p.isAutoSprint;
        }

        // 4キー
        // ダッシュ切替
        if (Keyboard.current.digit4Key.wasReleasedThisFrame)
        {
            p.isSwitchSprint = !p.isSwitchSprint;
        }
    }


    /// <summary>
    /// インスペクターで設定が必要な要素を自動的にセットする
    /// ※開発中に使用するメソッドです
    /// </summary>
    public void AutoSetParam()
    {
        // isAutoSetParamがtrueなら以下の値を自動取得する
        if (isAutoSetParam)
        {
            try
            {
                p_InputAction.playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
                p.fpsCam = GameObject.Find("FPScam").GetComponent<CinemachineVirtualCamera>();
                p.cameraPos = Camera.main.transform;
            }
            catch
            {
                Debug.LogError("isAutoSetParamエラー");
                Debug.LogError("「Player」または「FPScam」オブジェクトをHierarchyに配置してください");
            }
        }
    }

    /// <summary>
    /// プレイヤーの設定値を見たいとき用
    /// </summary>
    //public void DebugWindow()
    //{

    //    if (Keyboard.current.digit0Key.wasReleasedThisFrame)
    //    {
    //        Debug.Log(1111);
    //        pushCount++;
    //        if (pushCount >= 2)
    //        {
    //            pushCount = 0;
    //            debugWindowPanel.SetActive(false);
    //        }
    //        else
    //        {
    //            debugWindowPanel.SetActive(true);
    //        }

    //    }

    //    if (debugWindowPanel.activeSelf == true)
    //    {
    //        log.text =
    //        $"velocity:{}";
    //    }

    //}
}
