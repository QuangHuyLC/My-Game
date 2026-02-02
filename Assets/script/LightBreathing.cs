using UnityEngine;

public class LightBreathing : MonoBehaviour
{
    public float tocDoNhipTho = 2f; // Tốc độ sáng/tối
    public float doSangToiDa = 0.5f; // Alpha cao nhất (0 đến 1)
    public float doSangToiThieu = 0.2f; // Alpha thấp nhất

    private SpriteRenderer spriteRen;

    void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRen != null)
        {
            // Dùng hàm Sin để tạo nhịp lên xuống mượt mà
            float alpha = Mathf.Lerp(doSangToiThieu, doSangToiDa, (Mathf.Sin(Time.time * tocDoNhipTho) + 1.0f) / 2.0f);
            
            // Cập nhật lại màu với alpha mới
            Color colorCu = spriteRen.color;
            spriteRen.color = new Color(colorCu.r, colorCu.g, colorCu.b, alpha);
        }
    }
}