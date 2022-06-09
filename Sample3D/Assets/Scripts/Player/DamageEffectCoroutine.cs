using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// グラップルの耐久値に応じて適応されるダメージエフェクト
/// ※必要に応じてルーチンの中に処理を追加してください
/// </summary>
public class DamageEffectCoroutine : MonoBehaviour
{
    public static IEnumerator DamaeHighRoutine(Vignette v, float damageType, float effectMaxTime, Action action)
    {
        //Debug.Log("70～51");
        yield return new WaitForSeconds(effectMaxTime);
        v.intensity.value = damageType;

        action();
        //Debug.Log("終了");
    }

    public static IEnumerator DamageCautionRoutine(Vignette v, float damageType, float effectMaxTime, Action action)
    {
        //Debug.Log("50～30");
        yield return new WaitForSeconds(effectMaxTime);
        v.intensity.value = damageType;

        action();
        //Debug.Log("終了");
    }

    public static IEnumerator DamageDangerRoutine(Vignette v, float damageType, float effectMaxTime, Action action)
    {
        //Debug.Log("30～0");
        yield return new WaitForSeconds(effectMaxTime);
        v.intensity.value = damageType;
       
        action();
        //Debug.Log("終了");
    }

    public static IEnumerator DamageFineRoutine(Vignette v, float damageType, float effectMaxTime, Action action)
    {
        //Debug.Log("70～");
        yield return new WaitForSeconds(effectMaxTime);
        v.intensity.value = damageType;
        action();
        //Debug.Log("終了");
    }

    #region 過去処理
    //public static IEnumerator DamaeHighRoutine(Vignette v, float damageType, float effectMaxTime, float leepTime, Action action)
    //{
    //    // エフェクトが表示しきるまで待機
    //    Debug.Log("待機中");

    //    do
    //    {
    //        Debug.Log("70～51");
    //        yield return new WaitForSeconds(effectMaxTime);
    //        //v.intensity.value = Mathf.Lerp(v.intensity.value, damageType, leepTime);
    //        Debug.Log(v.intensity.value + "<=" + damageType + "→" + (v.intensity.value < damageType));
    //    }
    //    while (v.intensity.value < damageType);

    //    action();
    //    Debug.Log("終了");
    //}
    #endregion
}
