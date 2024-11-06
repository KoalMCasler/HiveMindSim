using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CityGernerator : MonoBehaviour
{
    public GameManager gameManager;
    public int dimensions = 20;
    public Tile[] tileObjects;
    public List<Cell> gridComponents;
    public Cell cellObj;
    public Tile[] backupTiles;
    public int iteration;
    public int maxIteration;
    public float frequency;
    private void Awake()
    {
        gameManager =  FindObjectOfType<GameManager>();
        gridComponents = new List<Cell>();
        InitializeGrid();
    }
    /// <summary>
    /// Builds starting grid based on dimensions
    /// </summary>
    void InitializeGrid()
    {
        for(int y = 0; y < dimensions; y++)
        {
            for(int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector3(x, 0, y), Quaternion.identity,transform);
                newCell.CreateCell(false, tileObjects);
                gridComponents.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }

    /// <summary>
    /// Used to orginaize the list so the next tile to check is always the one with the lowest variatons posable. 
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);
        tempGrid.Sort((a, b) => a.tileOptions.Length - b.tileOptions.Length);
        tempGrid.RemoveAll(a => a.tileOptions.Length != tempGrid[0].tileOptions.Length);

        yield return new WaitForSeconds(frequency);

        CollapseCell(tempGrid);
    }
    /// <summary>
    /// Puts tile into cell position
    /// </summary>
    /// <param name="tempGrid"></param>
    void CollapseCell(List<Cell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        Cell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.collapsed = true;
        try
        {
            Tile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
            cellToCollapse.tileOptions = new Tile[] { selectedTile };
        }
        catch
        {
            Tile selectedTile = backupTiles[UnityEngine.Random.Range(0, backupTiles.Length)];
            cellToCollapse.tileOptions = new Tile[] { selectedTile };
        }

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, foundTile.transform.rotation,tempGrid[randIndex].transform);

        UpdateGeneration();
    }
    /// <summary>
    /// Updates what tile is in each cell, calls validation.
    /// </summary>
    void UpdateGeneration()
    {
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for(int y = 0; y < dimensions; y++)
        {
            for(int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;

                if (gridComponents[index].collapsed)
                {
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach(Tile t in tileObjects)
                    {
                        options.Add(t);
                    }

                    if(y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach(Tile possibleOptions in up.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            if(validOption >= 0)
                            {
                                Tile[] valid = tileObjects[validOption].downNeighbours;
                                validOptions = validOptions.Concat(valid).ToList();    
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    if(x < dimensions - 1)
                    {
                        Cell left = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach(Tile possibleOptions in left.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            if(validOption >= 0)
                            {
                                Tile[] valid = tileObjects[validOption].rightNeighbours;
                                validOptions = validOptions.Concat(valid).ToList();   
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y+1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            if(validOption >= 0)
                            {
                                Tile[] valid = tileObjects[validOption].upNeighbours;
                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (x > 0)
                    {
                        Cell right = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            int validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            if(validOption >= 0)
                            {
                                Tile[] valid = tileObjects[validOption].leftNeighbours;
                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for(int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGenerationCell;
        iteration++;

        if (iteration < maxIteration)
        {
            StartCoroutine(CheckEntropy());
        }
        else
        {
            gameManager.Initialize();
        }
    }
    /// <summary>
    /// Makes sure tile is valid to place
    /// </summary>
    /// <param name="optionList"></param>
    /// <param name="validOption"></param>
    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for(int x = optionList.Count - 1; x >=0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}
