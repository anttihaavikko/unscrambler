using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Extensions;
using AnttiStarterKit.Managers;
using AnttiStarterKit.ScriptableObjects;
using AnttiStarterKit.Utils;
using AnttiStarterKit.Visuals;
using Cinemachine;
using Leaderboards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Hand : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    [SerializeField] private CardHolder hand, calculatorArea;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private HeartDisplay hearts;
    [SerializeField] private NumberScroller scoreDisplay;
    [SerializeField] private Appearer helpText;
    [SerializeField] private List<GameObject> scoreButtonPenaltyParts;
    [SerializeField] private TMP_Text penaltyDisplay;
    [SerializeField] private Elements elementList;
    [SerializeField] private TMP_Text calculatorDisplay;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] private Image multiplierTimer;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private Appearer multiplierAppearer, proceedAppearer, evalAppearer;
    [SerializeField] private Transform calculatorMachine;
    [SerializeField] private Appearer endOptions;
    [SerializeField] private List<ParticleSystem> confettiCannons;
    [SerializeField] private EffectCamera cam;
    [SerializeField] private Shaker calculatorShaker;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private StyledText styledHelpText, styledEvalText;
    [SerializeField] private Pulsater multiPulsater;
    [SerializeField] private SoundComposition errorSound, launchSound, multiSound, blipSound;
    [SerializeField] private Camera mainCam;

    private int level;
    private List<ElementCard> elements;
    private IEnumerator evaluationProcess;
    private int penalty, previousPenalty;
    private int lives = 10;
    private bool helpSeen;
    private List<string> currentParts;
    private float targetOrtho = 3f;
    private float multiplierTime = 60f;
    private int multiplier = 5;
    private bool doneDealing;
    private Vector3 multiPos;

    private Operation operation = Operation.Sum;

    private static string Spacer => "<color=white>~</color>";

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

        multiplierTimer.type = Image.Type.Filled;
        multiplierTimer.fillMethod = Image.FillMethod.Radial360;
        multiplierTimer.fillAmount = 1f;

        multiPos = mainCam.ScreenToWorldPoint(multiplierAppearer.transform.position).WhereZ(0);
    }

    private void CooldownMultiplier()
    {
        multiplierTime -= Time.deltaTime;

        if (multiplierTime <= 0 && multiplier > 1)
        {
            multiPulsater.Pulsate();
            SetMultiplier(multiplier - 1);
            multiSound.Play(multiPos);
        }
        
        multiplierTimer.fillAmount = Mathf.Max(0, multiplierTime / 60f);
    }

    private void SetMultiplier(int value)
    {
        multiplier = value;
        multiplierText.text = $"x{multiplier}";

        if (multiplier > 1)
        {
            multiplierTime = 60f;   
        }
    }

    private LevelDefinition GetDefinition()
    {
        return levelDefinitions[Mathf.Min(level, levelDefinitions.Count - 1)];
    }

    private void CreateCards()
    {
        doneDealing = false;
        StartCoroutine(CreateCardsCoroutine());
    }

    private IEnumerator CreateCardsCoroutine()
    {
        foreach (var part in currentParts)
        {
            CreateCard(part, Vector3.down * 5f, hand);
            yield return new WaitForSeconds(0.075f);
        }

        doneDealing = true;

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
        while (!doneDealing) yield return null;
        
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
                    previousPenalty = penalty;
                    penalty = totalCount - len;
                    
                    if (penalty == 0 && previousPenalty != 0)
                    {
                        AudioManager.Instance.Highpass();
                        
                        confettiCannons.ForEach(ps =>
                        {
                            var pos = ps.transform.position;
                            ps.Play();
                            launchSound.Play(pos, 0.6f);
                            EffectManager.AddEffect(3, pos);
                        });
                        cam.BaseEffect(0.3f);

                        var pos = Vector3.zero;
                        const float vol = 1.75f;
                        
                        AudioManager.Instance.PlayEffectFromCollection(2, pos, vol);
                        AudioManager.Instance.PlayEffectFromCollection(3, pos, vol);
                        
                        this.StartCoroutine(() =>
                        {
                            AudioManager.Instance.PlayEffectFromCollection(3, pos, vol);
                            AudioManager.Instance.PlayEffectFromCollection(2, pos, vol);
                        }, Random.Range(0f, 0.5f));
                        
                        this.StartCoroutine(() =>
                        {
                            AudioManager.Instance.PlayEffectFromCollection(3, pos, vol);
                            AudioManager.Instance.PlayEffectFromCollection(2, pos, vol);
                        }, Random.Range(0f, 0.5f));
                    }
                    else
                    {
                        AudioManager.Instance.Highpass(false);
                    }
                    
                    var message = penalty == 0 ? $"{Spacer}<wobble>Perfect word found!</wobble>{Spacer}" : $"Current best: <bulge>{word.ToUpper()}</bulge>";
                    styledEvalText.SetText(message);
                    helpSeen = true;
                    UpdateEvaluateButton();
                    yield break;
                }
                yield return null;
            }

            len--;
        }

        penalty = totalCount;
        styledEvalText.SetText($"{Spacer}<bulge>No words found!</bulge>{Spacer}");
        helpSeen = true;
        UpdateEvaluateButton();
    }

    private void UpdateEvaluateButton()
    {
        var penalized = penalty > 0;
        scoreButtonPenaltyParts.ForEach(p => p.SetActive(penalized));
        penaltyDisplay.text = $"-{penalty}";
        proceedAppearer.ShowAfter(0.3f);
        evalAppearer.Show();
    }

    private void Start()
    {
        NewWord();
    }

    private void Update()
    {
        CooldownMultiplier();
        
        DebugControls();
        virtualCam.m_Lens.OrthographicSize =
            Mathf.MoveTowards(virtualCam.m_Lens.OrthographicSize, targetOrtho, 3f * Time.deltaTime);
    }

    private void DebugControls()
    {
        if (!Application.isEditor) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            NextLevel();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            level += 10;
            NextLevel();
        }
    }

    public void Restart()
    {
        AudioManager.Instance.Highpass(false);
        AudioManager.Instance.Lowpass(false);
        AudioManager.Instance.TargetPitch = 1f;
        SceneChanger.Instance.ChangeScene("Main");
    }

    public void BackToMenu()
    {
        AudioManager.Instance.Highpass(false);
        AudioManager.Instance.Lowpass(false);
        AudioManager.Instance.TargetPitch = 1f;
        SceneChanger.Instance.ChangeScene("Start");
    }

    public void Proceed()
    {
        if (!doneDealing) return;
        
        cam.BaseEffect(0.1f);
        
        AudioManager.Instance.Highpass(false);
        
        proceedAppearer.Hide();
        
        doneDealing = false;
        multiplierAppearer.Hide();
        
        var amount = (elements.Count - penalty) * (level + 1) * multiplier;
        if (amount > 0)
        {
            this.StartCoroutine(() =>
            {
                scoreDisplay.Add(amount);
            }, 0.25f);
        }

        lives -= penalty;
        hearts.LoseLives(penalty);

        if (penalty > 0)
        {
            helpText.ShowAfter(0.3f);
            styledHelpText.SetText($"Perfect solution was: <bulge>{wordDictionary.GetWord().ToUpper()}</bulge>");
            AudioManager.Instance.Lowpass();
        }

        if (lives > 0)
        {
            NextLevel();
            PlayFanfare(4);
            return;
        }

        PlayFanfare(5);
        AudioManager.Instance.TargetPitch = 0.8f;
        
        hand.RemoveAll();
        calculatorArea.RemoveAll();
        elements.Clear();
        
        styledEvalText.SetText($"{Spacer}<bulge>Game Over</bulge>{Spacer}");

        this.StartCoroutine(() =>
        {
            scoreManager.SubmitScore(PlayerPrefs.GetString("PlayerName"), scoreDisplay.Value, level + 1, GetId());
        }, 0.3f);
        
        endOptions.ShowAfter(0.5f);
    }

    private void PlayFanfare(int index)
    {
        this.StartCoroutine(() =>
        {
            AudioManager.Instance.PlayEffectFromCollection(index, Vector3.zero, 1.2f, false);
        }, 0.3f);
    }

    private static string GetId()
    {
        if (PlayerPrefs.HasKey("PlayerId"))
        {
            return PlayerPrefs.GetString("PlayerId");
        }

        var id = Guid.NewGuid().ToString();
        PlayerPrefs.SetString("PlayerId", id);
        return id;
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
        var def = GetDefinition();
        wordDictionary.GenerateWord(def.tiles, def.splits, def.operations);
        SetMultiplier(5);
        multiplierAppearer.Show();

        if (level == 0)
        {
            helpText.ShowAfter(1.25f);
            styledHelpText.SetText($"{Spacer}Drag elements to <wobble>correct order</wobble> to form a word...{Spacer}");
        }

        if (level == 2)
        {
            helpText.ShowAfter(0.1f);
            styledHelpText.SetText($"Use the <wobble>combiner machine</wobble> to merge elements...");
        }

        if (def.splits > 0)
        {
            Tweener.MoveToBounceOut(calculatorMachine, calculatorMachine.position.WhereY(2f), 0.4f);
        }
        
        AudioManager.Instance.Lowpass(false);
    }

    private void ClearTiles()
    {
        elements.Clear();
        hand.RemoveAll();
        calculatorArea.RemoveAll();
    }

    private void NextLevel()
    {
        level++;
        ClearTiles();
        Invoke(nameof(NewWord), 3f);

        evalAppearer.HideWithDelay(0.25f);
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

    private void CalculationErrorSound()
    {
        errorSound.Play(calculatorDisplay.transform.position, 0.7f);
    }

    public void RunCalculations()
    {
        if (calculatorArea.CardCount() != 2)
        {
            calculatorDisplay.text = "ERR!!!";
            cam.BaseEffect(0.1f);
            CalculationErrorSound();
            return;
        }
        
        var matches = elements.Where(e => e.transform.position.y > 1)
            .OrderBy(e => e.transform.position.x)
            .ToList();

        if (matches.Count != 2) 
        {
            calculatorDisplay.text = "ERR!!!";
            cam.BaseEffect(0.1f);
            CalculationErrorSound();
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
        MachineBlip();
        
        var result = GetOperationResult(first, second);
        var e = elementList.GetMatch(result);
        
        cam.BaseEffect(0.1f);

        if (e != default)
        {
            var pos = calculatorArea.transform.position;
            elements.Remove(first);
            elements.Remove(second);
            calculatorArea.RemoveAll();
            CreateCard(e.abbreviation, pos, calculatorArea);
            EffectManager.AddEffect(3, pos);
            UpdateOperation();
            return;
        }
        
        calculatorShaker.Shake();
        CalculationErrorSound();
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
        MachineBlip();
    }
    
    public void SwitchOperation(int dir)
    {
        MachineBlip();
        
        var cur = (int)operation;
        var op = (cur + dir) % 4;
        if (op < 0) op = 3;
        operation = (Operation)op;
        UpdateOperation();
        
        cam.BaseEffect(0.05f);
        calculatorShaker.Shake();
        
        CancelInvoke(nameof(DoCalculations));
        Invoke(nameof(DoCalculations), 1f);
    }

    private void MachineBlip()
    {
        blipSound.Play(calculatorDisplay.transform.position, 0.5f);   
    }

    public void ResetLevel()
    {
        MachineBlip();
        if (!elements.Any() || !doneDealing) return;
        
        calculatorShaker.Shake();
        cam.BaseEffect(0.05f);
        evalAppearer.Hide();
        proceedAppearer.Hide();
        ClearTiles();
        
        Invoke(nameof(CreateCards), 0.5f);
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