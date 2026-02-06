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
    [SerializeField] SpriteRenderer[] _portraitSRs = null;

    //state
    Tween _slideTween;
    public bool IsAgent => _isAgent;

    public TileHandler CurrentTile => GetCurrentTile();
    public List<TileHandler> LegalMoves => GetLegalMoves();

    public Sprite ActorSprite => _portraitSRs[0].sprite;

    [SerializeField] List<AgentData.AgentAbility> _abilityQueue = new List<AgentData.AgentAbility>();

    public void SetAgentData(AgentData data, int agentIndex)
    {
        _agentData = data;
        foreach (var sr in _portraitSRs)
        {
            sr.sprite = _agentData.AgentSprite;
            sr.enabled = false;
        }
        _portraitSRs[agentIndex].enabled = true;
    }

    public void SetDefaultAgentData()
    {
        SetAgentData(_agentData, 0);
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
            TileController.Instance.DeHighlightAllTiles();
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
            SetCursorMatchNextAction();

            if (_abilityQueue.Count == 0)
            {
                CompleteTurn();
            }
            else
            {
                TileController.Instance.DeHighlightAllTiles();
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
            tile.ColorTileToAbility(_abilityQueue[0]);
        }
        CurrentTile.ColorTileToAbility(AgentData.AgentAbility.Pass);
        
        SetCursorMatchNextAction();

    }

    private void SetCursorMatchNextAction()
    {
        if (_abilityQueue.Count == 0)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (_abilityQueue[0] == AgentData.AgentAbility.Move)
        {
            Cursor.SetCursor(ActorController.Instance.MoveAbilityTexture, new Vector2(92, 92), CursorMode.Auto);
        }
        else if (_abilityQueue[0] == AgentData.AgentAbility.Search)
        {
            Cursor.SetCursor(ActorController.Instance.SearchAbilityTexture, new Vector2(136, 136), CursorMode.Auto);
        }

    }

    public void BeginTurn()
    {
        _abilityQueue = new List<AgentData.AgentAbility>(_agentData.AgentAbilities);
        TileController.Instance.DeHighlightAllTiles();
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
