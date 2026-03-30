using UnityEngine;

public class TuHuy : MonoBehaviour
{
    public float thoiGianSong = 1.5f; // Sống được 1.5 giây

    void Start()
    {
        // Vừa đẻ ra là cài bom hẹn giờ tự sát luôn!
        Destroy(gameObject, thoiGianSong); 
    }
}