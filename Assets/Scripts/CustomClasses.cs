using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnlisted : ScriptableObject {

    public GameObject unit;
    public int organisation;


    // unit's data presented in a way updateGM() can use
    public UnitEnlisted(GameObject newUnit)
    {
        this.unit = newUnit;
        this.organisation = newUnit.GetComponent<Unit>().org;
    }

    // no use, at least for now
    public void initialize(GameObject newUnit)
    {
        this.unit = newUnit;
        this.organisation = newUnit.GetComponent<Unit>().org;
    }

    // loading current data about unit
    public void updateData()
    {
        this.organisation = this.unit.GetComponent<Unit>().org;
    }

    public static int compareByOrganisation(UnitEnlisted unit1, UnitEnlisted unit2)
    {
        return unit2.organisation.CompareTo(unit1.organisation);
    }
}
