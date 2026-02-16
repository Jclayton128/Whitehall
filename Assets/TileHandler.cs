using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TileHandler : MonoBehaviour
{
    public enum ClueTypes { None, Origin, Passage, JustSearched }

    //refs
    [SerializeField] SpriteRenderer _tileSR = null;
    [SerializeField] SpriteRenderer _borderSR = null;
    [SerializeField] SpriteRenderer _clueSR = null;
    [SerializeField] TextMeshPro _text = null;
    [SerializeField] TextMeshPro _agentDistanceTMP = null;
    [SerializeField] TextMeshPro _destinationDistanceTMP = null;

    [SerializeField] Transform _visualsTransform = null;
    public Transform VisualsTransform => _visualsTransform;

    Collider2D _coll;

    //settings
    [SerializeField] float hoverTweenTime = 0.125f;
    [SerializeField] float _hoverYIncrease = 0.1f;


    //state
    Tween _hoverTween;
    Tween _colorTween;
    public Vector2Int IndexPos;
    public List<TileHandler> LinkedTiles = new List<TileHandler> ();
    public List<TileHandler> RandomizedLinkedTiles => GetRandomizeLinkedTiles();


    public ActorHandler Occupant => GetComponentInChildren<ActorHandler> ();
    [SerializeField] ClueTypes _clueType = ClueTypes.None;
    //[SerializeField] bool _isClueRevealed = false;
    Color _previousTileColor;
    public int TileIndex { get; private set; }

    public TileHandler PreviousTile;
    public int AgentDist;
    public int DestinationDist;


    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        SetClue(ClueTypes.None);
        PreviousTile = null;
    }

    private void Start()
    {
        ActorController.Instance.PriorityActorTurnCompleting += HandlePriorityActorTurnCompleting;
    }

    private List<TileHandler> GetRandomizeLinkedTiles()
    {
        List<TileHandler> newList = new List<TileHandler>(LinkedTiles);
       System.Random rng = new System.Random();

        int n = newList.Count;
        while (n > 1)
        {
            n--;
            // Generate a random index between 0 and n inclusive
            int k = rng.Next(n + 1);
            // Swap the element at the current position (n) with the element at the random index (k)
            TileHandler value = newList[k];
            newList[k] = newList[n];
            newList[n] = value;
        }

        return newList;
    }


    #region Tile Setup

    public void AssignIndexNumber(int indexNumber)
    {
        TileIndex = indexNumber;
        _text.text = TileIndex.ToString();
    }

    public void AssignIndexPos(Vector2Int index)
    {
        IndexPos = index;
    }

    public void AddLinkedTile(TileHandler tileToLink)
    {
        if (!LinkedTiles.Contains(tileToLink) && tileToLink != this)
        {
            LinkedTiles.Add(tileToLink);
        }

    }

    public static void Shuffle(List<TileHandler> list)
    {
        System.Random r = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // Pick a random element from the remaining elements
            int k = r.Next(n + 1);
            // Swap the current element with the randomly chosen element
            TileHandler value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void RemoveRandomLinkedTiles()
    {
        List<TileHandler> quickDeck = new List<TileHandler>(LinkedTiles);

        Shuffle(quickDeck);

        if (LinkedTiles.Count >= 6)
        {
            //Main, remove 3;

            TileHandler r0 = quickDeck[0];
            TileHandler r1 = quickDeck[1];
            TileHandler r2 = quickDeck[2];


            if (r0.LinkedTiles.Count <= 2)
            {
                //removing this might reduce too many options for the selected neighbor. do nothing
            }
            else
            {
                r0.RemoveSpecificLinkedTile(this);
                LinkedTiles.Remove(r0);
            }


            if (r1.LinkedTiles.Count <= 2)
            {
                //removing this might reduce too many options for the selected neighbor. do nothing
            }
            else
            {
                r1.RemoveSpecificLinkedTile(this);
                LinkedTiles.Remove(r1);
            }


            if (r2.LinkedTiles.Count <= 2)
            {
                //removing this might reduce too many options for the selected neighbor. do nothing
            }
            else
            {
                r2.RemoveSpecificLinkedTile(this);
                LinkedTiles.Remove(r2);
            }
        }
        else if (LinkedTiles.Count >= 4)
        {
            //edge, remove 1

            TileHandler r0 = quickDeck[0];

            if (r0.LinkedTiles.Count <= 2)
            {
                //removing this might reduce too many options for the selected neighbor. do nothing
            }
            else
            {
                r0.RemoveSpecificLinkedTile(this);
                LinkedTiles.Remove(r0);
            }

        }
        else if (LinkedTiles.Count == 3)
        {
            //corner, remove none

        }
        else
        {
            //already has been trimmed via neighbor, remove no more
        }
    }

    public void RemoveSpecificLinkedTile(TileHandler tileToRemove)
    {
        if (LinkedTiles.Contains(tileToRemove))
        {
            LinkedTiles.Remove(tileToRemove);
        }
    }

    #endregion

    #region Mouse Handlers

    private void OnMouseEnter()
    {
        if (ActorController.Instance.PriorityActor.LegalMoves.Contains(this) ||
            ActorController.Instance.PriorityActor.LegalSearches.Contains(this) ||
            ActorController.Instance.PriorityActor.CurrentTile == this)
        {
            TileController.Instance.SetTileUnderCursor(this);
            HighlightTile();
        }
    }

    private void OnMouseExit()
    {
        //if (ActorController.Instance.PriorityActor.LegalMoves.Contains(this) ||
        //    ActorController.Instance.PriorityActor.CurrentTile == this)
        //{
        //    SemiHighlightTile(AgentData.AgentAbility.None);
        //}
        //else
        //{
        //    DeHighlightTile();
        //}
        DeHighlightTile();
        TileController.Instance.SetTileUnderCursor(null);
    }



    #endregion

    #region Tile Mechanics

    public void SetClue(ClueTypes newClueType)
    {
        if (newClueType == ClueTypes.None)
        {
            _clueType = newClueType;
            _clueSR.sprite = null;
        }

        else if (newClueType == ClueTypes.Origin)
        {
            _clueType = ClueTypes.Origin;
            _clueSR.sprite = TileController.Instance.OriginClue;
            _clueSR.enabled = true;
        }
        else if (newClueType == ClueTypes.Passage)
        {                
            if (_clueType == ClueTypes.Origin)
            {
                // do nothing; origin clues should outlast passage clues.
            }
            else if (_clueType == ClueTypes.None)
            {
                _clueType = ClueTypes.Passage;
                _clueSR.sprite = null; //remain null until player check-reveals this tile
            }

                    
        }
        
    }

    public bool CheckRevealClue()
    {
        //returning true implies that new information was given to the player.

        //_isClueRevealed = true;

        if (_clueType == ClueTypes.None)
        {
            _clueType = ClueTypes.JustSearched;
            _clueSR.enabled = true;
            _clueSR.sprite = TileController.Instance.JustSearchedClue;
            return false;
        }
        else if (_clueType == ClueTypes.Origin)
        {
            return false;
        }
        else if (_clueType == ClueTypes.Passage)
        {
            _clueSR.enabled = true;
            _clueSR.sprite = TileController.Instance.PassageClue;
            return true;
        }

        return false;
    }

    public void HighlightTile()
    {
        _borderSR.enabled = true;
        //_hoverTween = _visualsTransform.transform.DOLocalMoveY(_hoverYIncrease, hoverTweenTime).SetEase(Ease.OutBack);
    }

    public void ColorTileToAbility(AgentData.AgentAbility abilityToDepict)
    {
        _hoverTween.Kill();

        //_hoverTween = _visualsTransform.transform.DOLocalMoveY(_hoverYIncrease / 2f, hoverTweenTime).SetEase(Ease.OutBack);
       
        if (abilityToDepict == AgentData.AgentAbility.Move)
        {
            _tileSR.color = TileController.Instance.Color_legalMove;
        }
        else if (abilityToDepict == AgentData.AgentAbility.Search)
        {
            _tileSR.color = TileController.Instance.Color_legalSearch;
        }
        else if (abilityToDepict == AgentData.AgentAbility.Pass)
        {
            _tileSR.color = TileController.Instance.Color_pass;
        }
        else
        {
            _tileSR.color = Color.white;
        }

    }



    public void DeHighlightTile()
    {
        _hoverTween.Kill();

        _borderSR.enabled = false;
        //_hoverTween = _visualsTransform.DOLocalMoveY(0, hoverTweenTime).SetEase(Ease.OutBack);
    }

    private void HandlePriorityActorTurnCompleting()
    {
        if (_clueType == ClueTypes.JustSearched)
        {
            _clueType = ClueTypes.None;
            _clueSR.enabled = false;
        }
    }

    public void FindAndPublishClosestAgentDistance()
    {
        AgentDist = TileController.Instance.GetDistanceToClosestAgent(this);
        _agentDistanceTMP.text = AgentDist.ToString();
    }

    public void FindAndPublishDestinationDistance()
    {
        DestinationDist = TileController.Instance.GetDistanceToDestination(this);
        _destinationDistanceTMP.text = DestinationDist.ToString();
    }


    #endregion

    public void RemoveTile()
    {
        Debug.LogWarning("not implemented");
    }
   

}
