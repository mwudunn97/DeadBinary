using UnityEngine;

public class UnitActionShootRocket : UnitActionShoot
{
    private float _areaOfEffect;
    private Tile _targetTile;
    [SerializeField] private ParticleSystem rocketEffect;

    private Vector3 TriggerPosition => _targetTile.transform.position;

    public override void UseAction(Unit target)
    {
        // Override for convenience

        TargetUnit = target;
        unit.GetActor().targetCharacter = target;
        UseAction(target.currentTile);
    }

    public override void UseAction(Tile target)
    {
        // Locks in target information and begins shoot sequence
        // Sets action to "performing" state

        if (unit.GetActor().IsActing())
            return;

        if (!target)
        {
            Debug.Log(string.Format("{0} was called with no target by {1}", this, unit.gameObject));
            return;
        }

        TargetPosition = target.transform.position;
        _areaOfEffect = unit.EquippedWeapon.GetAreaOfEffect();
        unit.AddFlag(FlagType.AIM);
        unit.SpendActionPoints(actionCost);

        _targetDamaged = false;
        _targetHit = false;
        _bufferStartTimer = new(bufferStart);
        _bufferEndTimer = new(bufferEnd);

        StartPerformance();
    }


    public override void CheckAction()
    {
        // Wait for the start buffer
        while (!_bufferStartTimer.CheckTimer())
            return;

        // Perform shoot animation, inflict damage, spend ammo and AP
        if (ActionStage(0))
        {
            PerformShot();
            NextStage();
        }

        // Waits until shoot animation completes
        while (unit.GetAnimator().AnimatorIsPlaying("Shoot"))
            return;

        // Wait for the end buffer
        while (!_bufferEndTimer.CheckTimer())
            return;

        // Revert to idle state
        unit.GetActor().ClearTarget();
        TargetUnit = null;
        EndPerformance();
    }

    public override void TriggerAction(Projectile projectile = null)
    {
        if (projectile)
            Map.MapEffects.DestroyEffect(projectile);

        DamageTargets();
        HitTargets();
        ShowRocketEffect();
    }
    protected override void HitTargets()
    {
        if (_targetHit)
            foreach (Unit impactedUnit in Tile.GetTileOccupants(Tile.GetAreaOfEffect(_targetTile, _areaOfEffect)))
                impactedUnit.GetAnimator().TakeDamageEffect(unit.EquippedWeapon);
    }

    protected override void DamageTargets()
    {
        // Use on unit if possible, otherwise on empty tile
        _targetTile = TargetUnit ? TargetUnit.currentTile : unit.grid.GetTile(TargetPosition);

        if (!_targetDamaged)
        {
            foreach (Unit impactedUnit in Tile.GetTileOccupants(Tile.GetAreaOfEffect(_targetTile, _areaOfEffect)))
                impactedUnit.TakeDamage(unit, unit.EquippedWeapon.GetDamage(), TriggerPosition);
            _targetHit = true;
            _targetDamaged = true;
        }
    }

    public override void SpawnProjectile(Projectile projectile, Transform barrelEnd, float speed)
    {
        if (!projectile)
            TriggerAction();

        Vector3 destination = TargetUnit ? TargetUnit.GetAnimator().GetBoneTransform(HumanBodyBones.Chest).transform.position : TargetPosition;
        projectile = Map.MapEffects.CreateEffect(projectile, barrelEnd.position, barrelEnd.rotation);
        projectile.Init(this, destination, speed);
    }

    private void ShowRocketEffect()
    {
        // Creates the item effect object at the trigger position

        if (!rocketEffect)
            return;

        GameObject spawnEffect = GlobalManager.ActiveMap.CreateTimedEffect(rocketEffect.gameObject, TriggerPosition, rocketEffect.transform.rotation, 3f);
        spawnEffect.transform.localScale = Vector3.one * (_areaOfEffect / 2);
        PlayRocketSFX(spawnEffect);
    }

    private void PlayRocketSFX(GameObject spawnEffect)
    {
        // Plays the rocket effect sound
        
        AudioSource audioSource = spawnEffect.GetComponent<AudioSource>();
        AudioClip audioClip = AudioManager.GetSound(ItemEffectType.EXPLOSION);
        audioSource.PlayOneShot(audioClip);
    }
}
