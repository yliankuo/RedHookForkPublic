using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInOnBool : MonoBehaviour
{
    [SerializeField] bool_var boolean;
    [SerializeField] float slideScale;
    RectTransform rt;
    Vector2 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
        startPosition = rt.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, boolean ? slideScale * new Vector2(rt.sizeDelta.x,0) + startPosition : startPosition, 0.1f);
    }
}
