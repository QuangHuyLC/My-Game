using UnityEngine;

public class SniperEnemy : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab; // Tí nữa mình sẽ tạo cái này
    public float fireRate = 2f;
    private float nextFireTime;

    void Update()
    {
        // Tạm thời cho nó tự động bắn mỗi 2 giây để ông test chém đạn
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Sinh ra viên đạn tại FirePoint
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}