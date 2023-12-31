using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public abstract class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Class used to handle sprites for action buttons
    // Will be used to control mouse-over effects as well

    protected AudioSource _audioSource;
    protected Button _button;
    protected TextMeshProUGUI _buttonLabel;
    protected Image _buttonFrame;
    protected Image _buttonBackground;

    protected bool _requirementsMet;
    protected ButtonState _buttonState = ButtonState.PASSIVE;

    public Item BoundItem;
    public UnitAction BoundAction;
    public Unit BoundUnit;

    // Colors for the icon and frame
    protected Dictionary<ButtonState, Color32> IconColors = new() {
        { ButtonState.ACTIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.PASSIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.DISABLED, new Color32(100, 100, 100, 255) },
    };

    // Colors for the background
    protected Dictionary<ButtonState, Color32> BackgroundColors = new() {
        { ButtonState.ACTIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.PASSIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.DISABLED, new Color32(0, 0, 0, 255) },
    };

    protected virtual void Awake()
    {
        _audioSource = UIManager.AudioSource;
        _button = GetComponentInChildren<Button>();
        _buttonLabel = GetComponentInChildren<TextMeshProUGUI>();
        _buttonFrame = transform.Find("Frame").GetComponent<Image>();
        _buttonBackground = transform.Find("Background").GetComponent<Image>();
    }

    protected virtual void Start()
    {
        _button.onClick.AddListener(ButtonPress);
    }

    protected virtual void Update()
    {
        CheckRequirements();
        if (BoundItem) CheckQuantity();
    }

    public virtual void LoadResources(string newSpritePath)
    {
        Debug.Log("Load Resources override missing for button!");
    }

    public virtual void LoadResources(string[] newSpritePath)
    {
        Debug.Log("Load Resources override missing for button!");
    }

    protected virtual void CheckRequirements()
    {
        Debug.Log(string.Format("Check Requirements override missing for button {0}!", gameObject));
    }

    protected virtual void CheckQuantity()
    {
        Debug.Log("Check Quantity override missing for button!");
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights icon on mouse over

        if (!_requirementsMet)
            return;

        _buttonState = ButtonState.ACTIVE;
        AudioClip audioClip = AudioManager.GetSound(InterfaceType.MOUSE_OVER, 0);
        _audioSource.PlayOneShot(audioClip);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave

        if (!_requirementsMet)
            return;

        _buttonState = ButtonState.PASSIVE;
    }

    public virtual void BindAction(UnitAction action)
    {
        // Stores action for checking requirements

        BoundAction = action;
    }

    public virtual void BindItem(Item item)
    {
        // Binds item for requirement checking

        BoundItem = item;
    }

    public virtual void BindUnit(Unit unit)
    {
        BoundUnit = unit;
    }

    public UnitAction GetAction()
    {
        // Returns bound action

        return BoundAction;
    }

    public Item GetItem()
    {
        // Returns bound action

        return BoundItem;
    }

    public InCombatPlayerAction GetPlayerAction()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        return playerAction;
    }

    public FiniteState<InCombatPlayerAction> GetCurrentState()
    {
        FiniteState<InCombatPlayerAction> state = GetPlayerAction().stateMachine.GetCurrentState();
        return state;
    }

    public void UnbindButton()
    {
        // Removes button bindings

        _button.onClick.RemoveAllListeners();
    }

    protected virtual void ButtonPress()
    {
        Debug.Log("Button Press override missing for button!");
    }

    public void ButtonTrigger()
    {
        // Handle to kick off button trigger effect

        StartCoroutine(ButtonTriggerEffect());
    }

    private IEnumerator ButtonTriggerEffect()
    {
        // Simulates the visual look of the action button being clicked
        // For use when action button is triggered via another input

        AudioClip audioClip = AudioManager.GetSound(InterfaceType.MOUSE_CLICK, 0);
        _audioSource.PlayOneShot(audioClip);

        _buttonState = ButtonState.ACTIVE;
        yield return new WaitForSeconds(0.2f);
        _buttonState = ButtonState.PASSIVE;
    }
}

public static class ActionButtons
{
    // Loads action button sprites from resources

    public static string btn_background = "Buttons/btn_background";
    public static string btn_action_move = "Buttons/btn_move";
    public static string btn_action_shoot = "Buttons/btn_shoot";
    public static string btn_action_reload = "Buttons/btn_reload";
    public static string btn_action_swap = "Buttons/btn_swap";
    public static string btn_action_chooseItem = "Buttons/btn_chooseItem";
    public static string btn_action_useItem = "Buttons/btn_useItem";
    public static string btn_action_medkit = "Buttons/btn_medkit";
    public static string btn_action_grenade = "Buttons/btn_grenade";
}

public enum ActionButtonSprite { MOVE, SHOOT, RELOAD, SWAP, CHOOSE_ITEM, MEDKIT, GRENADE }
public enum ButtonState { ACTIVE, PASSIVE, DISABLED };