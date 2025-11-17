using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletTrajectoryPreview : MonoBehaviour
{
    public Transform firePoint;
    public float speed = 10f;
    public int lineSteps = 30;
    public float stepTime = 0.1f;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = lineSteps;
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.yellow;
        line.endColor = new Color(1, 1, 0, 0); // fondu vers transparent
    }

    void Update()
    {
        if (firePoint == null) return;

        // Direction basée sur la rotation réelle de l’arme
        Vector2 startPos = firePoint.position;
        Vector2 velocity = firePoint.right * speed;

        // Gravité (même que Physics2D)
        Vector2 gravity = Physics2D.gravity * Time.fixedDeltaTime;

        Vector3[] points = new Vector3[lineSteps];

        for (int i = 0; i < lineSteps; i++)
        {
            points[i] = startPos;
            velocity += gravity * stepTime;
            startPos += velocity * stepTime;
        }

        line.SetPositions(points);
    }
}
