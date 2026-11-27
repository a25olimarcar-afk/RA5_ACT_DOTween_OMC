using DG.Tweening;
using UnityEngine;

public class JumpScenarioController : MonoBehaviour
{
    [Header("Scenario 1 - Jump")]
    [SerializeField] private Transform playerJump;
    [SerializeField] private Transform jumpLandingPoint;
    [SerializeField] private Transform jumpSlipPoint;
    [SerializeField] private Transform jumpSpikePoint;

    private DOTweenUXController ux;
    private Sequence jumpSequence;
    private Vector3 jumpStartPosition;
    private Quaternion jumpStartRotation;
    private Vector3 jumpStartScale;

    internal bool IsPlaying => jumpSequence != null && jumpSequence.IsActive() && jumpSequence.IsPlaying();

    private void Awake()
    {
        ux = GetComponent<DOTweenUXController>();

        //guardamos como empieza el jugador para poder recuperarlo al reiniciar
        jumpStartPosition = playerJump.position;
        jumpStartRotation = playerJump.rotation;
        jumpStartScale = playerJump.localScale;
    }

    private void OnDestroy()
    {
        jumpSequence?.Kill();
    }

    internal void PlaySequence()
    {
        jumpSequence.Play();
    }

    internal void Restart()
    {
        //paramos la secuencia anterior y devolvemos al jugador al punto de inicio
        jumpSequence?.Kill();
        playerJump.position = jumpStartPosition;
        playerJump.rotation = jumpStartRotation;
        playerJump.localScale = jumpStartScale;
        ux.ResetJumpEffects();

        CreateJumpSequence();
        Debug.Log("Scenario 1 restarted!");
    }

    //preparamos el recorrido con los efectos que estan marcados en este momento
    private void CreateJumpSequence()
    {
        jumpSequence = DOTween.Sequence().SetRecyclable(false).Pause();

        //el jugador salta a la plataforma y recupera la forma al aterrizar
        jumpSequence.Append(playerJump.DOJump(jumpLandingPoint.position, 2f, 2, 1.5f)
            .SetEase(Ease.OutQuad));

        if (ux.StretchSquashEnabled)
            jumpSequence.Append(ux.CreateSquashStretch(playerJump, jumpStartScale));

        jumpSequence.AppendInterval(0.1f);

        //el jugador se inclina al resbalar y cae hacia los pinchos
        jumpSequence.Append(playerJump.DOMove(jumpSlipPoint.position, 0.5f).SetEase(Ease.InQuad));
        jumpSequence.Join(playerJump.DORotate(new Vector3(20f, 0f, 45f), 0.2f)
            .SetEase(Ease.InOutQuad));
        jumpSequence.Append(playerJump.DOMove(jumpSpikePoint.position, 0.8f).SetEase(Ease.OutBounce));

        if (ux.ParticlesEnabled)
            jumpSequence.AppendCallback(() => ux.PlayJumpParticles(jumpSpikePoint.position));

        //si estan activos los dos efectos, el destello empieza junto al aplastamiento
        if (ux.StretchSquashEnabled)
        {
            jumpSequence.Append(ux.CreateSquashStretch(playerJump, jumpStartScale));

            if (ux.ColorFlashEnabled)
                jumpSequence.Join(ux.CreateJumpFlash());
        }
        else if (ux.ColorFlashEnabled)
        {
            jumpSequence.Append(ux.CreateJumpFlash());
        }

        jumpSequence.Join(playerJump.DORotate(new Vector3(20f, 0f, 180f), 1f).SetEase(Ease.InQuad));
        jumpSequence.Append(playerJump.DOScale(Vector3.zero, 2f).SetEase(Ease.InBack));
        jumpSequence.OnComplete(() => Debug.Log("Jumping Player died!"));
    }
}
