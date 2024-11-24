using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;


public class ParallaxBackground : MonoBehaviour
{

    private Material _material;
    private float _offsetX = 0.0f;
    private float _offsetY = 0.0f;
    public float SpeedX;
    public float SpeedY;
    public float MaxY = 0.2f;
    private float _lastPosX, _lastPosY;
    
    private void Start()
    {
        _material = GetComponent<Renderer>().material;
        _lastPosX = transform.position.x;
        _lastPosY = transform.position.y;
    }

    private void Update()
    {
        _offsetX += (_lastPosX - transform.position.x) * -SpeedX;
        _lastPosX = transform.position.x;
        if (_offsetX >= 1) _offsetX -= 2;
        else if (_offsetX < -1) _offsetX += 2;
        _material.SetFloat("_OffsetUvX", _offsetX);
        
        _offsetY += (_lastPosY - transform.position.y) * -SpeedY;
        _lastPosY = transform.position.y;
        float finalY = _offsetY;
        if (_offsetY >= MaxY) finalY = MaxY;
        else if (_offsetY < -MaxY) finalY = -MaxY;
        _material.SetFloat("_OffsetUvY", finalY);
    }
}