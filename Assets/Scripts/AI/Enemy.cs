using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    private State currentState;
    [SerializeField] Transform target;
    [SerializeField] Transform explosion;
    private Rigidbody2D rb;
    [SerializeField] private float weaponsRange = 5f;

    [SerializeField]
    private PID_Profile pidPos;
    [SerializeField]
    private PID_Profile pidRot;

    [SerializeField] BulletSource gun;

    private PIDController anglePIDController;
    private PIDController xPIDController;
    private PIDController yPIDController;

    //private Vector3 debugEnemyNode;
    //private List<Node> debugPath;

    private Vector2 targetPosition;
    private Vector2 targetRotation;
    [Header("Health")]
    [SerializeField] private float maxHp = 100;


    // Start is called before the first frame update
    void Start()
    {
        gun = GetComponentInChildren<BulletSource>();

        currentState = new IdleState(this);
        rb = GetComponent<Rigidbody2D>();
        targetPosition = target.position;
        targetRotation = target.position;
        anglePIDController = new PIDController(pidRot.P, pidRot.I, pidRot.D);
        xPIDController = new PIDController(pidPos.P, pidPos.I, pidPos.D);
        yPIDController = new PIDController(pidPos.P, pidPos.I, pidPos.D);
    }

    private void OnDrawGizmos()
    {
        //if (debugPath == null) return;
        //Gizmos.color = Color.white;
        //Gizmos.DrawSphere(debugEnemyNode, 0.25f);
        //Gizmos.color = Color.red;

        //for (int i = 0; i < debugPath.Count - 1; i++)
        //{
        //    Vector3 p1 = new Vector3(debugPath[i].Data.Item1, debugPath[i].Data.Item2, 0);
        //    Vector3 p2 = new Vector3(debugPath[i + 1].Data.Item1, debugPath[i + 1].Data.Item2, 0);
        //    //Debug.Log(p1);
        //    Gizmos.DrawLine(p1, p2);
        //}
    }
    //public void SetDebugPath(List<Node> path) => debugPath = path;

    //public void SetDebugEnemyNode(Node node)
    //{
    //    debugEnemyNode = new Vector3(node.Data.Item1, node.Data.Item2, 0);
    //}

    // Update is called once per frame
    void Update()
    {
        anglePIDController.SetPID(pidRot.P, pidRot.I, pidRot.D);
        xPIDController.SetPID(pidPos.P, pidPos.I, pidPos.D);
        yPIDController.SetPID(pidPos.P, pidPos.I, pidPos.D);

        currentState = currentState.PlayState();
    }

    private void FixedUpdate()
    {
        Vector2 targetDirection = (targetRotation - (Vector2)transform.position).normalized;
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
    public void SetTargetRotation(Vector2 position) { targetRotation = position; }
    public void SetTarget(Transform transform) { target = transform; }


    public void TakeDamage(float damage)
    {
        maxHp -= damage;
        if (maxHp <= 0)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
