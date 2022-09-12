using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, PlacedObjectSO placedObjectSO)
    {
        Transform placedObjectTransform = Instantiate(placedObjectSO.prefab, worldPosition, Quaternion.identity);

        PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();

        placedObject.placedObjectSO = placedObjectSO;
        placedObject.origin = origin;

        return placedObject;
    } 

    private PlacedObjectSO placedObjectSO;
    private Vector2Int origin;

    public List<Vector2Int> GetGridPositionList()
    {
        return placedObjectSO.GetGridPositionList(origin);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
