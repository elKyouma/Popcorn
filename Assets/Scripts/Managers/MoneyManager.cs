using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [SerializeField] private int money = 0;
    public int Amount => money;

    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (text == null)
            return;
        text.text = money.ToString();
    }
}
