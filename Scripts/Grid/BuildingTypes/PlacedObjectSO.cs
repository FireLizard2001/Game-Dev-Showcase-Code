using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSO", menuName = "ScriptableObjects/Building")]
public class PlacedObjectSO : ScriptableObject
{
    new public string name;
    public Transform prefab;
    public Transform visual;
    public int width;
    public int height;

    /**
     * Returns a list of all tiles that are covered
     * by this object, starting at a given grid tile.
     */
    public List<Vector2Int> GetGridPositionList(Vector2Int offset)
    {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridPositionList.Add(offset + new Vector2Int(x, y));
            }
        }

        return gridPositionList;
    }
}
