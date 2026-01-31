using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class TileHandler : MonoBehaviour
{

    //refs
    [SerializeField] List<SpriteRenderer> _srs = new List<SpriteRenderer>();
    [SerializeField] SpriteRenderer _tileSR = null;
    [SerializeField] Transform _visualsTransform = null;
    public Transform VisualsTransform => _visualsTransform;
    [SerializeField] SpriteRenderer _borderSR = null;

    //settings
    [SerializeField] float hoverTweenTime = 0.125f;
    [SerializeField] float _hoverYIncrease = 0.1f;

    //state
    Tween _hoverTween;
    Tween _slideTween;
    Tween _colorTween;
    Collider2D _coll;
    public Vector2Int IndexPos;
    public List<TileHandler> LinkedTiles = new List<TileHandler> ();
    

    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
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
        //_sizeTween.Kill();
        //_sizeTween = transform.DOScale(Vector2.one * _scaleIncrease, hoverTweenTime).SetEase(Ease.OutBack);

        if (_slideTween.IsActive()) return;

        TileController.Instance.UnraiseAllTiles();
        //TileController.Instance.HalfraiseAllOrthogonalTiles(this);
        FullraiseTile();
    }

    private void OnMouseExit()
    {
        //_sizeTween.Kill();
        //_sizeTween = transform.DOScale(Vector2.one, hoverTweenTime).SetEase(Ease.OutBack);

        //if (_slideTween.IsActive()) return;

        UnraiseTile();

        //DimTile();

    }

    private void OnMouseUpAsButton()
    {
        HandleClick();
    }

    private void HandleClick()
    {
        //
        TileController.Instance.HandleTileClick(this);
    }

    #endregion

    #region Tile Mechanics
   


    public void DimTile()
    {
        _colorTween.Kill();
        _colorTween = _tileSR.DOColor(new Color(0.8f, .8f, .8f, 1f), hoverTweenTime);
    }

    public void HalfdimTile()
    {
        _colorTween.Kill();
        _colorTween = _tileSR.DOColor(new Color(0.9f, .9f, .9f, 1f), hoverTweenTime);
    }

    public void BrightenTile()
    {
        _colorTween.Kill();
        _tileSR.DOColor(Color.white, hoverTweenTime);
    }

    public void FullraiseTile()
    {
        _hoverTween.Kill();
        _hoverTween = _visualsTransform.transform.DOLocalMoveY(_hoverYIncrease, hoverTweenTime).SetEase(Ease.OutBack);
    }

    public void HalfraiseTile()
    {
        _hoverTween.Kill();
        _hoverTween = _visualsTransform.transform.DOLocalMoveY(_hoverYIncrease / 2f, hoverTweenTime).SetEase(Ease.OutBack);
    }

    public void UnraiseTile()
    {
        _hoverTween.Kill();
        _hoverTween = _visualsTransform.DOLocalMoveY(0, hoverTweenTime).SetEase(Ease.OutBack);
    }

    #endregion

    public void RemoveTile()
    {
        Debug.LogWarning("not implemented");
    }
   

}
