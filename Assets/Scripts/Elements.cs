using System;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "New Elements", menuName = "Elements", order = 0)]
public class Elements : ScriptableObject
{
    [SerializeField] private TextAsset elementList;

    private List<Element> elements;
    private bool hasLoaded;

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

    public void Load()
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

        // var groups = elements.Select(e => e.colorIndex).OrderBy(val => val).ToList();
        // groups.Distinct().ToList().ForEach(g =>
        // {
        //     Debug.Log($"{g} => {groups.Count(group => group == g)}");
        // });
        
        hasLoaded = true;
    }

    public Element GetRandom()
    {
        return elements.Random();
    }

    public Element GetMatch(string abbreviation)
    {
        return elements.FirstOrDefault(e => string.Equals(e.abbreviation, abbreviation, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public Element GetMatch(int num)
    {
        return elements.FirstOrDefault(e => e.number == num);
    }

    public IEnumerable<string> GetAbbreviations()
    {
        return elements.Select(e => e.abbreviation);
    }
}