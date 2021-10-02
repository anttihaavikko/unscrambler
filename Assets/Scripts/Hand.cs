using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Utils;
using Cinemachine;
using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    [SerializeField] private CardHolder hand, calculatorArea;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private TMP_Text evaluationDisplay;
    [SerializeField] private HeartDisplay hearts;
    [SerializeField] private NumberScroller scoreDisplay;
    [SerializeField] private Appearer helpText;
    [SerializeField] private List<GameObject> scoreButtonPenaltyParts;
    [SerializeField] private TMP_Text penaltyDisplay;
    [SerializeField] private Elements elementList;
    [SerializeField] private TMP_Text calculatorDisplay;
    [SerializeField] private CinemachineVirtualCamera virtualCam;

    private int level;
    private List<ElementCard> elements;
    private IEnumerator evaluationProcess;
    private int penalty;
    private int lives = 10;
    private bool helpSeen;
    private List<string> currentParts;
    private float targetOrtho = 3f;

    private Operation operation = Operation.Sum;

    private static List<LevelDefinition> levelDefinitions = new List<LevelDefinition>
    {
        new LevelDefinition(5, 0, new List<Operation>()),
        new LevelDefinition(6, 0, new List<Operation>()),
        new LevelDefinition(5, 1, new List<Operation> { Operation.Sum }),
        new LevelDefinition(6, 1, new List<Operation> { Operation.Sub }),
        new LevelDefinition(5, 2, new List<Operation> { Operation.Sum, Operation.Sub }),
        new LevelDefinition(5, 1, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul }),
        new LevelDefinition(5, 1, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(7, 3, new List<Operation> { Operation.Sum, Operation.Sub }),
        new LevelDefinition(6, 1, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(6, 2, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(7, 2, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(8, 2, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(8, 3, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(9, 3, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(10, 3, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(10, 4, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
        new LevelDefinition(10, 5, new List<Operation> { Operation.Sum, Operation.Sub, Operation.Mul, Operation.Div }),
    };

    private void Awake()
    {
        elements = new List<ElementCard>();
        
        wordDictionary.wordPicked += parts =>
        {
            currentParts = parts.ToList();
            CreateCards();
        };

        hand.reordered += StartEvaluation;
        calculatorArea.reordered += DoCalculations;
        
        UpdateOperation();
    }

    private LevelDefinition GetDefinition()
    {
        return levelDefinitions[Mathf.Min(level, levelDefinitions.Count - 1)];
    }

    private void CreateCards()
    {
        foreach (var part in currentParts)
        {
            CreateCard(part, Vector3.down * 10f, hand);
        }

        var els = elements.Count;
        var zoom = Mathf.Max(0, (els - 8) * 0.4f);
        targetOrtho = 3 + zoom;
    }

    private void CreateCard(string code, Vector3 pos, CardHolder holder)
    {
        var card = Instantiate(cardPrefab, pos, Quaternion.identity);
        var e = card.GetComponent<ElementCard>();
        e.Setup(code);
        elements.Add(e);
        holder.AddCard(card, true);
    }

    private IEnumerator Evaluate()
    {
        var parts = elements.Where(e => e.transform.position.y < 1f)
            .OrderBy(e => e.transform.position.x)
            .Select(e => e.GetAbbreviation())
            .ToList();
        
        var len = parts.Count;
        var totalCount = elements.Count;

        while (len > 0)
        {
            for (var i = 0; i + len <= parts.Count; i++)
            {
                var word = string.Join(string.Empty, parts.GetRange(i, len));
                if (wordDictionary.IsWord(word))
                {
                    penalty = totalCount - len;
                    evaluationDisplay.text = penalty == 0 ? "Perfect word found!" : $"Best found word: {word.ToUpper()}";
                    helpSeen = true;
                    UpdateEvaluateButton();
                    yield break;
                }
                yield return null;
            }

            len--;
        }

        penalty = totalCount;
        evaluationDisplay.text = "No words found!";
        helpSeen = true;
        UpdateEvaluateButton();
    }

    private void UpdateEvaluateButton()
    {
        var penalized = penalty > 0;
        scoreButtonPenaltyParts.ForEach(p => p.SetActive(penalized));
        penaltyDisplay.text = $"-{penalty}";
    }

    private void Start()
    {
        NewWord();
    }

    private void Update()
    {
        DebugControls();
        virtualCam.m_Lens.OrthographicSize =
            Mathf.MoveTowards(virtualCam.m_Lens.OrthographicSize, targetOrtho, 3f * Time.deltaTime);
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
        calculatorArea.RemoveAll();
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
        calculatorArea.RemoveAll();
        var def = GetDefinition();
        wordDictionary.GenerateWord(def.tiles, def.splits, def.operations);
    }

    private void NextLevel()
    {
        level++;
        NewWord();
    }
    
    private void DoCalculations()
    {
        UpdateOperation();
        
        if (calculatorArea.CardCount() != 2) return;

        var matches = elements.Where(e => e.transform.position.y > 1)
            .OrderBy(e => e.transform.position.x)
            .ToList();

        if (matches.Count != 2) return;
        
        PreviewCalculation(matches[0], matches[1]);
    }

    public void RunCalculations()
    {
        if (calculatorArea.CardCount() != 2)
        {
            calculatorDisplay.text = "ERR!!!";
            return;
        }
        
        var matches = elements.Where(e => e.transform.position.y > 1)
            .OrderBy(e => e.transform.position.x)
            .ToList();

        if (matches.Count != 2) 
        {
            calculatorDisplay.text = "ERR!!!";
            return;
        }
        
        Operate(matches[0], matches[1]);
    }

    private void UpdateOperation()
    {
        calculatorDisplay.text = operation switch
        {
            Operation.Sum => "ADDITION",
            Operation.Sub => "SUBTRACTION",
            Operation.Mul => "MULTIPLICATION",
            Operation.Div => "DIVISION",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private string GetOperationSign()
    {
        return operation switch
        {
            Operation.Sum => "+",
            Operation.Sub => "-",
            Operation.Mul => "*",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void Operate(ElementCard first, ElementCard second)
    {
        var result = GetOperationResult(first, second);
        var e = elementList.GetMatch(result);

        if (e != default)
        {
            elements.Remove(first);
            elements.Remove(second);
            calculatorArea.RemoveAll();
            CreateCard(e.abbreviation, calculatorArea.transform.position, calculatorArea);
            UpdateOperation();
        }
    }

    private int GetOperationResult(ElementCard first, ElementCard second)
    {
        return operation switch
        {
            Operation.Sum => first.GetNumber() + second.GetNumber(),
            Operation.Sub => first.GetNumber() - second.GetNumber(),
            Operation.Mul => first.GetNumber() * second.GetNumber(),
            Operation.Div => SafeDivision(first.GetNumber(), second.GetNumber()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int SafeDivision(int a, int b)
    {
        if (b == 0) return -1;
        return a % b == 0 ? a / b : -1;
    }

    private void PreviewCalculation(ElementCard first, ElementCard second)
    {
        var result = GetOperationResult(first, second);
        var e = elementList.GetMatch(result);
        var res = e != default ? $"{e.abbreviation} ({e.number})" : "ERR";
        calculatorDisplay.text = $"{first.GetForCalculator()} {GetOperationSign()} {second.GetForCalculator()} = {res}";
    }
    
    public void SwitchOperation(int dir)
    {
        var cur = (int)operation;
        var op = (cur + dir) % 4;
        if (op < 0) op = 3;
        operation = (Operation)op;
        UpdateOperation();
        
        Invoke(nameof(DoCalculations), 1f);
    }

    public void ResetLevel()
    {
        hand.RemoveAll();
        calculatorArea.RemoveAll();
        elements.Clear();
        CreateCards();
    }
}

public enum Operation
{
    Sum,
    Sub,
    Mul,
    Div
}

public class LevelDefinition
{
    public int tiles;
    public int splits;
    public List<Operation> operations;

    public LevelDefinition(int tiles, int splits, List<Operation> operations)
    {
        this.tiles = tiles;
        this.splits = splits;
        this.operations = operations;
    }
}