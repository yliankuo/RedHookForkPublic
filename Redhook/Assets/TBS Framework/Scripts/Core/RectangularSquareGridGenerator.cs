using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates rectangular shaped grid of squares.
/// </summary>
[ExecuteInEditMode()]
public class RectangularSquareGridGenerator : ICellGridGenerator
{
    public GameObject SquarePrefab;

    public int Width;
    public int Height;

    public override List<Cell> GenerateGrid()
    {
        var ret = new List<Cell>();

        if (SquarePrefab.GetComponent<Square>() == null)
        {
            Debug.LogError("Invalid square cell prefab provided");
            return ret;
        }

        // Fill out row by row, from row (height - 1) to row 0 (so left to right, bottom to top)
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                var square = Instantiate(SquarePrefab);
                var squareSize = square.GetComponent<Cell>().GetCellDimensions();

                square.transform.position = new Vector3(j*squareSize.x, 0, i*squareSize.z);
                square.GetComponent<Cell>().OffsetCoord = new Vector2(j, i);
                square.GetComponent<Cell>().MovementCost = 1;
                ret.Add(square.GetComponent<Cell>());

                square.transform.parent = CellsParent;
            }
        }
        return ret;
    }
}
