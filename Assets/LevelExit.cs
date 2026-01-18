using UnityEngine;
using UnityEngine.SceneManagement; // <-- Cần dòng này để quản lý màn chơi

public class LevelExit : MonoBehaviour
{
    [Header("Settings")]
    // Tên màn chơi tiếp theo (phải giống hệt tên file trong thư mục Project)
    public string nextLevelName = "Level2"; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ khi thằng Player chạm vào thì mới chuyển cảnh
        // (Để tránh quái vật hay đạn bay vào cũng bị chuyển cảnh oan)
        if (collision.CompareTag("Player")) 
        {
            Debug.Log("Chạm cửa! Đang chuyển sang: " + nextLevelName);
            SceneManager.LoadScene(nextLevelName);
        }
    }
}