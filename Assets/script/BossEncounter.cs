using UnityEngine;
using System.Collections;

public class BossEncounter : MonoBehaviour
{
    [Header("--- THÀNH PHẦN ĐẠO DIỄN ---")]
    public GameObject bossHealthUI;    
    public GameObject cinematicCamera; 
    public Playermovement playerScript;

    // 👉 [MỚI THÊM] Kéo con Boss vào đây để kích hoạt nó
    public BossAI bossAI; 

    [Header("--- THỜI GIAN CHIẾU PHIM ---")]
    public float timeLookingAtBoss = 2.0f; 
    public float timeToPanBack = 1.0f;     

    private bool hasTriggered = false;

    void Start()
    {
        if (bossHealthUI != null) bossHealthUI.SetActive(false);
        if (cinematicCamera != null) cinematicCamera.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayCinematic());
        }
    }

    IEnumerator PlayCinematic()
    {
        // 1. Khóa cứng Ninja
        if (playerScript != null) playerScript.LockPlayerForUltimate(true);

        // 2. Zoom máy quay vào Boss
        if (cinematicCamera != null) cinematicCamera.SetActive(true);

        yield return new WaitForSeconds(timeLookingAtBoss);

        // 3. Hiện thanh máu Boss
        if (bossHealthUI != null) bossHealthUI.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // 4. Trả máy quay về cho Ninja
        if (cinematicCamera != null) cinematicCamera.SetActive(false);

        yield return new WaitForSeconds(timeToPanBack);

        // 5. Trả lại tự do cho Ninja
        if (playerScript != null) playerScript.LockPlayerForUltimate(false);
        
        // 👉 [MỚI THÊM] BẬT CÔNG TẮC CHO BOSS ĐI TÌM PLAYER!
        if (bossAI != null) 
        {
            bossAI.isActivated = true; 
        }

        // Tự hủy cái Trigger sau khi dùng xong
        Destroy(gameObject);
    }
}