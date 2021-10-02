using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardHolder : MonoBehaviour
{
	[SerializeField] private bool isVertical;

	private List<Card> cards;
	public int cardMax = 1;
	
	public Card cardPrefab;

	public CardHolder targetHolder;

	public bool demoSpawn = false;

	private Vector3 Forward => isVertical ? Vector3.up : Vector3.right;

	public Action reordered;

	private void Awake ()
	{
		cards = new List<Card> ();
	}

	public void SpawnCard()
	{
		if (cards.Count >= cardMax) return;
		var c = Instantiate (cardPrefab, transform.position + ((cards.Count + 1) * 0.5f * 1.1f + 5f) * Forward, Quaternion.identity);
		AddCard (c, true);
		PositionCards ();
	}
	
	private void Update () {
		if (Application.isEditor) {
			if (Input.GetKeyDown (KeyCode.Q)) {
				SpawnCard ();
			}

			if (Input.GetKeyDown (KeyCode.W)) {
				PositionCards ();
			}
		}
	}

	public Card RemoveFirst() {
		Card c = null;
		if (cards.Count > 0) {
			c = cards [0];
			cards.RemoveAt (0);
		}
		PositionCards ();
		return c;
	}

	public Card RemoveRandom()
	{
		Card c = null;
		var i = Random.Range (0, cards.Count);

		if (cards.Count > 0)
		{
			c = cards [i];
			cards.RemoveAt (i);
		}
		PositionCards ();
		return c;
	}

	public void RemoveCard(Card c)
	{
		if (!cards.Contains(c)) return;
		cards.Remove (c);
		PositionCards ();
	}

	public void RemoveAll()
	{
		cards.ForEach(c => Destroy(c.gameObject));
		cards.Clear();
	}

	private float GetAxisFor(Vector3 v)
	{
		return isVertical ? v.y : v.x;
	}

	public void PreviewCard(Card c)
	{
		var slot = 0;
		var axisValue = GetAxisFor(c.transform.position);

		for (var i = 0; i < cards.Count; i++)
		{
			if (axisValue > GetAxisFor(cards [i].transform.position))
			{
				slot = i + 1;
			}
		}

		PositionCards (slot);
	}

	public void AddCard(Card c, bool toEnd)
	{
		if(!cards.Contains(c))
		{

			if (cards.Count >= cardMax)
			{
				var swap = cards [0];
				cards.RemoveAt (0);
				swap.currentHolder.targetHolder.AddCard (swap, false);
			}

			var slot = 0;

			if (toEnd) {
				
				cards.Add (c);

			}
			else
			{
				for (var i = 0; i < cards.Count; i++)
				{
					if (GetAxisFor(c.transform.position) > GetAxisFor(cards [i].transform.position))
					{
						slot = i + 1;
					}
				}

				cards.Insert (slot, c);
			}
		}

		c.currentHolder = this;
		PositionCards ();
	}

	public void PositionCards(int spaceBefore = -1)
	{
		var areaSize = (cards.Count - 1) * 0.1f + cards.Sum(t => GetAxisFor(t.transform.localScale));

		var curPos = spaceBefore == -1 ? 0f : -0.525f;

		for (var i = 0; i < cards.Count; i++)
		{
			curPos += GetAxisFor(cards [i].transform.localScale) * 0.525f;

			if (i == spaceBefore)
			{
				curPos += 0.525f * 2;
			}

			cards [i].Move(transform.position + (-areaSize * 0.5f + curPos) * Forward + Vector3.back * 0.01f);
			curPos += GetAxisFor(cards [i].transform.localScale) * 0.5f + 0.1f;
		}

		if (spaceBefore == -1)
		{
			reordered?.Invoke();
		}
	}

	public int CardCount() {
		return cards.Count;
	}

	public bool HasSpace()
	{
		return cards.Count < cardMax;
	}
}
