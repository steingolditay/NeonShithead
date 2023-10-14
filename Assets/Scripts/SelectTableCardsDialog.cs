

using UnityEngine;
using UnityEngine.UI;

public class SelectTableCardsDialog : MonoBehaviour
{
    private Button readyButton;

    private void Awake()
    {
        readyButton = transform.Find("CardsBackground").Find("Button").GetComponent<Button>();
        readyButton.interactable = false;

        readyButton.onClick.AddListener(() =>
        {
            StartCoroutine(GameMaster.Singleton.SetPlayerSelectedTableCards());
        });
    }
//
    public void SetActive(bool state)
    {
        readyButton.interactable = state;
    }
    
    


}
