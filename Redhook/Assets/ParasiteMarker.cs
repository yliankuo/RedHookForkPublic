using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiteMarker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BaseUnit unit = gameObject.GetComponentInParent(typeof(BaseUnit)) as BaseUnit; 
        if (unit != null)
        {
            this.GetComponentInChildren<Renderer>().enabled = unit.CurrentlyVisible;
        }
    }
}
