using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private float nextDialogueLineDelay;
    [SerializeField] private float startYoffset;
    [SerializeField] private float XOffset;
    [SerializeField] private float separationYoffset;
    [SerializeField] private float dialogueWidth;
    [SerializeField] private bool_var dialogueOn;
    [SerializeField] private int_var dialogueBlock;
    [SerializeField] private RectTransform dialogueBox;

    [SerializeField] private DialogLine DEBUGDialog;

    private RectTransform[] displayedDialogues;
    private DialogLine currentDialogue;
    private int currentLine;
    private float nextDialogueLineDelayCurrent;

    void Start()
    {
        currentDialogue.isNull = true;
        dialogueBlock.val = 0;
        dialogueOn.val = false;
        displayedDialogues = new RectTransform[10000];
        StartCoroutine("StartDelay");
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(2f);
        if (!DEBUGDialog.isNull)
        {
            StartDialogue(DEBUGDialog);
        }
    }

    public void StartDialogue( DialogLine newDialog )
    {
        StartCoroutine(WaitUntilCanStart(newDialog));
    }

    IEnumerator WaitUntilCanStart( DialogLine newDialog )
    {
        while(dialogueBlock.val > 0)
        {
            yield return null;
        }
        currentDialogue = newDialog;
        dialogueOn.val = true;
        currentLine = 0;
       
        nextDialogueLineDelayCurrent = 0.1f;
    }

    void DisplayNextDialogueLine()
    {
       
        if (!currentDialogue.isNull && currentLine < currentDialogue.DialogEntries.Length)
        {
            GameObject newDialog = currentDialogue.DialogEntries[currentLine].dialogueSide == 0 ?
                                    (GameObject)Instantiate(Resources.Load("Dialog0")) : (GameObject)Instantiate(Resources.Load("Dialog1"));
            newDialog.transform.SetParent(dialogueBox.transform, false);
            displayedDialogues[currentLine] = newDialog.GetComponent<RectTransform>();
            displayedDialogues[currentLine].GetComponentInChildren<TextMeshProUGUI>().SetText(currentDialogue.DialogEntries[currentLine].DialogueLine);
            displayedDialogues[currentLine].sizeDelta = new Vector2(dialogueWidth, displayedDialogues[currentLine].sizeDelta.y);
            displayedDialogues[currentLine].GetComponentInChildren<TextMeshProUGUI>().alignment = 
                    currentDialogue.DialogEntries[currentLine].dialogueSide == 0 ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            float xOffset = currentDialogue.DialogEntries[currentLine].dialogueSide == 0 ? - XOffset: XOffset;
            displayedDialogues[currentLine].anchoredPosition = new Vector2(xOffset, 100);
            currentLine++;
            nextDialogueLineDelayCurrent = nextDialogueLineDelay;
        }
    }

    void Update()
    {
        if(!currentDialogue.isNull)
        {

            if(nextDialogueLineDelayCurrent <= 0 && currentLine < currentDialogue.DialogEntries.Length)
            {
                DisplayNextDialogueLine();
            }
            else
            {
                nextDialogueLineDelayCurrent -= Time.deltaTime;
            }

            if (displayedDialogues.Length > 0 && displayedDialogues[0])
            {
                float ypos = -startYoffset - displayedDialogues[0].sizeDelta.y / 2;
                for (int i = 0; i < currentLine; i++)
                {
                    displayedDialogues[i].anchoredPosition = new Vector2(displayedDialogues[i].anchoredPosition.x, ypos);
                    ypos -= (displayedDialogues[i].sizeDelta.y + separationYoffset);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (currentLine < currentDialogue.DialogEntries.Length)
                {
                    DisplayNextDialogueLine();
                }
                else
                {
                    CloseDialogue();
                }
            }
        }
    }

    public void ForceCloseDialogue()
    {

    }

    private void CloseDialogue()
    {
        for (int i = 0; i < currentLine; i++)
        {
            Destroy(displayedDialogues[i].gameObject);
        }
        currentLine = 0;
        currentDialogue.isNull = true;
        dialogueOn.val = false;
    }






}
