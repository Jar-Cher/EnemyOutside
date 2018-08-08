using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    // the tile on which unit placed
    public GameObject tile;

    public GameManager gm;

    // in-game coordinates of unit's tile
    public int x, y;
    // how much moves can unit make at the beginning of its turn?
    public int maxMoves = 2;
    // how much moves can unit make at some particular point of time?
    public int moves;
    // basic organisation of unit
    public int org = 100;


    // basicaly, this MonoBehaviour's org and GameObject converted to ScriptableObject to send it to GameManager
    UnitEnlisted thisUnitEnlisted;

    // list of tiles to which unit should go
    public List<GameObject> upcomingTargets;

    // time should be spent on visual moving
    public float moveRate = 0.3f;

    // when does the unit started his visual moving and when he will stop it?
    public float startMove, stopMove;

    // in-editor coordinates of tiles from which unit started moving and at which unit will stop
    Vector3 startingPos, finishingPos;

    // should unit move visually?
    public bool movingAllowed = false;

    // should unit start move to the next tile?
    bool beginNextMoving = true;


    private void OnEnable()
    {
        // set initial parameters
        setCoordinatesAndTile();
        gm = tile.transform.parent.GetComponent<GameManager>();
        moves = maxMoves;
        // updateGM();
    }

    void Update () {
        if ((upcomingTargets.Count > 0) && (movingAllowed)) {       // this code manages moves of the unit
            if (beginNextMoving)
            {
                // set parameters of upcoming visual move
                startingPos = transform.position;
                finishingPos = upcomingTargets[0].transform.position;
                startMove = Time.time;
                stopMove = Time.time + moveRate;

                // adjust new unit's placement
                transform.SetParent(upcomingTargets[0].transform);
                upcomingTargets[0].GetComponent<Tile>().unit = this.gameObject;
                setCoordinatesAndTile();

                // we should firstly wait for this movement to be finished
                beginNextMoving = false;

                //Debug.Log("Nailed it!");
            }
            
            // define our current visual position
            float a = ((Time.time - startMove) / (stopMove - startMove));
            transform.position = Vector3.Lerp(startingPos, finishingPos, a);

            // if we have arrived
            if (a >= 1)
            {
                //Debug.Log("Nailed it!!");
                //startingPos = transform.position;
                //upcomingTargets[0].GetComponent<Tile>().unit = null;

                // remove reached tile from our "to go" list
                upcomingTargets.RemoveAt(0);

                // if we have to go somewhere elsewhere
                if (upcomingTargets.Count > 0)
                {
                    // indicate that we are ready to go
                    beginNextMoving = true;

                    //Debug.Log("Nailed it!!!");
                }
                else
                {
                    // if our last tile from "to go" list is reached, then return booleans to "planning" mode
                    beginNextMoving = true;
                    movingAllowed = false;

                    // delete copy of current unit
                    thisUnitEnlisted = null;

                    // send a note gameManager that we are done with this unit
                    gm.proceedNextUnit();

                    //Debug.Log("Nailed it!!!!");
                }
            }
        }

    }

    public void plot(GameObject target)
    {
        // stores previous tile from which we will go here
        GameObject prev = target.GetComponent<Tile>().prevTile;
        if (prev)
        {
            // if previous tile exists, we add it to our route
            plot(prev);
            upcomingTargets.Add(target);
        } else
        {
            // otherwise, inform gameManager that this unit will act in this turn
            updateGM();
        }
        
    }

    public void setCoordinatesAndTile()    
    {
        tile = transform.parent.gameObject;
        x = tile.GetComponent<Tile>().x;
        y = tile.GetComponent<Tile>().y;
    }

    public void updateGM()
    {
        if(!gm)
            // if gameManager is not specified, do it
            gm = tile.transform.parent.GetComponent<GameManager>();
        if (!thisUnitEnlisted)
        {
            // if unit has not been duplicated to UnitEnlisted state usable by gameManager, do it
            thisUnitEnlisted = ScriptableObject.CreateInstance<UnitEnlisted>();
            thisUnitEnlisted.unit = this.gameObject;
            thisUnitEnlisted.updateData();

            // the main purpose of funcction: inform the gameManager
            // that this unit will act at this turn
            gm.units.Add(thisUnitEnlisted);
        }
    }

}
