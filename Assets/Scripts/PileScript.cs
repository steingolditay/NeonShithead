
using UnityEngine;

public class PileScript : MonoBehaviour
{
    private GameMaster gameMaster;

    private void Start()
    {
        gameMaster = GameMaster.Singleton;
    }


    private void OnMouseEnter()
    {
        if (IsMyTurn() && PileHasCards())
        {
            HighlightTopDeckCard(true);

        }
    }

    private void OnMouseUp()
    {
        if (IsMyTurn() && PileHasCards())
        {
            gameMaster.PlayerTakePile();
        }
    }

    private void OnMouseExit()
    {
        if (IsMyTurn() && PileHasCards())
        {
            HighlightTopDeckCard(false);
        }
        
    }

    private bool IsMyTurn()
    {
        return gameMaster.GetCurrentPlayerTurn() == GameMaster.PlayerTurn.Player;
    }

    private bool PileHasCards()
    {
        return transform.childCount > 0;
    }

    private void HighlightTopDeckCard(bool state)
    {
        transform.GetChild(transform.childCount -1).GetComponent<Card>().ToggleHighlight(state);

    }
}
