
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameDialog : MonoBehaviour
{
    private GameMaster gameMaster;
    
    private Transform background;
    private Button rematchButton;
    private Button exitButton;
    private TextMeshProUGUI title;


    private void Awake()
    {
        background = transform.Find("Background");
        title = background.Find("Title").GetComponent<TextMeshProUGUI>();
        rematchButton = background.Find("RematchButton").GetComponent<Button>();
        exitButton = background.Find("ExitButton").GetComponent<Button>();
        
    }

    void Start()
    {
        gameMaster = GameMaster.Singleton;
        rematchButton.onClick.AddListener(() =>
        {
            gameMaster.AddToRematch();
        });
        
        exitButton.onClick.AddListener((() =>
        {
            gameMaster.ExitGame();
        }));
    }
    

    public void SetTitle(string text)
    {
        title.text = text;
    }
}
