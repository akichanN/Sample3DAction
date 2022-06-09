using System;
using UnityEngine;


/// <summary>
/// Standard Assetsのスクリプト
/// </summary>
[Serializable]
public class CurveControlledBob
{
    
    public float horizontalBobRange = 0.05f; // 水平方向の閾値
    public float verticalBobRange = 0.05f; // 垂直方向の閾値

    // オブジェクトの揺れる挙動
    public AnimationCurve bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                        new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                        new Keyframe(2f, 0f)); 

    public float verticaltoHorizontalRatio = 2f;

    private float m_CyclePositionX;
    private float m_CyclePositionY;
    private float m_BobBaseInterval;
    private Vector3 m_OriginalCameraPosition;
    private float m_Time;

    /// <summary>
    /// 振動させる処理の初期設定
    /// </summary>
    /// <param name="obj">振動させたいオブジェクト</param>
    /// <param name="bobBaseInterval">振動させる間隔</param>
    public void Setup(GameObject obj, float bobBaseInterval)
    {
        m_BobBaseInterval = bobBaseInterval;
        m_OriginalCameraPosition = obj.transform.localPosition;

        // get the length of the curve in time
        m_Time = bobcurve[bobcurve.length - 1].time;
    }

    /// <summary>
    /// プレイヤーの移動速度に合わせて振動する間隔を制御する
    /// </summary>
    /// <param name="speed">プレイヤーの移動速度に合わせて算出したSpeed値</param>
    /// <returns></returns>
    public Vector3 DoHeadBob(float speed)
    {
        float xPos = m_OriginalCameraPosition.x + (bobcurve.Evaluate(m_CyclePositionX) * horizontalBobRange);
        float yPos = m_OriginalCameraPosition.y + (bobcurve.Evaluate(m_CyclePositionY) * verticalBobRange);
        float zPos = m_OriginalCameraPosition.z;

        m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
        m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * verticaltoHorizontalRatio;

        if (m_CyclePositionX > m_Time)
        {
            m_CyclePositionX = m_CyclePositionX - m_Time;
        }
        if (m_CyclePositionY > m_Time)
        {
            m_CyclePositionY = m_CyclePositionY - m_Time;
        }

        return new Vector3(xPos, yPos, zPos);
    }
}
