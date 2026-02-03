using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelEntry : MonoBehaviour
{
    [Header("--- Kéo tấm Panel đen vào đây ---")]
    public Image manHinhDen; 
    public float thoiGianMoMan = 1.5f; // Thời gian sáng dần

    void Awake()
    {
        // Bắt buộc màn hình phải đen ngay lập tức khi game vừa load (tránh bị nháy hình)
        if (manHinhDen != null)
        {
            manHinhDen.gameObject.SetActive(true);
            manHinhDen.color = Color.black; // Alpha = 1
        }
    }

    void Start()
    {
        if (manHinhDen != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        
        while (timer < thoiGianMoMan)
        {
            timer += Time.unscaledDeltaTime; // Dùng unscaled để không bị ảnh hưởng nếu game pause
            
            // Dùng SmoothStep cho mượt
            float t = Mathf.SmoothStep(0f, 1f, timer / thoiGianMoMan);
            
            // Giảm độ mờ (Alpha) từ 1 về 0
            if (manHinhDen != null)
            {
                Color c = manHinhDen.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                manHinhDen.color = c;
            }

            yield return null;
        }

        // Sáng hẳn rồi thì tắt tấm panel đi cho đỡ tốn tài nguyên
        if (manHinhDen != null) manHinhDen.gameObject.SetActive(false);
    }
}