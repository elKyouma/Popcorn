using UnityEngine;

public class Enemy : MonoBehaviour
{
    private State currentState;
    [SerializeField] Transform target;
    private Rigidbody2D rb;
    private float weaponsRange = 5f;

    [SerializeField]
    [Range(0f, 10f)]
    float P, I, D;

    [SerializeField]
    [Range(0f, 10f)]
    float angleP, angleI, angleD;

    [SerializeField] BulletSource gun;

    private PIDController anglePIDController;
    private PIDController xPIDController;
    private PIDController yPIDController;

    private Vector2 targetPosition;

    static int x = 1;
    // Start is called before the first frame update
    void Start()
    {
        gun = GetComponentInChildren<BulletSource>();

        P = x;
        I = x;
        D = 2 * x;
        x++;
        currentState = new IdleState(this);
        rb = GetComponent<Rigidbody2D>();
        targetPosition = target.position;
        anglePIDController = new PIDController(angleP, angleI, angleD);
        xPIDController = new PIDController(P, I, D);
        yPIDController = new PIDController(P, I, D);
    }

    // Update is called once per frame
    void Update()
    {
        anglePIDController.SetPID(angleP, angleI, angleD);
        xPIDController.SetPID(P, I, D);
        yPIDController.SetPID(P, I, D);
        currentState = currentState.PlayState();
    }

    private void FixedUpdate()
    {
        Vector2 targetDirection = (targetPosition - (Vector2)transform.position).normalized;
        float directionAngle = 360 - Mathf.Atan2(targetDirection.x, targetDirection.y) * Mathf.Rad2Deg;

        float torqueCorrection = anglePIDController.UpdateAngle(Time.fixedDeltaTime, rb.rotation, directionAngle);

        float xForceCorrection = xPIDController.Update(Time.fixedDeltaTime, transform.position.x, targetPosition.x);
        float yForceCorrection = yPIDController.Update(Time.fixedDeltaTime, transform.position.y, targetPosition.y);


        rb.AddTorque(torqueCorrection);
        rb.AddForce(new Vector2(xForceCorrection, yForceCorrection), ForceMode2D.Force);
        //rb.velocity = new Vector2(moveDirection.x, moveDirection.y) * enemySpeed;
    }

    public Transform GetTarget() => target;
    public Rigidbody2D GetEnemyRb() => rb;
    public float GetWeaponsRange() => weaponsRange;
    public BulletSource GetGun() => gun;
    public void SetTargetPosition(Vector2 position) { targetPosition = position; }

}
