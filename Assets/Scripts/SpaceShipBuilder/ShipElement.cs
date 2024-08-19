using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ShipElement : MonoBehaviour, IDamagable
{
    public enum ShipElementType
    {
        EMPTY,
        ENGINE,
        CORE,
        WEAPON,
        FULL
    }

    protected float maxHp;
    [SerializeField] private float HP;

    protected Color baseColor;

    protected Color GetColor { get
        {
            float multiplier = (1.0f - HP/maxHp)/20.0f;
            return new Color(Mathf.Clamp01(baseColor.r + multiplier), Mathf.Clamp01(baseColor.g - multiplier), Mathf.Clamp01(baseColor.b - multiplier));
                } }

    protected SpriteRenderer rend;
    private void Awake() => rend = GetComponent<SpriteRenderer>();
    private void Start()
    {
        maxHp = HP;
        baseColor = rend.color;
    }

    protected Orientation orientation;
    protected Vector2Int coord = Vector2Int.zero;
    protected ShipBuilder builder;
    [Header("Element Upgrading")]
    public int currentLevel = 0;
    public Sprite[] levelSprites;
    [Tooltip("How much does it cost to each level. Building is at index 0")]
    public int[] costs; 
    
    public void SetOrientation(Orientation orientation) => this.orientation = orientation;
    public Orientation GetOrientation() => orientation;
    public void SetCoords(Vector2Int coords) => coord = coords;
    public Vector2Int GetCoords() => coord;
    public void SetBuilderRef(ShipBuilder builder) => this.builder = builder;

    private void OnMouseEnter()
    {
        var col = GetColor;
        builder.SetValidCoord(true);
        builder.ChangeActiveElement(coord);
        rend.color = new Color(Mathf.Clamp01(col.r - 0.5f), Mathf.Clamp01(col.g + 0.5f), Mathf.Clamp01(col.b - 0.5f));
    }

    private void OnMouseExit()
    {
        builder.SetValidCoord(false);
        rend.color = GetColor;
    }

    public abstract ShipElementType GetElementType();

    public void TakeDamage(float amount)
    {
        HP -= amount;

        if(HP < 0)
            OnDeath();
    }

    public abstract void OnDeath();

    public void UpgradeElement() {
        // Add stats to the element
        // Assuming that the element is upgradable meaning: 
        // 1. It is not on the max level
        // 2. Player has enough money to upgrade it

        MoneyManager.Instance.RemoveMoney(costs[currentLevel]);
        currentLevel++;
        rend.sprite = levelSprites[currentLevel];
    }
}
