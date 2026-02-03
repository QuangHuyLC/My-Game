using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossIntroController : MonoBehaviour
{
    [Header("--- KẾT NỐI UI ---")]
    public RectTransform topBar;    
    public RectTransform bottomBar; 
    public RectTransform bossImage; 
    public RectTransform bossName;  

    [Header("--- KẾT NỐI GAMEPLAY ---")]
    public MonoBehaviour playerScript;   
    public Transform bossObject; 

    [Header("--- CẤU HÌNH CAMERA ---")]
    public float doZoom = 2.5f;     
    public float tocDoZoom = 1.0f;
    public Vector2 cameraOffset; 

    [Header("--- CẤU HÌNH UI ---")]
    public float tocDoThanhDen = 0.5f; 
    public float tocDoNoiDung = 0.8f;  
    public float thoiGianDungHinh = 2.0f; 

    private CanvasGroup group;
    private Camera mainCam;
    private MonoBehaviour cinemachineBrain;

    private float startSize;
    private Vector3 startPos;
    
    private Vector2 startTop, endTop, startBottom, endBottom;
    private Vector2 startImg, endImg, startName, endName;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        
        mainCam = Camera.main;

        // --- ĐÃ XÓA ĐOẠN CODE TỰ SẮP XẾP HIERARCHY ---
        // Bây giờ ông xếp sao trong Unity thì vào game nó y hệt vậy.
        // ---------------------------------------------

        // 1. LƯU VỊ TRÍ ĐẸP (Hiện tại trong Scene)
        if(topBar) endTop = topBar.anchoredPosition;
        if(bottomBar) endBottom = bottomBar.anchoredPosition;
        if(bossImage) endImg = bossImage.anchoredPosition;
        if(bossName) endName = bossName.anchoredPosition;

        // 2. TÍNH VỊ TRÍ ẨN
        if(topBar) startTop = new Vector2(endTop.x, endTop.y + 300f); 
        if(bottomBar) startBottom = new Vector2(endBottom.x, endBottom.y - 300f);
        
        float screenWidth = 1920f; 
        if (bossImage) startImg = new Vector2(endImg.x + screenWidth, endImg.y);
        if (bossName) startName = new Vector2(endName.x - screenWidth, endName.y);

        // 3. ẨN NGAY LẬP TỨC
        ResetVeBanDau();
        group.alpha = 0f; 
    }

    void ResetVeBanDau()
    {
        if(topBar) topBar.anchoredPosition = startTop;
        if(bottomBar) bottomBar.anchoredPosition = startBottom;
        if(bossImage) bossImage.anchoredPosition = startImg;
        if(bossName) bossName.anchoredPosition = startName;
    }

    public void BatDauIntro()
    {
        if (bossObject == null) {
            Debug.LogError("CHƯA KÉO BOSS KÌA!");
            return;
        }
        StartCoroutine(ChayIntro());
    }

    IEnumerator ChayIntro()
    {
        // 1. TẮT CINEMACHINE
        if (mainCam != null)
        {
            cinemachineBrain = mainCam.GetComponent("CinemachineBrain") as MonoBehaviour;
            if (cinemachineBrain != null) cinemachineBrain.enabled = false;
        }

        // 2. KHÓA GAME
        Time.timeScale = 0f; 

        // 3. ZOOM CAMERA
        if (mainCam != null)
        {
            startSize = mainCam.orthographicSize;
            startPos = mainCam.transform.position;

            Vector3 bossPos = bossObject.position;
            Vector3 targetPos = new Vector3(bossPos.x + cameraOffset.x, bossPos.y + cameraOffset.y, startPos.z);

            yield return StartCoroutine(MoveCamera(startPos, targetPos, startSize, doZoom, tocDoZoom));
        }

        // 4. HIỆN UI
        group.alpha = 1f;
        if(topBar) StartCoroutine(MoveUI(topBar, startTop, endTop, tocDoThanhDen));
        if(bottomBar) yield return StartCoroutine(MoveUI(bottomBar, startBottom, endBottom, tocDoThanhDen));

        if(bossImage) StartCoroutine(MoveUI(bossImage, startImg, endImg, tocDoNoiDung));
        if(bossName) yield return StartCoroutine(MoveUI(bossName, startName, endName, tocDoNoiDung));

        // 5. NGẮM BOSS
        yield return new WaitForSecondsRealtime(thoiGianDungHinh);

        // 6. RÚT UI
        if(bossImage) StartCoroutine(MoveUI(bossImage, endImg, startImg, tocDoNoiDung/2));
        if(bossName) StartCoroutine(MoveUI(bossName, endName, startName, tocDoNoiDung/2));
        yield return new WaitForSecondsRealtime(0.3f);
        if(topBar) StartCoroutine(MoveUI(topBar, endTop, startTop, tocDoThanhDen));
        if(bottomBar) yield return StartCoroutine(MoveUI(bottomBar, endBottom, startBottom, tocDoThanhDen));

        // 7. TRẢ CAMERA
        group.alpha = 0f;
        if (mainCam != null)
        {
            yield return StartCoroutine(MoveCamera(mainCam.transform.position, startPos, doZoom, startSize, 0.5f));
        }

        // 8. KẾT THÚC
        if (cinemachineBrain != null) cinemachineBrain.enabled = true; 
        Time.timeScale = 1f;
        
        if (playerScript != null) playerScript.enabled = true;
        MonoBehaviour[] bossScripts = bossObject.GetComponents<MonoBehaviour>();
        foreach(var script in bossScripts) script.enabled = true;

        Destroy(gameObject);
    }

    IEnumerator MoveCamera(Vector3 sPos, Vector3 ePos, float sSize, float eSize, float time)
    {
        float t = 0;
        while(t < time)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0, 1, t/time);
            mainCam.transform.position = Vector3.Lerp(sPos, ePos, p);
            mainCam.orthographicSize = Mathf.Lerp(sSize, eSize, p);
            yield return null;
        }
        mainCam.transform.position = ePos;
        mainCam.orthographicSize = eSize;
    }

    IEnumerator MoveUI(RectTransform target, Vector2 start, Vector2 end, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; 
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            target.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        target.anchoredPosition = end;
    }
}