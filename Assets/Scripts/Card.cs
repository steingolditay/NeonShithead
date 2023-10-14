using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    private GameMaster gameMaster;
    private Image frontImage, coverImage;
    private GameObject highlight;
    private GameObject selection;
    private Vector3 originalScale = Vector3.zero;
    private CardModel cardModel;

    private string BaseMap = "_BaseMap";

    private bool isHoverdOn = false;
    private bool isHighlighted = false;
    private bool isOnSelectionStage = false;
    private bool isSelected = false;
    private bool canStick = false;
    
    void Awake()
    {
        gameMaster = GameMaster.Singleton;

        frontImage = transform.Find("Front").transform.Find("Image").GetComponent<Image>();
        coverImage = transform.Find("Cover").transform.Find("Image").GetComponent<Image>();
        highlight = transform.Find("Highlight").gameObject;
        selection = transform.Find("Selection").gameObject;

        ToggleHighlight(false);
        originalScale = transform.localScale;
        Color frontImageColor = frontImage.color;
        frontImageColor.a = 0;
        frontImage.color = frontImageColor;

    }

    public int GetCardClass()
    {
        return cardModel.cardClass;
    }
    
    public CardFlavour GetCardFlavour()
    {
        return cardModel.cardFlavour;
    }

    public void SetCardModel(CardModel model)
    {
        cardModel = model;
        SetFront( Utils.GetSpriteForCardModel(model));;
        
    }

    public void SetCanStick(bool state)
    {
        canStick = state;
    }

    private void SetFront(Sprite front)
    {
        frontImage.sprite = front;

        Material material = new Material(frontImage.material);
        material.SetTexture(BaseMap, front.texture);
        frontImage.material = material;

        Color frontImageColor = frontImage.color;
        frontImageColor.a = 1;
        frontImage.color = frontImageColor;
    }
    
    public void SetCover(Sprite cover)
    {
        
        coverImage.sprite = cover;

        Material material = new Material(coverImage.material);
        material.SetTexture(BaseMap, cover.texture);
        coverImage.material = material;
        
    }


    public void SetIsOnSelectionStage(bool state)
    {
        isOnSelectionStage = state;
    }
    
    
    public void ToggleHighlight(bool state)
    {
        highlight.SetActive(state);
    }

    public void ToggleSelection(bool state)
    {
        selection.SetActive(state);
    }

    private void OnMouseDown()
    {
        if (isOnSelectionStage)
        {
            HandleSelectionStage();
            return;
        }
        
        if (!IsMyCard())
        {
            return;
        }

        if (IsMyTurn())
        {
            if ((CardIsInMyHand() && CanPlayCard()) ||
                (IsTableCard() && CanPlayTableCard() && (CardIsHiddenOnMyTable() || CanPlayCard())))
            {
                ToggleHighlight(false);
                ToggleSelection(false);
                gameMaster.PutCardsInPile(this, GetCardLocation(), false);
            }
        }
        else if (!gameMaster.opponentPlayedTurn && canStick)
        {
            ToggleHighlight(false);
            ToggleSelection(false);
            gameMaster.PutCardsInPile(this, GetCardLocation(), true);
        }
    }

    private void OnMouseEnter()
    {
        if (IsMyCard() && !isHoverdOn)
        {
            isHoverdOn = true;
            if (CardIsInMyHand())
            {
                if (!isSelected)
                {
                    ToggleRaised(true);
                }
                if ((IsMyTurn() && CanPlayCard() && !isSelected) ||
                    (!IsMyTurn() && !gameMaster.opponentPlayedTurn && canStick))
                {
                    ToggleHighlight(!isSelected);
                }
            } else if (CanPlayTableCard() && IsMyTurn())
            {
                if (CardIsVisibleOnMyTable() && !CanPlayCard())
                {
                    return;
                }   
                ToggleHighlight(!isSelected);
            }
        }
    }

    private void OnMouseOver()
    {
        if (IsMyTurn())
        {
            if (CardIsInMyHand() && CanPlayCard())
            {
                if (Input.GetMouseButtonUp(1))
                {
                    isSelected = !isSelected;
                    ToggleSelection(isSelected);
                    ToggleHighlight(false);
                    ToggleRaised(true);
                    gameMaster.OnCardSelected(this, isSelected);
                }
            } else if (CardIsVisibleOnMyTable() && CanPlayTableCard() && CanPlayCard())
            {
                if (Input.GetMouseButtonUp(1))
                {
                    isSelected = !isSelected;
                    ToggleSelection(isSelected);
                    ToggleHighlight(false);
                    gameMaster.OnCardSelected(this, isSelected);
                }
            }
        }
    }
    
    private void OnMouseExit()
    {
        if (isHoverdOn)
        {
            if (IsMyCard())
            {
                isHoverdOn = false;
                ToggleHighlight(false);

                if (CardIsInMyHand())
                {
                    if (!isSelected)
                    {
                        ToggleRaised(false);
                        transform.LeanScale(
                            new Vector3(originalScale.x, originalScale.y, originalScale.z), 
                            0.1f);
                    }
                }
            }
        }

    }

    public void DisableSelection()
    {
        isSelected = false;
        ToggleSelection(false);
        ToggleHighlight(false);
        ToggleRaised(false);
    }


    
    private void HandleSelectionStage()
    {
        if (isHighlighted)
        {
            gameMaster.RemoveCardFromSelectedTableCards(this);
        }
        else
        {
            gameMaster.AddCardToSelectedTableCards(this);

        }
        isHighlighted = !isHighlighted;
        ToggleHighlight(isHighlighted);
    }

    private GameMaster.CardLocation GetCardLocation()
    {
        GameMaster.CardLocation cardLocation = GameMaster.CardLocation.Hand;
        if (CardIsVisibleOnMyTable())
        {
            cardLocation = GameMaster.CardLocation.VisibleTable;
        } else if (CardIsHiddenOnMyTable())
        {
            cardLocation = GameMaster.CardLocation.HiddenTable;
        }

        return cardLocation;
    }

    private bool IsMyTurn()
    {
        return gameMaster.GetCurrentPlayerTurn() == GameMaster.PlayerTurn.Player;
    }

    private bool IsTableCard()
    {
        return CardIsVisibleOnMyTable() || CardIsHiddenOnMyTable();
    }

    private bool IsMyCard()
    {
        return IsTableCard() || CardIsInMyHand();
    }

    public void ToggleRaised(bool state)
    {
        float x = state ? originalScale.x * 1.1f : originalScale.x;
        float z = state ? originalScale.z * 1.1f : originalScale.z;
        transform.LeanScale(new Vector3(x, originalScale.y, z), 0.1f);

    }

    private bool CardIsInMyHand()
    {
        return transform.parent == gameMaster.GetPlayerHand();
    }

    private bool CardIsVisibleOnMyTable()
    {
        Transform parent = transform.parent;
        bool isTableCard = parent == gameMaster.playerTableCard1 || 
                           parent == gameMaster.playerTableCard2 || 
                           parent == gameMaster.playerTableCard3;
        if (!isTableCard || parent.childCount < 2)
        {
            return false;
        }
        return parent.GetChild(1) == transform;
    }
    
    private bool CardIsHiddenOnMyTable()
    {
        Transform parent = transform.parent;
        bool isTableCard = parent == gameMaster.playerTableCard1 || 
                           parent == gameMaster.playerTableCard2 || 
                           parent == gameMaster.playerTableCard3;
        if (!isTableCard || parent.childCount == 0)
        {
            return false;
        }
        return parent.GetChild(0) == transform;
    }

    private bool CanPlayTableCard()
    {
        return gameMaster.GetDeckCardsCount() == 0 && gameMaster.GetPlayerHand().childCount == 0;
    }

    private bool CanPlayCard()
    {
        return Utils.CanPlayCard(GetCardClass(), gameMaster.GetTopPileCardClass());
    }
}
