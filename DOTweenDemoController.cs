using UnityEngine;
using UnityEngine.UI;

public class DOTweenDemoController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button restartButton;

    [SerializeField] private Button Scene1PlayButton;
    [SerializeField] private Button Scene1RestartButton;
    [SerializeField] private Button Scene2PlayButton;
    [SerializeField] private Button Scene2RestartButton;

    private JumpScenarioController jumpScenario;
    private CombatScenarioController combatScenario;

    private void Awake()
    {
        jumpScenario = GetComponent<JumpScenarioController>();
        combatScenario = GetComponent<CombatScenarioController>();
    }

    private void Start()
    {
        //enlazamos los botones generales y los de cada escenario por codigo
        playButton.onClick.AddListener(PlayAll);
        restartButton.onClick.AddListener(RestartAll);
        Scene1PlayButton.onClick.AddListener(PlayScene1);
        Scene1RestartButton.onClick.AddListener(RestartScene1);
        Scene2PlayButton.onClick.AddListener(PlayScene2);
        Scene2RestartButton.onClick.AddListener(RestartScene2);
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(PlayAll);
        restartButton.onClick.RemoveListener(RestartAll);
        Scene1PlayButton.onClick.RemoveListener(PlayScene1);
        Scene1RestartButton.onClick.RemoveListener(RestartScene1);
        Scene2PlayButton.onClick.RemoveListener(PlayScene2);
        Scene2RestartButton.onClick.RemoveListener(RestartScene2);
    }

    private void PlayAll()
    {
        //si alguno sigue en marcha, esperamos a que termine antes de lanzar los dos
        if (jumpScenario.IsPlaying || combatScenario.IsPlaying)
            return;

        jumpScenario.Restart();
        combatScenario.Restart();
        jumpScenario.PlaySequence();
        combatScenario.PlaySequence();
    }

    private void RestartAll()
    {
        jumpScenario.Restart();
        combatScenario.Restart();
        Debug.Log("All simulations restarted!");
    }

    private void PlayScene1()
    {
        if (jumpScenario.IsPlaying)
            return;

        jumpScenario.Restart();
        jumpScenario.PlaySequence();
    }

    private void RestartScene1()
    {
        jumpScenario.Restart();
    }

    private void PlayScene2()
    {
        if (combatScenario.IsPlaying)
            return;

        combatScenario.Play();
    }

    private void RestartScene2()
    {
        combatScenario.Restart();
    }
}
