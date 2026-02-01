using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ActorController : MonoBehaviour
{
    public static ActorController Instance { get; private set; }

    //settings

    [SerializeField] TextMeshProUGUI _turnCountTMP = null;

    [SerializeField] Image[] _turnorderIcons = null;
    Vector2 _smallScale = Vector2.one * 0.5f;

    [SerializeField] ActorHandler[] _agentPrefab = null;
    [SerializeField] ActorHandler _enemyPrefab = null;


    [SerializeField] Vector2Int[] _playerStartingSpot = null;
    Vector2Int _enemyStartingSpot = new Vector2Int(6, 6);

    float _turnOrderTweenTime = 0.5f;
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
            _turnorderIcons[i].sprite = _actorTurnOrder[i].ActorSprite;
            _turnorderIcons[i].GetComponent<RectTransform>().localScale = _smallScale;
        }

        _priorityIndex = 0;
        _turnorderIcons[_priorityIndex].GetComponent<RectTransform>().DOScale(Vector2.one, _turnOrderTweenTime);
        PriorityActor.BeginTurn();
        _turns = 1;
        _turnCountTMP.text = "1";
    }

    private void SpawnAgent(int index)
    {
        var actor = Instantiate(_agentPrefab[index],
            TileController.Instance.GetTileAtVec2Int(_playerStartingSpot[index]).VisualsTransform);

        AddActorToEndOfTurnOrder(actor);
    }

    private void SpawnEnemy()
    {
        var actor = Instantiate(_enemyPrefab,
            TileController.Instance.GetTileAtVec2Int(_enemyStartingSpot).VisualsTransform);

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
        _turnorderIcons[_priorityIndex].GetComponent<RectTransform>().DOScale(_smallScale, _turnOrderTweenTime);
        
        _priorityIndex++;
        if (_priorityIndex >= _actorTurnOrder.Count)
        {
            _priorityIndex = 0;
            _turns ++;
            _turnCountTMP.text = _turns.ToString();
        }

        _turnorderIcons[_priorityIndex].GetComponent<RectTransform>().DOScale(Vector2.one, _turnOrderTweenTime);
        PriorityActor.BeginTurn();
    }

    #endregion
}
