using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    [SerializeField] private CardHolder hand;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private TMP_Text evaluationDisplay;

    private int level;
    private List<ElementCard> elements;
    private IEnumerator evaluationProcess;

    private void Awake()
    {
        elements = new List<ElementCard>();
        
        wordDictionary.wordPicked += parts =>
        {
            foreach (var part in parts)
            {
                var card = Instantiate(cardPrefab, Vector3.down * 10f, Quaternion.identity);
                var e = card.GetComponent<ElementCard>();
                e.Setup(part);
                elements.Add(e);
                hand.AddCard(card, true);
            }
        };

        hand.reordered += StartEvaluation;
    }

    private IEnumerator Evaluate()
    {
        var parts = elements.OrderBy(e => e.transform.position.x).Select(e => e.GetAbbreviation()).ToList();
        var len = parts.Count;

        while (len > 0)
        {
            for (var i = 0; i + len <= parts.Count; i++)
            {
                var word = string.Join(string.Empty, parts.GetRange(i, len));
                if (wordDictionary.IsWord(word))
                {
                    var penalty = parts.Count - len;
                    evaluationDisplay.text = penalty == 0 ? "Perfect word found!" : $"Best found word: {word.ToUpper()}";
                    yield break;
                }
                yield return null;
            }

            len--;
        }
        
        evaluationDisplay.text = "No words found!";
    }

    private void Start()
    {
        NewWord();
    }

    private void Update()
    {
        DebugControls();
    }

    private void DebugControls()
    {
        if (!Application.isEditor) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewWord();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            NextLevel();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartEvaluation();
        }
    }

    private void StartEvaluation()
    {
        if (evaluationProcess != null)
        {
            StopCoroutine(evaluationProcess);
        }

        evaluationProcess = Evaluate();
        StartCoroutine(evaluationProcess);
    }

    private void NewWord()
    {
        elements.Clear();
        hand.RemoveAll();
        wordDictionary.GenerateWord(level + 5);
    }

    private void NextLevel()
    {
        level++;
        NewWord();
    }
}