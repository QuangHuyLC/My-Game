using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Cần cái này để chỉnh UI

public class LevelExit : MonoBehaviour
{
    [Header("--- Cấu hình Màn Chơi ---")]
    public string nextLevelName = "Level2"; 

    [Header("--- Cấu hình Hiệu Ứng ---")]
    public Image manHinhDen; // Kéo cái tấm Panel màu đen vào đây
    public float thoiGianChuyenCanh = 1.5f; // Thời gian tối dần

    private bool daChamCua = false;

    void Start()
    {
        // Đảm bảo lúc đầu tấm màn đen tàng hình (nếu lỡ để hiện)
        if (manHinhDen != null) 
        {
            manHinhDen.gameObject.SetActive(false);
            var color = manHinhDen.color;
            color.a = 0f;
            manHinhDen.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu Player chạm vào và chưa kích hoạt lần nào
        if (collision.CompareTag("Player") && !daChamCua) 
        {
            daChamCua = true; // Khóa lại không cho kích hoạt nhiều lần
            StartCoroutine(ChuyenCanhMuotMa());
        }
    }

    IEnumerator ChuyenCanhMuotMa()
    {
        Debug.Log("Chạm cửa! Bắt đầu Fade Out...");

        // 1. Bật tấm màn đen lên (đang trong suốt)
        if (manHinhDen != null)
        {
            manHinhDen.gameObject.SetActive(true);
            float timer = 0f;

            while (timer < thoiGianChuyenCanh)
            {
                timer += Time.unscaledDeltaTime;
                
                // Dùng SmoothStep cho mượt (nhanh ở giữa, chậm 2 đầu)
                float t = Mathf.SmoothStep(0f, 1f, timer / thoiGianChuyenCanh);
                
                // Tăng độ đậm (Alpha) từ 0 lên 1
                Color c = manHinhDen.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                manHinhDen.color = c;

                yield return null;
            }
            
            // Đảm bảo đen kịt 100% trước khi load
            Color finalColor = manHinhDen.color;
            finalColor.a = 1f;
            manHinhDen.color = finalColor;
        }

        // 2. Đợi thêm xíu cho người chơi định thần (0.5s)
        yield return new WaitForSecondsRealtime(0.5f);

        // 3. Load màn mới
        SceneManager.LoadScene(nextLevelName);
    }
}