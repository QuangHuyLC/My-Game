using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Tạo một cái "Đường dây nóng" để các script khác dễ dàng gọi cho thằng DJ này
    public static MusicManager instance; 

    [Header("--- CÀI ĐẶT NHẠC ---")]
    public AudioSource audioSource; // Cái loa

    [Space(10)]
    public AudioClip nhacNenChinh;  // Nhạc lúc đang chơi máu lửa
    [Range(0f, 1f)] public float amLuongNhacChinh = 0.5f; // Chỉnh to nhỏ ở đây

    [Space(10)]
    public AudioClip nhacLucChet;   // Nhạc u ám lúc Game Over
    [Range(0f, 1f)] public float amLuongNhacChet = 0.8f; // Chỉnh to nhỏ ở đây


    void Awake()
    {
        // Giữ cho thằng DJ này không bị bay màu khi chuyển Map
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    void Start()
    {
        // Vừa vào game là mở nhạc xập xình ngay
        PlayNhacChinh();
    }

    // Hàm gọi nhạc chơi game
    public void PlayNhacChinh()
    {
        if (audioSource != null && nhacNenChinh != null)
        {
            audioSource.clip = nhacNenChinh;
            audioSource.volume = amLuongNhacChinh; // Set đúng mức âm lượng sếp kéo
            audioSource.Play();
        }
    }

    // Hàm gọi nhạc Game Over
    public void PlayNhacChet()
    {
        if (audioSource != null && nhacLucChet != null)
        {
            audioSource.clip = nhacLucChet;
            audioSource.volume = amLuongNhacChet; // Set đúng mức âm lượng sếp kéo
            audioSource.Play();
        }
    }
}