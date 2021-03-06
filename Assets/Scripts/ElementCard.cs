using System;
using System.Collections.Generic;
using System.Globalization;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Managers;
using TMPro;
using UnityEngine;

public class ElementCard : MonoBehaviour
{
    [SerializeField] private TMP_Text number, abbreviation, title, mass;
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private Elements elements;
    [SerializeField] private Card card;
    [SerializeField] private CursorManager cursorManager;

    private Element element;
    private Vector3 baseSize;

    private static Color GetColor(int index)
    {
        return index == 0 ? Color.white : Color.HSVToRGB(index / 18f, 0.2f, 1f);
    }

    private void Setup(Element e)
    {
        element = e;
        number.text = e.number.ToString(CultureInfo.InvariantCulture);
        abbreviation.text = e.abbreviation;
        title.text = e.title;
        mass.text = e.mass.ToString(CultureInfo.InvariantCulture);
        backgroundSprite.color = GetColor(e.colorIndex);
        baseSize = transform.localScale;
    }

    public void Setup(string code)
    {
        Setup(elements.GetMatch(code));
    }

    public string GetAbbreviation()
    {
        return element.abbreviation.ToLower();
    }

    public int GetNumber()
    {
        return element.number;
    }

    public string GetForCalculator()
    {
        return $"{element.abbreviation} ({element.number})";
    }

    private void OnMouseEnter()
    {
        if (CanHover()) return;
        Swoosh();
        Tweener.ScaleToBounceOut(transform, baseSize * 1.1f, 0.1f);
        cursorManager.Hover();
    }

    private void OnMouseExit()
    {
        if (CanHover()) return;
        Swoosh(0.75f);
        Tweener.ScaleToQuad(transform, baseSize, 0.15f);
        cursorManager.Normal();
    }

    private bool CanHover()
    {
        return card.Locked || card.IsDragging || Input.GetMouseButton(0);
    }
    
    private void Swoosh(float volume = 1f)
    {
        var pos = transform.position;
        AudioManager.Instance.PlayEffectFromCollection(1, pos, 0.3f * volume);
    }
}

public class Element
{
    public readonly int number;
    public readonly string abbreviation;
    public readonly string title;
    public readonly float mass;
    public readonly int colorIndex;

    public Element(int number, string abbreviation, string title, float mass, int colorIndex)
    {
        this.number = number;
        this.abbreviation = abbreviation;
        this.title = title;
        this.mass = mass;
        this.colorIndex = colorIndex;
    }
}