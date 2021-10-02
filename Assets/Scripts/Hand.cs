using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Utils;
using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    [SerializeField] private CardHolder hand;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private TMP_Text evaluationDisplay;
    [SerializeField] private HeartDisplay hearts;
    [SerializeField] private NumberScroller scoreDisplay;
    [SerializeField] private Appearer helpText;

    private int level;
    private List<ElementCard> elements;
    private IEnumerator evaluationProcess;
    private int penalty;
    private int lives = 10;
    private bool helpSeen;

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
                    penalty = parts.Count - len;
                    evaluationDisplay.text = penalty == 0 ? "Perfect word found!" : $"Best found word: {word.ToUpper()}";
                    helpSeen = true;
                    yield break;
                }
                yield return null;
            }

            len--;
        }

        penalty = parts.Count;
        evaluationDisplay.text = "No words found!";
        helpSeen = true;
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
            SceneChanger.Instance.ChangeScene("Main");
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            NextLevel();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Proceed();
        }
    }

    public void Proceed()
    {
        var amount = (elements.Count - penalty) * (level + 1);
        if (amount > 0)
        {
            scoreDisplay.Add(amount);   
        }

        lives -= penalty;
        hearts.LoseLives(penalty);

        if (penalty > 0)
        {
            helpText.ShowWithText($"Previous solution: {wordDictionary.GetWord().ToUpper()}", 0.3f);
        }

        if (lives > 0)
        {
            NextLevel();
            return;
        }
        
        hand.RemoveAll();
        evaluationDisplay.text = "Game Over";
    }

    private void StartEvaluation()
    {
        if (helpSeen)
        {
            helpText.Hide();
        }
        
        if (evaluationProcess != null)
        {
            StopCoroutine(evaluationProcess);
        }

        evaluationProcess = Evaluate();
        StartCoroutine(evaluationProcess);
    }
    
    

    private void NewWord()
    {
        helpSeen = false;
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