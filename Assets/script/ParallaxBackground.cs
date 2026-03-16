using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Camera chính của game")]
    public Transform mainCamera;

    [Header("Tốc độ trôi (0 = Đứng im, 1 = Đi cùng Camera)")]
    [Range(0f, 1f)]
    public float parallaxEffectMultiplier;

    private Vector3 lastCameraPosition;

    void Start()
    {
        // Tự động tìm Camera chính nếu sếp quên kéo vào
        if (mainCamera == null) mainCamera = Camera.main.transform;
        
        lastCameraPosition = mainCamera.position;
    }

    void LateUpdate() // Dùng LateUpdate để chống giật lag khi Camera di chuyển
    {
        // Tính xem Camera đã nhích đi bao nhiêu so với khung hình trước
        Vector3 deltaMovement = mainCamera.position - lastCameraPosition;
        
        // Di chuyển tấm Background theo tỷ lệ Parallax
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, deltaMovement.y * parallaxEffectMultiplier, 0);
        
        // Cập nhật lại vị trí Camera cho khung hình tiếp theo
        lastCameraPosition = mainCamera.position;
    }
}