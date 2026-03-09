using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeHandler : MonoBehaviour
{
    //refs
    [SerializeField] Sprite[] _treeSprites = null;
    [SerializeField] SpriteRenderer _treeSR = null;

    //state

    [SerializeField] int _treeCount = 0;

    public void SetupTree(int treeCount)
    {
        _treeCount = treeCount;

        Color col = _treeSR.color;
        col.r *= UnityEngine.Random.Range(.8f, 1.2f);
        col.g *= UnityEngine.Random.Range(.8f, 1.2f);
        col.b *= UnityEngine.Random.Range(.8f, 1.2f);

        if (treeCount == 0)
        {
            _treeSR.sortingOrder = -9;
            _treeSR.sprite = _treeSprites[0];
            transform.localScale *= 1.2f;
            col.a = 0.15f;
        }
        else 
        {
            _treeSR.sprite = _treeSprites[treeCount - 1];

            if (treeCount == 1)
            {
                _treeSR.sortingOrder = -9;
                transform.localScale *= 1.7f;
                col.a = 0.5f;
            }


        }
        _treeSR.color = col;
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.transform.gameObject.layer == 6)
    //    {
    //        _treeSR.enabled = false;
    //    }
    //}
}
