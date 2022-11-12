using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    static StatusMenuUI _statusMenuUI;
    static ActionPanelScript _actionPanel;
    static InCombatPlayerActionUI _inCombatPlayerActionUI;
    static InventoryPanelScript _inventoryPanel;
    static InfoPanelScript _infoPanel;
    static TurnIndicatorPanel _turnIndicator;
    static TextMeshProUGUI _stateDebugText;

    private void Awake()
    {
        Instance = this;
    }

    public static InfoPanelScript GetInfoPanel()
    {
        if (!_infoPanel) _infoPanel = Instance.GetComponentInChildren<InfoPanelScript>();
        return _infoPanel;
    }

    public static StatusMenuUI GetStatusMenu()
    {
        if (!_statusMenuUI) _statusMenuUI = Instance.GetComponentInChildren<StatusMenuUI>();
        return _statusMenuUI;
    }

    public static ActionPanelScript GetActionPanel()
    {
        if (!_actionPanel) _actionPanel = Instance.GetComponentInChildren<ActionPanelScript>();
        return _actionPanel;
    }

    public static InCombatPlayerActionUI GetPlayerAction()
    {
        if (!_inCombatPlayerActionUI) _inCombatPlayerActionUI = Instance.GetComponentInChildren<InCombatPlayerActionUI>();
        return _inCombatPlayerActionUI;
    }

    public static InventoryPanelScript GetInventoryPanel()
    {
        if (!_inventoryPanel) _inventoryPanel = Instance.GetComponentInChildren<InventoryPanelScript>();
        return _inventoryPanel;
    }

    public static TurnIndicatorPanel GetTurnIndicator()
    {
        if (!_turnIndicator) _turnIndicator = Instance.GetComponentInChildren<TurnIndicatorPanel>();
        return _turnIndicator;
    }

    public static TextMeshProUGUI GetStateDebug()
    {
        if (!_stateDebugText) _stateDebugText = Instance.transform.Find("StateDebugText").GetComponent<TextMeshProUGUI>();
        return _stateDebugText;
    }
}
