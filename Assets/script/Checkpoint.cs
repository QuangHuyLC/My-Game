using UnityEngine;

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
            PlayerPrefs.Save(); // Bút sa gà chết, chốt đơn!

            Debug.Log("Đã lưu Checkpoint tại tọa độ: " + transform.position);

            // 2. PHÁT TIẾNG ĐỘNG
            if (audioSource != null && saveSound != null)
            {
                audioSource.PlayOneShot(saveSound);
            }

            // 3. ĐẺ RA HIỆU ỨNG (VFX)
            if (vfxEffectPrefab != null)
            {
                // Đẻ ra cái hiệu ứng ngay tại vị trí bức tượng
                Instantiate(vfxEffectPrefab, transform.position, Quaternion.identity);
            }

            // 4. CHẠY ANIMATION TƯỢNG (Nếu có)
            if (statueAnimator != null)
            {
                statueAnimator.SetTrigger("Glow"); 
            }
        }
    }
}