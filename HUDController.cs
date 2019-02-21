/// <copyright file="HUDController.cs" company="BrokenMyth Studios, LLC"> 
/// Copyright (c) 2015 All Rights Reserved 
/// </copyright> 
/// <author>Slater de Mont</author> 
/// <date>5/5/2015 10:30 AM</date>
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Holds and displays all values related to displaying cards, player variables, and Card Center Display
/// notifications. Displays current player or shows selected player
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class HUDController : Photon.MonoBehaviour {

	#region Variables
	//player who's turn it currently is
	[HideInInspector]
	public Player currentPlayer;
	[HideInInspector]
	public Player[] players;
	//The number of players in the game
	private int playerCount;

	public PlayerInfoManager playerInfoManager;
	public GameObject hand;
	public GameObject surge;
	
	[HideInInspector]
	public CentralZone centralZone;
	[HideInInspector]
	public CentralZoneRotation centralZoneRotation;

	//histograns
	public SizeHistogram currentSizeHistogram;
	public SizeHistogram pastSizeHistogram;
	
	//Energy surge token
	[HideInInspector]
	public EnergySurgeToken energySurge;

	//The player displayed on the HUD
	[HideInInspector]
	public Player selectedPlayer;

	// The warzone and wrapHand components of the HUD
	public WarZone warZone;
	public WrapHand wrapHand;

	//Variable to hold the center display object
	public DisplayCardsInCenter centerDisplay;
	public GameObject HistoryObject;
	public DisplayCardsInCenter invadeCenterDisplay;

	//Round and Sector objects and text
	public Text roundText;
	public Text sectorText;

	//The canvas that holds the dashboard
	[HideInInspector]
	public GameObject HUDCanvas;

	private bool isHandSet = false;

	//Buttons on HUD that are used for game controls
	public Button endTurnButton;
	public Button undoButton;
	public Button passButton;
	public Toggle deckToggle;
	//TA_Edit public GameObject deckButton;
	public Button worldButton;
	public Button unitButton;
	private Button helpButton;

	[HideInInspector]
	private bool isHelpOff = true;

	// Checks if the game is changing rounds
	private bool isHistogramSet = false;
	[HideInInspector]
	public bool otherPlayerShown;

	//Boolean to determine if the HUD has been initialized
	public bool IsInitialized { get; private set; }

	//The card that was dragged onto a central zone planet to start an invasion
	[HideInInspector]
	public Card draggedCard;
	
	//array of roman numeral to be displayed in the sector and round
	private string[] RomanNumerals;

	//Can the user select a card
	[HideInInspector]
	public bool cantSelectCard = false;

	//Are the buttons currently off
	[HideInInspector]
	private bool buttonsOff = false;

	//animation materials
	[HideInInspector]
	public Material actionGradient;
	[HideInInspector]
	public Material groundMaterial;
	[HideInInspector]
	public Material fleetMaterial;
	[HideInInspector]
	public Material empireMaterial;

	//stores previous values to only change text when values change
	private int prevAction = -1;
	private int prevEnergy = -1;
	private int prevEmpire = -1;
	private int prevFleet = -1;
	private int prevGround = -1;
	private string prevName = "";
	private string prevPhase = "";
	private Player prevPlayer = null;
	
	//current gamephase
	[HideInInspector]
	public GamePhase currentPhase;

	// Manager responsible for handling visual notifications and messaging
	[HideInInspector] public NotificationManager notificationManager;
	
	// HUD UI Handles
	public Text empirePointsText;
	public Text fleetPointsText;
	public Text groundPointsText;
	public Text actionPointsText;
	public Text energyPointsText;
	public Text nameText;
	public Text phaseText;
	public Image factionImage;

	//is network game
	private bool isNetworkGame = false;

	public HUD_VFX vfx;
	#endregion

	#region Unity Methods
	/// <summary>
	/// Assigning the rules engine if it is not already created
	/// </summary>
	public void Awake() {
		//populating the array of roman numeral
		RomanNumerals = new string[15];
		RomanNumerals [0] = "I";
		RomanNumerals [1] = "II";
		RomanNumerals [2] = "III";
		RomanNumerals [3] = "IV";
		RomanNumerals [4] = "V";
		RomanNumerals [5] = "VI";
		RomanNumerals [6] = "VII";
		RomanNumerals [7] = "VIII";
		RomanNumerals [8] = "IX";
		RomanNumerals [9] = "X";
		RomanNumerals [10] = "XI";
		RomanNumerals [11] = "XII";
		RomanNumerals [12] = "XIII";
		RomanNumerals [13] = "XIV";
		RomanNumerals [14] = "XV";
	}
	
	/// <summary>
	/// Used for Initializing the variables in HUDController.
	/// Called by RulesEngine after the players have finished being created
	/// </summary>
	public void Start() {
		// Obtain a reference to the Notification Manager
		notificationManager = GameObject.FindObjectOfType<NotificationManager>();
		
		//initialize energy surge token
		energySurge = surge.GetComponent<EnergySurgeToken> ();

		//Assigning HUD canvas
		HUDCanvas = transform.FindChild ("HUD Canvas").gameObject;

		// Obtain a reference to the Central Zone
		centralZone = GameObject.FindObjectOfType<CentralZone>();
		centralZoneRotation = centralZone.GetComponent<CentralZoneRotation>();
		
		// Assigning the buttons and toggle
		endTurnButton = transform.FindChild ("HUD Canvas/Dashboard/Actions & Turn Control/EndTurn").GetComponent<Button>();
		passButton = transform.FindChild ("HUD Canvas/Dashboard/Actions & Turn Control/Pass").GetComponent<Button>();
		undoButton = transform.FindChild ("HUD Canvas/Dashboard/Actions & Turn Control/Undo").GetComponent<Button> ();
		deckToggle = transform.FindChild ("HUD Canvas/Dashboard/Actions & Turn Control/Deck").GetComponent<Toggle> ();
		
		// TA_Edit deckButton = transform.FindChild ("HUD Canvas/Dashboard/Actions & Turn Control/Deck_TA").gameObject;
		worldButton = transform.FindChild ("HUD Canvas/Dashboard/Status Values/WorldButton").GetComponent<Button>();
		unitButton = transform.FindChild ("HUD Canvas/Dashboard/Status Values/UnitButton").GetComponent<Button> ();
		helpButton = transform.FindChild ("HUD Canvas/Dashboard/Help Button").GetComponent<Button> ();
		actionGradient = transform.FindChild ("HUD Canvas/Dashboard/Status Values/ActionGradient").GetComponent<Image> ().material;
		groundMaterial = transform.FindChild ("HUD Canvas/Dashboard/Status Values/GroundLogo").GetComponent<Image> ().material;
		fleetMaterial = transform.FindChild ("HUD Canvas/Dashboard/Status Values/FleetLogo").GetComponent<Image> ().material;
		empireMaterial = transform.FindChild ("HUD Canvas/Dashboard/Status Values/EmpireLogo").GetComponent<Image> ().material;
		
		wrapHand = hand.GetComponent<WrapHand>();

		//display round and sector text
		roundText.text = RomanNumerals [0];
		sectorText.text = RomanNumerals [0];

	}
	
	/// <summary>
	/// Used for Initializing the variables in HUDController.
	/// Called by RulesEngine after the players have finished being created
	/// </summary>
	[PunRPC]
	public void Initialize ()
	{
		//getting array of players from engine then creating playercards
		players = FindObjectsOfType<Player>();
		players = players.OrderBy(p => p.Order).ToArray();
		playerCount = players.Length;
		playerInfoManager.CreatePlayerCard(players);

		//checking if the game is network
		isNetworkGame = CheckNetworkGame();

		// Initialize the curernt player
		currentPlayer = GetCurrentPlayer();

		//in hotseat game, currentplayer is used as selected player unless viewing another player
		SetSelectedPlayerBasedOnGameType();

		//setting player's HUD
		SetActivePlayer();

		wrapHand.PopulateHand();
		GetCurrentPlayer().IsSwitching = false;
		GetCurrentPlayer().DisplayRounds = false;

		//populate histograms size and position for current and past sections
		currentSizeHistogram.SizeRect(playerCount);
		pastSizeHistogram.SizeRect(playerCount);

		//creates current histograms
		//TODO: Update how the Histogram generates the spots so the logic is more 
		// clearly defined and can be refactored
		//REFACTOR FIX
		foreach (Player player in players)
		{
			currentSizeHistogram.AddHistogram(player.Order + 1);
		}
		//sets the histograms to the current player
		currentSizeHistogram.SetCurrent(players[0].Order + 1);

		//the HUD is initialized
		IsInitialized = true;
	}

	/// <summary>
	/// Set's the default selected player based on whether or not this is a
	/// networked game.
	/// </summary>
	/// <remarks>For a networked game, the default player is the local player.
	/// For a local game, Single or HotSeat, the default selected player is the
	/// current player.</remarks>
	private void SetSelectedPlayerBasedOnGameType()
	{
		if (isNetworkGame == false)
		{
			selectedPlayer = currentPlayer;
		}
		// In Networked games, the displayed HUD player is the local human player
		else
		{
			Player[] myPlayer = players.Where(p => p.photonView.isMine && p is LocalPlayer).ToArray();
			// Ensure a player was found.  Count should be 1, but we only care if its 0
			if (myPlayer == null || myPlayer.Length == 0)
			{
				throw new UnityException("No player exists owned by this connection");
			}
			selectedPlayer = myPlayer[0];
		}
	}

	/// <summary>
	/// Shows selected player and sets current player to engine's current player
	/// </summary>
	void FixedUpdate () {

		//if Initialize has been called
		if(IsInitialized == true){
			//TODO: Remove once UNDO functionality has been implemented
			undoButton.interactable = false;
			//vfx.ToggleInteractable(undoButton, 0);

			// Make sure Current and Selected players are set correctly
			currentPlayer = GetCurrentPlayer();
			//if the player isn't viewing other player, set selected player to current player

			//update displayed player values
			SetActivePlayer ();

			// Disable the Pass button if it is not available, i.e.:
			// Not in the Action Phase, Action has already been performed, or
			// Other player is being viewed
			if (currentPhase != GamePhase.Action
				|| selectedPlayer.CurrentActionPhase != GamePhase.None
				|| otherPlayerShown == true)
			{
				passButton.interactable = false;
			}
			else {
				// IF OFFLINE: ENABLE PASS & DISABLE END TURN IF CURRENT = SELECTED
				// IF NETWORK: ENABLE PASS & DISABLE END TURN IF CURRENT = SELECTED
				//		AND CURRENT IS MY PLAYER
				if( (currentPlayer == selectedPlayer) 
					&& (PhotonNetwork.offlineMode || currentPlayer.photonView.isMine) 
					&& buttonsOff == false )
				{
					//Turn on Pass button and turn off End Turn buttonn
					passButton.interactable = true;
					endTurnButton.interactable = false;
				}
			}
		}
	}
	#endregion

	#region UI Controls
	/// <summary>
	/// Opens the invade screen. Displays the card that was dragged and the world card in
	/// Invade screen
	/// </summary>
	/// <param name="CardToDisplay">Card that is to be invaded</param>
	public void OpenInvadeScreen(Card CardToDisplay){
		//if the current player is active on the HUD and the gamephase is action
		if(selectedPlayer == GetCurrentPlayer() && currentPhase == GamePhase.Action){
			//assign the invadedCard
			Card invadedCard = CardToDisplay;

			//if this card is different from the centralzone selectedcard,
			//assign it
			if(centralZone.SelectedCard != CardToDisplay){
				centralZone.SelectedCard = CardToDisplay;
			}
			//if the invadedCard isn't null and it is a world type card
			if(invadedCard != null && invadedCard.Type.Contains ("World") == true){
				//set the invaded world as the centralZone selected card
				invadeCenterDisplay.DisplayValues (invadedCard);
				//if the dragged card hasn't been selected, select it
				if(draggedCard.Selected == false){
					draggedCard.OnShortTap();
				}
				//create invasion display
				invadeCenterDisplay.AddCards (draggedCard);
				
				//go through selected invasion cards and if they are a unit and not the dragged card,
				//add them to the invasion window
				for(int i = 0; i< selectedPlayer.warZone.SelectedInvadeCards.Count; i++){
					if(draggedCard != selectedPlayer.warZone.SelectedInvadeCards[i]){
						if(selectedPlayer.warZone.SelectedInvadeCards[i].Type.Contains ("World") == false){
							invadeCenterDisplay.AddCards (selectedPlayer.warZone.SelectedInvadeCards[i]);
						}
					}
				}
			}else{
				//reset the dragged card
				draggedCard = null;
			}
		}else{
			//reset the dragged card
			draggedCard = null;
		}
	}

	/// <summary>
	/// Called by player cards to switch player. Uses the player card's stored index
	/// as the index of the selected player
	/// </summary>
	/// <param name="player">Player.</param>
	public void ViewOtherPlayer(int player) {
		//if the center display is turned on, turn it off
		if (centerDisplay.isTurnedOn == true || centerDisplay.isDiscardTurnedOn == true) {
			//turn the button off if discard is on
			if (centerDisplay.isDiscardTurnedOn == true) {
				deckToggle.isOn = false;
			}
			//else call hide
			else {
				centerDisplay.Hide();
			}
		}

		//checking if current player is selected player
		//hand is hidden and buttons disabled if not
		if (isNetworkGame == false) {
			//if the player is current player
			if (players[player] == currentPlayer) {
				otherPlayerShown = false;
				wrapHand.DestroyHandBack();
				EnableButtons();
			} else {
				//get the number of cards in the player's hand
				wrapHand.SetHandBack(players[player].Hand.Count);
				otherPlayerShown = true;
				DisableButtons();
			}
		} else {
			if (players[player] == CurrentDevicePlayer) {
				wrapHand.DestroyHandBack();
				otherPlayerShown = false;
				EnableButtons();
			} else {
				wrapHand.SetHandBack(players[player].Hand.Count);
				otherPlayerShown = true;
				DisableButtons();
			}
		}

		// Return player's point cards
		for (int i = 0; i < warZone.PointCards.Count; i++) {
			warZone.PointCards[i].transform.SetParent(selectedPlayer.warZone.transform);
			if (isNetworkGame == true) {
				//move the cards of the AI player over
				if (selectedPlayer.GetType() == typeof(AIPlayer)) {
					float xpos = warZone.PointCards[i].gameObject.transform.localPosition.x;
					float ypos = warZone.PointCards[i].gameObject.transform.localPosition.y;
					warZone.PointCards[i].gameObject.transform.localPosition = new Vector3(xpos + 100f, ypos + 100f, 0f);
				} else {
					warZone.PointCards[i].gameObject.SetActive(false);
				}
			} else {
				warZone.PointCards[i].gameObject.SetActive(false);
			}
		}
		// Return player's Deployed Units
		for (int i = 0; i < warZone.DeployedUnits.Count; i++) {
			warZone.DeployedUnits[i].transform.SetParent(selectedPlayer.warZone.transform);
			if (isNetworkGame == true) {
				//move the cards of the AI player over
				if (selectedPlayer.GetType() == typeof(AIPlayer)) {
					float xpos = warZone.DeployedUnits[i].gameObject.transform.localPosition.x;
					float ypos = warZone.DeployedUnits[i].gameObject.transform.localPosition.y;
					warZone.DeployedUnits[i].gameObject.transform.localPosition = new Vector3(xpos + 100f, ypos + 100f, 0f);
				} else {
					warZone.DeployedUnits[i].gameObject.SetActive(false);
				}
			} else {
				warZone.DeployedUnits[i].gameObject.SetActive(false);
			}
		}
		//returns hand cards to player
		for (int i = 0; i < wrapHand.cards.Count; i++) {
			wrapHand.cards[i].transform.SetParent(selectedPlayer.hand.transform);
			wrapHand.cards[i].SetActive(false);
		}
		//clear the warzone and reset the spot holders
		warZone.DeployedUnits.Clear();
		warZone.PointCards.Clear();
		warZone.ResetSpots();
		isHandSet = false;

		//sets the selected player
		selectedPlayer = players[player];

		//displays energy tokens if selected player has any
		energySurge.tokenCount = selectedPlayer.energyTokens;
		if (selectedPlayer.energyTokens > 0) {
			surge.SetActive(true);
		}
		DisplayPlayerWarzone();
	}

	private void MoveCardsBackToHand(){
		//returns hand cards to player
		for (int i = 0; i< wrapHand.cards.Count; i++) {
			wrapHand.cards [i].transform.SetParent (selectedPlayer.hand.transform);
			wrapHand.cards [i].SetActive (false);
		}
		DisplayPlayerHand ();
	}
	
	/// <summary>
	/// Sets the active player. Updates values, hand, and warzone
	/// </summary>
	public void SetActivePlayer() {
		// Make sure there is a selected player
		if (otherPlayerShown == false) {
			SetSelectedPlayerBasedOnGameType ();
		}

		#region set HUD values
		//only update action gradient if it changes
		if (prevAction != selectedPlayer.ActionPoints) {
			//Debug.Log (prevAction);
			actionPointsText.text = "0" + selectedPlayer.ActionPoints.ToString ();
			vfx.UpdateAction (actionGradient, selectedPlayer.ActionPoints, prevAction);
			prevAction = selectedPlayer.ActionPoints;
		}
		
		//only update energy if values change
		if (prevEnergy != selectedPlayer.Energy) {
			//if the player's energy is less than 10, add a "0" to the front
			string energy = "00";
			if(selectedPlayer.Energy < 10){
				energy = "0" + selectedPlayer.Energy.ToString ();
			}else{
				energy = selectedPlayer.Energy.ToString ();
			}
			energyPointsText.text = energy;
			prevEnergy = selectedPlayer.Energy;
		}
		
		//only update empire if values change
		if (prevEmpire != selectedPlayer.GetEmpirePoints()) {
			empirePointsText.text = selectedPlayer.GetEmpirePoints().ToString();

			//only play animation if player's empire points increase
			if(prevEmpire < selectedPlayer.GetEmpirePoints() && prevPlayer == selectedPlayer){
				vfx.FlashIcon(empireMaterial);
			}
			prevEmpire = selectedPlayer.GetEmpirePoints();
		}
		
		//only update fleet if values change
		if (prevFleet != selectedPlayer.warZone.FleetStrength()) {
			fleetPointsText.text =selectedPlayer.warZone.FleetStrength().ToString();

			//only play animation if player's fleet strenght increase
			if(prevFleet < selectedPlayer.warZone.FleetStrength () && prevPlayer == selectedPlayer){
				vfx.FlashIcon(fleetMaterial);
			}
			prevFleet = selectedPlayer.warZone.FleetStrength ();
		}
		
		//only update ground if values change
		if (prevGround != selectedPlayer.warZone.GroundStrength()) {
			groundPointsText.text = selectedPlayer.warZone.GroundStrength().ToString ();

			//only play animation if player's ground strenght increase
			if(prevGround < selectedPlayer.warZone.GroundStrength () && prevPlayer == selectedPlayer){
				vfx.FlashIcon(groundMaterial);
			}
			prevGround = selectedPlayer.warZone.GroundStrength ();
		}
		
		//only update name if values change
		if (prevName != selectedPlayer.Name) {
			nameText.text = selectedPlayer.Name;
			prevName = selectedPlayer.Name;
		}

		//if the displayed faction image is not the selectedplayer's faction, load the correct image
		if(factionImage != null && factionImage.sprite != null && selectedPlayer != null )
		{
			if (factionImage.sprite.name != selectedPlayer.Faction + "_White"){
				factionImage.sprite = Resources.Load ("Factions/New Factions/" + selectedPlayer.Faction + "_White", typeof(Sprite)) as Sprite;
			}
		} else {
			//TODO: Remove this log statement and determine why we're sometimes getting this error
			Debug.LogError("Unable to set faction images for: " + selectedPlayer
				+ "\nTarget Image: " + factionImage);
		}

		//only update phase text if different
		if (prevPhase != selectedPlayer.GetPhase ().ToString ()) {
			//display phase in correct format and color scheme
			phaseText.text = "<color=#E20336FF>[</color> " + selectedPlayer.GetPhase ().ToString ().ToUpper () + " PHASE <color=#E20336FF>]</color>";
			prevPhase = selectedPlayer.GetPhase ().ToString ();
		}

		if(prevPlayer != selectedPlayer){
			prevPlayer = selectedPlayer;
		}
		#endregion

		// Show the player's hand and WarZone
		if (isHandSet == false && selectedPlayer.Hand.Count > 0) {
			// Show the hand if the player owns it
			if (otherPlayerShown == false && selectedPlayer.photonView.isMine) {
				DisplayPlayerHand();
			}
			DisplayPlayerWarzone();
		}

		if (otherPlayerShown == false && selectedPlayer.photonView.isMine ){
			warZone.MaxSelectableCards = selectedPlayer.warZone.MaxSelectableCards;
			//only want to set the histogram once 
//TODO: Why are we getting the current player again?  This shoudl be managed by
// SetSelectedPlayerBasedOnGameType
			currentPlayer = GetCurrentPlayer();

			//TODO hack to fix hand
			// Player's hand wasn't being populated durring action phase, must 
			// be related to Networking since it was working before merge. Hack 
			// checks if the player's hand isn't empty and if the displayed
			// hand doesn't have any children
			if(selectedPlayer.Hand.Count > 0 && hand.transform.childCount == 0){
				if(centerDisplay.isTurnedOn == false 
					&& invadeCenterDisplay.isInvadeTurnedOn == false){
					DisplayPlayerHand();
				}
			}

			//check if the cards displayed belong to the selected player
			if (wrapHand.CheckCardsDisplayed () == false) {
				//return the cards to their owner
				wrapHand.ReturnCardsToOwner ();
				
				DisplayPlayerHand ();
			}
			
			if(isHistogramSet == false){
				//TODO: What is this trying to do?  How can we refactor this to simplify the 
				// code and decouple it from the Player Manager
				//REFACTOR FIX
				currentSizeHistogram.SetCurrent( 
				    GetCurrentPlayerIndex() % players.Length + 1);
				energySurge.tokenCount = currentPlayer.energyTokens;
				if (currentPlayer.energyTokens > 0) {
					surge.SetActive(true);
				}
				isHistogramSet = true;
			}
		}
	}

	/// <summary>
	/// Disables all the buttons on the HUD. Called when center display is open
	/// to prevent crashes and bugs.
	/// </summary>
	/// <remarks>Visual states are not controlled here for the buttons because 
	/// the animation for pressing them plays as the notification panel pops up.
	/// If the buttons grayed out immediately, the press animation for the 
	/// buttons would not light up, but be dimmed.
	/// </remarks>
	public void DisableButtons(){
		buttonsOff = true;
		//if the Center display is open, disable warzone buttons
		//allows users to see other players warzones but not interact with their cards or buttons
		if(centerDisplay.isTurnedOn == true || centerDisplay.isDiscardTurnedOn == true || invadeCenterDisplay.isInvadeTurnedOn == true){
			TurnOffHistosAndCards();
		}

		surge.GetComponent<Button> ().interactable = false;
		undoButton.interactable = false;
		passButton.interactable = false;
		endTurnButton.interactable = false;

		// Visual states are not controlled here for the buttons because the animation for pressing them plays as the notification panel pops up
		// if the buttons grayed out immediately, the press animation for the buttons would not light up, but be dimmed

		//Deck button needs to remain interactable if 
		if (centerDisplay.isDiscardTurnedOn == false){
			deckToggle.interactable = false;
		}
	}

	/// <summary>
	/// Changes the state of help button. Prevents the user from creating multiple instances of the help
	/// page
	/// </summary>
	/// <param name="isOn">If set to <c>true</c> is on.</param>
	public void ChangeStateOfHelp(bool isOn){
		//if isOn is true, then the help button is enabled and the help screen is off
		helpButton.interactable = isOn;
		isHelpOff = isOn;
	}
	
	/// <summary>
	/// Turns the off histograms, cards, and world and unit buttons
	/// </summary>
	public void TurnOffHistosAndCards(){
		//turning off the button components so the user can't click them
		worldButton.enabled = false;
		unitButton.enabled = false;
		
		//turns the histogram spots off to make them uninteractible
		currentSizeHistogram.TurnOffHistoSpots();
		pastSizeHistogram.TurnOffHistoSpots();
		playerInfoManager.TurnOffOnPlayerCards(false);
	}

	/// <summary>
	/// Enables the buttons, called when the selectedPlayer is the currentPlayer
	/// </summary>
	public void EnableButtons(){
		//if the help page isn't up
		if (isHelpOff == true) {
			//if the garrision, enhancment, discard, and invade window are off
			if (centerDisplay.isTurnedOn == false 
				&& centerDisplay.isDiscardTurnedOn == false 
				&& invadeCenterDisplay.isInvadeTurnedOn == false) 
			{
				//turns the histogram spots on to make them interactible
				currentSizeHistogram.TurnOnHistoSpots ();
				pastSizeHistogram.TurnOnHistoSpots ();
		
				playerInfoManager.TurnOffOnPlayerCards (true);

				//turning on the button components so the user can interact with them again
				worldButton.enabled = true;
				unitButton.enabled = true;
			}

			// If no other player is viewing the card
			if (otherPlayerShown == false) {
				buttonsOff = false;

				//turn on energy surge button
				surge.GetComponent<Button> ().interactable = true;

				//if the garrision, enhancment, discard, and invade window are off
				if (centerDisplay.isTurnedOn == false 
					&& centerDisplay.isDiscardTurnedOn == false 
					&& invadeCenterDisplay.isInvadeTurnedOn == false) 
				{
					undoButton.interactable = false;
					endTurnButton.interactable = true;
				}

				//if in the action phase and the user hasn't performed any actions yet
				if (currentPhase == GamePhase.Action 
					&& selectedPlayer.CurrentActionPhase == GamePhase.None) {
					//if not viewing another player
					if (otherPlayerShown == false) {
						//if the engine's current player is the selected player
						if (GetCurrentPlayer () == selectedPlayer) {
							//enable the pass button and disable the endturn button
							passButton.interactable = true;
							endTurnButton.interactable = false;
						}
					}
				}

				//HACK to fix the discard being visible while invasion window is open
				//Prevents the toggle button from being turned on if the invade
				// window or enhancment/garrision window is up
				if (centerDisplay.isTurnedOn == false 
					&& invadeCenterDisplay.isInvadeTurnedOn == false) 
				{
					deckToggle.interactable = true;
				}
			}
		}
	}

	/// <summary>
	/// Switches the player. Returns the cards to the warzone and the hand. Then clears both
	/// </summary>
	public void SwitchPlayer(){
		//if not a network game
		if (isNetworkGame == false) {
			currentPlayer = GetCurrentPlayer();
		}
		selectedPlayer = currentPlayer;

		//returns players cards to their hand
		foreach (GameObject cardGO in wrapHand.cards) {
			Card card = cardGO.GetComponent<Card>();
			
			// Make sure the card hasn't been moved to the deck already
			if (card.cardOwner.playerDeck.DiscardPile.Contains (card) == false && 
			    card.cardOwner.playerDeck.DrawPile.Contains (card) == false) {
				cardGO.transform.SetParent(card.cardOwner.hand.transform);
				cardGO.SetActive(false);
			}
		}
		
		//returns the warzone cards to their owner and turn them off
		for(int i = 0; i< warZone.PointCards.Count; i++){
			warZone.PointCards[i].transform.SetParent(warZone.PointCards[i].cardOwner.warZone.transform);
			warZone.PointCards[i].gameObject.SetActive(false);
		}
		for(int i = 0; i< warZone.DeployedUnits.Count; i++){
			warZone.DeployedUnits[i].transform.SetParent(warZone.DeployedUnits[i].cardOwner.warZone.transform);
			warZone.DeployedUnits[i].gameObject.SetActive(false);
		}

		//if the garrison, enhancement or discard window is on, turn them off
		if(centerDisplay.isTurnedOn == true || centerDisplay.isDiscardTurnedOn == true){
			if(centerDisplay.isDiscardTurnedOn == true){
				deckToggle.isOn = false;
			}else{
				centerDisplay.Hide();
			}
		}
		
		//clear the warzone and reset the spots
		warZone.DeployedUnits.Clear ();
		warZone.PointCards.Clear ();
		warZone.ResetSpots ();

		//fixing end turn button not being enabled in Single Player game in discard phase
		if (currentPhase != GamePhase.Action && currentPhase != GamePhase.Draw){
			endTurnButton.interactable = true;
		}
	}

	/// <summary>
	/// Resets the hand and histograms at the end of the turn so they can update correctly
	/// at the end of the turn
	/// </summary>
	public void ResetHandAndHisto(){
		//resets hand and histogram to make sure they populate
		isHandSet = false;
		isHistogramSet = false;
	}

	/// <summary>
	/// Displays the selectedPlayer hand. Should only be called if the current player is the
	/// selected player
	/// </summary>
	public void DisplayPlayerHand(){
		wrapHand.DestroyHandBack();
		wrapHand.PopulateHand ();
		isHandSet = true;
	}

	/// <summary>
	/// Prepares the next round for play, updating visual components to reflect
	/// the new state.
	/// </summary>
	public void StartNewRound(int roundNumber){
		// The sector increases by one for every two rounds.  RoundToInt rounds
		// X.5 to the nearest even integer so add 0.1 to get the number closer 
		// to the higher value for deisred rounding output
		int sectorNum = Mathf.RoundToInt((roundNumber + 0.1f) / 2);

		//update the round and sector text to roman numerals
		roundText.text = RomanNumerals [roundNumber - 1];
		sectorText.text = RomanNumerals [sectorNum - 1];

		//clear histograms past for the previous round
		pastSizeHistogram.ClearHistogram ();
	}

	/// <summary>
	/// Displays the selectedPlayer warzone. Clears the warzone if there are cards left,
	/// then adds the selectedPlayer's cards to the list in the warzone
	/// </summary>
	public void DisplayPlayerWarzone(){
		// Reset selected and shown player if no longer looking at another player
		if(otherPlayerShown == false){
			currentPlayer = GetCurrentPlayer();
			SetSelectedPlayerBasedOnGameType();
		}

		//clear the warzone
		warZone.DeployedUnits.Clear ();
		warZone.PointCards.Clear ();
		//add the cards of the selectedPlayer to the warzone
		for(int i = 0; i< selectedPlayer.warZone.PointCards.Count; i++){
			warZone.AddCardToList(selectedPlayer.warZone.PointCards[i]);
		}
		for(int i = 0; i< selectedPlayer.warZone.DeployedUnits.Count; i++){
			warZone.AddCardToList(selectedPlayer.warZone.DeployedUnits[i]);
		}
		
		isHandSet = true;
	}

	/// <summary>
	/// Called by End Turn button in HUD
	/// </summary>
	public void EndPlayersTurn() {
		// Make sure the selected player can perform this action
		if( !selectedPlayer.IsCurrent ) return;

		// Disable the controls to ensure it can't be fired again
		DisableButtons();

		// Inform the server to end the player's turn and continue with play
		selectedPlayer.EndTurn();

		// Clean up any UI locally, deselect any selected cards
		selectedPlayer.DeselectAll();

		// Deselect all cards in the central zone
		centralZone.DeselectAll();

		// Reset any used cards
		selectedPlayer.ResetPlayedCards();
	}

	/// <summary>
	/// Sets the phase from the rulesEngine
	/// </summary>
	/// <param name="phase">Phase.</param>
	[PunRPC]
	public void SetPhase(byte phase) {
		currentPhase = (GamePhase)phase;
	}

	/// <summary>
	/// Gets the current player.
	/// </summary>
	/// <returns>The current player.</returns>
	public Player GetCurrentPlayer(){
		foreach (Player player in players) {
			if(player.IsCurrent == true){
				return player;
			}
		}
		//TODO: throw new UnityException("No player is set current");
		return players[0];
	}
	
	/// <summary>
	/// Gets the current player.
	/// </summary>
	/// <returns>The current player.</returns>
	public int GetCurrentPlayerIndex(){
		int currentPIndex = 0;
		for (int i = 0; i< players.Length; i++) {
			if(players[i].IsCurrent == true){
				currentPIndex = i;
			}
		}
		return currentPIndex;
	}
	
	/// <summary>
	/// Checks if the game is a network game.
	/// </summary>
	/// <returns><c>true</c>, if network game was checked, <c>false</c> otherwise.</returns>
	private bool CheckNetworkGame(){
		GameDetails details = FindObjectOfType<GameDetails>();
		return details.GameType == GameDetails.NETWORK_GAME;
	}

	/// <summary>
	/// Passes the players turn. Called by Pass button in HUD
	/// </summary>
	public void PassPlayersTurn() {
		//of the gamephase is the action phase and the selected player hasn't preformed any actions yet
		if (currentPhase == GamePhase.Action 
			&& selectedPlayer.CurrentActionPhase == GamePhase.None)
		{
			// Pass the player's turn.  This is against local as it affects
			// player values
			selectedPlayer.Pass();
			
			// Clean up any UI locally, deselect any selected cards
			selectedPlayer.DeselectAll();
			
			// Deselect all cards in the central zone
			centralZone.DeselectAll();
			
			// Reset any used cards
			selectedPlayer.ResetPlayedCards();
		}	
	}

	/// <summary>
	/// Records the history event and stores it inside of the past histogramSpot
	/// </summary>
	/// <param name="action">Action taken this turn</param>
	/// <param name="resultantCards">Cards action was preformed on</param>
	/// <param name="player">Player who preformed action</param>
	public void RecordHistoryEvent(ActionType action, List<Card> resultantCards, Player player) {
		//TODO: Remove after fully implemented
		string debugLog = player.name + " perfomed " + action.ToString() + " on ";
		foreach (Card card in resultantCards) {
			debugLog += card.Title + " ";
		}

		//Create the History Event
		HistoryEvent historyEvent = new HistoryEvent(action, resultantCards, player);

		//get the index of the player
		int indexOfPlayer = 0;
		for(int i = 0; i < players.Length; i++){
			if(players[i] == player){
				indexOfPlayer = i;
			}
		}
		//use the index as the number to display on the histogram spot
		//create histospot for history event
		pastSizeHistogram.AddHistogram((indexOfPlayer + 1), historyEvent);
	}
	#endregion

	#region Properties
	/// <summary>
	/// Retrieve the player who is currently using the device, aka the current
	/// device owner or player.
	/// </summary>
	private Player CurrentDevicePlayer
	{
		get
		{
			Player player = null;

			// Network games, use local player
			if (isNetworkGame)
			{
				player = players.Where(p => p.photonView.isMine).ToArray()[0];
			}
			// Single Device Game
			else
			{
				//TODO: This is technically not correct as it includes the chance that
				// an AI player may be the current device player.
				player = currentPlayer;
			}

			return player;
		}
	}
	#endregion
}

