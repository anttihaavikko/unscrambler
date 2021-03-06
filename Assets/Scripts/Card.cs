using System;
using System.Collections;
using System.Collections.Generic;
using AnttiStarterKit.Animations;
using AnttiStarterKit.Managers;
using AnttiStarterKit.Visuals;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using AnttiStarterKit.Extensions;

public class Card : MonoBehaviour
{

	[SerializeField] private SortingGroup sortingGroup;
	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private LayerMask areaMask;
	[SerializeField] private Collider2D cardCollider;
	[SerializeField] private CursorManager cursorManager;
	
	public bool Locked { get; private set; }

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
	private Camera cam;

	public bool IsDragging => dragging;

	private void Start ()
	{
		shadowScale = shadow.transform.localScale;
		currentSpeed = moveSpeed;
		cam = Camera.main;
	}

	private void Update ()
	{
		var lastPos = transform.position;

		if (dragging)
		{
			UpdateArea();
			currentHolder.PreviewCard (this);
			
			var mousePos = GetMousePosInWorld();

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

		var position = transform.position;
		Tilt (lastPos, position);
		var offset = dragging ? 0.1f : 0f;
		
		shadow.transform.position = dragging ? 
			Vector3.MoveTowards(position, Vector3.zero, 0.07f) : 
			position;
	}

	private Vector3 GetMousePosInWorld()
	{
		var mousePos = Input.mousePosition;
		mousePos.z = -cam.transform.position.z + height;
		mousePos = cam.ScreenToWorldPoint(mousePos);
		return mousePos;
	}

	private void Tilt(Vector3 prevPos, Vector3 curPos) {
		float maxAngle = 10f;

		float xdiff = curPos.x - prevPos.x;
		xdiff = Mathf.Clamp (xdiff * 1000f, -maxAngle, maxAngle);

		float ydiff = curPos.y - prevPos.y;
		ydiff = Mathf.Clamp (ydiff * 1000f, -maxAngle, maxAngle);

		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (new Vector3 (-ydiff, xdiff, 0)), 0.5f);
	}

	public void OnMouseDown()
	{
		cardCollider.enabled = false;
		
		currentSpeed = moveSpeed + Random.Range(-0.5f, 0.5f);
		
		SetHeight (true);
		sortingGroup.sortingOrder = 2;

		startPoint = transform.position;

		dragPoint = startPoint - GetMousePosInWorld();
		
		currentHolder.RemoveCard (this);
		
		AudioManager.Instance.PlayEffectFromCollection(0, startPoint, 0.8f);
		
		cursorManager.Grab();
	}

	public void SetHeight(bool raised)
	{
		dragging = raised;
		height = raised ? -1f : 0f;
	}

	public void OnMouseUp()
	{
		cardCollider.enabled = true;
		
		UpdateArea();

		SetHeight(false);
		this.StartCoroutine(() => sortingGroup.sortingOrder = 1, 0.3f);
		currentHolder.AddCard (this, false);
		
		AudioManager.Instance.PlayEffectFromCollection(0, transform.position, 0.8f);
		
		cursorManager.Hover();
	}

	private void UpdateArea()
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
	}

	private bool LeftArea(float distance) {
		return (transform.position - startPoint).magnitude > distance;
	}

	public void Move(Vector3 pos)
	{
		fromPosition = transform.position;
		toPosition = pos;
		moveDuration = 0f;
		if ((fromPosition - toPosition).magnitude > 1f)
		{
			AudioManager.Instance.PlayEffectFromCollection(1, fromPosition, 1.5f);	
		}
	}

	public void DestroyAfterAnimation()
	{
		var delay = Random.Range(0f, 0.1f);
		Invoke(nameof(DestroyAnimation), delay);
	}

	private void DestroyAnimation()
	{
		var pos = transform.position;
		AudioManager.Instance.PlayEffectFromCollection(0, pos, 1.5f);
		EffectManager.AddEffect(0, pos);
		EffectManager.AddEffect(1, pos);
		Tweener.ScaleToQuad(transform, Vector3.zero, 0.15f);
		var angle = Random.Range(1, 5) * 180f;
		Tweener.RotateToQuad(transform, Quaternion.Euler(0, 0, angle), 0.15f);
		Locked = true;
		Invoke(nameof(DoDestroy), 0.17f);
	}

	private void DoDestroy()
	{
		Destroy(gameObject);
	}
}
