using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("--- THAM CHIẾU ---")]
    [Tooltip("Kéo Camera chính vào đây")]
    public Transform cameraTransform;

    [Header("--- CÀI ĐẶT TỐC ĐỘ ---")]
    [Tooltip("Tốc độ trôi của lớp này so với camera. Bầu trời = 0.05, Núi xa = 0.5, Núi gần = 0.8")]
    public float parallaxFactor; 

    // Biến để lưu trữ vật liệu và vị trí camera
    private Material mat;
    private float offset;

    void Start()
    {
        // 1. Tự lấy cái Material đang gắn trên vật này (Quad)
        mat = GetComponent<MeshRenderer>().material;

        // 2. Nếu sếp lười không kéo camera, code sẽ tự tìm Camera chính
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 3. Tính toán độ trôi dựa trên vị trí của Camera và Factor sếp cài đặt
        // Code này chỉ làm parallax theo trục X (ngang), sếp muốn Y thì làm tương tự
        offset = cameraTransform.position.x * parallaxFactor;

        // 4. CẬP NHẬT TEXTURE OFFSET - ĐÂY LÀ PHÉP THUẬT!
        // Nó sẽ kéo tấm ảnh trôi đi trong khi cái Quad vẫn đứng im
        mat.mainTextureOffset = new Vector2(offset, 0);
    }
}