using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// 汎用コルーチン
public class MyCoroutine : MonoBehaviour
{

    /// <summary>
    /// １フレーム待ってから処理実行
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator OneFrameDelay(Action action)
    {
        yield return null;
        action();
    }

    /// <summary>
    /// 一定時間後に関数を実行する
    /// </summary>
    /// <param name="delayTime"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator Delay(float delayTime, Action action)
    {
        yield return new WaitForSeconds(delayTime);
        action();
    }


    /// <summary>
    /// 関数を一定間隔で実行する
    /// </summary>
    /// <param name="spanTime">実行間隔</param>
    /// <param name="action">処理</param>
    /// <returns></returns>
    public static IEnumerator Loop(float spanTime, Action action)
    {
        while(action != null)
        {
            yield return new WaitForSeconds(spanTime);
            action();
        }
    }
}
