using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DOTweenUXController : MonoBehaviour
{
    [Header("User Experience")]
    [SerializeField] private Toggle toggleTrails;
    [SerializeField] private Toggle toggleStretchSquash;
    [SerializeField] private Toggle toggleColorFlash;
    [SerializeField] private Toggle toggleTelegraph;
    [SerializeField] private Toggle toggleTimeDilation;
    [SerializeField] private Toggle toggleCameraShake;
    [SerializeField] private Toggle toggleParticles;

    [Header("UX References")]
    [SerializeField] private Renderer playerEnemyRenderer;
    [SerializeField] private Renderer playerJumpRenderer;
    [SerializeField] private Transform telegraphIndicator;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private TrailRenderer playerJumpTrail;
    [SerializeField] private TrailRenderer playerEnemyTrail;
    [SerializeField] private TrailRenderer playerBatTrail;
    [SerializeField] private ParticleSystem jumpDeathParticles;
    [SerializeField] private ParticleSystem enemyHitParticles;
    [SerializeField] private Transform mainCameraTransform;

    private Material jumpMaterial;
    private string jumpColorProperty;
    private Color jumpStartColor;
    private Material enemyMaterial;
    private string enemyColorProperty;
    private Color enemyStartColor;
    private Vector3 telegraphStartScale;
    private Tween timeDilationTween;
    private float initialTimeScale;

    internal bool StretchSquashEnabled => toggleStretchSquash.isOn;
    internal bool ColorFlashEnabled => toggleColorFlash.isOn;
    internal bool TelegraphEnabled => toggleTelegraph.isOn;
    internal bool TimeDilationEnabled => toggleTimeDilation.isOn;
    internal bool CameraShakeEnabled => toggleCameraShake.isOn && mainCameraTransform != null;
    internal bool ParticlesEnabled => toggleParticles.isOn;

    private void Awake()
    {
        //todos los efectos empiezan apagados sin disparar los eventos de los toggles
        toggleTrails.SetIsOnWithoutNotify(false);
        toggleStretchSquash.SetIsOnWithoutNotify(false);
        toggleColorFlash.SetIsOnWithoutNotify(false);
        toggleTelegraph.SetIsOnWithoutNotify(false);
        toggleTimeDilation.SetIsOnWithoutNotify(false);
        toggleCameraShake.SetIsOnWithoutNotify(false);
        toggleParticles.SetIsOnWithoutNotify(false);
        SetTrailsEnabled(false);

        initialTimeScale = Time.timeScale;
        telegraphStartScale = telegraphIndicator.localScale;
        telegraphIndicator.gameObject.SetActive(false);

        //usamos el material de cada personaje para que el destello solo le afecte a el
        jumpMaterial = playerJumpRenderer.material;
        jumpColorProperty = GetColorProperty(jumpMaterial);
        jumpStartColor = jumpMaterial.GetColor(jumpColorProperty);

        enemyMaterial = playerEnemyRenderer.material;
        enemyColorProperty = GetColorProperty(enemyMaterial);
        enemyStartColor = enemyMaterial.GetColor(enemyColorProperty);
    }

    private void Start()
    {
        //las estelas cambian al tocar el toggle aunque ya haya una secuencia en marcha
        toggleTrails.onValueChanged.AddListener(SetTrailsEnabled);
    }

    private void OnDestroy()
    {
        toggleTrails.onValueChanged.RemoveListener(SetTrailsEnabled);
        StopTimeDilation();
        StopParticles(jumpDeathParticles);
        StopParticles(enemyHitParticles);

        if (jumpMaterial != null)
            Destroy(jumpMaterial);

        if (enemyMaterial != null)
            Destroy(enemyMaterial);
    }

    internal void ResetJumpEffects()
    {
        jumpMaterial.SetColor(jumpColorProperty, jumpStartColor);

        if (playerJumpTrail != null)
            playerJumpTrail.Clear();

        StopParticles(jumpDeathParticles);
    }

    internal void ResetCombatEffects()
    {
        enemyMaterial.SetColor(enemyColorProperty, enemyStartColor);
        telegraphIndicator.gameObject.SetActive(false);
        telegraphIndicator.localScale = telegraphStartScale;
        StopParticles(enemyHitParticles);
        StopTimeDilation();
    }

    internal void ClearCombatTrails()
    {
        if (playerEnemyTrail != null)
            playerEnemyTrail.Clear();

        if (playerBatTrail != null)
            playerBatTrail.Clear();
    }

    internal Sequence CreateSquashStretch(Transform target, Vector3 originalScale)
    {
        Vector3 squashedScale = Vector3.Scale(originalScale, new Vector3(1.2f, 0.7f, 1.2f));
        Vector3 stretchedScale = Vector3.Scale(originalScale, new Vector3(0.9f, 1.15f, 0.9f));

        //primero aplastamos, luego estiramos y al final recuperamos la escala inicial
        return DOTween.Sequence()
            .Append(target.DOScale(squashedScale, 0.10f))
            .Append(target.DOScale(stretchedScale, 0.10f))
            .Append(target.DOScale(originalScale, 0.12f).SetEase(Ease.OutBack));
    }

    internal Tween CreateJumpFlash()
    {
        return jumpMaterial.DOColor(flashColor, jumpColorProperty, 0.08f).SetLoops(2, LoopType.Yoyo);
    }

    internal Tween CreateEnemyFlash()
    {
        return enemyMaterial.DOColor(flashColor, enemyColorProperty, 0.08f).SetLoops(2, LoopType.Yoyo);
    }

    private static string GetColorProperty(Material material)
    {
        return material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
    }

    internal void AddTelegraph(Sequence sequence)
    {
        //mostramos el aviso, lo dejamos un momento en pantalla y lo ocultamos antes del golpe
        sequence.AppendCallback(() =>
        {
            telegraphIndicator.gameObject.SetActive(true);
            telegraphIndicator.localScale = Vector3.zero;
        });
        sequence.Append(telegraphIndicator.DOScale(telegraphStartScale, 0.3f).SetEase(Ease.OutBack));
        sequence.AppendInterval(0.3f);
        sequence.AppendCallback(() => telegraphIndicator.gameObject.SetActive(false));
    }

    internal void PlayTimeDilation()
    {
        StopTimeDilation();

        //usamos tiempo real para que la camara lenta pueda recuperar la velocidad por si sola
        timeDilationTween = DOTween.Sequence()
            .SetRecyclable(false)
            .Append(DOTween.To(() => Time.timeScale, value => Time.timeScale = value,
                initialTimeScale * 0.2f, 0.08f))
            .AppendInterval(0.15f)
            .Append(DOTween.To(() => Time.timeScale, value => Time.timeScale = value,
                initialTimeScale, 0.3f))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = initialTimeScale;
                timeDilationTween = null;
            });
    }

    private void StopTimeDilation()
    {
        if (timeDilationTween == null)
            return;

        timeDilationTween.Kill();
        timeDilationTween = null;
        Time.timeScale = initialTimeScale;
    }

    internal Tween CreateCameraShake()
    {
        return mainCameraTransform.DOShakePosition(0.3f, new Vector3(0.12f, 0.08f, 0.02f),
            20, 90f, false, true);
    }

    private void SetTrailsEnabled(bool enabled)
    {
        SetTrailEnabled(playerJumpTrail, enabled);
        SetTrailEnabled(playerEnemyTrail, enabled);
        SetTrailEnabled(playerBatTrail, enabled);
    }

    private static void SetTrailEnabled(TrailRenderer trail, bool enabled)
    {
        if (trail == null)
            return;

        //limpiamos el rastro anterior cada vez que cambiamos el estado de la estela
        trail.Clear();
        trail.enabled = enabled;
        trail.emitting = enabled;
    }

    internal void PlayJumpParticles(Vector3 position)
    {
        PlayParticles(jumpDeathParticles, position);
    }

    internal void PlayEnemyParticles(Vector3 position)
    {
        PlayParticles(enemyHitParticles, position);
    }

    private static void PlayParticles(ParticleSystem particles, Vector3 position)
    {
        if (particles == null)
            return;

        //movemos el emisor al impacto y limpiamos lo que quedaba de la reproduccion anterior
        particles.transform.position = position;
        StopParticles(particles);
        particles.Play(true);
    }

    private static void StopParticles(ParticleSystem particles)
    {
        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
