using UnityEngine;
using UnityEngine.UIElements;

public abstract class ShipElement : MonoBehaviour, IDamagable
{
    public enum ShipElementType
    {
        EMPTY,
        ENGINE,
        CORE,
        FULL
    }

    protected Color baseColor;

    protected SpriteRenderer rend;
    private void Awake() => rend = GetComponent<SpriteRenderer>();
    private void Start() => baseColor = rend.color;

    protected Orientation orientation;
    protected Vector2Int coord = Vector2Int.zero;
    protected ShipBuilder builder;
    
    public void SetOrientation(Orientation orientation) => this.orientation = orientation;
    public Orientation GetOrientation() => orientation;
    public void SetCoords(Vector2Int coords) => this.coord = coords;
    public Vector2Int GetCoords() => coord;
    public void SetBuilderRef(ShipBuilder builder) => this.builder = builder;

    private void OnMouseEnter()
    {
        builder.SetValidCoord(true);
        builder.ChangeActiveElement(coord);
        rend.color = new Color(baseColor.r - 0.5f, baseColor.g + 0.5f, baseColor.b - 0.5f);
    }

    private void OnMouseExit()
    {
        builder.SetValidCoord(false);
        rend.color = baseColor;
    }

    public abstract ShipElementType GetElementType();

    public void TakeDamage(float amount)
    {
    }
}
