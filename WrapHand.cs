/// <copyright file="MultiplayerMenu.cs" company="BrokenMyth Studios, LLC"> 
/// Copyright (c) 2015 All Rights Reserved 
/// </copyright> 
/// <author>Slater de Mont</author> 
/// <date>4/20/2015 10:30 AM</date>


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles card placement, movement, states, and outlines for cards displayed
/// in the HUD's hand
/// </summary>
public class WrapHand : MonoBehaviour, IDraggable {

	//list of cards and gameobject
	public List<Card> cardsList;
	public List<GameObject> cards;

	//scrolling variables
	private bool isDecelerating = false;
	private float speed;
	private bool scrollWasDisabled = false;

	//card placement and visibility variables
	private float startingPointNumber;
	private float cardsLength = 0f;
	//private float endNumber = 125f;

	//slider for display card order variables
	public GameObject slider;
	private Slider sliderValue;

	//variables for displaying number of cards in the hand
	public GameObject deckView;
	public GameObject cardBack;

	//reference to hudcontroller
	private HUDController hudController;

	//used to check hand cards owner
	private Card firstCard;


	/// <summary>
	/// Populates the hand by getting the selected player's hand
	/// </summary>
	public void PopulateHand () {
		if(hudController == null){
			hudController = this.GetComponentInParent<HUDController> ();
		}

		firstCard = null;

		cardsList = hudController.selectedPlayer.Hand;
		//Debug.Log (hudController.selectedPlayer);
		cards.Clear ();
		for(int i = 0; i < cardsList.Count; i++){
			cards.Insert(i, cardsList[i].gameObject);
		}

		//If the hudController isn't displaying another player
		if (hudController.otherPlayerShown == false) {

			//set first card
			if (cards != null && cards.Count > 0) {
				firstCard = cards [0].GetComponent<Card> ();
			}

			ResizeCards ();
		}
		//Sets the card back number for the other player
		else {
			SetHandBack (hudController.selectedPlayer.Hand.Count);
		}
	}


	/// <summary>
	/// Checks if the cards displayed belong to the HUDController's
	/// selected player
	/// </summary>
	/// <returns><c>true</c>, if cards belong to selectedplayer.</returns>
	/// <returns><c>false</c> otherwise.</returns>
	public bool CheckCardsDisplayed(){
		//preventing null reference
		if(hudController == null){
			hudController = this.GetComponentInParent<HUDController> ();
		}

		if (hudController.selectedPlayer != null) {
			if (cards != null && cards.Count > 0) {
				if(firstCard == null){
					firstCard = cards [0].GetComponent<Card> ();
				}

				if(firstCard.cardOwner != hudController.selectedPlayer){
					return false;
				}
			}
		}
		return true;
	}


	/// <summary>
	/// Returns the cards to owner.
	/// </summary>
	public void ReturnCardsToOwner(){
		if (cards != null && cards.Count > 0) {
			foreach (GameObject cardGO in cards) {
				Card card = cardGO.GetComponent<Card> ();
			
				// Make sure the card hasn't been moved to the deck already
				if (card.cardOwner.playerDeck.DiscardPile.Contains (card) == false && 
					card.cardOwner.playerDeck.DrawPile.Contains (card) == false) {
					cardGO.transform.SetParent (card.cardOwner.hand.transform);
					cardGO.SetActive (false);
				}
			}
		}
	}


	
	/// <summary>
	/// Calls scrolling and dragging functions on update to animate movement
	/// </summary>
	void FixedUpdate () {
		// Normal dragging is handled by OnDragging, check to see if dragging has 
		// ended and we should be decelerating to the final resting location
		if (isDecelerating) {
			DecelerateCards();
		}
	}

	/// <summary>
	/// Adds the card to hand.
	/// </summary>
	/// <param name="card">Card.</param>
	public void AddCardToHand(GameObject card){
		if (cards.Contains (card) == false){
			cards.Add (card);
			cardsList.Add (card.GetComponent<Card> ());

			//If the hudController isn't displaying another player
			if (hudController.otherPlayerShown == false) {
				ResizeCards ();
			}
			//Sets the card back number for the other player
			else {
				SetHandBack (hudController.selectedPlayer.Hand.Count);
			}
		}
	}

