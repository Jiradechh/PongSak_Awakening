using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public Vector3 rotationOffset = new Vector3(30f, 0f, 0f);
    public float smoothSpeed = 5f; 

    private Transform player;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer(); 
            return;
        }

        FollowPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ No Player found with tag 'Player'!");
        }
    }

    private void FollowPlayer()
    {
        Vector3 desiredPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        Quaternion desiredRotation = Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed * Time.deltaTime);
    }
}
