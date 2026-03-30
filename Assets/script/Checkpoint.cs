using UnityEngine;
using UnityEngine.SceneManagement; // ---> BẮT BUỘC thêm thư viện này để lấy tên Map

public class Checkpoint : MonoBehaviour
{
    [Header("--- HIỆU ỨNG ÂM THANH ---")]
    public AudioSource audioSource;
    public AudioClip saveSound; // Tiếng "Ting" báo hiệu lưu game

    [Header("--- HIỆU ỨNG HÌNH ẢNH ---")]
    [Tooltip("Kéo Prefab hiệu ứng lấp lánh (hoặc xịt máu tạm) vào đây")]
    public GameObject vfxEffectPrefab; 
    
    [Tooltip("Kéo Animator của bức tượng vào đây (nếu có làm Animation chớp sáng)")]
    public Animator statueAnimator;    

    // Biến này để chặn Player cà qua cà lại lưu liên tục
    private bool daLuu = false; 

    // Hàm này tự động chạy khi có một vật thể (được đánh dấu Is Trigger) chạm vào bức tượng
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem người chạm vào có đúng là "Player" không, và tượng này chưa từng được lưu
        if (collision.CompareTag("Player") && !daLuu)
        {
            daLuu = true; // Khóa chốt! Từ giờ đi ngang qua nó không thèm lưu đè nữa

            // 1. LƯU TỌA ĐỘ VÀO "SỔ TAY" CỦA UNITY
            PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
            PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
            
            // BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ NINJA BIẾT ĐƯỜNG ĐỌC SỔ
            PlayerPrefs.SetInt("HasCheckpoint", 1); 

            // ---> [NÂNG CẤP CHÍ MẠNG]: Ghi nhớ luôn tên cái Map sếp đang đứng (Map 1, Map 2, Map 3...) <---
            PlayerPrefs.SetString("SavedMap", SceneManager.GetActiveScene().name);

            PlayerPrefs.Save(); // Bút sa gà chết, chốt đơn!

            Debug.Log("Đã lưu Checkpoint tại Map: " + SceneManager.GetActiveScene().name + " - Tọa độ: " + transform.position);

            // 2. PHÁT TIẾNG ĐỘNG
            if (audioSource != null && saveSound != null)
            {
                audioSource.PlayOneShot(saveSound);
            }

            // 3. ĐẺ RA HIỆU ỨNG (VFX) VÀ TỰ DỌN RÁC
            if (vfxEffectPrefab != null)
            {
                // Đẻ ra cái hiệu ứng ngay tại vị trí bức tượng, và nhét nó vào cái túi tên là "hieuUng"
                GameObject hieuUng = Instantiate(vfxEffectPrefab, transform.position, Quaternion.identity);
                
                // Tiêu diệt cái "hieuUng" đó sau 1.5 giây
                Destroy(hieuUng, 1.5f); 
            }

            // 4. CHẠY ANIMATION TƯỢNG (Nếu có)
            if (statueAnimator != null)
            {
                statueAnimator.SetTrigger("Glow"); 
            }
        }
    }
}