	/// <summary>
	/// Creates the hand card and sets the number to display in the corner
	/// </summary>
	/// <param name="numberCards">Number of cards in hand.</param>
	public void SetHandBack(int numberCards){
		//checking if the scroll was disabled before
		if(IsDraggable == true){
			IsDraggable = false;
			scrollWasDisabled = true;
		}
		Destroy(GameObject.Find ("HandCard"));
		deckView = Instantiate(cardBack) as GameObject;
		deckView.gameObject.SetActive (true);
		
		deckView.transform.SetParent(this.transform);
		deckView.transform.localPosition = new Vector3(0, -6, -1f);
		
		deckView.transform.localRotation = new Quaternion(0, 0, 0, 0);
		deckView.transform.localScale = new Vector3(40, 40, 40);
		deckView.layer = 9;
		foreach (Transform child in deckView.transform) {
			child.gameObject.layer = 9;
		}
		deckView.name = "HandCard";
		deckView.GetComponent<DeckCard> ().SetNumber (numberCards);
	}

	/// <summary>
	/// Creates the draw pile card and sets the number to display in the corner
	/// </summary>
	public void DestroyHandBack(){
		//checking if the scroll was disabled before
		if(scrollWasDisabled == true){
			IsDraggable = true;
		}
		scrollWasDisabled = false;
		Destroy(GameObject.Find ("HandCard"));
	}

	/// <summary>
	/// Removes the card from hand
	/// </summary>
	/// <param name="card">Card.</param>
	public void RemoveCardFromHand(GameObject card){

		cards.Remove (card);
		cardsList.Remove (card.GetComponent<Card> ());

		//If the hudController isn't displaying another player
		if (hudController.otherPlayerShown == false) {
			ResizeCards ();
		}
		//Sets the card back number for the other player
		else {
			//called before the card is removed from the hand
			int handCount = hudController.selectedPlayer.Hand.Count;
			//if the count is greater than 0, subtract 1
			if(handCount > 0){
				handCount -= 1;
			}
			SetHandBack (handCount);
		}
	}
	
	/// <summary>
	/// Displays the discard deck
	/// </summary>
	/// <param name="discardCards">Discard cards.</param>
	public void DisplayDiscard(List<Card> discardCards){
		if(hudController == null){
			hudController = this.GetComponentInParent<HUDController> ();
		}
		hudController.centerDisplay.ShowDiscard (discardCards, hudController.selectedPlayer.playerDeck.DrawPile.Count);
	}

	/// <summary>
	/// Makes cards the correct size
	/// </summary>
	public void ResizeCards(){
		//this.GetComponent<Image> ().enabled = false;
		float length = 80;
		if(cards.Count < 5){
			if(sliderValue == null){
				sliderValue = slider.GetComponent<Slider>();
			}
			sliderValue.value = 0;
			//Debug.Log (cards.Count);
			IsDraggable = false;
			float startingNumber = -120;
			for(int i =0; i< cards.Count; i++){
				cards[i].gameObject.SetActive (true);
				cards[i].transform.SetParent(this.transform);
				cards[i].transform.localRotation = new Quaternion(0, 0, 0, 0);
				cards[i].transform.localScale = new Vector3(40, 40, 40);
				cards[i].GetComponent<Card>().cardIsDisplayed = false;
				//sets position so only 7 cards are shown at a time
				cards[i].transform.localPosition = new Vector3(startingNumber, -6, -1f);
				startingNumber += length;
			}
		}else{
			//scales the cards
			//Debug.Log (cards.Count);
			//float length = Screen.width / 4.27f;
			IsDraggable = true;
			cardsLength = length;
			//calculate the starting index
			if((cards.Count % 2) == 1){
				startingPointNumber = ((((cards.Count -1) / 2.0f)));
			}
			else{
				startingPointNumber = (((cards.Count) / 2.0f));
			}
			//Debug.Log (startingPointNumber);
			float startingNumber = (-1 * startingPointNumber);
			float startingPoint = length * startingNumber;
			//Debug.Log (startingPoint);
			for(int i =0; i< cards.Count; i++){
				cards[i].gameObject.SetActive (true);
				cards[i].transform.SetParent(this.transform);
				cards[i].transform.localRotation = new Quaternion(0, 0, 0, 0);
				cards[i].transform.localScale = new Vector3(40, 40, 40);
				cards[i].GetComponent<Card>().cardIsDisplayed = false;
				//sets position so only 7 cards are shown at a time
				cards[i].transform.localPosition = new Vector3(startingPoint, -6, -1f);
				startingNumber += 1;
				startingPoint = length * startingNumber;

				if(cards[i].transform.localPosition.x > 203f || cards[i].transform.localPosition.x < -203f){
					cards[i].SetActive(false);
				}else{
					cards[i].SetActive(true);
				}
			}
			CalculatePosition();
		}
	}


