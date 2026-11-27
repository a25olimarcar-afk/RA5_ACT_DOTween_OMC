using DG.Tweening;
using UnityEngine;

public class CombatScenarioController : MonoBehaviour
{
    [Header("Scenario 2 - Combat")]
    [SerializeField] private Transform playerAttack;
    [SerializeField] private Transform playerBat;
    [SerializeField] private Transform playerEnemy;
    [SerializeField] private Transform attackPosition;
    [SerializeField] private Transform enemyHitPoint;

    private DOTweenUXController ux;
    private Sequence combatSequence;
    private Vector3 attackStartPosition;
    private Quaternion attackStartRotation;
    private Vector3 attackStartScale;
    private Vector3 enemyStartPosition;
    private Quaternion enemyStartRotation;
    private Vector3 enemyStartScale;
    private Vector3 batStartPosition;
    private Quaternion batStartRotation;
    private Vector3 batStartScale;

    internal bool IsPlaying => combatSequence != null && combatSequence.IsActive() && combatSequence.IsPlaying();

    private void Awake()
    {
        ux = GetComponent<DOTweenUXController>();

        attackStartPosition = playerAttack.position;
        attackStartRotation = playerAttack.rotation;
        attackStartScale = playerAttack.localScale;

        enemyStartPosition = playerEnemy.position;
        enemyStartRotation = playerEnemy.rotation;
        enemyStartScale = playerEnemy.localScale;

        //guardamos el bate en local porque se mueve con el atacante
        batStartPosition = playerBat.localPosition;
        batStartRotation = playerBat.localRotation;
        batStartScale = playerBat.localScale;
    }

    private void OnDestroy()
    {
        combatSequence?.Kill();
    }

    internal void Play()
    {
        //limpiamos las estelas antes de recolocar a los personajes
        ux.ClearCombatTrails();
        Restart();
        PlaySequence();
    }

    internal void PlaySequence()
    {
        combatSequence.Play();
    }

    internal void Restart()
    {
        //paramos el ataque y recuperamos las posiciones, rotaciones y escalas iniciales
        combatSequence?.Kill();
        playerAttack.position = attackStartPosition;
        playerAttack.rotation = attackStartRotation;
        playerAttack.localScale = attackStartScale;

        playerEnemy.position = enemyStartPosition;
        playerEnemy.rotation = enemyStartRotation;
        playerEnemy.localScale = enemyStartScale;

        playerBat.localPosition = batStartPosition;
        playerBat.localRotation = batStartRotation;
        playerBat.localScale = batStartScale;

        ux.ResetCombatEffects();
        CreateCombatSequence();
        Debug.Log("Scenario 2 restarted!");
    }

    //preparamos el ataque con los efectos que estan marcados en este momento
    private void CreateCombatSequence()
    {
        combatSequence = DOTween.Sequence().SetRecyclable(false).Pause();

        //el atacante se acerca y levanta el bate para preparar el golpe
        combatSequence.Append(playerAttack.DOMove(attackPosition.position, 0.8f).SetEase(Ease.OutQuad));
        combatSequence.Append(playerBat.DOLocalMove(batStartPosition + Vector3.up * 0.5f, 0.35f)
            .SetEase(Ease.OutBack));

        if (ux.TelegraphEnabled)
            ux.AddTelegraph(combatSequence);

        combatSequence.Join(playerBat.DOLocalRotate(
            batStartRotation.eulerAngles + new Vector3(0f, 0f, -35f), 0.35f).SetEase(Ease.OutQuad));

        combatSequence.Append(playerBat.DOLocalMove(batStartPosition + Vector3.forward, 0.15f)
            .SetEase(Ease.InQuad));

        if (ux.ParticlesEnabled)
            combatSequence.AppendCallback(() => ux.PlayEnemyParticles(playerEnemy.position));

        if (ux.TimeDilationEnabled)
            combatSequence.AppendCallback(ux.PlayTimeDilation);

        //juntamos el retroceso y los efectos del impacto en el mismo punto de la secuencia
        combatSequence.Join(playerEnemy.DOMove(enemyHitPoint.position, 0.3f).SetEase(Ease.OutBounce));

        if (ux.CameraShakeEnabled)
            combatSequence.Join(ux.CreateCameraShake());

        if (ux.StretchSquashEnabled)
        {
            combatSequence.Join(ux.CreateSquashStretch(playerAttack, attackStartScale));
            combatSequence.Join(ux.CreateSquashStretch(playerEnemy, enemyStartScale));
        }

        if (ux.ColorFlashEnabled)
            combatSequence.Join(ux.CreateEnemyFlash());

        //el bate vuelve a su sitio y el enemigo cae antes de desaparecer
        combatSequence.Append(playerBat.DOLocalMove(batStartPosition, 0.25f).SetEase(Ease.OutQuad));
        combatSequence.Join(playerBat.DOLocalRotateQuaternion(batStartRotation, 0.25f));
        combatSequence.Append(playerEnemy.DORotate(
            enemyStartRotation.eulerAngles + new Vector3(90f, 0f, 0f), 0.5f).SetEase(Ease.InBack));
        combatSequence.Append(playerEnemy.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
        combatSequence.OnComplete(() => Debug.Log("Zombie died!"));
    }
}
