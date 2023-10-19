using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public static class Utils
{
    
    private const float baseHorizontalDistance = 0.45f;
    private const float baseVerticalDistance = 0.02f;

    public static Collection<CardModel> GetAllCardModels()
    {
        Collection<CardModel> list = new Collection<CardModel>();

        for (int i = 1; i <= 13; i++)
        {
            list.Add(new CardModel(i, CardFlavour.Club));
            list.Add(new CardModel(i, CardFlavour.Heart));
            list.Add(new CardModel(i, CardFlavour.Spade));
            list.Add(new CardModel(i, CardFlavour.Diamond));
        }
        list.Add(new CardModel(14, CardFlavour.Joker01));
        list.Add(new CardModel(14, CardFlavour.Joker02));
        
        return list;
    }
    
    public static Vector3 SortOpponentHand(Transform opponentHand, GameObject newCard) 
    {
        if (opponentHand.childCount == 0)
        {
            return Vector3.zero;
        }
        Collection<GameObject> opponentCards = GetExistingCardObjectsInSlot(opponentHand);
        return SortHand(opponentCards, newCard);
    }
    
    public static Vector3 SortPlayerHand(Transform playerHand, GameObject newCard)
    {
        if (playerHand.childCount == 0)
        {
            return Vector3.zero;
        }
        Collection<GameObject> playerCards = GetExistingCardObjectsInSlot(playerHand);
        List<GameObject> orderedList = playerCards.OrderBy(card => card.GetComponent<Card>().GetCardClass()).ToList();
        return SortHand(orderedList, newCard);
    }
    
    private static Collection<GameObject> GetExistingCardObjectsInSlot(Transform slot)
    {
        Collection<GameObject> opponentCards = new Collection<GameObject>();
        for (int i = 0; i < slot.childCount; i++)
        {
            opponentCards.Add(slot.GetChild(i).gameObject);
        }

        return opponentCards;
    }
    
    private static Vector3 SortHand(IList<GameObject> slotCards, GameObject newCard)
    { 
        Vector3 newCardPosition = new Vector3();
        int numberOfCards = slotCards.Count;
        float horizontalDistance = numberOfCards > 24 ? baseHorizontalDistance / 1.3f : baseHorizontalDistance;
        bool evenNumberOfCards = numberOfCards % 2 == 0;
        int middleCardIndex = numberOfCards / 2;
        
        GameObject middleCard = slotCards[middleCardIndex];
        Vector3 middleCardPosition = Vector3.zero;
        middleCardPosition.x = evenNumberOfCards ? horizontalDistance / 2 : 0;
        if (middleCard != newCard)
        {
            middleCard.LeanMoveLocal(middleCardPosition, 0.3f);
            middleCard.LeanRotateY(0, 0.3f);
        }
        else
        {
            newCardPosition = middleCardPosition;
        }
        
        for (int i = 0; i < numberOfCards; i++)
        {
            if (i != middleCardIndex)
            {
                GameObject cardObject = slotCards[i];

                int relativePosition = Mathf.Abs(i - middleCardIndex);
                Vector3 destination = Vector3.zero;
                destination.x = middleCardPosition.x + relativePosition * (i < middleCardIndex ? -horizontalDistance : horizontalDistance);
                destination.y = middleCardPosition.y + relativePosition * (i < middleCardIndex ? -baseVerticalDistance : baseVerticalDistance);
                if (cardObject == newCard)
                {
                    newCardPosition = destination;
                }
                else
                {
                    cardObject.LeanMoveLocal(destination, 0.3f);
                    cardObject.LeanRotateY(0, 0.3f);

                }
            }
        }
        
        return newCardPosition;
    }

    public static bool CanPlayCard(int cardToPlay, int onCard)
    {
        if (onCard == 0 || onCard == 14 || onCard == 2 || onCard == 4)
        {
            return true;
        }
        switch (cardToPlay)
        {
             case 1:
                 return onCard != 7;
             case 2:
                 return true;
             case 3:
                 return true;
             case 4:
                 return onCard > 1 && (onCard <= 4 || onCard == 7);
             case 5: 
                 return onCard > 1 && (onCard <= 5 || onCard == 7);
             case 6: 
                 return onCard > 1 && (onCard <= 6 || onCard == 7);
             case 7:
                 return onCard > 1 && onCard <= 7;
             case 8:
                 return onCard > 1 && onCard <= 8 && onCard != 7;
             case 9:
                 return onCard > 1 && onCard <= 9 && onCard != 7;
             case 10:
                 return onCard != 7;
             case 11:
                 return onCard > 1 && onCard <= 11 && onCard != 7;
             case 12:
                 return onCard > 1 && onCard <= 12 && onCard != 7;
             case 13:
                 return onCard > 1 && onCard <= 13 && onCard != 7;
             case 14:
                 return true;
             default: 
                 return false;
             
        }
    }

    public static Sprite GetSpriteForCardModel(CardModel cardModel)
    {
        Sprite[] cardFronts = GameMaster.Singleton.GetCardFronts();
        if (cardModel.cardFlavour == CardFlavour.Joker01 || cardModel.cardFlavour == CardFlavour.Joker02)
        {
            foreach (Sprite sprite in cardFronts)
            {
                if (sprite.name == cardModel.cardFlavour.ToString())
                {
                    return sprite;
                }
            }
        }
        else
        {
            string cardClass = cardModel.cardClass < 10 ? "0" + cardModel.cardClass : cardModel.cardClass.ToString();
            foreach (Sprite sprite in cardFronts)
            {
                if (sprite.name == cardModel.cardFlavour + cardClass)
                {
                    return sprite;
                }
            }
        }

        return null;
    }

    public static CardFlavour GetCardFlavourForString(String flavour)
    {
        foreach (CardFlavour value in Enum.GetValues(typeof(CardFlavour)))
        {
            if (value.ToString() == flavour)
            {
                return value;
            }
        }

        return CardFlavour.Club;
    }
    
    public static bool IsBoom(Transform pile, int cardClass)
    {
        if (pile.childCount < 4)
        {
            return false;
        }

        List<Card> topFourCards = new List<Card>();
        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            Card pileCard = pile.GetChild(i).GetComponent<Card>();
            if (pileCard.GetCardClass() != cardClass)
            {
                return false;
            }
            topFourCards.Add(pileCard);
            if (topFourCards.Count == 4)
            {
                return true;
            }
        }

        return false;
    }
    

}
