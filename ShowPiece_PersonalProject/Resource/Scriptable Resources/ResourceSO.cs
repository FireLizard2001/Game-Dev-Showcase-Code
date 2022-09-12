using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceSO", menuName = "ScriptableObjects/Resource")]

public class ResourceSO : ScriptableObject // Resource object scriptable object, has name, max_stack and the current stack.
{
    public string NAME;
    public int MAX_STACK_SIZE;            
    public int current_stack_size;
}
