using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// アタッチしたオブジェクトを振動させるクラス
/// ※カメラを振動させるために使用中
/// </summary>
public class CameraShake : SingletonMonoBehaviour<CameraShake>
{
    private Coroutine doSheke;


    /// <summary>
    /// コルーチン再生用のメソッド（カメラ振動処理）
    /// </summary>
    /// <param name="duration">振動させる時間</param>
    /// <param name="magnitude">振動の大きさ</param>
    public void Shake(float duration, float magnitude)
    {
        if (doSheke == null)
        {
            doSheke = StartCoroutine(DoShake(duration, magnitude));
        }
    }

    /// <summary>
    /// カメラ振動処理
    /// </summary>
    /// <param name="duration">振動させる時間</param>
    /// <param name="magnitude">振動の大きさ</param>
    /// <returns></returns>
    private IEnumerator DoShake(float duration, float magnitude)
    {
        var pos = transform.localPosition;

        var elapsed = 0f;

        while (elapsed < duration)
        {
            var x = pos.x + Random.Range(-1f, 1f) * magnitude;
            var y = pos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, pos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = pos;

        doSheke = null;
    }
}
