using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class VNController : MonoBehaviour
{
    [Header("--- UI Elements ---")]
    public GameObject vnCanvas;
    public TextMeshProUGUI textNoiDung;
    public TextMeshProUGUI textTenNhanVat;
    public Button nutBamToanManHinh;

    [Header("--- Nhân vật ---")]
    public Image anhNhanVat1;
    public Image anhNhanVat2;
    public Color mauSang = Color.white;
    public Color mauToi = new Color(0.5f, 0.5f, 0.5f, 1f); // Màu xám khi không nói

    [Header("--- Cấu hình Chữ & Âm thanh ---")]
    public float tocDoChu = 0.03f;
    public AudioSource amThanhGoChu;

    [Header("--- Kịch Bản ---")]
    public string[] danhSachTen; // Nhập tên nhân vật tương ứng từng câu
    [TextArea(3, 10)] public string[] danhSachThoai;
    public int[] aiDangNoi; // 1: NV1 nói, 2: NV2 nói, 0: Cả hai cùng tối

    private int cauHienTai = 0;
    private bool dangChayChu = false;
    private Coroutine typeRoutine;

    void Start()
    {
        Time.timeScale = 0;
        vnCanvas.SetActive(true);
        
        if (nutBamToanManHinh != null)
            nutBamToanManHinh.onClick.AddListener(XuLyBamChuot);

        BatDauCauThoai();
    }

    void XuLyBamChuot()
    {
        if (dangChayChu)
        {
            // Bấm khi đang chạy -> Hiện hết câu luôn
            StopCoroutine(typeRoutine);
            textNoiDung.text = danhSachThoai[cauHienTai];
            dangChayChu = false;
        }
        else
        {
            // Bấm khi đã hiện hết -> Sang câu mới
            cauHienTai++;
            if (cauHienTai < danhSachThoai.Length)
                BatDauCauThoai();
            else
                KetThuc();
        }
    }

    void BatDauCauThoai()
    {
        // 1. Cập nhật tên
        textTenNhanVat.text = danhSachTen[cauHienTai];

        // 2. Làm nổi bật nhân vật
        FocusNhanVat(aiDangNoi[cauHienTai]);

        // 3. Chạy hiệu ứng chữ
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
            if (amThanhGoChu != null) amThanhGoChu.PlayOneShot(amThanhGoChu.clip);
            yield return new WaitForSecondsRealtime(tocDoChu);
        }
        dangChayChu = false;
    }

    void FocusNhanVat(int ID)
    {
        if (anhNhanVat1 == null || anhNhanVat2 == null) return;

        anhNhanVat1.color = (ID == 1) ? mauSang : mauToi;
        anhNhanVat2.color = (ID == 2) ? mauSang : mauToi;
        
        // Hiệu ứng nhún nhẹ cho đứa đang nói
        Image nvn = (ID == 1) ? anhNhanVat1 : (ID == 2) ? anhNhanVat2 : null;
        if (nvn != null) StartCoroutine(NhunNhanVat(nvn.transform));
    }

    IEnumerator NhunNhanVat(Transform t)
    {
        Vector3 defaultScale = Vector3.one;
        t.localScale = defaultScale * 1.05f;
        yield return new WaitForSecondsRealtime(0.1f);
        t.localScale = defaultScale;
    }

    void KetThuc()
    {
        vnCanvas.SetActive(false);
        Time.timeScale = 1;
    }
}