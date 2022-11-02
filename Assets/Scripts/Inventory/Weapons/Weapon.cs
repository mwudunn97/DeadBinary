using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Main class used for weapon objects
    private Transform _shellEject;
    private ParticleSystem[] _gunParticles;
    private Light[] _gunLights;

    [SerializeField] private Vector3 _offset;
    [SerializeField] private GameObject _shellPrefab;
    [SerializeField] private UnitAction _weaponAction;
    [SerializeField] private AudioClip[] _fireSound;
    [SerializeField] private AudioClip[] _reloadSound;

    public WeaponStats Stats;
    public WeaponAttributes Attributes;

    public UnitAction WeaponAction { get { return _weaponAction; } }

    private void Start()
    {
        // Weapon always starts with full ammo
        Stats.AmmoCurrent = Stats.AmmoMax;
    }

    private void Awake()
    {
        _gunParticles = GetComponentsInChildren<ParticleSystem>();
        _gunLights = GetComponentsInChildren<Light>();
        _shellEject = transform.Find("ShellEject");
    }

    public void DefaultPosition(Unit parent)
    {
        // Used to place newly-created weapon objects into the default position

        transform.parent = parent.GetAnimator().GetWeaponDefaultPosition();
        transform.position = transform.parent.position;
        transform.localPosition = transform.localPosition + _offset;
        transform.rotation = transform.parent.transform.rotation;
    }

    public void Shoot()
    {
        // Used by animation to kick off shoot effect

        StartCoroutine(ShootEffect());
    }

    public int GetAnimationLayer()
    { return Attributes.AnimationLayer; }

    public WeaponImpact GetImpact()
    { return Attributes.Impact; }

    public int GetDamage()
    { return Stats.Damage; }

    public float GetAreaOfEffect()
    { return Stats.AreaOfEffect; }

    public int GetMinimumRange()
    { return Stats.RangeMin; }

    public int GetMaximumRange()
    { return Stats.RangeMax; }

    public float GetAccuracyPenalty(int range)
    {
        int maxRangeDiff = range - GetMaximumRange();
        int minRangeDiff = -(range - GetMinimumRange());

        float penalty = 0;

        if (maxRangeDiff > 0) 
        {
            penalty = maxRangeDiff * Stats.OverRangeAccuracyPenalty;
        }
        else if (minRangeDiff > 0)
        {
            penalty = minRangeDiff * Stats.UnderRangeAccuracyPenalty;
        }

        return penalty;
    }

    public IEnumerator ShootEffect()
    {
        // Display gun flash
        foreach (Light gunLight in _gunLights)
            gunLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        foreach (ParticleSystem gunParticle in _gunParticles)
            gunParticle.Stop();

        foreach (ParticleSystem gunParticle in _gunParticles)
            gunParticle.Play();

        PlaySound(WeaponSound.FIRE);

        if (_shellEject.gameObject.activeSelf) EjectShell();

        yield return new WaitForSecondsRealtime(_gunParticles[0].main.duration);
        foreach (Light gunLight in _gunLights)
            gunLight.enabled = false;
    }

    public void ReloadEffect()
    {
        // Reload sound effect

        PlaySound(WeaponSound.RELOAD);
        Stats.AmmoCurrent = Stats.AmmoMax;
    }

    public void Reload()
    {
        // Sets current ammo to weapon's maximum

        Stats.AmmoCurrent = Stats.AmmoMax;
    }

    public void DropGun()
    {
        // Detaches gun from Character and adds physics

        transform.parent = null;
        gameObject.AddComponent<Rigidbody>();
    }

    public void PlaySound(WeaponSound weaponSound, Unit character=null)
    {
        // Plays sound from selected weapon sound choice
        // Optionally can play sound from Character's audio source instead of weapon

        AudioClip audioClip = null;
        AudioSource audioSource = GetComponent<AudioSource>();

        if (character)
            audioSource = character.GetComponent<AudioSource>();

        // Determine sound to play
        switch (weaponSound)
        {
            case (WeaponSound.FIRE):
                audioClip = _fireSound[Random.Range(0, _fireSound.Length)];
                break;
            case (WeaponSound.RELOAD):
                audioClip = _reloadSound[Random.Range(0, _reloadSound.Length)];
                break;
        }

        // Play the sound at the desired audio source
        audioSource.PlayOneShot(audioClip);
    }

    void EjectShell()
    {
        // Generate a shell as an effect and eject it with force

        float randomForce = (Random.Range(3, 5));
        GameObject shell = GlobalManager.ActiveMap.CreateEffect(_shellPrefab, _shellEject.position, Quaternion.Euler(Random.Range(5, 15), 0, Random.Range(-10, -15)));
        shell.GetComponent<Rigidbody>().AddForce(_shellEject.forward * randomForce, ForceMode.VelocityChange);
        shell.GetComponent<Rigidbody>().AddForce(-_shellEject.right * randomForce/2, ForceMode.VelocityChange);

    }

    public void SpendAmmo(int amount = 1)
    {
        // Spends a shot from the current ammo, defaults to 1 shot

        Stats.AmmoCurrent -= amount;
    }

    [System.Serializable]
    public struct WeaponAttributes
    {
        public int AnimationLayer;
        public float AnimationSpeed;
        public WeaponImpact Impact;
    }

    [System.Serializable]
    public struct WeaponStats
    {
        public int Damage;
        public float AreaOfEffect;
        public int RangeMin;
        public int RangeMax;
        public int AmmoMax;
        public int AmmoCurrent;
        //Note: Not capped at 1.
        public float BaseAccuracyModifier;
        public float OverRangeAccuracyPenalty;
        public float UnderRangeAccuracyPenalty;
    }
}

public enum WeaponImpact { LIGHT, MEDIUM, HEAVY }

public enum WeaponSound { FIRE, RELOAD };
