using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementController : MonoBehaviour
{
    private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static ElementController previousSelected = null;
    private static ElementController previousSelectedTemp = null;
    private bool matchFound = false;
    private SpriteRenderer render;
    private bool isSelected = false;
    private bool cleared = false;

    private Vector2[] adjoinedDirections = new Vector2[] { Vector2.left, Vector2.up, Vector2.right, Vector2.down };
    private Vector2[] horizontalDirection = new Vector2[2] { Vector2.left, Vector2.right };
    private Vector2[] verticalDirection = new Vector2[2] { Vector2.up, Vector2.down };

    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
    }

    private void Select()
    {
        isSelected = true;
        render.color = selectedColor;
        previousSelected = gameObject.GetComponent<ElementController>();
    }

    private void Deselect()
    {
        isSelected = false;
        render.color = Color.white;
        previousSelected = null;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnMouseDown()
    {

        if (render.sprite == null || BoardController.instance.IsShifting)
        {
            return;
        }

        if (isSelected)
        {
            Deselect();
        }
        else
        {
            if (previousSelected == null)
            {
                Select();
            }
            else
            {
                if (GetAllAdjoinedTiles().Contains(previousSelected.gameObject))
                {
                    cleared = false;
                    SwapSprite(previousSelected.render);
                    previousSelectedTemp = previousSelected;
                    previousSelected.Deselect();
                    ClearAllMatches();

                    StopCoroutine(BoardController.instance.FindMatchesOnBoard());
                    StartCoroutine(BoardController.instance.FindMatchesOnBoard());

                    if(!cleared)
                    {
                        SwapSprite(previousSelectedTemp.GetComponent<SpriteRenderer>());
                        previousSelectedTemp = null;
                        cleared = false;
                    }
                }
                else
                {
                    previousSelected.GetComponent<ElementController>().Deselect();
                    Select();
                }


            }
        }
    }
    public void SwapSprite(SpriteRenderer render2)
    {
        if (render.sprite == render2.sprite)
        {
            return;
        }

        Sprite tempSprite = render2.sprite;
        render2.sprite = render.sprite;
        render.sprite = tempSprite;
    }
    private GameObject GetAdjoined(Vector2 castDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private List<GameObject> GetAllAdjoinedTiles()
    {
        List<GameObject> adjoinedTiles = new List<GameObject>();
        for (int i = 0; i < adjoinedDirections.Length; i++)
        {
            adjoinedTiles.Add(GetAdjoined(adjoinedDirections[i]));
        }
        return adjoinedTiles;
    }

    private List<GameObject> FindMatch(Vector2 castDir, ElementController gameObject)
    {
        List<GameObject> matchingTiles = new List<GameObject>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            matchingTiles.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
        }
        return matchingTiles;
    }

    private void ClearMatch(Vector2[] paths)
    {
        List<GameObject> matchingTiles = new List<GameObject>();
        for (int i = 0; i < paths.Length; i++)
        {
            matchingTiles.AddRange(FindMatch(paths[i], this));
        }
        if (matchingTiles.Count >= 2)
        {
            for (int i = 0; i < matchingTiles.Count; i++)
            {
                matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
            }
            matchFound = true;
        }
    }

    public void ClearAllMatches()
    {
        if (render.sprite == null)
            return;

        ClearMatch(horizontalDirection);
        ClearMatch(verticalDirection);
        if (matchFound)
        {

            render.sprite = null;
            matchFound = false;
            StopCoroutine(BoardController.instance.FindNullTiles());
            StartCoroutine(BoardController.instance.FindNullTiles());
            cleared = true;
        }
    }
}
