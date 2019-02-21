/// <author>Slater de Mont</author>
/// <date>11/28/2018</date>
/// <summary>
/// Controls game states and rules, set player tile images
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class GameEngine : MonoBehaviour {

	public Sprite player1;
	public Sprite player2;
	public Sprite tileImage;
	public int gameSize;
	public GameObject winText;
	public GameObject winScreen;
	public Text playerTurnText;
	public int numMove = 1;

	private int playerNum;
	private Tile selectedTile;
	private Sprite currentplayerImage;

	//array of tiles and 2d array representing board
	public Tile[] tileArray;
	private Tile[,] tiles;

	//Score array that tracks score for rows, cols and diagonals.
	private int[] scoreArray;

	//initializes game
	void Start () {
		tiles = new Tile[gameSize,gameSize];
		currentplayerImage = player1;
		playerNum = 1;
		PopulateArray ();
		scoreArray = new int[(2 * gameSize) + 2];
	}

	//Sets the selected tile and sets the tile
	public void TileClicked(Tile clickedTile){
		selectedTile = clickedTile;
		SetTile ();
	}

	//sets the tile, changes the text to display the current players turn, checks win conditions
	private void SetTile(){
		selectedTile.SetPiece (playerNum, currentplayerImage, numMove);
		numMove++;

		StoreMove ();

		//add 1 to scores if player1 or subtract 1 if player2
		int playerPoint = 0;
		if (playerNum == 1) {
			playerPoint = 1;
		} else {
			playerPoint = -1;
		}

		//set score for row
		scoreArray[selectedTile.row] += playerPoint;
		//set score for column
		scoreArray[gameSize + selectedTile.column] += playerPoint;

		//set score for diagonal
		if (selectedTile.row == selectedTile.column){
			scoreArray[2*gameSize] += playerPoint;
		}
		if (gameSize - 1 - selectedTile.column == selectedTile.row){
			scoreArray[2*gameSize + 1] += playerPoint;
		}
		
		if (playerNum == 1) {
			currentplayerImage = player2;
			playerNum = 2;
			playerTurnText.text = "Player 2 Turn";

		} else {
			currentplayerImage = player1;
			playerNum = 1;
			playerTurnText.text = "Player 1 Turn";
		}
		CheckWin ();
	}

	//create 2D array of tiles representing the game board
	private void PopulateArray(){
		int arrayIndex = 0;
		for (int i = 0; i < gameSize; i++)
		{	
			for (int j = 0; j < gameSize; j++)
			{
				tiles[i,j] = tileArray[arrayIndex];
				arrayIndex++;
			}
		}
		ResetGame ();
	}

	//checks score array for win
	private void CheckWin(){

		for (int i = 0; i < scoreArray.Length; ++i) {

			//if score is equal to gameSize, then player1 wins
			if(scoreArray[i] >= gameSize){
				GameOver(1);
			}
			else if(scoreArray[i] <= -(gameSize)){
				GameOver(2);
			}
		}
	}

	//turns on gameover screen 
	private void GameOver(int playerWon){
		winText.GetComponent<Text> ().text = "Player " + playerWon;
		winScreen.SetActive (true);
	}

	//Resets the game
	public void ResetGame(){
		for (int i = 0; i < gameSize; i++) {
			for (int j = 0; j < gameSize; j++) {
				tiles [i, j].Reset(tileImage);
			}
		}

		if (scoreArray != null) {
			for (int i = 0; i < scoreArray.Length; ++i) {
				scoreArray [i] = 0;
			}
		}
		currentplayerImage = player1;
		playerNum = 1;
		playerTurnText.text = "Player 1 Turn";
		numMove = 1;

	}


	//Write the game board to a text file
	public void StoreMove(){
		string turnMove = "";

		for (int i = 0; i < gameSize; i++)
		{
			for (int j = 0; j < gameSize; j++)
			{
				turnMove += tiles[i,j].id + " ";
			}
			turnMove += "\n";
		}
		turnMove += "\n";
		
		File.AppendAllText("./Assets/Games/Games.txt", turnMove);
	}

}
