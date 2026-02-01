using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ActorHandler : MonoBehaviour
{
    //ref
    [SerializeField] bool _isPlayer = false;

    [SerializeField] SpriteRenderer _visualSR = null;

    //state
    Tween _slideTween;
    public bool IsPlayer => _isPlayer;

    public TileHandler CurrentTile => GetCurrentTile();
    public List<TileHandler> LegalMoves => GetLegalMoves();

    public Sprite ActorSprite => _visualSR.sprite;

    public void SlideToNewTile(TileHandler newTile)
    {
        _slideTween.Kill();

        transform.parent = newTile.VisualsTransform;
        _slideTween = transform.DOLocalMove(Vector2.zero,
            ActorController.Instance.MoveTweenTime).OnComplete(CompleteTurn);
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
    public void BeginTurn()
    {
        TileController.Instance.UnraiseAllTiles();
        if (_isPlayer)
        {
            //highlight possible options
            foreach (var tile in LegalMoves)
            {
                tile.HalfraiseTile();
            }
        }
        else
        {
            Debug.Log("Monster does his movement...");

            int rand = UnityEngine.Random.Range(0, LegalMoves.Count);

            SlideToNewTile(LegalMoves[rand]);
        }

    }

    private void CompleteTurn()
    {
        ActorController.Instance.HandlePriorityActorTurnCompletion();
    }

    #endregion
}
