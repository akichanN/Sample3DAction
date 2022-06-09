using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レティクルが特定のオブジェクトを検知したときに色を変えるクラス
/// </summary>
public class PlayerReticleDirection : MonoBehaviour
{
    [Header("基礎設定値")]
    [SerializeField] private ReticleController r_Controller;
    [SerializeField] private LayerMask trashLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask canNotGrappleLayer;
 
    // Singleton
    private GrapplingGun g_Gun;
    //private GrappleAttack g_Attack;

    [Header("レティクルが検知した際に変更する色")]
    [SerializeField] private Color32 grappleColor = new Color32(0, 255, 255, 255); // グラップル → 赤
    [SerializeField] private Color32 enemyColor = new Color32(255, 162, 0, 255); // 敵 → オレンジ
    [SerializeField] private Color32 trashColor = new Color32(0, 255, 0, 255); // ゴミ → 緑
    private Color32 defaultColor = new Color32(255, 255, 255, 30); // ゲーム開始時のレティクルの色を取得

    private float distance;
    private Vector3 hitPos;
    

    // Start is called before the first frame update
    void Start()
    {
        // Singleton
        g_Gun = GrapplingGun.Instance;
        //g_Attack = GrappleAttack.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        OnRayHit();
    }

    /// <summary>
    /// プレイヤーが現在向いている方向にあるオブジェクトをレイを飛ばすメソッド
    /// </summary>
    private void OnRayHit()
    {
        RaycastHit hit;

        // レイの表示
        if (Physics.Raycast(g_Gun.cameraPos.transform.position, g_Gun.cameraPos.transform.forward, out hit, g_Gun.MaxDistance))
        {
            int layer = hit.transform.gameObject.layer;
            hitPos = hit.point;

            ColorDirection(layer);
        }
        else
        {
            // レティクルの色をもとの色に更新
            r_Controller.SetColor(defaultColor);
        }
    }

    /// <summary>
    /// 取得したレイヤーに応じてレティクルの色を更新するメソッド
    /// </summary>
    /// <param name="layer">Raycastで取得したlayer</param>
    private void ColorDirection(int layer) 
    {
        // グラップルできないレイヤーなら
        if(canNotGrappleLayer == (1 << layer | canNotGrappleLayer))
        {
            r_Controller.SetColor(defaultColor);
            return;
        }


        // レイヤーが敵のレイヤーなら実行
        if (enemyLayer == (1 << layer | enemyLayer))
        {
            // レイがヒットしたオブジェクトとの距離の大きさを求める
            distance = (g_Gun.playerPos.position - hitPos).sqrMagnitude;

            // 攻撃可能距離に敵がいるなら実行
            //if (distance < g_Attack.AttackMaxDistance)
            //{
            //    // レティクルの色を変更
            //    r_Controller.SetColor(enemyColor);
            //}
            //else
            //{
            //    return;
            //}
        }
        // ゴミ
        else if (trashLayer == (1 << layer | trashLayer))
        {
            distance = (g_Gun.playerPos.position - hitPos).sqrMagnitude;

            //if (distance < g_Attack.AttackMaxDistance)
            //{
            //    // レティクルの色を変更
            //    r_Controller.SetColor(trashColor);
            //}
            //else
            //{
            //    return;
            //}
        }
        // グラップル
        else if (g_Gun.GrappleLayer == (1 << layer | g_Gun.GrappleLayer))
        {
            r_Controller.SetColor(grappleColor);
        }
    }

}
