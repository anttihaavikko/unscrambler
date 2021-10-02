using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    [SerializeField] private CardHolder hand;
    [SerializeField] private Card cardPrefab;

    private int level = 0;

    private void Awake()
    {
        wordDictionary.wordPicked += parts =>
        {
            foreach (var part in parts)
            {
                var card = Instantiate(cardPrefab, Vector3.down * 10f, Quaternion.identity);
                var e = card.GetComponent<ElementCard>();
                e.Setup(part);
                hand.AddCard(card, true);
            }
        };
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.R))
        {
            hand.RemoveAll();
            wordDictionary.GenerateWord(level + 5);
        }
    }
}