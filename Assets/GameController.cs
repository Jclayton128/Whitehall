using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public enum GameStates {OutOfRun, InRun }

    //refs
    [SerializeField] TextMeshProUGUI _turnCountTMP = null;
    [SerializeField] TextMeshProUGUI _gameEndTMP = null;
    [SerializeField] TextMeshProUGUI _scoreTMP = null;
    [SerializeField] GameObject _gameEndPanel = null;

    //settings
    [SerializeField] int _startingScore = 1000;
    [SerializeField] int _arrestPenalty = 100;
    [SerializeField] int _searchPenalty = 10;
    [SerializeField] int _secondPenalty = 1;
    [SerializeField] int _maxTurns = 15;
    public int MaxTurns => _maxTurns;


    //state

    int _turns = 0;
    public int TurnCount => _turns;
    public int RemainingTurns => (_maxTurns - _turns);
    public GameStates GameState = GameStates.OutOfRun;
    int _score;
    float _timer;

    private void Awake()
    {
        Instance = this;
        GameState = GameStates.OutOfRun;
        //_gameEndPanel.SetActive(false);
    }

    public void StartRun()
    {
        _timer = 0;
        _score = _startingScore;
        _scoreTMP.text = "Score: " + _score.ToString();

        _gameEndPanel.SetActive(false);
        GameState = GameStates.InRun;

        TileController.Instance.BuildNewArena();
        ActorController.Instance.ClearActors();
        ActorController.Instance.SpawnActors();
        TileController.Instance.SetActorSpecificTiles();

        ReplayController.Instance.ResetReplayQueue();

        ActorController.Instance.StartPriorityActor();

        _turns = 1;
        _turnCountTMP.text = $"{_turns} / {_maxTurns}";

    }

    public void EndRun_Defeat()
    {
        _gameEndPanel.SetActive(true);
        _gameEndTMP.text = "Defeat - the fox got away!";
        GameState = GameStates.OutOfRun;
        ReplayController.Instance.BeginPlayback();
    }

    public void EndRun_Victory_Arrest()
    {
        _gameEndPanel.SetActive(true);
        _gameEndTMP.text = $"Victory - you found the fox! Score: {_score.ToString()}";
        GameState = GameStates.OutOfRun;
        ReplayController.Instance.BeginPlayback();
    }

    public void EndRun_Victory_Time()
    {
        _gameEndPanel.SetActive(true);
        _gameEndTMP.text = $"Victory - the fox ran out of time! Score: {_score.ToString()}";
        GameState = GameStates.OutOfRun;
        ReplayController.Instance.BeginPlayback();
    }

    public void EndRun_Neither()
    {
        _gameEndPanel.SetActive(true);
        _gameEndTMP.text = "FOXHUNT";
        GameState = GameStates.OutOfRun;
    }

    public void AdvanceTurn()
    {
        _turns++;
        SetTurnIndication(_turns);
        ReplayController.Instance.AddStep(new ReplayStep(null, ReplayStep.StepTypes.TurnCountIncrement, null, _turns));
    }

    public void SetTurnIndication(int turnToDisplay)
    {
        _turnCountTMP.text = $"{turnToDisplay} / {_maxTurns}";
    }

    private void Update()
    {
        if (GameState == GameStates.InRun)
        {
            _timer += Time.deltaTime;
            if (_timer >= 1.0f)
            {
                _timer = 0;
                ModifyScore(_secondPenalty);
            }
        }
    }

    private void ModifyScore(int scoreToSubtract)
    {
        _score -= scoreToSubtract;
        _scoreTMP.text = "Score: " + _score.ToString();
    }

    public void ModifyScore_Arrest()
    {
        ModifyScore(_arrestPenalty);
    }
    public void ModifyScore_Search()
    {
        ModifyScore(_searchPenalty);
    }
}
