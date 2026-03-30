using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("Lượng máu hồi phục")]
    public float healAmount = 30f;

    [Header("Hiệu ứng bay lơ lửng")]
    public float hoverSpeed = 2f;    // Tốc độ nhấp nhô
    public float hoverHeight = 0.2f; // Độ cao nhấp nhô
    private Vector2 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Thuật toán nhấp nhô ảo ma Canada bằng hàm Sin
        float newY = startPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Quét trúng Player
        if (col.CompareTag("Player"))
        {
            // Bắt đống code PlayerHealth của sếp ra
            PlayerHealth ninjaHealth = col.GetComponent<PlayerHealth>();
            
            // Chỉ cho ăn khi máu Ninja CHƯA ĐẦY
            if (ninjaHealth != null && ninjaHealth.currentHealth < ninjaHealth.maxHealth)
            {
                // Bơm máu
                ninjaHealth.Heal(healAmount);
                
                // Ăn xong thì bay màu!
                Destroy(gameObject);
            }
        }
    }
}