using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("Sống được mấy giây thì nổ?")]
    public float timeToLive = 2f; // Sếp tự chỉnh số này cho vừa với độ dài hiệu ứng

    void Start()
    {
        // Gắn bom hẹn giờ: Tiêu diệt chính GameObject này sau đúng [timeToLive] giây
        Destroy(gameObject, timeToLive);
    }
}