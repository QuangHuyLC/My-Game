using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    // Thời gian tồn tại (giây). Chỉnh số này cho khớp với độ dài animation của ông.
    // Ví dụ animation 6 ảnh chạy trong 0.3 giây thì điền 0.3f.
    public float lifeTime = 0.2f; 

    void Start()
    {
        // Ra lệnh tự hủy sau lifeTime giây
        Destroy(gameObject, lifeTime);
    }
}