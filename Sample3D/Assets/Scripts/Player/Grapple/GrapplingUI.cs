using UnityEngine;
using UnityEngine.UI;

// グラップルの耐久値を反得させるUIのテストを行うための
// メッシュ生成の頂点のカウントするクラス
public class GrapplingUI : SingletonMonoBehaviour<GrapplingUI>
{

    [Header("グラップル関係のUI処理（テスト用）")]
    [SerializeField] private Slider durabilityGauge; // グラップルの耐久値を反映するシリンダー
    //[SerializeField] private Text meshPosCount; // メッシュ生成処理に送信した頂点の数を反映させるテキスト



    private void Start()
    {
        //meshPosCount.text = $"{0} / 3";
    }

    
    /// <summary>
    /// グラップルの耐久値をシリンダーに反映させるメソッド（初期設定）
    /// </summary>
    /// <param name="val">現在の耐久値を取得</param>
    public void InitializeSilder(float durability, float maxDurability)
    {
        durabilityGauge.value = durability;
        durabilityGauge.maxValue = maxDurability;
    }

    /// <summary>
    /// 耐久値更新用のメソッド（リアルタイム更新）
    /// </summary>
    /// <param name="val"></param>
    public void DurabilitySilder(float val)
    {
        durabilityGauge.value = val;
    }

    /// <summary>
    ///  メッシュ生成処理にプッシュした頂点座標の個数をテキストに反映させるメソッド
    /// </summary>
    /// <param name="count">現在スタックされているメッシュ頂点の個数を取得</param>
    public void MeshCreatePosCounter(int count)
    {
        //meshPosCount.text = $"{count} / 3";
    }
}
