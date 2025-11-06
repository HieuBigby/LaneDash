using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private float moveSpeed;
    private float destroyYPosition;
    private ObstacleManager manager;

    public void Initialize(float speed, float bottomY, ObstacleManager obstacleManager)
    {
        moveSpeed = speed;
        destroyYPosition = bottomY;
        manager = obstacleManager;
    }

    private void FixedUpdate()
    {
        // Move downward
        transform.position += Vector3.down * moveSpeed * Time.fixedDeltaTime;

        // Destroy when it goes below screen
        if (transform.position.y < destroyYPosition)
        {
            if (manager != null)
            {
                manager.RemoveObstacle(this);
            }
            Destroy(gameObject);
        }
    }
}
