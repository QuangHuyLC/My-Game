using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem; 

public class VNController : MonoBehaviour
{
    [Header("--- Cấu Hình Intro (Kể chuyện đầu game) ---")]
    public GameObject introPanel; 
    public TextMeshProUGUI introText; 
    [TextArea(3, 5)] public string[] noiDungIntro; 
    public float tocDoHienIntro = 1f; 

    [Header("--- UI Elements ---")]
    public GameObject vnCanvas; 
    public CanvasGroup uiCanvasGroup;
    public TextMeshProUGUI textNoiDung;
    public TextMeshProUGUI textTenNhanVat;
    public Button nutBamToanManHinh;

    [Header("--- Nhân vật ---")]
    public Image anhNhanVat1;
    public Image anhNhanVat2; 
    public Color mauSang = Color.white;
    public Color mauToi = new Color(0.5f, 0.5f, 0.5f, 1f); 

    [Header("--- Cấu hình hội thoại ---")]
    public float tocDoChu = 0.03f;
    public AudioSource amThanhGoChu;
    [Range(0.5f, 2f)] public float doMeoTieng = 0.1f; 

    [Header("--- Kịch Bản Chính ---")]
    public string[] danhSachTen; 
    [TextArea(3, 10)] public string[] danhSachThoai;
    public int[] aiDangNoi; 

    [Header("--- KẾT NỐI PLAYER ---")]
    public Playermovement playerScript; 

    private int cauHienTai = 0;
    private int introIndex = 0;
    private bool dangChayChu = false;
    private bool dangOIntro = true; 
    private Coroutine typeRoutine;
    
    private Vector3 scaleGocNV1;
    private Vector3 scaleGocNV2;

