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
    [SerializeField] GameObject _gameEndPanel = null;

    //settings

    [SerializeField] int _maxTurns = 15;


    //state
    int _turns = 0;
    public int TurnCount => _turns;
    public int RemainingTurns => (_maxTurns - _turns);
    public GameStates GameState = GameStates.OutOfRun;

    private void Awake()
    {
        Instance = this;
        GameState = GameStates.OutOfRun;
        //_gameEndPanel.SetActive(false);
    }

    public void StartRun()
    {
        _gameEndPanel.SetActive(false);
        GameState = GameStates.InRun;

        TileController.Instance.BuildNewArena();
        ActorController.Instance.ClearActors();
        ActorController.Instance.SpawnActors();
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
        _gameEndTMP.text = "Victory - you found the fox!";
        GameState = GameStates.OutOfRun;
        ReplayController.Instance.BeginPlayback();
    }

    public void EndRun_Victory_Time()
    {
        _gameEndPanel.SetActive(true);
        _gameEndTMP.text = "Victory - the fox ran out of time!";
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
        _turnCountTMP.text = $"{_turns} / {_maxTurns}";
    }
}
