using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RendererEnabledOnBool : MonoBehaviour
{
    [SerializeField] bool_var boolean;
    Renderer r;
    Graphic g;

    private void Start()
    {
        r = GetComponent<Renderer>();
        g = GetComponent<Graphic>();
    }

    void Update()
    {
        if(r) r.enabled = boolean.val;   
        if(g) g.enabled = boolean.val; 
    }
}
