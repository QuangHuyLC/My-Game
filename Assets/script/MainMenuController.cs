using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

public class MainMenuController : MonoBehaviour
{
    [Header("--- CÀI ĐẶT TÊN SCENE ---")]
    [Tooltip("Gõ chính xác tên Scene Map 1 (Map bắt đầu chơi) vào đây")]
    public string tenSceneGame = "Level1"; 

    [Header("--- KHÓA NÚT LOAD ---")]
    public Button nutLoadGame; 

    void Start()
    {
        if (nutLoadGame != null)
        {
            // Quét sổ tay xem có Checkpoint chưa?
            if (PlayerPrefs.GetInt("HasCheckpoint", 0) == 1)
            {
                nutLoadGame.interactable = true; // Có Save -> Sáng nút, cho bấm!
            }
            else
            {
                nutLoadGame.interactable = false; // Không Save -> Xám xịt, cấm click!
            }
        }
    }

    // Hàm này chạy khi bấm nút NEW GAME
    public void PlayNewGame()
    {
        // 1. Tẩy não cuốn sổ: Xóa chốt lưu Checkpoint VÀ xóa luôn tên Map cũ đi
        PlayerPrefs.DeleteKey("HasCheckpoint"); 
        PlayerPrefs.DeleteKey("SavedMap"); 
        
        // 2. Chuyển sang màn chơi đầu tiên (Map 1)
        SceneManager.LoadScene(tenSceneGame); 
    }

    // Hàm này chạy khi bấm nút LOAD GAME
    public void LoadGame()
    {
        // Kiểm tra xem cuốn sổ tay có ghi Checkpoint nào chưa?
        if (PlayerPrefs.GetInt("HasCheckpoint", 0) == 1)
        {
            string mapCanLoad = PlayerPrefs.GetString("SavedMap", tenSceneGame);
            
            Debug.Log("Đang tải Checkpoint cũ tại Map: " + mapCanLoad);
            SceneManager.LoadScene(mapCanLoad);
        }
    }

    // Hàm này chạy khi bấm nút EXIT
    public void QuitGame()
    {
        Debug.Log("Đang thoát Game..."); 
        Application.Quit(); 
    }
}