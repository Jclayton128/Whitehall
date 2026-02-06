using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ActorController : MonoBehaviour
{
    public static ActorController Instance { get; private set; }
    public Action PriorityActorTurnCompleting;


    //settings

    [SerializeField] TextMeshProUGUI _turnCountTMP = null;

    [SerializeField] PortraitDriver[] _turnorderPortraits = null;


    [SerializeField] ActorHandler _agentPrefab = null;
    [SerializeField] AgentData[] _agentData = null;
    [SerializeField] ActorHandler _enemyPrefab = null;

    public Sprite MoveAbilityIcon = null;
    public Texture2D MoveAbilityTexture = null;
    public Sprite SearchAbilityIcon = null;
    public Texture2D SearchAbilityTexture = null;

    [SerializeField] Vector2Int[] _playerStartingSpot = null;
    Vector2Int _enemyStartingSpot = new Vector2Int(6, 6);


    public float MoveTweenTime = 0.75f;

    //state
    int _turns = 0;
    int _priorityIndex = 0;
    [SerializeField] List<ActorHandler> _actorTurnOrder = new List<ActorHandler>();
    public ActorHandler PriorityActor => _actorTurnOrder[_priorityIndex];

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnActors()
    {

        SpawnEnemy();
        SpawnAgent(0);
        SpawnAgent(1);
        SpawnAgent(2);

        for (int i = 0; i  < _actorTurnOrder.Count; i++)
        {
            _turnorderPortraits[i].SetPortrait(_actorTurnOrder[i].AgentData);
        }

        _priorityIndex = 0;
        _turnorderPortraits[_priorityIndex].EnlargePortrait();
        PriorityActor.BeginTurn();

        _turns = 1;
        _turnCountTMP.text = _turns.ToString();
    }

    private void SpawnAgent(int index)
    {
        var actor = Instantiate(_agentPrefab,
            TileController.Instance.GetTileAtVec2Int(_playerStartingSpot[index]).VisualsTransform);
        actor.SetAgentData(_agentData[index], index + 1); //injecting the +1 manually because I want the fox to always be in the top right corner

        AddActorToEndOfTurnOrder(actor);
    }

    private void SpawnEnemy()
    {
        var startTile = TileController.Instance.GetTileAtVec2Int(_enemyStartingSpot);
        var actor = Instantiate(_enemyPrefab, startTile.VisualsTransform);
        startTile.SetClue(TileHandler.ClueTypes.Origin);
        actor.SetDefaultAgentData();
        AddActorToStartOfTurnOrder(actor);
    }


    #region Turn Mechanics

    public void StartBattle()
    {
        _actorTurnOrder.Clear();
    }

    public void AddActorToStartOfTurnOrder(ActorHandler newActor)
    {
        _actorTurnOrder.Insert(0, newActor);
    }

    public void AddActorToEndOfTurnOrder(ActorHandler newActor)
    {
        _actorTurnOrder.Add(newActor);
    }

    public void HandlePriorityActorTurnCompletion()
    {
        PriorityActorTurnCompleting?.Invoke();

        _turnorderPortraits[_priorityIndex].ShrinkPortrait();
        
        _priorityIndex++;
        if (_priorityIndex >= _actorTurnOrder.Count)
        {
            _priorityIndex = 0;
            _turns ++;
            _turnCountTMP.text = _turns.ToString();
        }

        _turnorderPortraits[_priorityIndex].EnlargePortrait();
        PriorityActor.BeginTurn();
    }

    #endregion
}
