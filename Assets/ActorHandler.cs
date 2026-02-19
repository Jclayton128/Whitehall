using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering;


public class ActorHandler : MonoBehaviour
{
    //ref
    [SerializeField] bool _isAgent = false;

    [SerializeField] AgentData _agentData;
    public AgentData AgentData => _agentData;
    [SerializeField] SpriteRenderer _portraitSR = null;

    [SerializeField] int _moveRange = 1;
    [SerializeField] int _searchRange = 1;

    //state
    Tween _slideTween;
    public bool IsAgent => _isAgent;

    public TileHandler CurrentTile => GetCurrentTile();
    public List<TileHandler> LegalMoves => GetLegalMoves();
    public List<TileHandler> LegalSearches => GetLegalSearches();

    public Sprite ActorSprite => _portraitSR.sprite;

    [SerializeField] List<AgentData.AgentAbility> _abilityQueue = new List<AgentData.AgentAbility>();

    public void SetAgentData(AgentData data, int agentIndex)
    {
        _agentData = data;
        _portraitSR.sprite = data.AgentSprite;

        if (_isAgent)
        {
            ShowSprite();
        }
        else
        {
            HideSprite();
        }

    }

    public void SetDefaultAgentData()
    {
        SetAgentData(_agentData, 0);
    }

    public void ExecuteClickViaCurrentAction(TileHandler clickedTile)
    {
        if (clickedTile == CurrentTile)
        {
            CompleteAction();
            return;
        }

        if (_abilityQueue[0] == AgentData.AgentAbility.Move && LegalMoves.Contains(clickedTile))
        {
            TileController.Instance.DeHighlightAllTiles();
            SlideToNewTile(clickedTile);
            return;
        }
        else if (_abilityQueue[0] == AgentData.AgentAbility.Search && LegalSearches.Contains(clickedTile))
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
        List<TileHandler> legalMoves = new List<TileHandler>();

        if (_moveRange == 1)
        {
            foreach (var move in CurrentTile.LinkedTiles)
            {
                if (move.Occupant == null ||
                    (move.Occupant != null && !move.Occupant.IsAgent))
                {
                    legalMoves.Add(move);
                }
            }
        }
        else if (_moveRange == 2)
        {
            foreach (var move in CurrentTile.LinkedTiles)
            {
                if (move.Occupant == null ||
                    (move.Occupant != null && !move.Occupant.IsAgent))
                {
                    legalMoves.Add(move);
                }

                foreach (var move2 in move.LinkedTiles)
                {
                    if ((move2.Occupant == null ||
                        (move2.Occupant != null && !move2.Occupant.IsAgent))
                        && move2 != move)
                    {
                        legalMoves.Add(move2);
                    }
                }

            }

        }

            return legalMoves;
    }

    private List<TileHandler> GetLegalSearches()
    {
        //LegalMoves.Clear();
        List<TileHandler> legalSearches = new List<TileHandler>();

        if (_searchRange == 1)
        {
            foreach (var search in CurrentTile.LinkedTiles)
            {
                legalSearches.Add(search);
            }
        }
        else if (_searchRange == 2)
        {
            foreach (var search in CurrentTile.LinkedTiles)
            {
                legalSearches.Add(search);

                foreach (var search2 in search.LinkedTiles)
                {
                    legalSearches.Add(search2);
                }

            }

        }

        return legalSearches;

    }
    private void HighlightPossibleOptions()
    {
        if (!_isAgent) return;

        //highlight possible options
        if (_abilityQueue[0] == AgentData.AgentAbility.Move)
        {
            foreach (var tile in LegalMoves)
            {
                tile.ColorTileToAbility(_abilityQueue[0]);
            }
        }
        else if (_abilityQueue[0] == AgentData.AgentAbility.Search)
        {
            foreach (var tile in LegalSearches)
            {
                tile.ColorTileToAbility(_abilityQueue[0]);
            }
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

    public void ShowSprite()
    {
        _portraitSR.enabled = true;
    }

    public void HideSprite()
    {
        _portraitSR.enabled = false;
    }

    #region AI

    public void BeginTurn()
    {
        if (CurrentTile == TileController.Instance.FoxDestinationTile)
        {
            Debug.Log("Fox wins!");
        }

        _abilityQueue = new List<AgentData.AgentAbility>(_agentData.AgentAbilities);
        TileController.Instance.DeHighlightAllTiles();
        if (_isAgent)
        {
            HighlightPossibleOptions();
        }
        else
        {
            Debug.Log("Fox does his movement...");

            TileController.Instance.FindAllDestinationDistances();
            TileController.Instance.FindAllAgentDistances();




            //TileHandler highestAgent = null;
            //int bestAgentScore = 0;
            //foreach (var tile in CurrentTile.RandomizedLinkedTiles)
            //{
            //    if (tile.AgentDist > bestAgentScore)
            //    {
            //        highestAgent = tile;
            //        bestAgentScore = tile.AgentDist;
            //    }
            //}

            //TileHandler lowestDestination = null;
            //int bestDestinationScore = int.MaxValue;
            //foreach (var tile in CurrentTile.RandomizedLinkedTiles)
            //{
            //    if (tile.DestinationDist < bestDestinationScore)
            //    {
            //        lowestDestination = tile;
            //        bestDestinationScore = tile.DestinationDist;
            //    }
            //}

            //TileHandler nextTile = null;
            //if (lowestDestination.AgentDist <= 1)
            //{
            //    nextTile = ;
            //}
            //else if ()

            float scoreToBeat = 0;
            TileHandler nextTile = null;
            foreach (var tile in CurrentTile.LinkedTiles)
            {
                float score = 10 + tile.AgentDist - tile.DestinationDist;
                Debug.Log($"{tile.TileIndex} scored {score}");
                if (score > scoreToBeat)
                {
                    nextTile = tile;
                    scoreToBeat = score;
                }
                else if (score == scoreToBeat)
                {
                    if (tile.AgentDist > nextTile.AgentDist)
                    {
                        nextTile = tile;
                    }
                    else
                    {
                        //keep nextTile as nextTile if still coequal
                    }
                }
            }


            nextTile.SetClue(TileHandler.ClueTypes.Passage);
            SlideToNewTile(nextTile);
        }

    }

    public void CompleteTurn()
    {
        ActorController.Instance.HandlePriorityActorTurnCompletion();
    }

    #endregion
}
