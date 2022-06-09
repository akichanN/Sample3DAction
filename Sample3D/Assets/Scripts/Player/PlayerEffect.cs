using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


/// <summary>
/// 加速した際に表示するエフェクトのステート
/// </summary>
public enum SpeedEffectState
{
    DefaultEffect,
    DashEffect,
    GrappleEffect,
}


/// <summary>
///  プレイヤーのエフェクトを管理するクラス
/// </summary>
public class PlayerEffect : SingletonMonoBehaviour<PlayerEffect>
{

    [Header("ダッシュ、グラップルで移動している間表示するエフェクトの設定値")]
    public CinemachineVirtualCamera fpsCam;
    public SpeedEffectState speedEffectState;
    [SerializeField] private Volume volume;
    [SerializeField] private GameObject dashEffect;
    [SerializeField] private float grapInsensityVal = 0.4f;
    [SerializeField] private float grapClampVal = 0.1f;
    [SerializeField] private float dashInsensityVal = 0.2f;
    [SerializeField] private float dashClampVal = 0.1f;
    [SerializeField] private float fov = 10f;
    [SerializeField] private float dashEffectLeepTime = 0.1f;

    private MotionBlur motionBlur; // モーションブラー
    private float stackFovVal; // 現在の視野角の値を保存する
    private float dashFov; // ダッシュ時の視野角

    [Header("ダメージエフェクト関係の設定値")]
    [SerializeField] private float effectLeepTime = 0.2f; // ダメージエフェクトの反映する値
    [SerializeField] private float damageTypy_high = 0.2f; // ダメージ 小
    [SerializeField] private float damageType_Caution = 0.3f; // ダメージ 中
    [SerializeField] private float damageType_Danger = 0.4f;// ダメージ 大（瀕死）
    [SerializeField] private float effectTime = 0.2f;
    [SerializeField] private float effectMaxTime = 0.1f;

    private Vignette vignette; // ダメージエフェクト用

    // routine
    private IEnumerator damageDangerRoutine;
    private IEnumerator damageCautionRoutine;
    private IEnumerator damageHighRoutine;
    private IEnumerator damageFineRoutine;


    // テンプレート
    void Start()
    {
        if (fpsCam == null)
        {
            fpsCam = GameObject.Find("FPScam").GetComponent<CinemachineVirtualCamera>();
        }

        // MotionBlurの値を取得
        volume.profile.TryGet(out motionBlur);

        // 初期ではモーションブラーは付けない
        motionBlur.active = false;

        // ダッシュエフェクトの非表示
        dashEffect.SetActive(false);
        stackFovVal = fpsCam.m_Lens.FieldOfView;
        dashFov = fpsCam.m_Lens.FieldOfView + fov;

        // vignetteの値を取得
        volume.profile.TryGet(out vignette);
    }

    void Update()
    {

        // 現在のステートに応じて画面エフェクトを制御する
        switch (speedEffectState)
        {
            // ダッシュ時
            case SpeedEffectState.DashEffect:

                DashEffectStart();

                break;

            // グラップル使用時
            case SpeedEffectState.GrappleEffect:

                GrappleEffectStart();

                break;

            // 通常時のエフェクト
            case SpeedEffectState.DefaultEffect:

                EffectStop();

                break;
        }
    }


    /// <summary>
    /// ダッシュで移動している間画面に表示されるエフェクトの再生時に呼ばれるメソッド
    /// </summary>
    private void DashEffectStart()
    {
        // PostProcessing | MotionBlur
        motionBlur.active = true;
        motionBlur.intensity.value = dashInsensityVal;
        motionBlur.clamp.value = dashClampVal;

        // Particle
        dashEffect.SetActive(true);
    }

    /// <summary>
    /// グラップルで移動している間画面に表示されるエフェクトの再生時に呼ばれるメソッド
    /// </summary>
    private void GrappleEffectStart()
    {
        // PostProcessing | MotionBlur
        motionBlur.active = true;
        motionBlur.intensity.value = grapInsensityVal;
        motionBlur.clamp.value = grapClampVal;

        // Particle
        dashEffect.SetActive(true);

        // Camera
        fpsCam.m_Lens.FieldOfView = Mathf.Lerp(fpsCam.m_Lens.FieldOfView, dashFov, dashEffectLeepTime);
        //Debug.Log($"ダッシュ中： {fpsCam.m_Lens.FieldOfView}");
    }

    /// <summary>
    /// ダッシュで移動している間画面に表示されるエフェクトの停止時に呼ばれるメソッド
    /// </summary>
    private void EffectStop()
    {
        // エフェクトの停止
        motionBlur.active = false;
        dashEffect.SetActive(false);

        // 元の視野角に戻す
        fpsCam.m_Lens.FieldOfView = Mathf.Lerp(fpsCam.m_Lens.FieldOfView, stackFovVal, dashEffectLeepTime);
    }

    /// <summary>
    ///  ダメージエフェクトの反映
    /// </summary>
    /// <param name="durabilityVal"></param>
    public void DamageEffect(float durabilityVal)
    {
        // ダメージ 小
        if (durabilityVal <= 70 && durabilityVal > 50)
        {
            if (damageHighRoutine == null)
            {
                damageHighRoutine = DamageEffectCoroutine.DamaeHighRoutine(vignette, damageTypy_high, effectMaxTime, () =>
                {
                    // ルーチンの解放
                    damageCautionRoutine = null;
                    damageDangerRoutine = null;
                    damageFineRoutine = null;

                    // ルーチンの停止
                    StopCoroutine(damageHighRoutine);
                });

                // ルーチンの再生
                StartCoroutine(damageHighRoutine);

            }
        }
        // ダメージ 中
        else if (durabilityVal <= 50 && durabilityVal > 30)
        {
            if (damageCautionRoutine == null)
            {
                damageCautionRoutine = DamageEffectCoroutine.DamageCautionRoutine(vignette, damageType_Caution, effectMaxTime, () =>
                {
                    // ルーチンの解放
                    damageHighRoutine = null;
                    damageDangerRoutine = null;
                    damageFineRoutine = null;

                    // ルーチンの停止
                    StopCoroutine(damageCautionRoutine);
                });

                // ルーチンの再生
                StartCoroutine(damageCautionRoutine);
            }
        }
        // ダメージ 大
        else if (durabilityVal <= 30 && durabilityVal > 1)
        {
            if (damageDangerRoutine == null)
            {
                damageDangerRoutine = DamageEffectCoroutine.DamageDangerRoutine(vignette, damageType_Danger, effectMaxTime, () =>
                {
                    // ルーチンの解放
                    damageCautionRoutine = null;
                    damageHighRoutine = null;
                    damageFineRoutine = null;

                    // ルーチンの停止
                    StopCoroutine(damageDangerRoutine);
                });
                // ルーチンの再生
                StartCoroutine(damageDangerRoutine);
            }
        }
        // ダメージエフェクト無し
        else if (durabilityVal > 70)
        {
            if (damageFineRoutine == null)
            {
                damageFineRoutine = DamageEffectCoroutine.DamageFineRoutine(vignette, 0f, effectMaxTime, () =>
                {
                    // ルーチンの解放
                    damageCautionRoutine = null;
                    damageDangerRoutine = null;
                    damageHighRoutine = null;

                    // ルーチンの停止
                    StopCoroutine(damageFineRoutine);
                });
                // ルーチンの再生
                StartCoroutine(damageFineRoutine);
            }
        }
    }
}
