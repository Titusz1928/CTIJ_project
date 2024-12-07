using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneAction : PickableAction
{


    public override void ExecuteAction(GameObject pickableObject)
    {

        Debug.Log("object picked up");
        // Destroy the current herb (pickable object)
        Destroy(pickableObject);

    }

}

