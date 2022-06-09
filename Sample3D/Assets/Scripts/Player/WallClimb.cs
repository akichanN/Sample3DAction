using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClimb : MonoBehaviour
{
    Rigidbody rigidBody;

    //プレイヤーコントローラー
    PlayerController_FPS playerController;

    //乗り越え処理

    [System.NonSerialized] public bool climbFlag = false;

    //rayの設定
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector3 rayOffset = new Vector3(0, -1, 0); //レイの原点調整用
    private Vector3[] rayPos; //レイの場所[]
    [SerializeField] private float topRayMaxDistance = 1; //レイの最大長さ（床検知用）
    [SerializeField] private float bottomRayMaxDistance = 0.75f; //レイの最大長さ（床検知用）

    [SerializeField] private Vector3 climbPower = new Vector3(0, 50, 30);
    [SerializeField] private float climbMaxHeight = 1.5f;

    //ボックスキャスト用
    //RaycastHit boxHit;
    //[SerializeField] float boxcastSize = 0.5f;
    //Vector3 boxSize = new Vector3(1f, 0.5f, 0.5f);
    //[SerializeField] float boxcastDistance = 10f;

    

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController_FPS>();

        rayPos = new Vector3[2]
        {
            new Vector3(0, climbMaxHeight, 0), //これより高い段差は登れない
            new Vector3(0, 0.1f, 0),//足
        };

    }


    private void LateUpdate()
    {
        //ボックスキャスト用
        //var isHit = Physics.BoxCast(transform.position, boxSize, transform.forward, out boxHit,
        //    transform.rotation, boxcastDistance);
        //if (isHit)
        //{
        //    Debug.Log("box:" + boxHit.collider.name);
        //}
        //

        //Debug.DrawRay(transform.position + rayPos[1] + rayOffset, transform.forward * wallMaxDistance2, Color.blue);
        //Debug.DrawRay(transform.position + rayPos[0] + rayOffset, transform.forward * wallMaxDistance, Color.blue);
        //if (playerController.moveDirection.sqrMagnitude > 0.5f && !playerController.IsGround) //プレイヤーの入力値があったら
        //{
        //    //Debug.Log("移動中");
        //    // 1 段差があるかチェック
        //    Physics.Raycast(transform.position + rayPos[1] + rayOffset, transform.forward, out RaycastHit a, wallMaxDistance2, wallLayer);
        //    if (a.collider != null)
        //    {
        //        //Debug.DrawRay(transform.position + rayPos[1] + rayOffset, transform.forward * a.distance, Color.cyan);

        //        // 2 段差があるので乗り越えられるかチェックする
        //        Physics.Raycast(transform.position + rayPos[0] + rayOffset, transform.forward, out RaycastHit b, wallMaxDistance, wallLayer);
        //        if (b.collider == null)
        //        {
        //            //Debug.Log("乗り越えられる");
        //            rigidBody.velocity += Time.deltaTime * (transform.rotation * dansaPower);
        //        }
        //        else
        //        {
        //            //Debug.Log("乗り越えられない");
        //            Debug.DrawRay(transform.position + rayPos[0] + rayOffset, transform.forward * b.distance, Color.cyan);
        //        }
        //    }
        //}

    }

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position + rayPos[1] + rayOffset, transform.forward * bottomRayMaxDistance, Color.blue);
        Debug.DrawRay(transform.position + rayPos[0] + rayOffset, transform.forward * topRayMaxDistance, Color.blue);
        if (playerController.MoveDirection.sqrMagnitude > 0.5f && !playerController.IsGround) //プレイヤーの入力値があったら
        {
            //Debug.Log("移動中");
            // 1 段差があるかチェック
            Physics.Raycast(transform.position + rayPos[1] + rayOffset, transform.forward, out RaycastHit bottomRay, bottomRayMaxDistance, wallLayer);
            if (bottomRay.collider != null)
            {
                Debug.DrawRay(transform.position + rayPos[1] + rayOffset, transform.forward * bottomRayMaxDistance, Color.cyan);

                // 2 段差があるので乗り越えられるかチェックする
                Physics.Raycast(transform.position + rayPos[0] + rayOffset, transform.forward, out RaycastHit topRay, topRayMaxDistance, wallLayer);
                if (topRay.collider == null)
                {
                    
                    rigidBody.velocity += Time.deltaTime * (transform.rotation * climbPower);
                    climbFlag = true;
                }
                else
                {
                    //Debug.Log("乗り越えられない");
                    Debug.DrawRay(transform.position + rayPos[0] + rayOffset, transform.forward * topRayMaxDistance, Color.cyan);
                }
            }
        }
    }

    /// <summary>
    /// 壁走り処理
    /// </summary>
    //private void ClimbWall()
    //{
    //    rigidBody.velocity += Time.deltaTime * (transform.rotation * climbPower);
    //}

    //private void VelocityReset()
    //{
    //    rigidBody.velocity = Vector3.zero;
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    if (boxHit.collider != null)
    //    {
    //        Gizmos.DrawRay(transform.position, transform.forward * boxHit.distance);
    //        Gizmos.DrawWireCube(transform.position + transform.forward * boxHit.distance,
    //            boxSize * 2);
    //    }
    //    else
    //    {
    //        Gizmos.DrawRay(transform.position, transform.forward * boxcastDistance);
    //        Gizmos.DrawWireCube(transform.position + transform.forward * boxcastDistance,
    //            boxSize * 2);
    //    }

    //}
}
