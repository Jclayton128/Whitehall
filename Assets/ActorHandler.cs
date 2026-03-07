using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;



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
    Tween _colorTween;
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

            GameController.Instance.ModifyScore_Search();
            bool foundClue = TileController.Instance.SearchForClue(clickedTile);

            ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Search, clickedTile, 0));

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
            ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Arrest, clickedTile, 0));
            //Debug.Log("Attempting arrest: Found the fox!");

            clickedTile.SetActionTaken(TileHandler.ActionTypes.Arrested);
            PingController.Instance.SpawnPing(clickedTile.transform.position);
            ActorController.Instance.Enemy.ShowSprite();
            GameController.Instance.EndRun_Victory_Arrest();
        }
        else
        {
            GameController.Instance.ModifyScore_Arrest();
            ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Arrest, clickedTile, 0));
            clickedTile.SetActionTaken(TileHandler.ActionTypes.Arrested);
            //Debug.Log("Attempting arrest: nothing...");
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
        ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Move, newTile, 0));

        transform.parent = newTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            ActorController.Instance.MoveTweenTime).OnComplete(CompleteAction);

        if (!_isAgent)
        {
            newTile.SetClue(TileHandler.ClueTypes.Passage);
        }
    }

    public void SlideToNewTile_Replay(TileHandler newTile, float time)
    {
        _slideTween.Kill();

        transform.parent = newTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero, time);

        if (!_isAgent)
        {
            newTile.SetClue(TileHandler.ClueTypes.Passage);
        }
    }

    private void SlideToNewTileViaSecondTile(TileHandler destinationTile, TileHandler intermediateTile)
    {
        _slideTween.Kill();

        ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Move, intermediateTile, 0));
        ReplayController.Instance.AddStep(new ReplayStep(this, ReplayStep.StepTypes.Move, destinationTile, 0 ));

        transform.DOMove(intermediateTile.transform.position, ActorController.Instance.MoveTweenTime / 2f).SetEase(Ease.Linear);

        transform.parent = destinationTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            ActorController.Instance.MoveTweenTime/2f).SetDelay(ActorController.Instance.MoveTweenTime / 2f).OnComplete(CompleteAction).SetEase(Ease.Linear);

        if (!_isAgent)
        {
            intermediateTile.SetClue(TileHandler.ClueTypes.Passage);
            destinationTile.SetClue(TileHandler.ClueTypes.Passage);
        }
    }

    public void SlideToNewTileViaSecondTile_Replay(TileHandler destinationTile, TileHandler intermediateTile, float time)
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
            //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (_abilityQueue[0] == AgentData.AgentAbility.Move)
        {
            //Cursor.SetCursor(ActorController.Instance.MoveAbilityTexture, new Vector2(92, 92), CursorMode.Auto);
        }
        else if (_abilityQueue[0] == AgentData.AgentAbility.Search)
        {
            //Cursor.SetCursor(ActorController.Instance.SearchAbilityTexture, new Vector2(136, 136), CursorMode.Auto);
        }

    }

    public void ShowSprite()
    {
        _portraitSR.enabled = true;
        Color col = _portraitSR.color;
        col.a = 1;
        _portraitSR.color = col;
    }

    public void ShowSprite(float fadeTime)
    {
        _portraitSR.enabled = true;
        Color col = _portraitSR.color;
        col.a = 0;
        _portraitSR.color = col;
        _colorTween.Kill();
        _colorTween = _portraitSR.DOFade(1, fadeTime);
    }

    public void HideSprite()
    {
        _portraitSR.enabled = false;
        Color col = _portraitSR.color;
        col.a = 0;
        _portraitSR.color = col;
    }

    public void HideSprite(float fadeTime)
    {
        _portraitSR.enabled = true;
        _colorTween.Kill();
        _colorTween = _portraitSR.DOFade(0, fadeTime);
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
            if (CurrentTile == TileController.Instance.EnemyDestinationTile)
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
