using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("--- UI KÉO THẢ VÀO ĐÂY ---")]
    public GameObject pauseMenuUI; // Kéo cái PauseMenu_Panel vào đây

    // Biến này để các script khác biết game có đang dừng không
    public static bool isPaused = false; 

    void Start()
    {
        // Chắc cú vừa vào game là rã đông thời gian và giấu bảng đi
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;
    }

    void Update()
    {
        // Lắng nghe người chơi bấm phím ESC (Dùng Input System mới)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame(); // Đang dừng thì bấm ESC để chơi tiếp
            }
            else
            {
                PauseGame();  // Đang chơi thì bấm ESC để dừng
            }
        }
    }

    // 👉 HÀM DỪNG GAME
    public void PauseGame()
    {
        pauseMenuUI.SetActive(true); // Hiện bảng UI
        Time.timeScale = 0f;         // ĐÓNG BĂNG THỜI GIAN!
        isPaused = true;
    }

    // 👉 HÀM CHƠI TIẾP
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Giấu bảng UI
        Time.timeScale = 1f;          // RÃ ĐÔNG THỜI GIAN!
        isPaused = false;
    }

    // 👉 HÀM VỀ MENU CHÍNH
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // RẤT QUAN TRỌNG: Rã đông trước khi qua scene khác, nếu không scene mới cũng bị đứng hình!
        SceneManager.LoadScene("Main Menu"); // Nhớ gõ đúng tên Scene Menu
    }
}