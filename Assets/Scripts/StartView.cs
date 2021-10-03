using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Extensions;
using UnityEngine;

public class StartView : MonoBehaviour
{
    [SerializeField] private CardHolder holder;
    [SerializeField] private Elements elements;
    [SerializeField] private GameObject startCam;

    private void Start()
    {
        startCam.SetActive(false);
        
        elements.Load();
        
        var codes = GetCodes();
        StartCoroutine(AddLetters(codes));
    }

    private static List<string> GetCodes()
    {
        var sets = new List<List<string>>
        {
            new List<string> { "U", "N", "Sc", "Ra", "Nb", "La", "S" },
            new List<string> { "U", "N", "Sc", "Ra", "Mn", "La", "S" },
            new List<string> { "U", "N", "Sc", "Ru", "U", "B", "Lu", "Mn" },
            new List<string> { "U", "N", "Sc", "Ra", "Nb", "La", "Ru" },
            new List<string> { "U", "N", "Sc", "Ra", "Mn", "La", "Ru" },
            new List<string> { "U", "N", "S", "Cr", "Am", "B", "La", "Ru" },
            new List<string> { "U", "N", "S", "Cr", "Am", "B", "La", "S" },
            new List<string> { "U", "N", "S", "Cr", "Am", "B", "Lu", "Mn" },
        };

        return sets.Random();
    }

    private IEnumerator AddLetters(List<string> codes)
    {
        foreach (var code in codes)
        {
            AddLetter(code);
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void AddLetter(string code)
    {
        var card = Instantiate(holder.cardPrefab, Vector3.up * 5f, Quaternion.identity);
        var e = card.GetComponent<ElementCard>();
        e.Setup(code);
        holder.AddCard(card, true);
    }

    public void StarGame()
    {
        var scene = PlayerPrefs.HasKey("PlayerName") ? "Main" : "Name";
        SceneChanger.Instance.ChangeScene(scene);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            SceneChanger.Instance.ChangeScene("Start");
        }
    }
}