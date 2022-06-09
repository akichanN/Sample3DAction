using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// PlayerInputの各InputAction値を取得し扱うクラス
/// </summary>
public class PlayerInputActionHandler : SingletonMonoBehaviour<PlayerInputActionHandler>
{
    [Header("基本設定値")]
    public PlayerInput playerInput;
    [SerializeField] private PlayerController_FPS p_Controller;


    // Singleton
    private GrapplingGun g_Gun;

    [Header("PlayerIntputのアクション名")]
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string lookActionName = "Look";
    [SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string grappleActionName = "Grapple";

    // PlayerInputに設定したActionを格納する変数
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction grappleAction; 


    // テンプレート
    void Start()
    {

        // Singleton
        g_Gun = GrapplingGun.Instance;

        // InputSystem
        moveAction = playerInput.actions.FindAction(moveActionName);
        jumpAction = playerInput.actions.FindAction(jumpActionName);
        lookAction = playerInput.actions.FindAction(lookActionName);
        grappleAction = playerInput.actions.FindAction(grappleActionName);
        sprintAction = playerInput.actions.FindAction(sprintActionName);
        grappleAction = playerInput.actions.FindAction(grappleActionName);


        // InputSystem Performed
        jumpAction.performed += p_Controller.JumpActionPerformed; // PlayerController_FPS参照
        sprintAction.performed += p_Controller.SprintActionPerformed; // PlayerController_FPS参照
        grappleAction.performed += g_Gun.GrappleActionPerformed; // GrapplingGun参照

    }

    /// <summary>
    /// プレイヤーが生成、再生成されたときにInputSystemのパフォームでエラーが発生するのを防ぐメソッド
    /// </summary>
    private void OnDestroy()
    {
        try
        {
            // 各InputActionのperformedを解放する
            jumpAction.performed -= p_Controller.JumpActionPerformed;
            sprintAction.performed -= p_Controller.SprintActionPerformed;
            grappleAction.performed -= g_Gun.GrappleActionPerformed;
        }
        catch // 既にNULLならreturn
        {
            return;
        }
    }

    // ↓各InputActionの値を取得したい場合は以下の該当するメソッドを呼ぶ↓
    // また必要であればメソッドを作成する
   
    /// <summary>
    /// 移動値の取得用メソッド
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMoveInput()
    {
        return moveAction.ReadValue<Vector2>();
    }

    /// <summary>
    /// グラップルガン発射入力値を取得するメソッド
    /// </summary>
    /// <returns></returns>
    public float GetGrappleInput()
    {
        return grappleAction.ReadValue<float>();
    }

    /// <summary>
    /// ダッシュの入力値を取得するメソッド
    /// </summary>
    /// <returns></returns>
    public float GetSprintInput()
    {
        return sprintAction.ReadValue<float>();
    }

}