    void Start()
    {
        // 1. Khóa chân nhân vật ngay lập tức
        if (playerScript != null) 
        {
            playerScript.enabled = false; 
            Rigidbody2D rb = playerScript.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero; 
        }

        if (anhNhanVat1) scaleGocNV1 = anhNhanVat1.transform.localScale;
        if (anhNhanVat2) scaleGocNV2 = anhNhanVat2.transform.localScale;

        // Ẩn UI hội thoại trước
        if (uiCanvasGroup != null) uiCanvasGroup.alpha = 0f; 
        if (vnCanvas != null) vnCanvas.SetActive(false); 
        
        // Bắt đầu Intro
        if (introPanel != null && noiDungIntro.Length > 0) {
            introPanel.SetActive(true);
            dangOIntro = true;
            introIndex = 0;
            HienThiIntro();
        } else {
            VaoGameChinh();
        }

        if (nutBamToanManHinh != null)
            nutBamToanManHinh.onClick.AddListener(XuLyInput);
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool bamPhim = false;

        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) bamPhim = true;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) bamPhim = true;

        if (bamPhim) XuLyInput();
    }

    void XuLyInput()
    {
        if (dangOIntro)
        {
            NextIntro();
        }
        else
        {
            // Chỉ xử lý hội thoại nếu Canvas đang bật và đã hiện rõ
            if (vnCanvas != null && vnCanvas.activeSelf && uiCanvasGroup.alpha >= 0.9f) 
            {
                XuLyHoiThoai();
            }
        }
    }

    void HienThiIntro()
    {
        if (introText != null && introIndex < noiDungIntro.Length)
        {
            introText.text = noiDungIntro[introIndex];
        }
    }

    void NextIntro()
    {
        introIndex++;
        if (introIndex < noiDungIntro.Length)
        {
            HienThiIntro();
        }
        else
        {
            StartCoroutine(ChuyenCanhVaoGame());
        }
    }

    // --- SỬA ĐỔI 1: Intro tắt -> Nghỉ 1 tí -> Mới vào game ---
    IEnumerator ChuyenCanhVaoGame()
    {
        dangOIntro = true; // Vẫn khóa input
        
        // Fade Out màn hình đen Intro
        if (introPanel != null)
        {
            Image img = introPanel.GetComponent<Image>();
            TextMeshProUGUI txt = introText;
            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.unscaledDeltaTime * tocDoHienIntro;
                float alpha = Mathf.Lerp(1f, 0f, timer);
                if(img) img.color = new Color(0,0,0, alpha);
                if(txt) txt.color = new Color(1,1,1, alpha);
                yield return null;
            }
            introPanel.SetActive(false);
        }

        // --- QUAN TRỌNG: Đợi 1 giây để người chơi ngắm nhân vật ---
        yield return new WaitForSeconds(1f); 

        // Sau đó mới gọi hiện hội thoại
        VaoGameChinh(); 
    }

    void VaoGameChinh()
    {
        // Bật GameObject lên nhưng set Alpha = 0 (Tàng hình)
        if (vnCanvas != null) vnCanvas.SetActive(true);
        if (uiCanvasGroup != null) uiCanvasGroup.alpha = 0f;
        
        cauHienTai = 0;
        
        if (danhSachThoai.Length > 0)
        {
            // Gọi hàm hiện từ từ
            StartCoroutine(HienHoiThoaiTuTu());
        }
        else
        {
            KetThucHoiThoai();
        }
    }

    // --- SỬA ĐỔI 2: Hàm làm khung thoại hiện từ từ ---
    IEnumerator HienHoiThoaiTuTu()
    {
        float timer = 0f;
        float duration = 1.5f; // Hiện dần trong 1.5 giây

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            if (uiCanvasGroup != null) 
                uiCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }

        if (uiCanvasGroup != null) uiCanvasGroup.alpha = 1f;

        // Hiện xong mới bắt đầu chạy chữ & cho phép bấm
        BatDauCauThoai();
        dangOIntro = false; 
    }

    void XuLyHoiThoai()
    {
        if (dangChayChu)
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            if (amThanhGoChu != null) amThanhGoChu.Stop(); 
            textNoiDung.text = danhSachThoai[cauHienTai];
            dangChayChu = false;
        }
        else
        {
            cauHienTai++;
            if (cauHienTai < danhSachThoai.Length) 
            {
                BatDauCauThoai();
            }
            else 
            {
                KetThucHoiThoai();
            }
        }
    }

    void KetThucHoiThoai()
    {
        Debug.Log("Hết truyện! Player được di chuyển.");
        if (vnCanvas != null) vnCanvas.SetActive(false);
        if (playerScript != null) playerScript.enabled = true; 
    }

    void BatDauCauThoai()
    {
        if (cauHienTai < danhSachTen.Length) textTenNhanVat.text = danhSachTen[cauHienTai];
        
        int idNguoiNoi = 0;
        if (cauHienTai < aiDangNoi.Length) idNguoiNoi = aiDangNoi[cauHienTai];
        FocusNhanVat(idNguoiNoi);

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(ChayChu(danhSachThoai[cauHienTai]));
    }

    IEnumerator ChayChu(string cauThoai)
    {
        dangChayChu = true;
        textNoiDung.text = "";
        foreach (char chu in cauThoai.ToCharArray())
        {
            textNoiDung.text += chu;
            if (amThanhGoChu != null && !char.IsWhiteSpace(chu)) 
            {
                amThanhGoChu.pitch = Random.Range(1f - doMeoTieng, 1f + doMeoTieng);
                amThanhGoChu.Play(); 
            }
            yield return new WaitForSecondsRealtime(tocDoChu);
        }
        dangChayChu = false;
    }

    void FocusNhanVat(int ID)
    {
        if (anhNhanVat1 != null)
        {
            anhNhanVat1.color = (ID == 1) ? mauSang : mauToi;
            if (ID == 1) StartCoroutine(NhunNhanVat(anhNhanVat1.transform, scaleGocNV1));
        }

        if (anhNhanVat2 != null)
        {
            anhNhanVat2.color = (ID == 2) ? mauSang : mauToi;
            if (ID == 2) StartCoroutine(NhunNhanVat(anhNhanVat2.transform, scaleGocNV2));
        }
    }

    IEnumerator NhunNhanVat(Transform t, Vector3 scaleGoc)
    {
        float timer = 0;
        while(timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime; 
            t.localScale = Vector3.Lerp(scaleGoc, scaleGoc * 1.1f, timer / 0.1f);
            yield return null;
        }
        t.localScale = scaleGoc;
    }
}