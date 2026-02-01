using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public static ActorController Instance { get; private set; }

    //settings

    [SerializeField] ActorHandler _agentPrefab = null;
    [SerializeField] ActorHandler _enemytPrefab = null;

    public float MoveTweenTime = 0.75f;
    Vector2Int _playerStartingSpot = new Vector2Int(3, 2);
    Vector2Int _enemyStartingSpot = new Vector2Int(6, 6);

    //state

    [SerializeField] List<ActorHandler> _actorTurnOrder = new List<ActorHandler>();
    public ActorHandler PriorityActor => _actorTurnOrder[0];

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnActors()
    {
        SpawnAgent();
        SpawnEnemy();

        PriorityActor.BeginTurn();
    }

    private void SpawnAgent()
    {
        var actor = Instantiate(_agentPrefab,
            TileController.Instance.GetTileAtVec2Int(_playerStartingSpot).VisualsTransform);
       
        AddActorToStartOfTurnOrder(actor);
    }

    private void SpawnEnemy()
    {
        var actor = Instantiate(_enemytPrefab,
            TileController.Instance.GetTileAtVec2Int(_enemyStartingSpot).VisualsTransform);

        AddActorToEndOfTurnOrder(actor);

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
        ActorHandler completedActor = PriorityActor;
        _actorTurnOrder.Remove(completedActor);
        AddActorToEndOfTurnOrder(completedActor);

        PriorityActor.BeginTurn();
    }

    #endregion
}
