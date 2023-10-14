using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    private GameMaster gameMaster;

    private void Start()
    {
        gameMaster = GameMaster.Singleton;
        gameMaster.SetPlayerController(this);
        if (!IsHost)
        {
            DealDeck_ServerRpc();
        }
    }
    

    public void OnPlayerSelectedTableCards()
    {
        StartCoroutine(OnOpponentSelectionCards());
    }

    private IEnumerator OnOpponentSelectionCards()
    {
        ulong id = GetId();
        for (int i = 0; i < 3; i++)
        {
            Card card = gameMaster.selectedTableCards[i].GetComponent<Card>();
            SetOpponentSelectedTableCards_ServerRpc(GetId(), card.GetCardClass(), card.GetCardFlavour().ToString(), i);

            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(0.3f);
        AddPlayerReady_ServerRpc(id);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DealDeck_ServerRpc()
    {
        gameMaster.ShuffleCards();
        DealDeck_ClientRpc();
    }
    
    [ClientRpc]
    void DealDeck_ClientRpc()
    {
        if (!IsOwner) return;
        Task putCardsInDeck = new Task(gameMaster.PutCardInDeck());
        putCardsInDeck.Finished += delegate
        {
            if (IsHost)
            {
                DealHands_ServerRpc();
            }
        };
    }
    

    [ServerRpc(RequireOwnership = false)]
    void SetOpponentSelectedTableCards_ServerRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        DealOpponentSelectedTableCard_ClientRpc(id, cardClass, cardFlavor, number);
    }
    
    [ClientRpc]
    void DealOpponentSelectedTableCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (id != GetId())
        {
            CardModel cardModel = new CardModel(cardClass, Utils.GetCardFlavourForString(cardFlavor));
            gameMaster.SetOpponentSelectedTableCards(cardModel, number);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DealHands_ServerRpc()
    {
        StartCoroutine(DealHandsToClients());
    }

    [ClientRpc]
    void DealTableCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (id == GetId())
        {
            CardFlavour flavour;
            if (Enum.TryParse(cardFlavor, out flavour))
            {
                gameMaster.DealPlayerTableCard(new CardModel(cardClass, flavour), number);
            }
        }
        else
        {
            gameMaster.DealOpponentTableCard(number);
        }
    }
    
    [ClientRpc]
    void DealSelectionCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (number == 1)
        {
            gameMaster.ToggleSelectCardsDialog(true);
        }
        
        if (id == GetId())
        {
            CardFlavour flavour;
            if (Enum.TryParse(cardFlavor, out flavour))
            {
                gameMaster.DealPlayerSelectionCard(new CardModel(cardClass, flavour), number);
            }
        }
        else
        {
            gameMaster.OpponentDrawCardFromDeck();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void AddPlayerReady_ServerRpc(ulong id)
    {
        AddPlayerReady_ClientRpc(id);
    }
    
    [ClientRpc]
    void AddPlayerReady_ClientRpc(ulong id)
    {
        gameMaster.playersReady++;
        if (!IsHost) return;

        if (gameMaster.playersReady == 1)
        {
            gameMaster.firstPlayerToStart = id;
        }
        if (gameMaster.playersReady == 2)
        {
            SetFirstCardFromDeck_ServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SetPlayerTurn_ServerRpc(ulong playerId)
    {
        SetPlayerTurn_ClientRpc(playerId);
    }

    [ClientRpc]
    void SetPlayerTurn_ClientRpc(ulong playerId)
    {
        bool isMyTurn = GetId() == playerId;
        gameMaster.SetCurrentPlayerTurn(isMyTurn ? GameMaster.PlayerTurn.Player : GameMaster.PlayerTurn.Opponent);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SetFirstCardFromDeck_ServerRpc()
    {
        CardModel cardModel = gameMaster.GetTopDeckCardModel();
        SetFirstCardFromDeck_ClientRpc(cardModel.cardClass, cardModel.cardFlavour.ToString());
    }
    
    [ClientRpc] 
    void SetFirstCardFromDeck_ClientRpc(int cardClass, string cardFlavor)
    {
        gameMaster.SetFirstCardFromDeck(new CardModel(cardClass, Utils.GetCardFlavourForString(cardFlavor)));
        SetPlayerTurn_ServerRpc(gameMaster.firstPlayerToStart);

    }

    [ServerRpc(RequireOwnership = false)]
    void SetOpponentHandCardInPile_ServerRpc(ulong id, int cardClass, string cardFlavor)
    {
        SetOpponentHandCardInPile_ClientRpc(id, cardClass, cardFlavor);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetOpponentVisibleTableCardInPile_ServerRpc(ulong id, int position, string cardLocation, int cardClass, string cardFlavour)
    {
        SetOpponentTableCardInPile_ClientRpc(id, position, cardLocation, cardClass, cardFlavour);
    }

    [ClientRpc]
    void SetOpponentHandCardInPile_ClientRpc(ulong id, int cardClass, string cardFlavour)
    {
        if (id != GetId())
        {
            gameMaster.opponentPlayedTurn = true;
            Transform cardTransform = gameMaster.GetOpponentHandCard();
            cardTransform.GetComponent<Card>().SetCardModel(new CardModel(cardClass, cardFlavour));
            gameMaster.OpponentPlayCard(cardTransform);
        }
    }
    
    [ClientRpc]
    void SetOpponentTableCardInPile_ClientRpc(ulong id, int position, string cardLocation, int cardClass, string cardFlavour)
    {
        if (id != GetId())
        {
            bool isVisible = cardLocation == GameMaster.CardLocation.VisibleTable.ToString();
            gameMaster.opponentPlayedTurn = true;
            Transform cardTransform = gameMaster.GetOpponentTableCard(position, isVisible);
            if (!isVisible)
            {
                Card card = cardTransform.GetComponent<Card>();
                card.SetCardModel(new CardModel(cardClass, Utils.GetCardFlavourForString(cardFlavour)));
            }
            gameMaster.OpponentPlayCard(cardTransform);

        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    void TurnFinished_ServerRpc(ulong id)
    {
        IReadOnlyList<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (ulong clientId in clientIds)
        {
            if (clientId != id)
            {
                SetPlayerTurn_ClientRpc(clientId);
                return;
            }
        }
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    void DrawMissingCards_ServerRpc(ulong id)
    {
        CardModel cardModel = gameMaster.GetTopDeckCardModel();
        DrawMissingCards_ClientRpc(id, cardModel.cardClass, cardModel.cardFlavour.ToString());
    }

    [ClientRpc]
    void DrawMissingCards_ClientRpc(ulong id, int cardClass, string cardFlavour)
    {
        if (id == GetId())
        {
            gameMaster.PlayerDrawMissingCardsFromDeck(new CardModel(cardClass, cardFlavour));
        }
        else
        {
            gameMaster.OpponentDrawCardFromDeck();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ClearPileToGraveyard_ServerRpc(ulong id)
    {
        ClearPileToGraveyard_ClientRpc(id);
    }

    [ClientRpc]
    void ClearPileToGraveyard_ClientRpc(ulong id)
    {
        StartCoroutine(gameMaster.ClearPileToGraveyard(id == GetId()));
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerTakePileToHand_ServerRpc(ulong playedBy, bool isJoker)
    {
        PlayerTakePileToHand_ClientRpc(playedBy, isJoker);
    }

    [ClientRpc]
    void PlayerTakePileToHand_ClientRpc(ulong playedBy, bool isJoker)
    {
        bool playedByMe = playedBy == GetId();
        if ((playedByMe && isJoker) || (!playedByMe && !isJoker))
        {
            StartCoroutine(gameMaster.OpponentTakePileToHand(isJoker));
        }
        else
        {
            StartCoroutine(gameMaster.PlayerTakePileToHand(isJoker));
        }
    }
    
    public void OnPutCardInPile(Card card)
    {
        SetOpponentHandCardInPile_ServerRpc(GetId(), card.GetCardClass(), card.GetCardFlavour().ToString());
    }

    public void OnPutTableCardInPile(int position, GameMaster.CardLocation cardLocation, int cardClass, CardFlavour flavour)
    {
        SetOpponentVisibleTableCardInPile_ServerRpc(GetId(), position, cardLocation.ToString(), cardClass, flavour.ToString());
    }

    public void OnJokerPlayed()
    {
        PlayerTakePileToHand_ServerRpc(GetId(), true);
    }

    public void OnTakePile()
    {
        PlayerTakePileToHand_ServerRpc(GetId(), false);

    }


    public void OnTurnFinished()
    {
        ulong id = GetId();
        Task drawMissingCards = new Task(DrawMissingCards(), false);
        drawMissingCards.Finished += delegate
        {
            TurnFinished_ServerRpc(id);
        };
        drawMissingCards.Start();

    }

    public IEnumerator DrawMissingCards()
    {
        ulong id = GetId();
        int deckCards = gameMaster.GetDeckCardsCount();
        int cardsInHand = gameMaster.GetPlayerHand().childCount;
        int cardsToDraw = (deckCards > 0 && cardsInHand < 3) ? Mathf.Min(deckCards, 3 - cardsInHand) : 0;
        if (cardsToDraw > 0)
        {
            for (int i = 0; i < cardsToDraw; i++)
            {
                DrawMissingCards_ServerRpc(id);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnClearPileToGraveyard()
    {
        ClearPileToGraveyard_ServerRpc(GetId());
    }

    public ulong GetId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    private IEnumerator DealHandsToClients()
    {
        IReadOnlyList<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealTableCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 1);
            yield return new WaitForSeconds(0.3f);
        }

        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealTableCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 2);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealTableCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 3);
            yield return new WaitForSeconds(0.3f);
        }
        
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 1);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 2);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 3);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 4);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 5);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 6);
            yield return new WaitForSeconds(0.3f);
        }
        yield return null;
    }
}
