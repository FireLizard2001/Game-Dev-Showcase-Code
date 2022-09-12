using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    public event EventHandler OnSelectedChanged;
    public event EventHandler OnObjectPlaced;


    [SerializeField] private List<PlacedObjectSO> placedObjectSOList = null;
    private PlacedObjectSO placedObjectSO;

    private Grid<GridObject> grid;
    private Transform gridObjectContainer;

    void Awake()
    {
        Instance = this;

        int gridWidth = 18;
        int gridHeight = 10;
        float cellSize = 1f;

        gridObjectContainer = this.gameObject.transform.GetChild(0);

        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(-9, -5), (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y));

        /// When selecting a building to place, grab from the list and then refresh <------------------ !!!!!!!!!!!!!!!!!!!
        /// and please switch the following line to null, and delete 36 before release
        placedObjectSO = placedObjectSOList[1];
        RefreshSelectedObjectType();
    }

    public class GridObject
    {

        private Grid<GridObject> grid;
        private int x;
        private int y;
        private PlacedObject placedObject;

        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            placedObject = null;
        }

        public void SetPlacedObject(PlacedObject placedObject)
        {
            this.placedObject = placedObject;
            grid.TriggerGridObjectChanged(x, y);
        }

        public PlacedObject GetPlacedObject()
        {
            return placedObject;
        }

        public void ClearPlacedObject()
        {
            placedObject = null;
            grid.TriggerGridObjectChanged(x, y);
        }

        public bool CanBuild()
        {
            return placedObject == null;
        }

    }

    public PlacedObjectSO GetPlacedObjectSO()
    {
        return placedObjectSO;
    }


    public void mouseLeft(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (placedObjectSO != null)
            {
                grid.GetXY(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), out int x, out int y);

                List<Vector2Int> gridPositionList = placedObjectSO.GetGridPositionList(new Vector2Int(x, y));

                // Test if can build
                bool canBuild = true;
                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
                    {
                        // cannot build here
                        canBuild = false;
                        break;
                    }
                }

                if (canBuild)
                {
                    PlacedObject placedObject = PlacedObject.Create(grid.GetWorldPosition(x, y), new Vector2Int(x, y), placedObjectSO);

                    placedObject.transform.parent = gridObjectContainer;

                    foreach (Vector2Int gridPosition in gridPositionList)
                    {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
                    }

                    OnObjectPlaced?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Debug.Log("Cannot Build as Tile is Occupied");
                }
            }
            else
            {
                Debug.Log("Error: No Building is Selected to be Built");
            }
        }
    }

    public void mouseRight(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GridObject gridObject = grid.GetGridObject(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
            PlacedObject placedObject = gridObject.GetPlacedObject();
            if (placedObject != null)
            {
                placedObject.DestroySelf();

                List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
                }
            }
        }
    }

    private void DeselectObjectType()
    {
        placedObjectSO = null; RefreshSelectedObjectType();
    }

    private void RefreshSelectedObjectType()
    {
        OnSelectedChanged?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetMouseWorldSnappedPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        grid.GetXY(mousePosition, out int x, out int y);

        if (placedObjectSO != null)
        {

            Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, y);
            return placedObjectWorldPosition;
        }
        else
        {
            return mousePosition;
        }
    }

}
