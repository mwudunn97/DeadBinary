using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateIdle : FiniteState<InCombatPlayerAction>
{
    public StateIdle(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);

        if (t.selectedCharacter)
        {
            if (t.selectedCharacter.PotentialTargets != null)
                foreach (var v in t.selectedCharacter.PotentialTargets)
                    v.SelectUnit(SelectionType.CLEAR);

            t.selectedCharacter.PotentialTargets = null;

            if (t.selectedCharacter.TargetUnit != null && t.selectedCharacter.TargetUnit.Stats.HealthCurrent <= 0)
                t.selectedCharacter.TargetUnit = null;
        }
    }

    public override void Execute(InCombatPlayerAction t)
    {
        if (t.selectedCharacter == null)
            ChangeState(new StateNoSelection(Machine));
    }

    public override void InputPrimary(InCombatPlayerAction t)
    {
        Unit c = t.selectedCharacter;

        if (!IsPointerOverUIElement(t))
            t.SelectUnit();

        if (t.selectedCharacter == null) ChangeState(new StateNoSelection(Machine));
        if (t.selectedCharacter != c) ChangeState(new StateIdle(Machine));
    }

    public override void InputSecndry(InCombatPlayerAction t)
    {
        // Orders selected character to move to target tile
        UnitAction unitAction = t.selectedCharacter.FindActionOfType(typeof(UnitActionMove));
        UnitActionMove unitActionMove = (UnitActionMove)unitAction;

        // Check that we have a selected character and they meet the AP cost
        if (unitAction.CheckRequirements())
        {
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());
            int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

            // Find tile from right-click action
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.GetComponent<Tile>())
                {
                    var tile = hit.collider.GetComponent<Tile>();

                    //t.selectedCharacter.GetActor().ProcessAction(Action.action_move, contextTile: tile, contextPath: t.previewPath); // TO DO -- FIX THIS

                    //TODO: Clean this up. Need to check that this is a valid move before sending to state machine.
                    if (unitActionMove.IsTilePathable(tile))
                    {
                        t.PathPreviewClear();
                        unitActionMove.UseAction(tile);
                        ChangeState(new StateWaitForAction(Machine, unitAction));
                    }
                }
            }
        }

        // If we can't perform move, inform player
        else if (t.selectedCharacter && t.selectedCharacter.Stats.ActionPointsCurrent < unitAction.ActionCost)
        {
            Debug.Log("Out of AP, cannot move!"); // TODO: Display this to player in UI
        }

        // Something went wrong
        else
            Debug.Log("WARNING - Invalid move order detected");
    }

    public override void InputActionBtn(InCombatPlayerAction t, int index)
    {
        UnitAction action = t.GetBindings(index);

        if (!action)
            return;

        // If requirements aren't met, ignore button press
        bool requirementsMet = action.CheckRequirements();
        if (!requirementsMet) return;

        ButtonPress(index);
        if (action.GetType().IsSubclassOf(typeof(UnitTargetAction)))
            ChangeState(new StateChoosingTarget(Machine, (UnitTargetAction)action));
        else if (action.GetType().IsSubclassOf(typeof(UnitStateAction)))
            action.UseAction(this);
        else
        {
            action.UseAction();
            ChangeState(new StateWaitForAction(Machine, action));
        }
    }

    public override void InputTab(InCombatPlayerAction t, bool shiftPressed)
    {
        t.SelectNextUnit(reverseOrder: shiftPressed);
    }
}
