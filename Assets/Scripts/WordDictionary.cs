using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AnttiStarterKit.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordDictionary : MonoBehaviour
{
    public TextAsset dictionaryFile;
    [SerializeField] private Elements elements;

    private Dictionary<string, string> words;
    private List<string> letterPool;
    private List<string> allParts;
    private List<string> parts;

    private string next;
    private string solution;

    public Action<List<string>> wordPicked;

    // Start is called before the first frame update
    private void Awake()
    {
        letterPool = new List<string>();
        parts = new List<string>();
        Prep();
    }

    void Prep()
    {
        words = dictionaryFile.text.Split('\n').Select(w => {
            var word = w.Trim().ToLower();
            return word.Split('\t')[0];
        }).Distinct().ToDictionary(x => x, x => x);

        Debug.Log("Loaded dictionary of " + words.Count + " words.");

        var sample = RandomWord();
        Debug.Log("Random word sample: '" + sample + "'");
    }

    private void Start()
    {
        elements.Load();
        allParts = elements.GetAbbreviations().Select(a => a.ToLower()).ToList();
    }

    // void CheckTracks()
    // {
    //     foreach (var track in tracks.Where(t => t.NeedsCheck()))
    //     {
    //         track.Check();
    //     }
    // }

    public bool IsWord(string word)
    {
        return words.ContainsKey(word.ToLower());
    }

    public string RandomWord(int len)
    {
        var matches = words.Keys.Where(w => w.Length == len).ToArray();
        var key = matches[Random.Range(0, matches.Length)];
        return key;
    }
    
    public string RandomWord()
    {
        var key = words.Keys.ToArray()[Random.Range(0, words.Count)];
        var word = words[key];
        return word;
    }

    private bool Validate(string word)
    {
        var pos = 0;
        var wordLen = word.Length;
        parts.Clear();

        while (pos < word.Length)
        {
            if (wordLen >= pos + 2 && allParts.Contains(word.Substring(pos, 2)))
            {
                parts.Add(word.Substring(pos, 2));
                pos += 2;
                continue;
            }
            
            if (wordLen >= pos + 1 && allParts.Contains(word.Substring(pos, 1)))
            {
                parts.Add(word.Substring(pos, 1));
                pos += 1;
                continue;
            }
            
            // Debug.Log($"Can't do {word} at pos {pos}");
            return false;
        }
        
        Debug.Log($"Picked word '{word}'");
        Debug.Log(string.Join("-", parts));
        solution = word;

        return true;
    }

    private void ShuffleParts()
    {
        parts = parts.OrderBy(_ => Random.value).ToList();

        var tries = 0;
        while (IsWord(string.Join(string.Empty, parts).ToLower()) && tries < 10)
        {
            parts = parts.OrderBy(_ => Random.value).ToList();
            tries++;
        }
    }

    public void GenerateWord(int len, int splits, List<Operation> operations)
    {
        StartCoroutine(GenerationCoroutine(len, splits, operations));
    }

    private IEnumerator GenerationCoroutine(int len, int splits, List<Operation> operations)
    {
        while (allParts == null || !allParts.Any()) yield return new WaitForSeconds(0.2f);
        
        var word = RandomWord(len);
        
        while (!Validate(word))
        {
            word = RandomWord(len);
            yield return null;
        }

        for (var i = 0; i < splits; i++)
        {
            TryToSplit(operations.Random());   
        }

        ShuffleParts();

        wordPicked?.Invoke(parts);
    }

    private void TryToSplit(Operation op)
    {
        var target = parts.Random();
        var e = elements.GetMatch(target);
        var (a, b) = GetSplitElements(e.number, op);

        if (a == default || b == default) return;
        
        Debug.Log($"Will split {target} ({e.number}) => {a.abbreviation} and {b.abbreviation}");
        
        parts.Remove(target);
        parts.Add(a.abbreviation);
        parts.Add(b.abbreviation);
    }

    private (Element, Element) GetSplitElements(int source, Operation op)
    {
        var (a, b) = GetSplitNumbers(source, op);
        var elA = elements.GetMatch(a);
        var elB = elements.GetMatch(b);
        return (elA, elB);
    }

    private (int, int) GetSplitNumbers(int source, Operation op)
    {
        return op switch
        {
            Operation.Sum => GetSplitForSum(source),
            Operation.Sub => GetSplitForSub(source),
            Operation.Mul => GetSplitForMul(source),
            Operation.Div => GetSplitForDiv(source),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    private (int, int) GetSplitForSum(int source)
    {
        var a = Random.Range(1, source);
        var b = source - a;
        return (a, b);
    }
    
    private (int, int) GetSplitForSub(int source)
    {
        var a = Random.Range(1, source);
        var b = source + a;
        return (a, b);
    }

    private (int, int) GetSplitForMul(int source)
    {
        var factors = new List<int>();

        for (var i = 1; i <= source; i++)
        {
            if (source % i == 0)
            {
                factors.Add(i);
            } 
        }

        // I guess it's a prime
        if (!factors.Any()) return (-1, -1);
        
        var a = factors.Random();
        var b = source / a;
        return (a, b);
    }
    
    private (int, int) GetSplitForDiv(int source)
    {
        var a = Random.Range(1, source);
        var b = source * a;
        return (a, b);
    }

    private bool IsOk(string letter)
    {
        return !string.IsNullOrEmpty(letter) && char.IsLetter(letter[0]);
    }

    public string GetNext()
    {
        return next;
    }

    public string GetWord()
    {
        return solution;
    }
}
