using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitOnButtonClick : MonoBehaviour
{
    void Start()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(ExitOnClick);
    }

    void ExitOnClick()
    {
        Application.Quit();
    }
}
