
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject joinLobbyMenu;
    [SerializeField] private GameObject createLobbyMenu;

    private int menuHorizontalTransition = Screen.width;
    private float menuTransitionTime = 0.5f;
    
    void Start()
    {
        joinButton.onClick.AddListener(ShowLobbyMenu);
        createButton.onClick.AddListener(ShowCreateMenu);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void ShowLobbyMenu()
    {
        MoveMenuLeft(mainMenu);
        MoveMenuLeft(joinLobbyMenu);
        joinLobbyMenu.GetComponent<JoinLobbyMenu>().RefreshLobbyList();
    }

    public void ExitLobbyMenu()
    {
        MoveMenuRight(mainMenu);
        MoveMenuRight(joinLobbyMenu);
        joinLobbyMenu.GetComponent<JoinLobbyMenu>().ClearLobbyList();

    }
    
    private void ShowCreateMenu()
    {
        MoveMenuLeft(mainMenu);
        MoveMenuLeft(createLobbyMenu);
    }
    
    public void ExitCreateMenu()
    {
        MoveMenuRight(mainMenu);
        MoveMenuRight(createLobbyMenu);
    }

    private void MoveMenuLeft(GameObject menu)
    {
        menu.LeanMoveX(menu.transform.position.x -menuHorizontalTransition, menuTransitionTime)
            .setEase(LeanTweenType.easeOutQuart);
    }
    
    private void MoveMenuRight(GameObject menu)
    {
        menu.LeanMoveX(menu.transform.position.x + menuHorizontalTransition, menuTransitionTime)
            .setEase(LeanTweenType.easeOutQuart);
    }

    private void ExitGame()
    {
        Application.Quit();
    }


}
