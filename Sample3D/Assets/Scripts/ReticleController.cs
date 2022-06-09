using UnityEngine;
using UnityEngine.UI;

public class ReticleController : MonoBehaviour
{
    [SerializeField] private RectTransform reticles;
    [Header("0=Top,1=Right,2=Bottom,3=Left")]
    [SerializeField] private RectTransform[] reticlesPos = new RectTransform[4];
    [SerializeField] private Image[] reticlesImage = new Image[4];

    [SerializeField] private RectTransform center;
    [SerializeField] private Image centerImage;


    [Header("初期値")]

    [Tooltip("レティクルの長さ"), SerializeField] private float reticleLength = 50;
    [Tooltip("レティクルの幅"), SerializeField] private float reticleWidth = 5;
    [Tooltip("レティクル間の横幅"), SerializeField] private float intervalHorizontal = 30;
    [Tooltip("レティクル間の縦幅"), SerializeField] private float intervalVertical = 30;

    [Tooltip("レティクルの色"), SerializeField] private Color32 reticleColor = new Color32(255, 255, 255, 255);

    [Tooltip("画面中心の画像のサイズ"), SerializeField] private float centerSize = 5;

    void Start()
    {
        SetPos(reticleLength, reticleWidth);
        SetInterval(intervalHorizontal, intervalVertical);
        SetColor(reticleColor);
        SetCenterSize(centerSize, false);
    }

    public void SetPos(float length, float width)
    {
        var newSize = new Vector2(length, width);
        foreach (RectTransform i in reticlesPos)
        {
            if (i != null)
            {
                i.sizeDelta = newSize;
            }
        }
    }

    /// <summary>
    /// レティクルの間隔
    /// </summary>
    /// <param name="h">横幅</param>
    /// <param name="v">縦幅</param>
    public void SetInterval(float h, float v)
    {
        if (reticles != null)
        {
            reticles.sizeDelta = new Vector2(h, v);
        }
    }

    /// <summary>
    /// レティクルの色
    /// </summary>
    /// <param name="color">色</param>
    public void SetColor(Color32 color)
    {
        foreach (Image i in reticlesImage)
        {
            if (i != null)
            {
                i.color = color;
            }
        }

        if (centerImage != null)
        {
            centerImage.color = color;
        }
    }

    /// <summary>
    /// 画面の中心画像
    /// </summary>
    /// <param name="size">画像サイズ</param>
    /// <param name="reticleOver">画像サイズがレティクルの間隔を超えられるか否か（初期値false）</param>
    public void SetCenterSize(float size, bool reticleOver = false)
    {
        if (center != null)
        {
            if (reticleOver)
            {
                var max = (reticles.sizeDelta.x < reticles.sizeDelta.y) ?
                    reticles.sizeDelta.x : reticles.sizeDelta.y;
                center.sizeDelta = new Vector2(Mathf.Max(size, max), Mathf.Max(size, max));
            }
            else
            {
                center.sizeDelta = new Vector2(size, size);
            }
        }
    }
}
