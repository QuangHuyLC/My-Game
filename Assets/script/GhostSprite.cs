using UnityEngine;

public class GhostSprite : MonoBehaviour
{
    private SpriteRenderer mySprite;
    private SpriteRenderer playerSprite;
    private Color ghostColor;
    private float fadeSpeed;

    public void Setup(SpriteRenderer _playerSprite, Color _color, float _duration)
    {
        mySprite = GetComponent<SpriteRenderer>();
        playerSprite = _playerSprite;
        ghostColor = _color;
        
        // 1. Copy hình ảnh từ Player sang cái bóng
        mySprite.sprite = playerSprite.sprite;
        mySprite.flipX = playerSprite.flipX;
        
        // 2. Chỉnh màu và kích thước
        mySprite.color = ghostColor;
        transform.localScale = playerSprite.transform.localScale;
        transform.rotation = playerSprite.transform.rotation;

        // 3. Tính tốc độ mờ (Fade out)
        fadeSpeed = 1f / _duration;

        // 4. Hủy sau thời gian duration
        Destroy(gameObject, _duration);
    }

    void Update()
    {
        // Làm mờ dần Alpha theo thời gian
        if (mySprite != null)
        {
            float newAlpha = mySprite.color.a - (fadeSpeed * Time.deltaTime);
            mySprite.color = new Color(ghostColor.r, ghostColor.g, ghostColor.b, newAlpha);
        }
    }
}