using System.Collections.Generic;
using UnityEngine;

public abstract class UnitTargetAction : UnitAction
{
    [SerializeField] private bool _useCharacterCamera = false;
    
    protected Unit TargetUnit;
    protected Vector3 TargetPosition;
    public FactionAffinity TargetFaction;
    public List<TargetType> TargetTypes;
    public bool TargetSelf;
    public bool UseCharacterCamera { get { return _useCharacterCamera; } }

    public override void UseAction(Unit unit)
    {
        base.UseAction();
    }
}
