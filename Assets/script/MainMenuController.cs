using UnityEngine;
using UnityEngine.SceneManagement; // BẮT BUỘC có cái này để chuyển Scene

public class MainMenuController : MonoBehaviour
{
    // Hàm này chạy khi bấm nút NEW GAME
    public void PlayNewGame()
    {
        // 🚨 QUAN TRỌNG: Sếp phải gõ đúng y xì đúc tên cái Scene chứa map chơi game của sếp vào đây!
        SceneManager.LoadScene("Level1"); 
    }

    // Hàm này chạy khi bấm nút LOAD GAME
    public void LoadGame()
    {
        // Tính năng Load cần hệ thống Save Data hơi dài, tui để tạm ở đây cho nó in ra log đã nha!
        Debug.Log("Chức năng Load Game đang được xây dựng!");
    }

    // Hàm này chạy khi bấm nút EXIT
    public void QuitGame()
    {
        Debug.Log("Đã thoát Game!"); 
        Application.Quit(); // Lệnh này build ra game thật bấm mới tắt, trong Unity Editor nó không tự tắt đâu nha.
    }
}