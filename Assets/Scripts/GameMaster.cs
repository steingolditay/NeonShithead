
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Singleton { get; private set; }
    private PlayerController playerController;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite cover;
    [SerializeField] private Sprite[] cardFronts;

    [Header("Placements")] 

    [SerializeField] private Transform deckSpawnPoint;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform opponentHand;
    [SerializeField] public Transform playerTableCard1;
    [SerializeField] public Transform playerTableCard2;
    [SerializeField] public Transform playerTableCard3;
    [SerializeField] private Transform opponentTableCard1;
    [SerializeField] private Transform opponentTableCard2;
    [SerializeField] private Transform opponentTableCard3;
    [SerializeField] private Transform pile;
    [SerializeField] private Transform deck;
    [SerializeField] private Transform graveyard;

    [SerializeField] private Transform playerSelectionCard1;
    [SerializeField] private Transform playerSelectionCard2;
    [SerializeField] private Transform playerSelectionCard3;
    [SerializeField] private Transform playerSelectionCard4;
    [SerializeField] private Transform playerSelectionCard5;
    [SerializeField] private Transform playerSelectionCard6;

    [Header("Turn Indicators")] 
    [SerializeField] private Renderer playerTurnIndicator;
    [SerializeField] private Renderer opponentTurnIndicator;
    [SerializeField] private Renderer[] tableIndicators;
    [SerializeField] private Material playerTurnIndicatorMaterial;
    [SerializeField] private Material opponentTurnIndicatorMaterial;
    [SerializeField] private Material turnIndicatorOffMaterial;

    [Header("Dialogs")]
    [SerializeField] private GameObject selectCardsDialog;
    [SerializeField] private GameObject endGameDialog;

    [Header("Players")] 
    [SerializeField] private TextMeshProUGUI opponentName;
    [SerializeField] private TextMeshProUGUI opponentScore;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerScore;

    public enum PlayerTurn
    {
        Player,
        Opponent,
        None
    }

    public enum CardLocation
    {
        Hand, VisibleTable, HiddenTable 
    }

    private Collection<CardModel> cardModels = new Collection<CardModel>();
    private const float initialTimeBetweenPutCardInDeck = 0.2f;
    private const float deckCardDistance = 0.015f;
    private const float putCardInDeckAnimationSpeed = 0.5f;
    
    public List<Card> selectedTableCards = new List<Card>();
    public List<Card> unselectedTableCards = new List<Card>();

    public List<Card> selectedCards = new List<Card>();

    public int playersReady = 0;
    public int playersRematch = 0;
    public ulong firstPlayerToStart = 0;
    private PlayerTurn currentPlayerTurn = PlayerTurn.None;
    public bool opponentPlayedTurn = false;


    private void OnEnable()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }

        selectCardsDialog.transform.localScale = Vector3.zero;
        selectCardsDialog.SetActive(true);
        endGameDialog.transform.localScale = Vector3.zero;
        endGameDialog.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Singleton == this)
        {
            Singleton = null;
        }
    }

    public void SetPlayerController(PlayerController callback)
    {
        playerController = callback;
    }

    public void AddCardToSelectedTableCards(Card card)
    {
        selectedTableCards.Add(card);
        unselectedTableCards.Remove(card);
        selectCardsDialog.GetComponent<SelectTableCardsDialog>().SetActive(selectedTableCards.Count == 3);
    }

    public void RemoveCardFromSelectedTableCards(Card card)
    {
        selectedTableCards.Remove(card);
        unselectedTableCards.Add(card);
        selectCardsDialog.GetComponent<SelectTableCardsDialog>().SetActive(selectedTableCards.Count == 3);
    }
    
    public void SetCurrentPlayerTurn(PlayerTurn player)
    {
        currentPlayerTurn = player;
        Material[] playerMaterials = { player == PlayerTurn.Player ? playerTurnIndicatorMaterial : turnIndicatorOffMaterial };
        playerTurnIndicator.materials = playerMaterials;

        
        Material[] opponentMaterials = { player == PlayerTurn.Player ? turnIndicatorOffMaterial : opponentTurnIndicatorMaterial };
        
        opponentTurnIndicator.materials = opponentMaterials;

        foreach (Renderer renderer in tableIndicators)
        {
            renderer.material = player == PlayerTurn.Player ? playerTurnIndicatorMaterial : opponentTurnIndicatorMaterial;
        }
        if (currentPlayerTurn == PlayerTurn.Opponent)
        {
            opponentPlayedTurn = false;
        }
        else
        {
            for (int i = 0; i < playerHand.childCount; i++)
            {
                Card card = playerHand.GetChild(i).GetComponent<Card>();
                card.SetCanStick(false);
            }

        }

    }

    public PlayerTurn GetCurrentPlayerTurn()
    {
        return currentPlayerTurn;
    }

    public Transform GetPlayerHand()
    {
        return playerHand;
    }

    public IEnumerator PutCardInDeck()
    {
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, 180));
        Vector3 destination = deck.position;

        for (int i = 0; i < 19; i++)

        // for (int i = 0; i < 54; i++)
        {

            float speed = 1 / (i == 0 ? 1f : i);
            yield return new WaitForSeconds(speed * initialTimeBetweenPutCardInDeck);
            destination.y = deck.position.y + (i * deckCardDistance);
            GameObject card = Instantiate(cardPrefab, deckSpawnPoint.position, rotation);
            card.transform.SetParent(deck, true);
            card.GetComponent<Card>().SetCover(cover);
            LeanTween.move(card, destination, putCardInDeckAnimationSpeed);
        }

        yield return new WaitForSeconds(1.5f);
    }

    public void ShuffleCards()
    {
        // cardModels = Utils.GetAllCardModels();
        //
        // for (int i = 0; i < cardModels.Count - 1; i++)
        // {
        //     CardModel temp = cardModels[i];
        //     int random = Random.Range(i, cardModels.Count);
        //     cardModels[i] = cardModels[random];
        //     cardModels[random] = temp;
        // }
        //
        
        // test
        
        // Table Cards //
        Collection<CardModel> list = new Collection<CardModel>();
        list.Add(new CardModel(4, CardFlavour.Diamond));
        list.Add(new CardModel(4, CardFlavour.Club));
        list.Add(new CardModel(5, CardFlavour.Diamond));
        list.Add(new CardModel(5, CardFlavour.Club));
        list.Add(new CardModel(6, CardFlavour.Diamond));
        list.Add(new CardModel(6, CardFlavour.Club));

        // // Selection Cards //
        list.Add(new CardModel(12, CardFlavour.Diamond));
        list.Add(new CardModel(13, CardFlavour.Club));
        
        list.Add(new CardModel(12, CardFlavour.Diamond));
        list.Add(new CardModel(13, CardFlavour.Club));
        
        list.Add(new CardModel(12, CardFlavour.Diamond));
        list.Add(new CardModel(13, CardFlavour.Club));
        
        list.Add(new CardModel(12, CardFlavour.Diamond));
        list.Add(new CardModel(13, CardFlavour.Diamond));
        
        list.Add(new CardModel(2, CardFlavour.Club));
        list.Add(new CardModel(3, CardFlavour.Club));
        
        list.Add(new CardModel(2, CardFlavour.Spade));
        list.Add(new CardModel(3, CardFlavour.Spade));
        //
        // // First Card //
        list.Add(new CardModel(3, CardFlavour.Heart));
        //
        // // Deck Cards //
        //
        // list.Add(new CardModel(11, CardFlavour.Heart));
        // list.Add(new CardModel(12, CardFlavour.Heart));
        //
        // list.Add(new CardModel(10, CardFlavour.Diamond));
        // list.Add(new CardModel(10, CardFlavour.Club));
        // list.Add(new CardModel(10, CardFlavour.Spade));
        // list.Add(new CardModel(10, CardFlavour.Heart));
        // list.Add(new CardModel(9, CardFlavour.Diamond));
        // list.Add(new CardModel(9, CardFlavour.Club));
        // list.Add(new CardModel(9, CardFlavour.Spade));
        // list.Add(new CardModel(9, CardFlavour.Heart));
        // list.Add(new CardModel(1, CardFlavour.Diamond));
        // list.Add(new CardModel(1, CardFlavour.Club));
        // list.Add(new CardModel(1, CardFlavour.Spade));
        // list.Add(new CardModel(1, CardFlavour.Heart));

        
        cardModels = list;
    }

    public void DealPlayerTableCard(CardModel cardModel, int number)
    {
        Transform placement;
        switch (number)
        {
            case 1:
                placement = playerTableCard1;
                break;
            case 2:
                placement = playerTableCard2;
                break;
            case 3:
                placement = playerTableCard3;
                break;
            default:
                placement = playerTableCard1;
                break;
        }

        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);
        cardTransform.SetParent(placement, true);
        cardTransform.LeanMoveLocal(new Vector3(0, 0, 0), 0.3f);
    }

    public void DealPlayerSelectionCard(CardModel cardModel, int number)
    {
        Transform placement;
        switch (number)
        {
            case 1:
                placement = playerSelectionCard1;
                break;
            case 2:
                placement = playerSelectionCard2;
                break;
            case 3:
                placement = playerSelectionCard3;
                break;
            case 4:
                placement = playerSelectionCard4;
                break;
            case 5:
                placement = playerSelectionCard5;
                break;
            case 6:
                placement = playerSelectionCard6;
                break;
            default:
                placement = playerSelectionCard1;
                break;
        }

        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);
        card.SetIsOnSelectionStage(true);
        cardTransform.SetParent(placement, true);
        unselectedTableCards.Add(card);
        cardTransform.LeanMoveLocal(new Vector3(0, 0, 0), 0.3f);
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.3f);
    }

    public void DealOpponentTableCard(int number)
    {
        Transform placement;
        switch (number)
        {
            case 1:
                placement = opponentTableCard1;
                break;
            case 2:
                placement = opponentTableCard2;
                break;
            case 3:
                placement = opponentTableCard3;
                break;
            default:
                placement = opponentTableCard1;
                break;
        }

        Transform cardTransform = GetTopDeckCard();
        cardTransform.SetParent(placement, true);
        cardTransform.LeanMoveLocal(new Vector3(0, 0, 0), 0.3f);

    }

    public void PlayerTakePile()
    {
        playerController.OnTakePile();
    }

    public void PlayerDrawMissingCardsFromDeck(CardModel cardModel)
    {
        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);
        if (cardModel.cardClass == GetAbsoluteTopPileCardClass())
        {
            card.SetCanStick(true);
        }
        cardTransform.SetParent(playerHand, true);

        Vector3 cardPosition = Utils.SortPlayerHand(playerHand, cardTransform.gameObject);

        LeanTween.moveLocalY(cardTransform.gameObject, cardPosition.y, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                cardTransform.LeanMoveLocal(cardPosition, 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad);
            });
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.4f);
    }

    public void OpponentDrawCardFromDeck()
    {
        Transform cardTransform = GetTopDeckCard();
        cardTransform.SetParent(opponentHand, true);
        Vector3 cardPosition = Utils.SortOpponentHand(opponentHand, cardTransform.gameObject);

        LeanTween.moveLocalY(cardTransform.gameObject, cardPosition.y, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                cardTransform.LeanMoveLocal(cardPosition, 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad);
            });
    }

    public Sprite[] GetCardFronts()
    {
        return cardFronts;
    }

    public void ToggleSelectCardsDialog(bool state)
    {
        selectCardsDialog.LeanScale(state ? new Vector3(1, 1, 1) : Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInCubic)
            .setOvershoot(0.2f);
    }

    public bool IsWon()
    {
        int tableCardsCount = playerTableCard1.childCount + playerTableCard2.childCount + playerTableCard3.childCount;
        int deckCardsCount = deck.childCount;
        int handCardsCount = playerHand.childCount;
        return deckCardsCount == 0 && handCardsCount == 0 && tableCardsCount == 0;
    }

    public void ShowEndGameDialog(bool isWon)
    {
        endGameDialog.LeanScale(new Vector3(1, 1, 1), 0.3f)
            .setEase(LeanTweenType.easeInCubic)
            .setOvershoot(0.2f);
        endGameDialog.GetComponent<EndGameDialog>().SetTitle(isWon ? "YOU WIN" : "YOU LOSE");

        TextMeshProUGUI winnerScore = isWon ? opponentScore : playerScore;
        int currentScore = int.Parse(winnerScore.text);
        winnerScore.text = (currentScore + 1).ToString();
    }

    public void AddToRematch()
    {
        playerController.OnAddToRematch();
    }

    public void ExitGame()
    {
        
    }



    public void HideEndGameDialog()
    {
        endGameDialog.LeanScale(Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInCubic)
            .setOvershoot(0.2f);
    }

    public void ClearBoard()
    {
        List<Transform> positions = new List<Transform>
        {
            pile, deck, graveyard, playerHand, opponentHand, 
            opponentTableCard1, opponentTableCard2, opponentTableCard3,
            playerTableCard1, playerTableCard2, playerTableCard3
        };
        
        foreach (Transform position in positions)
        {
            foreach (Transform card in position)
            {
                Destroy(card.gameObject);
            }
        }
    }

    public IEnumerator SetPlayerSelectedTableCards()
    {
        
        playerController.OnPlayerSelectedTableCards();
        ToggleSelectCardsDialog(false);
        for (int i = 0; i < 3; i++)
        {
            Card card = selectedTableCards[i];
            Transform cardTransform = card.transform;
            Transform parent = GetPlayerTableCardPositionForIndex(i);

            cardTransform.SetParent(parent, true);
            card.SetIsOnSelectionStage(false);
            card.ToggleHighlight(false);
            Vector3 position = new Vector3(0, deckCardDistance, 0);
            cardTransform.LeanMoveLocal(position, 0.3f);
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.3f);


        for (int i = 0; i < 3; i++)
        {
            Card card = unselectedTableCards[i];
            card.SetIsOnSelectionStage(false);
            Transform cardTransform = card.transform;
            cardTransform.SetParent(playerHand, true);

            Vector3 cardPosition = Utils.SortPlayerHand(playerHand, cardTransform.gameObject);
            cardTransform.LeanMoveLocal(cardPosition, 0.3f)
                .setEase(LeanTweenType.easeInOutQuad);
            yield return new WaitForSeconds(0.3f);

        }

        yield return null;
    }

    public void SetOpponentSelectedTableCards(CardModel cardModel, int number)
    {
        Transform cardTransform = GetOpponentCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);

        Transform destination = GetOpponentTableCardPositionForIndex(number);
        cardTransform.SetParent(destination, true);

        Utils.SortOpponentHand(opponentHand, null);

        cardTransform.LeanMoveLocal(new Vector3(0, deckCardDistance, 0), 0.3f)
            .setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.3f);
    }

    private Transform GetPlayerTableCardPositionForIndex(int index)
    {
        Transform position;
        switch (index)
        {
            case 0:
                position = playerTableCard1;
                break;
            case 1:
                position = playerTableCard2;
                break;
            case 2:
                position = playerTableCard3;
                break;
            default:
                position = playerTableCard1;
                break;
        }

        return position;
    }

    private Transform GetOpponentTableCardPositionForIndex(int index)
    {
        Transform position;
        switch (index)
        {
            case 0:
                position = opponentTableCard1;
                break;
            case 1:
                position = opponentTableCard2;
                break;
            case 2:
                position = opponentTableCard3;
                break;
            default:
                position = opponentTableCard1;
                break;
        }

        return position;
    }

    private Transform GetOpponentCard()
    {
        return opponentHand.GetChild(opponentHand.childCount - 1);
    }

    public void SetFirstCardFromDeck(CardModel cardModel)
    {
        Transform topDeckCard = GetTopDeckCard();
        topDeckCard.GetComponent<Card>().SetCardModel(cardModel);
        topDeckCard.SetParent(pile, true);
        topDeckCard.LeanMoveLocal(Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotateZ(topDeckCard.gameObject, 0, 0.3f);

    }

    public void OnCardSelected(Card card, bool state)
    {
        if (!state)
        {
            selectedCards.Remove(card);
            return;
        }
        //
        if (selectedCards.Count == 0 || selectedCards[0].GetCardClass() == card.GetCardClass())
        {
            selectedCards.Add(card);
            return;
        }

        foreach (Card selectedCard in selectedCards)
        {
            selectedCard.DisableSelection();
        }
        selectedCards.Clear();
        selectedCards.Add(card);
    }

    public CardModel GetTopDeckCardModel()
    {
        CardModel cardModel = cardModels[0];
        cardModels.RemoveAt(0);
        return cardModel;
    }

    public Transform GetTopDeckCard()
    {
        return deck.GetChild(deck.childCount - 1);
    }
    
    public void PutCardsInPile(Card card, CardLocation cardLocation, bool isStick)
    {
        List<Card> cardsToPlay = new List<Card>();
        if (selectedCards.Count > 0 && selectedCards[0].GetCardClass() == card.GetCardClass())
        {
            cardsToPlay.AddRange(selectedCards);
        }
        else
        {
            cardsToPlay.Add(card);
        }
        //
        foreach (Card cardToPlay in cardsToPlay)
        {
            cardToPlay.DisableSelection();
        }
        selectedCards.Clear();
        StartCoroutine(PlayCards(cardsToPlay, cardLocation, isStick));
    }

    private IEnumerator PlayCards(List<Card> cards, CardLocation cardLocation, bool isStick)
    {
        int cardClass = cards[0].GetCardClass();
        int position = 0;
        bool canPlay = Utils.CanPlayCard(cardClass, GetTopPileCardClass());
        
        foreach (Card card in cards)
        {
            card.ToggleHighlight(false);
            card.ToggleSelection(false);
            card.ToggleRaised(false);
            Vector3 destination = new Vector3(0, deckCardDistance * pile.childCount, 0);
            if (cardLocation == CardLocation.VisibleTable || cardLocation == CardLocation.HiddenTable)
            {
                position = GetTableCardPosition(card);
            }
            card.transform.LeanRotateY(GetNextPileCardRotation(), 0.3f);
            card.transform.SetParent(pile, true);
            card.transform.LeanMoveLocal(destination, 0.3f);
            card.transform.LeanRotateZ(0, 0.3f);
            
            if (cardLocation != CardLocation.Hand)
            {
                playerController.OnPutTableCardInPile(position, cardLocation, card.GetCardClass(), card.GetCardFlavour());

            }
            else
            {
                playerController.OnPutCardInPile(card);
            }

            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(0.1f);

        if (!canPlay)
        {
            PlayerTakePile();
        }

        if (cardClass == 14)
        {
            playerController.OnJokerPlayed();
            yield break;
        }
        
        if (Utils.IsBoom(pile, cardClass) || cardClass == 10)
        {
            playerController.OnClearPileToGraveyard();
            yield break;
        }

        if (!isStick)
        {
            if (cardClass != 8 || cards.Count == 1 || cards.Count == 3)
            {
                playerController.OnTurnFinished();
            }
            else if (ShouldDrawCards())
            {
                StartCoroutine(playerController.DrawMissingCards());
            }
            Utils.SortPlayerHand(playerHand, null);

            yield break;
        }
        
        if (ShouldDrawCards())
        {
            StartCoroutine(playerController.DrawMissingCards());
        }
        else
        {
            Utils.SortPlayerHand(playerHand, null);
        }

    }

    private float GetNextPileCardRotation()
    {
        Transform topPileCard = GetTopPileCard();
        if (topPileCard == null)
        {
            return Random.Range(-30, 30);
        }

        Card card = topPileCard.GetComponent<Card>();
        int offset = Random.Range(card.GetCardClass() == 3 ? 15 : 10, 25);
        return topPileCard.rotation.eulerAngles.y + offset;
    }

    private bool ShouldDrawCards()
    {
        int deckCards = GetDeckCardsCount();
        int cardsInHand = GetPlayerHand().childCount;
        return deckCards > 0 && cardsInHand < 3;
    }

    public IEnumerator ClearPileToGraveyard(bool isMe)
    {

        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            Transform cardTransform = pile.GetChild(i);
            cardTransform.SetParent(graveyard, true);
            Vector3 destination = Vector3.zero;
            destination.y = graveyard.childCount * deckCardDistance;
            cardTransform.LeanMoveLocal(destination, 0.3f);
            cardTransform.LeanRotateZ(180, 0.3f);
            yield return new WaitForSeconds(0.2f);
        }


        if (isMe && ShouldDrawCards())
        {
            StartCoroutine(playerController.DrawMissingCards());
        }
    }
    
    public void OpponentPlayCard(Transform cardTransform)
    {
        Vector3 destination = new Vector3(0, deckCardDistance * pile.childCount, 0);
        cardTransform.LeanRotateY(GetNextPileCardRotation(), 0.3f);
        cardTransform.SetParent(pile, true);
        cardTransform.LeanMoveLocal(destination, 0.3f);
        cardTransform.LeanRotateZ(0, 0.3f);
    }

    public Transform GetOpponentHandCard()
    {
        return opponentHand.GetChild(opponentHand.childCount - 1);
    }

    public Transform GetOpponentTableCard(int position, bool isVisible)
    {
        Transform parent = opponentTableCard1;
        switch (position)
        {
            case 1:
                parent = opponentTableCard1;
                break;
            case 2:
                parent = opponentTableCard2;
                break;
            case 3:
                parent = opponentTableCard3;
                break;

        }

        return parent.GetChild(isVisible ? 1 : 0);
    }

    public IEnumerator OpponentTakePileToHand(bool isJoker)
    {
        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            Transform cardTransform = pile.GetChild(i);
            cardTransform.SetParent(opponentHand, true);
            cardTransform.LeanRotateZ(180, 0.3f);
        }
        Utils.SortOpponentHand(opponentHand, null);

        yield return new WaitForSeconds(0.5f);

        if (currentPlayerTurn == PlayerTurn.Player && isJoker)
        {
            playerController.OnTurnFinished();
        }
    }
    
    public IEnumerator PlayerTakePileToHand(bool isJoker)
    {
        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            Transform cardTransform = pile.GetChild(i);
            cardTransform.SetParent(playerHand, true);
        }
        Utils.SortPlayerHand(playerHand, null);

        yield return new WaitForSeconds(0.5f);

        if (currentPlayerTurn == PlayerTurn.Player && !isJoker)
        {
            playerController.OnTurnFinished();
        }
    }

    private int GetTableCardPosition(Card card)
    {
        Transform parent = card.transform.parent;
        if (parent == playerTableCard1)
        {
            return 1;
        } 
        if (parent == playerTableCard2)
        {
            return 2;
        }

        if (parent == playerTableCard3)
        {
            return 3;
        }

        return 0;
    }

    public int GetDeckCardsCount()
    {
        return deck.childCount;
    }
    
    private int GetAbsoluteTopPileCardClass()
    {
        if (pile.childCount > 0)
        {
            return pile.GetChild(pile.childCount - 1).GetComponent<Card>().GetCardClass();
        }

        return 0;

    }

    private Transform GetTopPileCard()
    {
        if (pile.childCount == 0)
        {
            return null;
        }

        return pile.GetChild(pile.childCount - 1);
    }
    
    public int GetTopPileCardClass()
    {
        if (pile.childCount == 0)
        {
            return 0;
        }

        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            int cardClass = pile.GetChild(i).GetComponent<Card>().GetCardClass();
            if (cardClass != 3)
            {
                return cardClass;
            }
        }
        return 3;

    }
}
