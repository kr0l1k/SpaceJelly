using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{

    public static BoardController instance;
    public List<Sprite> characters = new List<Sprite>();
    public GameObject tile;
    public int xSize, ySize;
    public InputField xSizeText;
    public InputField ySizeText;

    private GameObject[,] tiles;
    private Vector2 offset;

    public bool IsShifting { get; set; }

    void Start()
    {
        instance = GetComponent<BoardController>();

        offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
    }

    public void ReshufleBoard()
    {
        ValidateFieldSize();
        foreach (var item in tiles)
        {
            Destroy(item);
        }
        //Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
    }


    private void CreateBoard(float xOffset, float yOffset)
    {
        MoveCreator();

        tiles = new GameObject[xSize, ySize];
        float startX = transform.position.x;
        float startY = transform.position.y;

        Sprite[] previousLeft = new Sprite[ySize];
        Sprite previousBelow = null;
        int nameCounter = 0;



        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
                List<Sprite> possibleCharacters = new List<Sprite>();
                possibleCharacters.AddRange(characters);

                
                possibleCharacters.Remove(previousLeft[y]);
                possibleCharacters.Remove(previousBelow);
                Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
                newTile.GetComponent<SpriteRenderer>().sprite = newSprite;
                newTile.name = nameCounter.ToString();
                nameCounter++;
                previousLeft[y] = newSprite;
                previousBelow = newSprite;
                tiles[x, y] = newTile;
            }
        }
    }

    /// <summary>
    /// validate input fields
    /// </summary>
    private void ValidateFieldSize()
    {

        int.TryParse(xSizeText.text, out xSize);
        if (xSize < 5 || xSize >= 10)
        {
            xSize = 7;
            xSizeText.text = "7";
        }

        int.TryParse(ySizeText.text, out ySize);
        if (ySize < 5 || ySize >= 10)
        {
            ySize = 7;
            ySizeText.text = "7";
        }

    }
    /// <summary>
    /// move game board to center of screen
    /// </summary>
    private void MoveCreator()
    {
        transform.position = new Vector3(-xSize / 2 * offset.x, -ySize / 2 * offset.y);
    }

    /// <summary>
    /// find null on the board and start shiftDown
    /// </summary>
    /// <returns></returns>
    public IEnumerator FindNullTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
                {
                    yield return StartCoroutine(ShiftTilesDown(x, y));
                    break;
                }
            }
        }
    }
    /// <summary>
    /// shift down the sprite and create a new one in empty space
    /// </summary>
    /// <param name="x"></param>
    /// <param name="yStart"></param>
    /// <param name="shiftDelay"></param>
    /// <returns></returns>
    private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
    {
        IsShifting = true;
        List<SpriteRenderer> renders = new List<SpriteRenderer>();
        int nullCount = 0;

        for (int y = yStart; y < ySize; y++)
        {
            SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
            if (render.sprite == null)
            {
                nullCount++;
            }
            renders.Add(render);
        }

        for (int i = 0; i < nullCount; i++)
        {
            yield return new WaitForSeconds(shiftDelay);
            for (int k = 0; k < renders.Count - 1; k++)
            {
                renders[k].sprite = renders[k + 1].sprite;
                renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
            }
        }
        IsShifting = false;
    }

    /// <summary>
    /// check posible variants of sprite
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Sprite GetNewSprite(int x, int y)
    {
        List<Sprite> possibleCharacters = new List<Sprite>();
        possibleCharacters.AddRange(characters);

        if (x > 0)
        {
            possibleCharacters.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);
        }
        if (x < xSize - 1)
        {
            possibleCharacters.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
        }
        if (y > 0)
        {
            possibleCharacters.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);
        }

        return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
    }

    /// <summary>
    /// check matches on board
    /// </summary>
    /// <returns></returns>
    public IEnumerator FindMatchesOnBoard()
    {
        foreach (var gameObject in tiles)
        {
            gameObject.GetComponent<ElementController>().ClearAllMatches();
            yield return null;
        }
    }
}
