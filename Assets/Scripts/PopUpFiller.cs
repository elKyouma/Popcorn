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
    
    [SerializeField] private List<TextMeshProUGUI> keybindings;
    
    [SerializeField] private Image upgradeAvailebleImg;
    [SerializeField] private Image upgradeUnavailebleImg;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button deleteButton;
    public void SetShipElementConf(ShipElementConf shipConfig, ShipElement shipElement)
    {
        title.text = shipConfig.elementName;
        desc.text = shipConfig.elementDescription;
        int currentLevel = shipElement.currentLevel;
        SetUpgradePrice(shipElement.costs[shipElement.currentLevel]);
        SetDeletionRefund((int)(shipElement.costs[shipElement.currentLevel] * 0.75));

        upgradeButton.onClick.AddListener(() =>
        {
            shipElement.UpgradeElement();
            SetUpgradePrice(shipElement.costs[shipElement.currentLevel]);
            SetDeletionRefund((int)(shipElement.costs[shipElement.currentLevel] * 0.75));
        });
    }
    
    public void SetUpgradePrice(int price)
    {
        //upgradeAvailebleImg.gameObject.SetActive(true);
        //upgradeUnavailebleImg.gameObject.SetActive(false);
        upgradeCostText.gameObject.SetActive(true);
        upgradeCostText.text = $"Upg: {price}";
    }

    public void SetDeletionRefund(int refund)
    {
        deletionRefundText.text = refund.ToString();
        deletionRefundText.text = $"Destroy: {refund}";
    }

    public void SetUpgradeUnavailable()
    {
        //upgradeAvailebleImg.gameObject.SetActive(false);
        //upgradeUnavailebleImg.gameObject.SetActive(true);
        upgradeCostText.gameObject.SetActive(false);

    }

    public void SetKeybindings(List<KeyCode> keybindings)
    {
        int index = 0;
        foreach (KeyCode keyCode in keybindings)
        {
            this.keybindings[index++].text = keyCode.ToString();
            index++;
        }
    }
}
