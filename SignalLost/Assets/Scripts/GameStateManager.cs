using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] List<TextMeshPro> playerNameTexts = new List<TextMeshPro>();

    public string playerName;
    public int playerDeaths = 0;

    public void AddDeath() { playerDeaths++; RefreshText(); }

    // Start is called before the first frame update
    void Start()
    {
        RefreshText();
    }

    private void RefreshText()
    {
        int versionNum = playerDeaths + 1;
        string displayName = playerName + "#";

        if (versionNum >= 1000) displayName = "Scrapped Version";
        else if (versionNum >= 100) displayName = displayName + versionNum.ToString();
        else if (versionNum >= 10) displayName = displayName + "0" + versionNum.ToString();
        else displayName = displayName + "00" + versionNum.ToString();


        playerNameTexts[0].text = "Welcome, " + displayName;

        displayName = "Version: " + displayName;

        for (int i = 1; i < playerNameTexts.Count; ++i)
        {
            playerNameTexts[i].text = displayName;
        }
    }
}
