using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    // Kéo cái Canvas (có gắn script BossIntroController) vào đây
    public BossIntroController introController; 
    
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ kích hoạt 1 lần duy nhất khi Player chạm vào
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            if (introController != null)
            {
                // --- SỬA Ở ĐÂY: Gọi đúng tên hàm BatDauIntro ---
                introController.BatDauIntro(); 
            }
        }
    }
}