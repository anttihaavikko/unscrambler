using System;
using System.Collections.Generic;
using System.Globalization;
using AnttiStarterKit.Extensions;
using TMPro;
using UnityEngine;

public class ElementCard : MonoBehaviour
{
    [SerializeField] private TMP_Text number, abbreviation, title, mass;
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private Elements elements;

    private Element element;

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
    }

    public void Setup(string code)
    {
        Setup(elements.GetMatch(code));
    }

    public string GetAbbreviation()
    {
        return element.abbreviation.ToLower();
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