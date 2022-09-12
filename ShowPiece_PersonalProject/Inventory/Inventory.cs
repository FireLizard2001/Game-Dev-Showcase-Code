using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;


public class Inventory : MonoBehaviour
{
    public int inventory_size; // set in inspector
    public List<ResourceInstance> object_inventory = new List<ResourceInstance>();
    public bool which = true; // testing bool
    

    [SerializeField] private Transform DebugResource; // testing resources
    [SerializeField] private Transform DebugResource1;

    public void Start()
    {
        Vector2 inventory_position = this.transform.position; // handle resource dropping? Discuss <-------
    }
    

    /// <summary>
    /// Function that adds resource objects to the inventory
    /// Takes in resource parent and current stack values
    /// Handles overflow and new additions to the inventory
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="current_stack"></param>
    public void AddToInventory(ResourceParent resource, int current_stack) 
    {
        ResourceInstance resource_to_add = new ResourceInstance(resource, current_stack);
        List<int> minimum_of_resource = new List<int>(); // creates temporary list for sorting
        List<ResourceInstance> resource_parallel = new List<ResourceInstance>(); // getting the object

        for (int i = 0; i < object_inventory.Count; i++) // loop through existing inventory
        {
            ResourceInstance current_resource = object_inventory[i]; // Get current resource object in list
            if (resource_to_add.GET_NAME() == current_resource.GET_NAME()) // Check if the index's object matches our "to_add" object via name
            {
                minimum_of_resource.Add(current_resource.GET_CURRENT_STACK()); // add same resource to new list to sort then add
                resource_parallel.Add(current_resource);
            }
        }

        if (minimum_of_resource.Count != 0)  // resource exists in inventory transfered to
        {
            int stack_of_min = minimum_of_resource.Min(); // Get minimum val
            int min_index = minimum_of_resource.IndexOf(stack_of_min);
            ResourceInstance resource_index = resource_parallel[min_index];     // We get the min index in minimum resource
            int object_index = object_inventory.IndexOf(resource_index);       
                                                                                // We then use that index to find the object,
                                                                                // and then using that index, we find the resource in the object inventory list
                                                                            
            if (stack_of_min + resource_to_add.GET_CURRENT_STACK() <= resource.GET_MAX_STACK()) //Checks for overflow
            {
                object_inventory[object_index].AddToStack(resource_to_add.GET_CURRENT_STACK());
                // if no overflow, set stack to sum
            }
            else 
            {
                if (object_inventory.Count < inventory_size) // at least 1 empty space in inventory
                {
                    int overflow = stack_of_min + resource_to_add.GET_CURRENT_STACK() - resource_to_add.GET_MAX_STACK(); //get overflow amount
                    object_inventory[object_index].SET_STACK(resource_to_add.GET_MAX_STACK()); // Set the existing index to maximum stack value
                    resource_to_add.SET_STACK(overflow); // set the new resource's stack to the overflow
                    object_inventory.Add(resource_to_add); // add new object

                    // if overflow, check for inventory space then add 
                }
                else 
                {
                    //Handle maxing existing stack if applicable---------------------------------------------------
                    Debug.LogWarning("WARNING, OUT OF SPACE"); // temp handler for exceeding inventory size(discuss dropping object?)
                }
            }
            
        }

        else 
        {
            object_inventory.Add(resource_to_add);
        }

    }

    /// <summary>
    /// Unity new input system mouse 1 function
    /// Temporary function to test and to add to the existing inventory
    /// Currently adds a square object and a circle object with a flipping bool var
    /// Debugs the list to keep track of what is being added
    /// </summary>
    /// <param name="context"></param>
    public void mouseOne(InputAction.CallbackContext context)  // unity new input system
    {
        
        if (context.performed) 
        {

            if (which)
            {
                AddToInventory(DebugResource.GetComponent<ResourceParent>(), 1);
            }
            else 
            {
                AddToInventory(DebugResource1.GetComponent<ResourceParent>(), 1);
            }
            which = !which;

            foreach (var resourceinstance in object_inventory)
            {
                Debug.Log(resourceinstance.GET_CURRENT_STACK() + "     " +  resourceinstance.GET_NAME());
            }
        }
    }


    public class ResourceInstance
    {
        private int current_stack_size;
        private string name;
        private int max_stack_size;

        public ResourceInstance(int cur_stack, string name_, int max_stack) // testing constructor
        {
            this.name = name_;
            this.current_stack_size = cur_stack;
            this.max_stack_size = max_stack;
        }

        public ResourceInstance(ResourceParent parent, int cur_stack) // used constructor
        {
            this.current_stack_size = cur_stack;
            this.max_stack_size = parent.GET_MAX_STACK();
            this.name = parent.GET_RESOURCE_NAME();
        }
        public void AddToStack(int addition) // Adds a value to the current stack
        {
            current_stack_size += addition;
        }

        public void RemoveStack() // empties out a stack
        {
            current_stack_size = 0;
        }

        public string GET_NAME() // Gets the name of the stack resource
        {
            return name;
        }

        public int GET_CURRENT_STACK() // Gets the current stack of the resource
        {
            return current_stack_size;
        }

        public int GET_MAX_STACK() // Gets the max stack of the resource
        {
            return max_stack_size;
        }
        public void SET_STACK(int new_stack) // Sets the current stack of the resource
        {
            current_stack_size = new_stack;
        }

    }
}
