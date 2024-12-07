using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public abstract class PickableAction : MonoBehaviour
    {
        public abstract void ExecuteAction(GameObject pickableObject);
    }
