using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [Header("Lượng máu trừ đi")]
    public float trapDamage = 5f;

    // Hàm này như cái bẫy chuột, hễ có vật thể lọt vào vùng Trigger là nó cắn!
    void OnTriggerEnter2D(Collider2D col)
    {
        // Xét xem kẻ vừa đạp lên có mác (Tag) là Player không?
        if (col.CompareTag("Player"))
        {
            // Bắt đống code Playermovement của nạn nhân ra
            Playermovement ninja = col.GetComponent<Playermovement>();
            if (ninja != null)
            {
                // Gọi hàm Ăn Đòn bên máy Ninja, truyền số sát thương và vị trí của bẫy qua
                ninja.TakeDamage(trapDamage, transform);
            }
        }
    }
}