	/// <summary>
	/// Views the deck window or turns it off
	/// </summary>
	public void TurnOnOffDeck(){
		if(hudController == null){
			hudController = this.GetComponentInParent<HUDController> ();
		}
		DisplayDiscard(hudController.selectedPlayer.playerDeck.DiscardPile);
	}


	/// <summary>
	/// Calculates the position of the slider
	/// </summary>
	public void CalculatePosition(){
		//gets the position of the first card
		float cardPos = cards [0].transform.localPosition.x;

		//gets the farthest left postion the cards can be in
		float minSpot = (cardsLength * -(cards.Count + 1) * .5f);

		//gets the total distance form the max to the min position
		float distance = (cardsLength * (cards.Count + 1));

		//distance traveled is equal to the first card's position minus the minimum position
		float distanceTraveled = cardPos - minSpot;

		//percent is then the distanceTraveled devided by the total distance
		float percent = (distanceTraveled / distance);

		if(sliderValue == null){
			sliderValue = slider.GetComponent<Slider>();
		}

		//assign the slider's value
		sliderValue.value = percent;
	}
	
	/// <summary>
	/// Slows down card movement.  Stop moving after the |speed| < 1.
	/// </summary>
	private void DecelerateCards() {
		// If we're not decelerating, exit
		if (!isDecelerating) return;

		// Slow down movement and set the distance to move
		speed *= .9f;

		// Move the cards
		MoveCards(speed);

		// If we've reached our decelerated threshhold, stop moving the cards
		if (speed < 1 && speed > -1) {
			isDecelerating = false;
			speed = 0;
		}
	}

	/// <summary>
	/// Move all cards the horizontal distance and direction specified.
	/// </summary>
	/// <param name="distance">The distance and direction which the card is to be moved</param>
	private void MoveCards(float distance) {
		// Go over each card and, if it can be moved, update its position based on 
		// the distance and direction which it is to be moved
		foreach( GameObject cardObj in cards ) {
			// The Card Component
			Card card = cardObj.GetComponent<Card>();
			// Can this card be moved?
			bool isCardMovable = IsCardMovable(card);

			// If the card is movable, update its position
			if (isCardMovable == true) {

				//move the card along the x axis by adding the distance to it's current position
				cardObj.transform.localPosition = new Vector3((cardObj.transform.localPosition.x + distance), -6, -1f);

				//checks to see if the card needs to wrap to the other side
				CheckCardPosition(cardObj);
			
				// Show/Hide the card based on whether or not it is inside our outside the visible bounds
				if (cardObj.transform.localPosition.x > 203f || cardObj.transform.localPosition.x < -203f) {
					//if the card is on, turn it off
					if(cardObj.activeSelf == true){
						cardObj.SetActive(false);
					}
				}
				else {
					//if the card is off, turn it on
					if(cardObj.activeSelf == false){
						cardObj.SetActive(true);
					}
				}
			}
		}

		// Update scrollbar arrow
		CalculatePosition();
	}


	/// <summary>
	/// Checks the card position.
	/// </summary>
	/// <param name="card">Card.</param>
	private void CheckCardPosition(GameObject card) {
		//If the card is over the threshold in the negative direction
		if (card.transform.localPosition.x < (cardsLength * -(cards.Count + 1) * .5f)) {
			float x = card.transform.localPosition.x + (cardsLength * cards.Count);
			card.transform.localPosition = new Vector3(x, -6, -1f);
		}
		
		//If the card is over in the postive position
		if (card.transform.localPosition.x > (cardsLength * (cards.Count + 1) * .5f)) {
			float x = card.transform.localPosition.x - (cardsLength * cards.Count);
			card.transform.localPosition = new Vector3(x, -6, -1f);
		}
	}


