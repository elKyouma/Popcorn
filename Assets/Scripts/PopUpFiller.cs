using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PopUpFiller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI deletionRefundText;
    [SerializeField] private TextMeshProUGUI keybindingsText;

    [SerializeField] private List<Text> keybindings;
    [SerializeField] private List<Toggle> keybindingsToggles;

    [SerializeField] private Image upgradeAvailebleImg;
    [SerializeField] private Image upgradeUnavailebleImg;

    private PopUpImpl impl;
    private void Awake() => impl = GetComponent<PopUpImpl>();
    [SerializeField] private GameObject levelContainer;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button deleteButton;

    public void SetShipElementConf(ShipBuilder builder)
    {
        title.text = builder.GetCurrentConfig().elementName;
        desc.text = builder.GetCurrentConfig().elementDescription;
        int currentLevel = builder.ActiveElement.CurrentLevel;
        UpdateLevelIndicator(currentLevel);

        SetDeletionRefund((int)(builder.GetCurrentConfig().costs[currentLevel] * 0.5));
    }

    public void SetUpgradeCost(int cost)
    {
        upgradeButton.interactable = true;
        upgradeCostText.gameObject.SetActive(true);
        SetUpgradePrice(cost);
    }

    public void DisableUpgrading()
    {
        upgradeButton.interactable = false;
        upgradeCostText.gameObject.SetActive(false);
        //upgradeAvailebleImg.gameObject.SetActive(false);
        //upgradeUnavailebleImg.gameObject.SetActive(true);
    }

    public void SetUpgradePrice(int price)
    {
        //upgradeAvailebleImg.gameObject.SetActive(true);
        //upgradeUnavailebleImg.gameObject.SetActive(false);
        upgradeCostText.gameObject.SetActive(true);
        upgradeCostText.text = $"Upg: {price}";
    }

    public void UpdateLevelIndicator(int level)
    {
        GameObject[] levels = new GameObject[levelContainer.transform.childCount];
        for (int i = 0; i < levelContainer.transform.childCount; i++)
        {
            levels[i] = levelContainer.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i <= level; i++)
        {
            levels[i].GetComponent<Image>().color = activeColor;
        }
        for (int i = level + 1; i < levels.Length; i++)
        {
            levels[i].GetComponent<Image>().color = inactiveColor;
        }
    }

    public void SetDeletionRefund(int refund)
    {
        deletionRefundText.text = refund.ToString();
        deletionRefundText.text = $"Destroy: {refund}";
    }
    public void HideKeyBindings()
    {
        foreach (var toggle in keybindingsToggles)
            toggle.gameObject.SetActive(false);
        keybindingsText.gameObject.SetActive(false);
    }

    private void ShowKeyBindings()
    {
        foreach (var toggle in keybindingsToggles)
            toggle.gameObject.SetActive(true);
        keybindingsText.gameObject.SetActive(true);
    }

    public void SetKeybindings(List<KeyCode> keybindings, List<KeyCode> activeBindings)
    {
        ShowKeyBindings();
        int index = 0;
        impl.Lock();

        foreach (KeyCode keyCode in keybindings)
        {
            this.keybindings[index].text = keyCode.ToString();
            if (activeBindings.Find(x => x == keybindings[index]) != KeyCode.None)
                keybindingsToggles[index].isOn = true;
            else
                keybindingsToggles[index].isOn = false;
            index++;
        }
        impl.Unlock();
    }
}
