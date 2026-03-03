using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;


public class ActorHandler : MonoBehaviour
{
    //ref
    [SerializeField] bool _isAgent = false;

    [SerializeField] AgentData _agentData;
    public AgentData AgentData => _agentData;
    [SerializeField] SpriteRenderer _portraitSR = null;

    [SerializeField] int _moveRange = 1;
    [SerializeField] int _searchRange = 1;

    [SerializeField] AnimationCurve _agentWeighting = null;
    [SerializeField] AnimationCurve _destinationWeighting = null;

    //state
    Tween _slideTween;
    public bool IsAgent => _isAgent;

    public TileHandler CurrentTile => GetCurrentTile();
    public List<TileHandler> LegalMoves => GetLegalMoves();
    public List<TileHandler> LegalSearches => GetLegalSearches();

    public Sprite ActorSprite => _portraitSR.sprite;

    [SerializeField] List<AgentData.AgentAbility> _abilityQueue = new List<AgentData.AgentAbility>();
    List<TileHandler> _visitedTiles = new List<TileHandler>();
    bool _hasSearchedForCluesThisTurn = false;

    [SerializeField] TileHandler _startingTile;
    public TileHandler StartingTile => _startingTile;

    public void SetAgentData(AgentData data, int agentIndex)
    {
        _agentData = data;
        _portraitSR.sprite = data.AgentSprite;

        //ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Start, CurrentTile));

        if (_isAgent)
        {
            ShowSprite();
        }
        else
        {
            HideSprite();
        }

        _startingTile = GetCurrentTile();

    }

    public void SetDefaultAgentData()
    {
        SetAgentData(_agentData, 0);
    }

    public void ExecuteLMBClickViaCurrentAction(TileHandler clickedTile)
    {
        if (clickedTile == CurrentTile)
        {

            CompleteAction();
            return;
        }

        if (_abilityQueue[0] == AgentData.AgentAbility.Move && LegalMoves.Contains(clickedTile))
        {
            TileController.Instance.DeHighlightAllTiles();

            var pathing = TileController.Instance.GetShortestPathToDestination(CurrentTile, clickedTile);
            if (pathing.Count == 2)
            {
                SlideToNewTile(pathing[1]);
            }
            else if (pathing.Count == 3)
            {
                SlideToNewTileViaSecondTile (pathing[2], pathing[1]);
            }

                return;
        }
        else if (_abilityQueue[0] == AgentData.AgentAbility.Search && LegalSearches.Contains(clickedTile))
        {
            _hasSearchedForCluesThisTurn = true;
            bool foundClue = TileController.Instance.SearchForClue(clickedTile);
            if (foundClue)
            {
                CompleteAction();
            }
            return;
        }
    }

    public void ExecuteRMBClickViaCurrentAction(TileHandler clickedTile)
    {
        if (!LegalSearches.Contains(clickedTile))
        {
            //can't arrest in places you can't search
            //do nothing
            return;
        }

        if (_hasSearchedForCluesThisTurn)
        {
            //can't arrest on the same turn that you've searched for clues
            return;
        }

        if (ActorController.Instance.Enemy.CurrentTile == clickedTile)
        {
            Debug.Log("Attempting arrest: Found the fox!");
            ActorController.Instance.Enemy.ShowSprite();
            GameController.Instance.EndRun_Victory_Arrest();
        }
        else
        {
            Debug.Log("Attempting arrest: nothing...");
        }

        CompleteTurn();
        return;
        
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
        ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Move, CurrentTile, newTile));

        transform.parent = newTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            ActorController.Instance.MoveTweenTime).OnComplete(CompleteAction);

        
    }

    private void SlideToNewTileViaSecondTile(TileHandler destinationTile, TileHandler intermediateTile)
    {
        _slideTween.Kill();

        ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Move, CurrentTile, intermediateTile));
        ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Move, intermediateTile, destinationTile));

        transform.DOMove(intermediateTile.transform.position, ActorController.Instance.MoveTweenTime / 2f).SetEase(Ease.Linear);

        transform.parent = destinationTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            ActorController.Instance.MoveTweenTime/2f).SetDelay(ActorController.Instance.MoveTweenTime / 2f).OnComplete(CompleteAction).SetEase(Ease.Linear);

        
    }

    public void SlideToNewTile(TileHandler newTile, float time)
    {
        _slideTween.Kill();

        transform.parent = newTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero, time);
    }

    public void SlideToNewTileViaSecondTile(TileHandler destinationTile, TileHandler intermediateTile, float time)
    {
        _slideTween.Kill();

        transform.DOMove(intermediateTile.transform.position, time / 2f).SetEase(Ease.Linear);

        transform.parent = destinationTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            time / 2f).SetDelay(time / 2f).SetEase(Ease.Linear);

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
        if (GameController.Instance.GameState == GameController.GameStates.OutOfRun) return;
        _abilityQueue = new List<AgentData.AgentAbility>(_agentData.AgentAbilities);
        TileController.Instance.DeHighlightAllTiles();
        if (_isAgent)
        {
            HighlightPossibleOptions();
            _hasSearchedForCluesThisTurn = false;
        }
        else
        {
            if (CurrentTile == TileController.Instance.FoxDestinationTile)
            {
                GameController.Instance.EndRun_Defeat();
                Debug.Log("Fox wins!");
                return;
            }

            else if (GameController.Instance.RemainingTurns <= 0)
            {
                GameController.Instance.EndRun_Victory_Arrest();
                Debug.Log("Agents win via time!");
                return;
            }


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

            float scoreToBeat = float.NegativeInfinity;
            TileHandler nextTile = null;
            foreach (var tile in CurrentTile.LinkedTiles)
            {
                


                //as times goes on, moving to destination begins to be worth more
                float score = GenerateAgentScore(tile.AgentDist) + GenerateDestinationScore(tile.DestinationDist);

                if (_visitedTiles.Contains(tile)) 
                {
                    //As time goes on, doubling-back more penalized.
                    score -= (0.02f * GameController.Instance.TurnCount);
                }

                float randomFuzz = UnityEngine.Random.Range(-.1f, .1f);

                score += randomFuzz;
                
                if (DebugController.Instance && DebugController.Instance.IsInDebugMode)
                {
                    Debug.Log($"{tile.TileIndex} scored {score}. Fuzz: {randomFuzz}. Agent Score: {GenerateAgentScore(tile.AgentDist)}. Dest Score: {GenerateDestinationScore(tile.DestinationDist)}");
                }
                
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
            _visitedTiles.Add(nextTile);
        }

    }

    private float GenerateDestinationScore(int destinationDistance)
    {
        float destinationFactor = Mathf.Clamp01(Mathf.InverseLerp(0, 12, destinationDistance));

        float destScore;

        if (destinationDistance < GameController.Instance.RemainingTurns - 1)
        {
            destScore = (_destinationWeighting.Evaluate(destinationFactor) * (1 + (0.1f * GameController.Instance.TurnCount)));
        }
        else
        {
            destScore = _destinationWeighting.Evaluate(destinationFactor) * 10;
        }
            return destScore;
    }

    private float GenerateAgentScore(int agentDistance)
    {
        float agentFactor = Mathf.InverseLerp(0, 12, agentDistance);
        return _agentWeighting.Evaluate(agentFactor);
    }

    public void CompleteTurn()
    {
        ActorController.Instance.HandlePriorityActorTurnCompletion();
    }

    #endregion
}
