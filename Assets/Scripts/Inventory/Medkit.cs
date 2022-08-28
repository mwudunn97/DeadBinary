using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Restores target to Max health
/// </summary>
public partial class Medkit : Item
{
    enum RestoreType { Percentage, Flat }
    [SerializeField] private RestoreType _restoreType;
    [SerializeField] private float _restoreAmount;

    // Tell the item to be used.
    protected override void UseItem
        (Character owner, Character charTarget = null, CoverObject covTarg = null)
    {
        RestoreHealth(charTarget);
    }

    // The medkit heals
    public void RestoreHealth(Character target)
    {
        switch (_restoreType)
        {
            case RestoreType.Flat:
                target.RestoreHealth((int)_restoreAmount);
                Debug.Log("Restored " + (int)_restoreAmount + " health to " + target.name + ".");
                break;
            case RestoreType.Percentage:
                int amt = Mathf.RoundToInt(target.stats.healthMax * (_restoreAmount / 100));
                target.RestoreHealth(amt);
                Debug.Log("Restored " + amt + " health to " + target.name + ". (" + _restoreAmount + "%)");
                break;
            default:
                Debug.Log("Error using Medkit");
                break;
        }
    }
}


public partial class Medkit
{
    [CustomEditor(typeof(Medkit), true)]
    public class MedkitEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
