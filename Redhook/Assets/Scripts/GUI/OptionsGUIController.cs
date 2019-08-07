using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionsGUIController : MonoBehaviour
{
    public CellGrid CellGrid;

    public Image Panel;
    public Sprite OldArmyPanel;
    public Sprite NewArmyPanel;
    public Text ObjectivesText;
    public Text InfoText;
    public Button NextTurnButton;

    public GameObject PhaseChangeGUI;
    public Text PhaseChangeText;
    public float PhaseChangeTime;

    public GameObject GameEndGUI;
    public GameObject GameEndBacking;
    public TextMeshPro GameEndText;


    public Sprite DefeatPanel;
    public Sprite VictoryPanel;
    public string DefeatDescription;
    public string VictoryDescription;
    public float panelExpandTime;
    public float endTextTime;

    public int_var dialogueBlock;
    public bool gameOver;


    void Awake()
    {
        gameOver = false;
        CellGrid.GameStarted += OnGameStarted;
        CellGrid.TurnEnded += OnTurnEnded;
        CellGrid.GameEnded += OnGameEnded;
        CellGrid.UnitAdded += OnUnitAdded;
    }

    private void OnUnitAdded(object sender, UnitCreatedEventArgs e)
    {
        e.unit.GetComponent<Unit>().UnitNoMovePoints += OnUnitFinished;
    }

    IEnumerator OnUnitFinishedHandler(object sender, EventArgs e)
    {
        Unit un = sender as Unit;
        while (un.deselecting) { yield return 0; }

        List<Unit> currPlayerUnits = CellGrid.Units.FindAll(u => u.PlayerNumber.Equals(CellGrid.CurrentPlayerNumber));
        List<Unit> reachableEnemyUnits = CellGrid.Units.FindAll(u => (u.UnitState == Unit.UnitStateEnum.REACHABLE_ENEMY));

        bool noMove = true;
        foreach (Unit u in currPlayerUnits)
        {
            if (u.MovementPoints != 0 && u.UnitState != Unit.UnitStateEnum.FINISHED) noMove = false;
        }
        if (noMove && reachableEnemyUnits.Count == 0)
        {
            StartCoroutine(DelayEndTurn());
        }
    }

    private void OnUnitFinished(object sender, EventArgs e)
    {
        StartCoroutine(OnUnitFinishedHandler(sender, e));
    }

    IEnumerator DelayEndTurn()
    {
        yield return new WaitForSeconds(0.5f);
        CellGrid.EndTurn();
    }

    public void Exit_To_Menu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    private void OnGameStarted(object sender, EventArgs e)
    {
        OnTurnEnded(sender, e);
    }

    private IEnumerator PanelExpand(GameObject g)
    {
        float x = Time.deltaTime / panelExpandTime;
        float y = Time.deltaTime / panelExpandTime;
        g.transform.localScale = new Vector3 (x, y, 1.0f);

        // Expand x back out
        do
        {
            x += (Time.deltaTime / panelExpandTime);
            g.transform.localScale = new Vector3(x, y, 1.0f);
            yield return new WaitForEndOfFrame();
        } while (x < 1.0f);

        // Expand y back out
        do
        {
            y += (Time.deltaTime / panelExpandTime);
            g.transform.localScale = new Vector3(x, y, 1.0f);
            yield return new WaitForEndOfFrame();
        } while (y < 1.0f);
    }

    private void OnGameEnded(object sender, EventArgs e)
    {
        if (!gameOver)
        {
            if ((sender as CellGrid).CurrentPlayerNumber == 2)
            {
                SoundManager.Instance.FadeInMusic(6, 0.5f, 1.0f);
                GameEndGUI.SetActive(true);
                GameEndBacking.GetComponent<Image>().sprite = DefeatPanel;
                StartCoroutine(PanelExpand(GameEndBacking));
                StartCoroutine(TextScroll(DefeatDescription, GameEndText));
            }
            else
            {
                SoundManager.Instance.FadeInMusic(5, 0.5f, 1.0f);
                GameEndGUI.SetActive(true);
                GameEndBacking.GetComponent<Image>().sprite = VictoryPanel;
                StartCoroutine(PanelExpand(GameEndBacking));
                StartCoroutine(TextScroll(VictoryDescription, GameEndText));
            }
        }
        gameOver = true;
    }

    private IEnumerator TextScroll(string s, TextMeshPro field)
    {
        float timePerChar = endTextTime / s.Length;

        string b = "";
        float time = 0.0f;

        field.text = b;
        // Type out text
        for (int i = 0; i < s.Length; i++)
        {
            b = b + s[i];
            field.text = b;
            do
            {
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (time < (timePerChar * i));
        }
    }

    private IEnumerator TextScroll(string s, Text field, GameObject obj)
    {
        float startPause = PhaseChangeTime / 6.0f;
        float endPause = PhaseChangeTime / 2.0f;
        float typingTime = PhaseChangeTime - startPause - endPause;
        float timePerChar = typingTime / s.Length;

        string b = "";
        float time = 0.0f;

        field.text = b;
        obj.SetActive(true);
        // Delay before text appears
        do
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (time < startPause);

        // Type out text
        for(int i = 0; i < s.Length; i++)
        {
            b = b + s[i];
            field.text = b;
            do
            {
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            } while (time < (startPause + (timePerChar * i)));
        }

        // Delay after text types out
        do
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (time < PhaseChangeTime);
        obj.SetActive(false);
        CellGrid.OnNextPhaseBegin();
    }

    private void OnTurnEnded(object sender, EventArgs e)
    {
        NextTurnButton.interactable = ((sender as CellGrid).CurrentPlayer is HumanPlayer);

        int player = (sender as CellGrid).CurrentPlayerNumber;
        string t;
        if(player == 2)
        {
            t = "Enemy Phase";
        }
        else
        {
            int nextPlayer = ((sender as CellGrid).CurrentPlayerNumber + 1);
            t = "Player " + nextPlayer + " Phase";
            if (nextPlayer == 1)
            {
                Panel.sprite = OldArmyPanel;
            }
            else
            {
                Panel.sprite = NewArmyPanel;
            }
        }

        InfoText.text = t;
        StartCoroutine(TextScroll(t, PhaseChangeText, PhaseChangeGUI));

        foreach(Unit u in CellGrid.Units)
        {
            if(u.gameObject.tag == "Boss")
            {
                ObjectivesText.text = "◇ Defeat the Boss\n     ◇ Survive...";
            }
        }
    }
}
