using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is a generic class for grids. It can take any type.
 */
public class Grid<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    private bool debug = true;
    //private GameObject[,] debugObjectArray;

    /*
     * Grid constructor. Takes in x width, y height, and grid cell size.
     * Also takes an origin position to determine where to start drawing the grid, and a function to default all cell values.
     */
    public Grid(int width, int height, float cellSize, Vector3 originPosition, System.Func<Grid<TGridObject>, int, int, TGridObject> createGridObject )
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];
        //debugObjectArray = new GameObject[width, height];

        /*
         * Set default values
         */
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }

        }

        if (debug)
        {
            /*
             * This loops through the grid
             */
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    /*
                     * This function displayed objects within the grid for debugging purposes.
                     */

                    //debugObjectArray[x,y] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //debugObjectArray[x, y].transform.position = GetWorldPosition(x, y);

                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        }

    }

    /*
     * Based on xy coordinates of the grid, obtain the world coordinates
     */
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    /*
     * Based on a position in the world (used on mouseclick to select a cell), obtain the xy grid coords
     */
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    /*
     * This was used to create and display world text when debugging initially.
     * It would show coordinates for a tile.
     */
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0  && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            
            /*
             * This is where visuals would be updated.
             */
            //debugObjectArray[x, y].GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        }
    }

    /*
     * This was used to create and display world text when debugging initially.
     * It would show coordinates for a tile.
     */
    public void TriggerGridObjectChanged(int x, int y)
    {
        /*
         * This is where visuals would be updated as well.
         * Any time the values in a TGridObject gets update this should be called for the Grid Panel it is in
         */
        //debugObjectArray[x, y].GetComponent<Renderer>().material.color = new Color(0, 255, 0);
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }
}
