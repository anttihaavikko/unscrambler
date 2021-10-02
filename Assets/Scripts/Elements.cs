using System;
using System.Collections.Generic;
using AnttiStarterKit.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "New Elements", menuName = "Elements", order = 0)]
public class Elements : ScriptableObject
{
    [SerializeField] private TextAsset elementList;

    private List<Element> elements;

    private int GetIntOr(string input, int defaultValue)
    {
        var success = int.TryParse(input, out var value);
        return success ? value : defaultValue;
    }
    
    private float GetFloatOr(string input, float defaultValue)
    {
        var success = float.TryParse(input, out var value);
        return success ? value : defaultValue;
    }

    private void OnEnable()
    {
        elements = new List<Element>();
        
        var rows = elementList.text.Split('\n');
        foreach (var row in rows)
        {
            var values = row.Split(',');
            if (values.Length <= 8) continue;
            
            elements.Add(new Element(
                int.Parse(values[0]),
                values[2],
                values[1],
                GetFloatOr(values[3], 0f),
                GetIntOr(values[8], 0)
            ));
        }
    }

    public Element GetRandom()
    {
        return elements.Random();
    }
}