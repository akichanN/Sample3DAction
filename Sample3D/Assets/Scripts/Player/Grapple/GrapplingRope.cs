using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// グラップルのワイヤーに対してアニメーションを追加するメソッド
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class GrapplingRope : SingletonMonoBehaviour<GrapplingRope>
{
    [Header("ワイヤーのアニメーションの設定値")]
    [SerializeField] private LineRenderer lr;
    [SerializeField] private AnimationCurve effectOverTime; // アニメーション開始時の挙動
    [SerializeField] private AnimationCurve curve; // アニメーション中盤の挙動
    [SerializeField] private AnimationCurve curveEffectOverDistance; // アニメーション終盤の挙動
    [SerializeField] private float curveSize = 3f; // カーブの大きさ
    [SerializeField] private float scrollSpeed = 3f; // ワイヤーが波打つ早さ
    [SerializeField] private float segments = 200f; // アニメーションの稼働時間
    [SerializeField] private float animSpeed = 3.5f; // アニメーションが全て終わるまでの時間

    // プロパティ
    private Vector3 _start; // グラップルの始点が格納される
    private Vector3 _end; // グラップルのワイヤーが刺さった位置(終点値)が格納される
    private float _time; 
    private bool _active; // true → グラップル使用(アニメーション始動可能) : false → グラップルが解除されたとき

    /// <summary>
    /// ワイヤーを毎フレーム更新するメソッド
    /// </summary>
    public void UpdateGrapple()
    {
        lr.enabled = _active;

        if (_active)
        {
            // アニメーション開始
            ProcessBounce();
        }

    }


    /// <summary>
    ///  ワイヤーのアニメーション処理
    /// </summary>
    private void ProcessBounce()
    {

        var vectors = new List<Vector3>();

        _time = Mathf.MoveTowards(_time, 1f,
            Mathf.Max(Mathf.Lerp(_time, 1f, animSpeed * Time.deltaTime) - _time, 0.2f * Time.deltaTime));

        // リストにグラップルの開始地点の位置を追加する
        vectors.Add(_start);

        // 向いている方向の始点から終点までの距離を格納
        var forward = Quaternion.LookRotation(_end - _start);
        var up = forward * Vector3.up;

        for (var i = 1; i < segments + 1; i++)
        {
            var delta = 1f / segments * i;
            var realDelta = delta * curveSize;

            while (realDelta > 1f)
            {
                realDelta -= 1f;
            } 

            var calcTime = realDelta + -scrollSpeed * _time;

            while (calcTime < 0f) 
            {
                calcTime += 1f;
            }

            // グラップルの開始地点と終了地点の補完
            var defaultPos = Vector3.Lerp(_start, _end, delta);
            var effect = Eval(effectOverTime, _time) * Eval(curveEffectOverDistance, delta) * Eval(curve, calcTime);

            vectors.Add(defaultPos + up * effect);
        }

        // 線（グラップルの紐）を引く場所の設定
        lr.positionCount = vectors.Count;
        lr.SetPositions(vectors.ToArray());
    }


    private static float Eval(AnimationCurve ac, float t)
    {
        return ac.Evaluate(t * ac.keys.Select(k => k.time).Max());
    }


    /// <summary>
    /// ロープのアニメーションを作成するために必要な情報の取得
    /// </summary>
    /// <param name="start">グラップルの開始位置</param>
    /// <param name="end">グラップルの付いた位置</param>
    public void Grapple(Vector3 start, Vector3 end)
    {
        _active = true;
        _time = 0f;

        _start = start;
        _end = end;
    }

    /// <summary>
    /// アニメーションをの停止
    /// </summary>
    public void UnGrapple()
    {
       
        _active = false;
    }

    /// <summary>
    /// グラップルの開始値の毎フレーム更新
    /// </summary>
    /// <param name="start"></param>
    public void UpdateStart(Vector3 start)
    {
        _start = start;
    }

}


