using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PingController : MonoBehaviour
{
    public static PingController Instance { get; private set; }

    //settings
    [SerializeField] GameObject _pingPrefab;

    [SerializeField] float _pingTime = 1.5f;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPing(Vector2 pingLocation)
    {
        var ping = Instantiate(_pingPrefab, pingLocation, Quaternion.identity);
        var sr = ping.GetComponent<SpriteRenderer>();
        ping.transform.localScale = Vector3.one * 10f;
        Color col = sr.color;
        col.a = 0;
        sr.color = col;
        sr.DOFade(1, _pingTime);
        ping.transform.DOScale(Vector3.zero, _pingTime);
    }
}
