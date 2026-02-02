using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class TileHandler : MonoBehaviour
{
    public enum ClueTypes { None, Origin, Passage, JustSearched }

    //refs
    [SerializeField] SpriteRenderer _tileSR = null;
    [SerializeField] SpriteRenderer _borderSR = null;
    [SerializeField] SpriteRenderer _clueSR = null;

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
    public ActorHandler Occupant => GetComponentInChildren<ActorHandler> ();
    [SerializeField] ClueTypes _clueType = ClueTypes.None;
    //[SerializeField] bool _isClueRevealed = false;
    Color _previousTileColor;

    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        SetClue(ClueTypes.None);
    }

    private void Start()
    {
        ActorController.Instance.PriorityActorTurnCompleting += HandlePriorityActorTurnCompleting;
    }



    #region Tile Setup

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

    #endregion

    #region Mouse Handlers

    private void OnMouseEnter()
    {
        if (ActorController.Instance.PriorityActor.LegalMoves.Contains(this) ||
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

    public void SetClue(ClueTypes clueType)
    {
        if (clueType == ClueTypes.None)
        {
            _clueType = clueType;
            _clueSR.sprite = null;
        }

        else if (clueType == ClueTypes.Origin)
        {
            _clueType = ClueTypes.Origin;
            _clueSR.sprite = TileController.Instance.OriginClue;
            _clueSR.enabled = true;
        }
        else if (clueType == ClueTypes.Passage)
        {                
            if (_clueType == ClueTypes.Origin)
            {
                // do nothing; origin clues should outlast passage clues.
            }
            else
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
            _tileSR.color = TileController.Instance.Color_legalMove;
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

    #endregion

    public void RemoveTile()
    {
        Debug.LogWarning("not implemented");
    }
   

}
