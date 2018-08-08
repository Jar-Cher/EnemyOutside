using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // list of all units with plots
    public List<UnitEnlisted> units;
    public int i = 0;

    // current state of a game
    public string state = "Planning";

    // random child and this gameObject
    public GameObject someTile, tiles;

    private void OnEnable()
    {
        // set initial parameters
        i = 0;
        if (!tiles)
            tiles = this.gameObject;
        if (!someTile)
            someTile = tiles.transform.GetChild(0).gameObject;
    }


    void Update () {

        // if we are at asction state, then allow the most organaised unit move
		if(state == "Action")
        {
            units[i].unit.GetComponent<Unit>().movingAllowed = true;

            //Debug.Log("Nailed it!!!");
        }
	}

    // We are getting there by clicking "execute" button back in game
    public void prepareToProceedTurn()                                   
    {
        // getting everything ready for action game state
        state = "Action";

        // units with highest organisation will be proceeded firstly
        units.Sort(UnitEnlisted.compareByOrganisation);
    }

    public void proceedNextUnit()
    {
        // symple cycle, enter here when previous unit from "units" has finished his job
        if (i < (units.Count - 1) )
        {
            //Debug.Log("Nailed it!");
            i++;
        }
        else
        {

            // if we have proceeded every unit
            // get everything ready for next action state
            units.Clear();
            i = 0;

            // and go to planning state
            someTile.GetComponent<Tile>().resetChosenTile();
            state = "Planning";
            
            //Debug.Log("Nailed it!!");
        }
    }

}
