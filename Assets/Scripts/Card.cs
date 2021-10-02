using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

public class Card : MonoBehaviour
{

	[SerializeField] private SortingGroup sortingGroup;
	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private LayerMask areaMask;

	private bool dragging;
	private Vector3 dragPoint;

	public CardHolder currentHolder;

	private Vector3 startPoint;

	private float height;

	private Vector3 fromPosition, toPosition;
	private float moveDuration = -1f;

	private float currentSpeed;

	public SpriteRenderer shadow;

	private Vector3 shadowScale;

	private void Start ()
	{
		shadowScale = shadow.transform.localScale;
		currentSpeed = moveSpeed;
	}

	private void Update ()
	{

		var lastPos = transform.position;

		if (dragging)
		{
			var mousePos = Input.mousePosition;
			mousePos.z = -Camera.main.transform.position.z + height;
			mousePos = Camera.main.ScreenToWorldPoint (mousePos);

			transform.position = new Vector3 (mousePos.x, mousePos.y, height * 0.5f) + dragPoint;
		}
		else
		{
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (Vector3.zero), 1f);
		}

		if (moveDuration >= 0f && moveDuration <= 1f) {
			moveDuration += Time.deltaTime * currentSpeed;
			transform.position = Vector3.Lerp (fromPosition, toPosition, moveDuration);
		}

		Tilt (lastPos, transform.position);
		float offset = dragging ? 0.1f : 0f;

		shadow.transform.position = new Vector3 (transform.position.x, transform.position.y, dragging ? -0.25f : 0f);
		shadow.transform.localScale = dragging ? shadowScale * 1.1f : shadowScale;
	}

	private void Tilt(Vector3 prevPos, Vector3 curPos) {
		float maxAngle = 10f;

		float xdiff = curPos.x - prevPos.x;
		xdiff = Mathf.Clamp (xdiff * 1000f, -maxAngle, maxAngle);

		float ydiff = curPos.y - prevPos.y;
		ydiff = Mathf.Clamp (ydiff * 1000f, -maxAngle, maxAngle);

		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (new Vector3 (-ydiff, xdiff, 0)), 0.5f);
	}

	public void OnMouseDown() {

		currentSpeed = moveSpeed + Random.Range(-0.5f, 0.5f);
		
		SetHeight (true);

		startPoint = transform.position;

		dragPoint = Vector3.zero;
		
		currentHolder.RemoveCard (this);
	}

	public void SetHeight(bool raised)
	{
		dragging = raised;
		height = raised ? -1f : 0f;
		sortingGroup.sortingOrder = raised ? 2 : 1;
	}

	public void OnMouseUp()
	{
		var area = Physics2D.OverlapCircle(transform.position, 1f, areaMask);

		if (area)
		{
			var holder = area.GetComponent<CardHolder>();
			if (holder.HasSpace())
			{
				if (holder != currentHolder)
				{
					currentHolder.PositionCards();
				}
				
				currentHolder = area.GetComponent<CardHolder>();	
			}
		}
		
		SetHeight (false);
		currentHolder.AddCard (this, false);
	}

	void OnMouseOver() {
		if (dragging) {
			currentHolder.PreviewCard (this);
		}
	}

	private bool LeftArea(float distance) {
		return (transform.position - startPoint).magnitude > distance;
	}

	public void Move(Vector3 pos)
	{
		fromPosition = transform.position;
		toPosition = pos;
		moveDuration = 0f;
	}

	public void JustRemove() {
		Destroy (gameObject);
	}
}
