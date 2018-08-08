using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {


    // important variables (TBA in player interface)
    // in-game coordinates of the tile
    public int x, y;
    // movement cost, to be replaced with analog in Unit in upcoming versions
    public int movementCost = 1;                                           


    // technical variables (will be hidden within the game)
    // how many moves can be left if unit will pass through?
    public int maxMovesLeft;
    // tile from which unit will pass according to plot
    public GameObject prevTile;

    // tile currently chosen by player
    public static GameObject chosenTile;

    // child, active when mouse is over
    public GameObject grid;
    // child, active when its tile is chosen
    public GameObject hoverHex;
    // child, active when unit on a chosen tile can reach this tile wihin one turn
    public GameObject isAccessable;

    // one unit per tile, one building per tile
    public GameObject unit, building; 

    // parent gameobject, contain all tiles
    public static GameObject map;
    // map's script
    public static GameManager gm;

    void OnEnable()    
    {
        // set initial parameters
        x = (int)(transform.localPosition.x / 1.36);
        y = (int)(transform.localPosition.y / 1.7999999 + x * 0.5);

        grid = transform.Find("Grid").gameObject;
        hoverHex = transform.Find("Hover Hex").gameObject;
        isAccessable = transform.Find("Is Available").gameObject;
        if (isOccupied())
            unit = transform.Find("Unit").gameObject;
        map = this.transform.parent.gameObject;
        gm = map.transform.GetComponent<GameManager>();
        
        this.gameObject.name = "Tile " + x + " " + y;
    }


    void Update () {

        // if mouse is over this tile, right button is pressed during planning mode, ...
        if (grid.activeSelf && Input.GetMouseButtonDown(1) && (gm.state == "Planning"))
        {
            // ...chosen tile occupied by unit, which can reach this tile...
            if (chosenTile.GetComponent<Tile>().isOccupied() && isAccessable.activeSelf)              
            {
                // ...then erase previous plot and create a new one to move that unit here
                chosenTile.GetComponent<Tile>().unit.GetComponent<Unit>().upcomingTargets.Clear();
                chosenTile.GetComponent<Tile>().unit.GetComponent<Unit>().plot(this.gameObject);     
            }
        }
	}


    void OnMouseEnter()
    {
        grid.SetActive(true);
    }

    void OnMouseExit()
    {
        grid.SetActive(false);
    }

    void OnMouseDown()
    {
        if (gm.state == "Planning")
        {
            // if this tile is not chosen, then choose it 
            if (chosenTile != gameObject)
                chooseThisTile();
            // otherwise, unchose it
            else
                resetChosenTile();
        }
    }

    public bool isOccupied()
    {
        // if this tile have a unit on it, return true; otherwise return false
        try {
            GameObject unit = transform.Find("Unit").gameObject;
            return true;
        }
        catch (System.NullReferenceException) { return false; }
    }

    public void chooseThisTile()
    {
        // unchose previously chosen tile - no more than one tile can be chosen
        resetChosenTile();
        // set indicator to show chosen tile
        hoverHex.SetActive(true);
        // set this tile as chosen
        chosenTile = this.gameObject;
        // if tile have a unit on it, then...
        if (isOccupied())
        {
            // ... indicate how much moves the unit will have on this tile...
            maxMovesLeft = unit.GetComponent<Unit>().moves;
            // ... and show what we can reach from there
            showPossibleMoves(unit.GetComponent<Unit>().moves);
        }
    }

    public void resetChosenTile()
    {
        // there has to be a tile to unchose
        if (chosenTile)
        {
            // turning off the chosen indicator
            chosenTile.GetComponent<Tile>().hoverHex.SetActive(false);

            chosenTile = null;
            // reseting pathfinding variables
            foreach (Transform i in map.transform)
            {
                i.gameObject.GetComponent<Tile>().maxMovesLeft = -1;
                i.Find("Is Available").gameObject.SetActive(false);
                i.gameObject.GetComponent<Tile>().prevTile = null;
            }
        }
    }

    public List<GameObject> neighboringTiles()
    {
        // function return list of all neighboring tiles, if there any

        List<GameObject> neighbors = new List<GameObject>();
        // list of all possible directions
        string[] dir = { "n", "ne", "e", "s", "sw", "w" };
        foreach(string i in dir)
            try {
                GameObject nei = neighbor(i);
                // if there is no neighbor then crash
                if (nei)
                    neighbors.Add(nei);
            }
            catch(System.NullReferenceException) {
                // if there is a crash, simply do nothing
            }
        return neighbors;
    }

    public GameObject neighbor(string cardinalDirection)
    {
        // function will return a neighboring tile at the given cardinal direction or null
        // cardinalDirection can be "n", "ne", "e", "s", "sw", "w"; otherwise null will be returned
        switch (cardinalDirection)
        {
            case "n": // northern neighbor
                try {
                    // function will crash at this or similar cases below if there is no such tile
                    return map.transform.Find("Tile " + x + " " + (y + 1)).gameObject;
                }
                catch(System.NullReferenceException) {
                    // if there is no such tile, return null
                    return null;
                }
            case "ne": // north-eastern neighbor
                try {
                    return map.transform.Find("Tile " + (x + 1) + " " + (y + 1)).gameObject;
                }
                catch (System.NullReferenceException) {
                    return null;
                }
            case "e": // eastern neighbor
                try {
                    return map.transform.Find("Tile " + (x + 1) + " " + y).gameObject;
                }
                catch (System.NullReferenceException) {
                    return null;
                }
            case "s": // southern neighbor
                try {
                    return map.transform.Find("Tile " + x + " " + (y - 1)).gameObject;
                }
                catch (System.NullReferenceException) {
                    return null;
                }
            case "sw": // south-western neighbor
                try {
                    return map.transform.Find("Tile " + (x - 1) + " " + (y - 1)).gameObject;
                }
                catch (System.NullReferenceException) {
                    return null;
                }
            case "w": // western neighbor
                try {
                    return map.transform.Find("Tile " + (x - 1) + " " + y).gameObject;
                }
                catch (System.NullReferenceException) {
                    return null;
                }
            default:
                // in case of another, unexpected cardinal direction, return null and say "hello" to developer
                Debug.Log("Unable to determine neighbor");
                return null;
        }
    }

    public void showPossibleMoves(int movesLeft, int aggr = 1)
    {
        // function will show all tiles accessable by unit standing on the tile at this moment
        // as well as plot shortest possible way
        List<GameObject> AdjTiles = GetComponent<Tile>().neighboringTiles();

        // firstly, examine all adjacent tiles
        foreach (GameObject i in AdjTiles)
        {
            // how many moves would be left?
            int possibleMovesLeft = movesLeft - i.GetComponent<Tile>().movementCost;
            // how many moves will be taken should unit go via shortest known route?
            int mDist = i.GetComponent<Tile>().maxMovesLeft;
            // if unit will have enough moves to enter tile i and it is a quicker way to get here
            if ((possibleMovesLeft >= 0) && (possibleMovesLeft >= mDist))
            {
                // then remember new maximal moves left variable and new previous tile
                i.GetComponent<Tile>().maxMovesLeft = possibleMovesLeft;
                i.GetComponent<Tile>().prevTile = this.gameObject;
                // recursion, yay!
                i.GetComponent<Tile>().showPossibleMoves(possibleMovesLeft);
            }
        }
        // set indicator on
        GetComponent<Tile>().isAccessable.SetActive(true);

        
    }
    

}