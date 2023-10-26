
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyMenu : MonoBehaviour
{

    [SerializeField] private LobbyManager lobbyManager;
    
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private Button createButton;

    private Color textColor = Color.white;

    private string lobbyName = "";
    
    
    // Start is called before the first frame update
    void Start()
    {
        backButton.onClick.AddListener(Dismiss);
        createButton.onClick.AddListener(CreateLobby);

        inputField.characterLimit = 12;
        inputField.onValueChanged.AddListener(ValidateInput);
        createButton.interactable = false;
        subtitle.gameObject.SetActive(false);

    }

    private void ValidateInput(string input)
    {
        lobbyName = input;
        createButton.interactable = input.Length > 4;
    }

    private void CreateLobby()
    {
        lobbyManager.CreateLobby(lobbyName);
        createButton.interactable = false;
        inputField.interactable = false;
        subtitle.gameObject.SetActive(true);



        LeanTween.value(gameObject, 1f, 0f, 0.5f).setOnUpdate((float val) =>
        {
            textColor.a = val;
            subtitle.color = textColor;
        }).setLoopPingPong(-1);

    }



    private void Dismiss()
    {
        subtitle.gameObject.LeanCancel();
        subtitle.gameObject.SetActive(false);
        inputField.interactable = true;
        inputField.text = "";
        lobbyManager.CloseLobby();
        mainMenu.ExitCreateMenu();
    }
}
