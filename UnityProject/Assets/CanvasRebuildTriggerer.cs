using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TheGamedevGuru
{
public class CanvasRebuildTriggerer : MonoBehaviour
{
    private Image _image;

    void Awake()
    {
        _image = GetComponent<Image>();
    }
    void Update()
    {
        if (Time.time % 10 > 5)
        {
            _image.color = new Color(1, 1, 1, Random.value);
            //transform.Translate(Time.deltaTime, 0, 0);
        }
    }
}
}