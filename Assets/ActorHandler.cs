using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


public class ActorHandler : MonoBehaviour
{
    //ref
    [SerializeField] bool _isAgent = false;

    [SerializeField] AgentData _agentData;
    public AgentData AgentData => _agentData;
    [SerializeField] SpriteRenderer _visualSR = null;

    //state
    Tween _slideTween;
    public bool IsAgent => _isAgent;

    public TileHandler CurrentTile => GetCurrentTile();
    public List<TileHandler> LegalMoves => GetLegalMoves();

    public Sprite ActorSprite => _visualSR.sprite;

    [SerializeField] List<AgentData.AgentAbility> _abilityQueue = new List<AgentData.AgentAbility>();

    public void SetAgentData(AgentData data)
    {
        _agentData = data;
        _visualSR.sprite = _agentData.AgentSprite;
    }

    public void ExecuteClickViaCurrentAction(TileHandler clickedTile)
    {
        if (clickedTile == CurrentTile)
        {
            Debug.Log("skipping action");
            CompleteAction();
            return;
        }

        if (_abilityQueue[0] == AgentData.AgentAbility.Move)
        {
            SlideToNewTile(clickedTile);
            return;
        }
        else if (_abilityQueue[0] == AgentData.AgentAbility.Search)
        {
            bool foundClue = TileController.Instance.SearchForClue(clickedTile);
            if (foundClue)
            {
                CompleteAction();
            }
            return;
        }
    }

    private void CompleteAction()
    {
        if (_abilityQueue.Count == 0)
        {
            CompleteTurn();
        }
        else
        {
            _abilityQueue.RemoveAt(0);
            if (_abilityQueue.Count == 0)
            {
                CompleteTurn();
            }
            else
            {
                TileController.Instance.UnraiseAllTiles();
                if (_isAgent)
                {

                    HighlightPossibleOptions();
                }
            }
        }


    }

    private void SlideToNewTile(TileHandler newTile)
    {
        _slideTween.Kill();

        transform.parent = newTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            ActorController.Instance.MoveTweenTime).OnComplete(CompleteAction);
    }


    private TileHandler GetCurrentTile()
    {
        return transform.parent.parent.GetComponent<TileHandler>();
    }

    private List<TileHandler> GetLegalMoves()
    {
        //LegalMoves.Clear();
        return CurrentTile.LinkedTiles;
    }

    #region AI

    private void HighlightPossibleOptions()
    {
        if (!_isAgent) return;

        //highlight possible options
        foreach (var tile in LegalMoves)
        {
            tile.HalfraiseTile();
        }
        CurrentTile.HalfraiseTile();
    }

    public void BeginTurn()
    {
        _abilityQueue = new List<AgentData.AgentAbility>(_agentData.AgentAbilities);
        TileController.Instance.UnraiseAllTiles();
        if (_isAgent)
        {
            HighlightPossibleOptions();
        }
        else
        {
            Debug.Log("Monster does his movement...");

            int rand = UnityEngine.Random.Range(0, LegalMoves.Count);
            var newTile = LegalMoves[rand];
            newTile.SetClue(TileHandler.ClueTypes.Passage);
            SlideToNewTile(newTile);
        }

    }

    public void CompleteTurn()
    {
        ActorController.Instance.HandlePriorityActorTurnCompletion();
    }

    #endregion
}
