using UnityEngine;

public class SideViewCamera : MonoBehaviour
{
    private CameraMode savedMode;
    public enum CameraMode
    {
        FollowPlayer,
        AutoScroll
    }

    [Header("General Settings")]
    public CameraMode mode = CameraMode.FollowPlayer;
    public Transform player;
    public PlayerMovement playerMovement;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Follow Settings")]
    [Range(0.01f, 1f)] public float smoothSpeed = 0.1f;
    public bool lockYPosition = false;
    public bool lockXPosition = false;

    [Header("Auto Scroll Settings")]
    public Vector2 scrollDirection = Vector2.right;
    public float scrollSpeed = 2f;

    private Vector3 velocity = Vector3.zero;

    private void Update()
    {
        if(playerMovement != null && playerMovement.isReviving)
        {
            mode = CameraMode.FollowPlayer;
            lockYPosition = false;
            lockXPosition = false;
        }
        else if (playerMovement != null && !playerMovement.isReviving)
        {
            mode = CameraMode.AutoScroll;
            lockYPosition = true;
            lockXPosition = false;
        }
    }

    void LateUpdate()
    {
        //if (player != null)
        //{
        //    PlayerMovement pm = player.GetComponent<PlayerMovement>();
        //    if (pm != null && pm.isReviving)
        //    {
        //        if (mode != CameraMode.FollowPlayer)
        //        {
        //            savedMode = mode;
        //            mode = CameraMode.FollowPlayer;
        //        }
        //    }
        //    else if (savedMode != mode && !pm.isReviving)
        //    {
        //        mode = savedMode;
        //    }
        //}

        switch (mode)
        {
            case CameraMode.FollowPlayer:
                FollowPlayer();
                break;
            case CameraMode.AutoScroll:
                AutoScrollCamera();
                break;
        }
    }

    // --- Mode 1 : Suivi du joueur avec Lerp/SmoothDamp ---
    void FollowPlayer()
    {
        if (player == null)
            return;

        Vector3 targetPos = player.position + offset;

        // Verrouillage des axes selon les bools
        if (lockXPosition)
            targetPos.x = transform.position.x;

        if (lockYPosition)
            targetPos.y = transform.position.y;

        targetPos.z = offset.z;

        // lissage fluide
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothSpeed);
    }

    // --- Mode 2 : Défilement automatique ---
    void AutoScrollCamera()
    {
        transform.position += (Vector3)(scrollDirection.normalized * scrollSpeed * Time.deltaTime);
    }

    // --- Debug visuel ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + offset, 0.2f);
    }
}
