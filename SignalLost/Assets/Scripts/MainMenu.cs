using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public static MainMenu menu;

    [SerializeField] private TextMeshProUGUI inputText;
    [SerializeField] private GameObject warningText;

    public string savedPlayerName;

    private void Awake()
    {
        if (menu == null)
        {
            menu = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (menu != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayGame()
    {
        if (inputText.text.Length < 3)
        {
            warningText.GetComponent<TextMeshProUGUI>().text = "Please enter a name";
            warningText.SetActive(true);
        }
        else
        {
            savedPlayerName = inputText.text;
            gameObject.SetActive(false);
            warningText.SetActive(false);
            SceneManager.LoadScene(1);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Start()
    {
        warningText.SetActive(false);
        gameObject.SetActive(true);
    }
}
