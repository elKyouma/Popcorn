using UnityEngine;

public class Enemy : MonoBehaviour
{
    private State currentState;
    [SerializeField] Transform target;
    [SerializeField] float enemySpeed = 2f;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float weaponsRange = 2.5f;


    // Start is called before the first frame update
    void Start()
    {
        currentState = new IdleState(this);
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.PlayState();
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDirection.x, moveDirection.y) * enemySpeed;
    }

    public Transform GetTarget() => target;
    public Rigidbody2D GetEnemyRb() => rb;
    public float GetWeaponsRange() => weaponsRange;
    public void SetMoveDirection(Vector2 direction) { moveDirection = direction; }

}
