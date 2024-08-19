using System.Collections.Generic;
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
    protected List<KeyCode> bindings = new();

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

    public List<KeyCode> GetBindings() => bindings;
    public void ToogleBinding(KeyCode code)
    {
        if (bindings.Find(x => x == code) != KeyCode.None)
            bindings.Remove(code);
        else
            bindings.Add(code);
    }
    public abstract ShipElementType GetElementType();

    public void TakeDamage(float amount)
    {
        HP -= amount;

        if(HP < 0)
            OnDeath();
    }

    public abstract void OnDeath();
}
