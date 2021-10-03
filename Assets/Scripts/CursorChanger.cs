using System;
using AnttiStarterKit.Animations;
using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    [SerializeField] private CursorManager cursorManager;

    private void Start()
    {
        GetComponent<ButtonStyle>().hoverChanged += HoverChanged;
    }

    private void HoverChanged(bool state)
    {
        if (state)
        {
            cursorManager.Hover();
            return;
        }
        
        cursorManager.Normal();
    }
}