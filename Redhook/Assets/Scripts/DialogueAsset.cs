using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SubjectNerd.Utilities;

[CreateAssetMenu(menuName = "Dialog")]
public class DialogAsset : ScriptableObject
{
    [Reorderable]
    public DialogEntry[] DialogEntries;

    [Serializable]
    public class DialogEntry
    {
        [SerializeField]
        public int dialogueSide;

        [TextArea]
        [SerializeField]
        public string DialogueLine;
    }
}

[Serializable]
public struct DialogLine
{
    public bool isNull;

    [Reorderable]
    public DialogEntry_2[] DialogEntries;

    [Serializable]
    public struct DialogEntry_2
    {
        [SerializeField]
        public int dialogueSide;

        [TextArea]
        [SerializeField]
        public string DialogueLine;
    }
}