	/// <summary>
	/// Check to see if cards are in the Invasion window or other Center Display 
	/// screen.  If so, prevent those from being scrolled/moved while moving the 
	/// Hand.
	/// </summary>
	/// <param name="card">Card to check to see if it can be moved</param>
	/// <returns><c>True</c> if the card can be moved, <c>False</c> otherwise</returns>
	private bool IsCardMovable(Card card) {
		if(hudController == null){
			hudController = this.GetComponentInParent<HUDController> ();
		}
		bool cardCanDrag = false;

		// Center Display scroll area for invasion DisplayCardsInCenter
		DisplayCardsInCenter invadeDisplayScroll = 
			hudController.invadeCenterDisplay;

		//Center display scrol area for enhancments, garrison, and DiscardDraw
		DisplayCardsInCenter centerDisplayScroll = 
			hudController.centerDisplay;
		

		//checking if the invasion display is open and if it doesn't contain the card, then the card can drag
		if(invadeDisplayScroll.isInvadeTurnedOn == true
		   && invadeDisplayScroll.Cards.Contains(card) == false) {
			cardCanDrag = true;
		}
		//if the invasion window isn't open, then the card can drag
		else if(invadeDisplayScroll.isInvadeTurnedOn == false){
			cardCanDrag = true;
		}
		
		if(card.Selected == true && invadeDisplayScroll.isInvadeTurnedOn == true
		   && invadeDisplayScroll.Cards.Contains(card) == false){
			cardCanDrag = true;
		}
		else if(card.Selected == true && invadeDisplayScroll.isInvadeTurnedOn == true
		        && invadeDisplayScroll.Cards.Contains(card) == true){
			cardCanDrag = false;
		}
		
		if(centerDisplayScroll.isTurnedOn == true && centerDisplayScroll.Cards.Contains(card) == true){
			cardCanDrag = false;
		}



		// Return whether or not the card can be dragged
		return cardCanDrag;
	}

	//TODO not neccesary at the moment but might be wanted later
	/// <summary>
	/// Snaps the cards to the nearest correct position
	/// </summary>
	/// <param name="card">Card.</param>
	/// <param name="startNumber">Start number.</param>
	private void SnapCards(int card, float startNumber){

		int closestIndex = card;

		float startingNumber = startNumber;
		bool sortBack = false;
		if(startingNumber < 0){
			sortBack = true;
		}
		for(int i =0; i< cards.Count; i++){

			cards[closestIndex].transform.localPosition = new Vector3((cardsLength * startingNumber), -6, -1f);

			if(sortBack == true){

				if((closestIndex + 1) >= cards.Count){
					closestIndex = 0;
				}else{
					closestIndex = closestIndex + 1;
				}
				startingNumber += 1;
			}else{
				if((closestIndex - 1) < 0){
					closestIndex = cards.Count -1;
				}else{
					closestIndex = closestIndex - 1;
				}
				startingNumber -= 1;
			}
		}
	}

	/// <summary>
	/// Instantly stops the card drag and deceleration.
	/// Used when card is viewed full size or dragged
	/// </summary>
	public void InstantStopDrag(){
		isDecelerating = false;
		speed = 0;
	}


#region IDraggable
	/// <summary>
	/// Gets or sets a value indicating whether this instance is draggable.
	/// </summary>
	public bool IsDraggable {
		get;
		set;
	}

	/// <summary>
	/// Processes the start of a dragging event by setting the current position, 
	/// based on the dragInfo object passed in.
	/// </summary>
	/// <param name="dragInfo">Information pertatining to the drag event</param>
	public void OnDraggingStart(DragInfo dragInfo) {
		isDecelerating = false;
	}

	/// <summary>
	/// Process the drag event and move the cards based on the event ifo.
	/// </summary>
	/// <param name="dragInfo">Information pertatining to the drag event</param>
	public void OnDragging(DragInfo dragInfo) {
		speed = dragInfo.delta.x;
		MoveCards(dragInfo.delta.x);
	}

	/// <summary>
	/// Process the drag end event and start decelerating any movement.
	/// </summary>
	/// <param name="dragInfo">Information pertatining to the drag event</param>
	public void OnDraggingEnd(DragInfo dragInfo) {
		// If the speed is greater than one unit per update, enable deceleration
		if ( speed > 1 || speed < -1 ) {
			isDecelerating = true;
		}
		// Othwerise zero out the speed
		else {
			speed = 0;
		}
	}

	/// <summary>
	/// Orthaginal direction the object is expected to be dragged.
	/// </summary>
	/// <returns><c>DragDirection.Horizontal</c></returns>
	public DragDirection ExpectedDragDirection() {
		return DragDirection.Horizontal;
	}
#endregion
}
