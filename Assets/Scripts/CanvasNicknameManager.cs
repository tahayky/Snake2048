using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CanvasNicknameManager : MonoBehaviour
{
    #region Singleton
    // ... (Singleton kodu öncekiyle aynı) ...
    private static CanvasNicknameManager _instance;
    private static readonly object lockObject = new object();
    private static bool applicationIsQuitting = false;

    public static CanvasNicknameManager Instance
    {
        get
        {
            if (applicationIsQuitting) return null;
            lock (lockObject)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CanvasNicknameManager>();
                    if (_instance == null)
                    {
                        // Otomatik oluşturma (manuel ekleme tercih edilir)
                        // GameObject singletonObject = new GameObject("CanvasNicknameManager_Auto");
                        // _instance = singletonObject.AddComponent<CanvasNicknameManager>();
                        // Debug.LogWarning("...");
                        // Sahne kapanırken yeni oluşturmayı önlemek için null dönebiliriz
                        return null;
                    }
                }
                return _instance;
            }
        }
    }
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        applicationIsQuitting = false;
        Initialize();
    }
    #endregion

    [Header("Ayarlar")]
    [SerializeField] private GameObject nicknamePrefab;
    [SerializeField] private Canvas targetCanvas;

    [Tooltip("Nickname'in, karakterin ekrana yansıtılan üst noktasının kaç piksel üzerinde görüneceği.")]
    [SerializeField] private float screenSpaceVerticalOffset = 30f; // Piksel cinsinden ofset

    [Tooltip("Karakterin üst noktasını bulmak için Collider mı yoksa Renderer mı kullanılsın?")]
    [SerializeField] private BoundsSource boundsSource = BoundsSource.Collider;
    [Tooltip("Bounds bulunamazsa, karakterin pivot noktasına dünya birimi cinsinden uygulanacak yedek dikey ofset.")]
    [SerializeField] private float fallbackWorldVerticalOffset = 1.5f;
    [Tooltip("Bounds kullanırken, bounds'un üst noktasına eklenecek küçük dünya birimi ofseti (isteğe bağlı).")]
    [SerializeField] private float boundsTopWorldOffset = 0.1f; // Bounds'un tam tepesine yapışmaması için küçük bir ek ofset

    public enum BoundsSource { Collider, Renderer }

    private struct NicknameData
    {
        public TextMeshProUGUI TmpText;
        public Collider TargetCollider;
        public Renderer TargetRenderer;
        public Transform TargetTransform;
    }
    private Dictionary<Transform, NicknameData> activeNicknames = new Dictionary<Transform, NicknameData>();

    private Queue<TextMeshProUGUI> nicknamePool = new Queue<TextMeshProUGUI>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Ana kamera bulunamadı!");

        // Prefab pivot kontrolü
        if (nicknamePrefab != null)
        {
            RectTransform prefabRect = nicknamePrefab.GetComponent<RectTransform>();
            if (prefabRect != null && (Mathf.Abs(prefabRect.pivot.x - 0.5f) > 0.01f || Mathf.Abs(prefabRect.pivot.y - 0.0f) > 0.01f))
            {
                Debug.LogWarning($"Nickname Prefab ({nicknamePrefab.name}) RectTransform pivotu (0.5, 0) olarak ayarlanmalı. Mevcut: {prefabRect.pivot}", nicknamePrefab);
            }
        }
    }

    void Initialize()
    {
        // ... (Gerekli kontroller) ...
        if (nicknamePrefab == null || targetCanvas == null || nicknamePrefab.GetComponentInChildren<TextMeshProUGUI>() == null)
        {
             Debug.LogError("CanvasNicknameManager başlatılamadı. Inspector ayarlarını kontrol edin.");
             enabled = false;
             return;
        }
    }

    // ... (CreateNewNicknameInstance öncekiyle aynı) ...
     private TextMeshProUGUI CreateNewNicknameInstance()
    {
        if (targetCanvas == null || nicknamePrefab == null) return null;
        GameObject instanceGO = Instantiate(nicknamePrefab, targetCanvas.transform);
        TextMeshProUGUI tmpText = instanceGO.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText == null) {
             Debug.LogError("Prefab'da TextMeshProUGUI bulunamadı!");
             Destroy(instanceGO);
             return null;
        }
        return tmpText;
    }

    void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            TextMeshProUGUI nicknameInstance = CreateNewNicknameInstance();
            if (nicknameInstance != null) // Oluşturma başarılıysa
            {
                nicknameInstance.gameObject.SetActive(false);
                nicknamePool.Enqueue(nicknameInstance);
            }
        }
    }


    public void AddNickname(Transform target, string nicknameText, Color color)
    {
        if (target == null || applicationIsQuitting) return;

        if (activeNicknames.TryGetValue(target, out NicknameData existingData))
        {
            existingData.TmpText.text = nicknameText;
            existingData.TmpText.color = color;
            existingData.TmpText.gameObject.SetActive(true);
            // Pozisyon hemen güncellenmese de olur, LateUpdate halleder.
            // UpdateNicknamePosition(target, existingData);
            return;
        }

        TextMeshProUGUI tmpText;
        if (nicknamePool.Count > 0) { /* ... havuzdan al ... */
            tmpText = nicknamePool.Dequeue();
            tmpText.gameObject.SetActive(true);
        }
        else { /* ... yeni oluştur ... */
            tmpText = CreateNewNicknameInstance();
            if (tmpText == null) return;
        }

        tmpText.text = nicknameText;
        tmpText.color = color;

        Collider targetCollider = null;
        Renderer targetRenderer = null;
        if (boundsSource == BoundsSource.Collider) targetCollider = target.GetComponentInChildren<Collider>();
        else targetRenderer = target.GetComponentInChildren<Renderer>();

        NicknameData newData = new NicknameData
        {
            TmpText = tmpText,
            TargetCollider = targetCollider,
            TargetRenderer = targetRenderer,
            TargetTransform = target
        };

        activeNicknames.Add(target, newData);
        // Pozisyon hemen güncellenmese de olur, LateUpdate halleder.
        // UpdateNicknamePosition(target, newData);
    }

    public void RemoveNickname(Transform target)
    {
        if (target == null || applicationIsQuitting) return;
        if (activeNicknames.TryGetValue(target, out NicknameData data))
        {
            if (data.TmpText != null) // Null kontrolü
            {
                data.TmpText.gameObject.SetActive(false);
                nicknamePool.Enqueue(data.TmpText);
            }
            activeNicknames.Remove(target);
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null || applicationIsQuitting) return;

        List<Transform> targetsToRemove = null;
        List<Transform> currentTargets = new List<Transform>(activeNicknames.Keys);

        foreach (Transform target in currentTargets)
        {
            if (!activeNicknames.TryGetValue(target, out NicknameData data)) continue;

            if (target == null) // Hedef yok olduysa
            {
                if (targetsToRemove == null) targetsToRemove = new List<Transform>();
                targetsToRemove.Add(target);
                if (data.TmpText != null)
                {
                    data.TmpText.gameObject.SetActive(false);
                    nicknamePool.Enqueue(data.TmpText);
                }
                continue;
            }
            UpdateNicknamePosition(target, data); // Pozisyonu güncelle
        }

        if (targetsToRemove != null)
        {
            foreach (var targetToRemove in targetsToRemove)
            {
                activeNicknames.Remove(targetToRemove);
            }
        }
    }

    // *** ÖNEMLİ DEĞİŞİKLİK BURADA ***
    private void UpdateNicknamePosition(Transform target, NicknameData data)
    {
        if (data.TmpText == null || mainCamera == null) return;

        Vector3 targetTopWorldPosition; // Karakterin üst noktasının dünya koordinatı
        bool positionFound = false;

        // 1. Bounds kullanarak dünya pozisyonunu bul
        if (boundsSource == BoundsSource.Collider && data.TargetCollider != null && data.TargetCollider.enabled)
        {
            Bounds bounds = data.TargetCollider.bounds;
            targetTopWorldPosition = bounds.center + Vector3.up * bounds.extents.y + Vector3.up * boundsTopWorldOffset;
            positionFound = true;
        }
        else if (boundsSource == BoundsSource.Renderer && data.TargetRenderer != null && data.TargetRenderer.enabled)
        {
            Bounds bounds = data.TargetRenderer.bounds;
            targetTopWorldPosition = bounds.center + Vector3.up * bounds.extents.y + Vector3.up * boundsTopWorldOffset;
            positionFound = true;
        }
        // 2. Fallback: Transform pivotunu kullan
        else
        {
            if (data.TargetTransform == null) { data.TmpText.enabled = false; return; }
            targetTopWorldPosition = data.TargetTransform.position + Vector3.up * fallbackWorldVerticalOffset;
            positionFound = true; // Fallback pozisyonu bulundu
        }

        // Eğer bir pozisyon bulunduysa devam et
        if (positionFound)
        {
            // Dünya pozisyonunu *temel* ekran pozisyonuna çevir
            Vector3 baseScreenPosition = mainCamera.WorldToScreenPoint(targetTopWorldPosition);

            // Kamera arkası veya çok uzak kontrolü
            bool isVisible = baseScreenPosition.z > 0;

            // Ekran sınırları kontrolü (opsiyonel, biraz taşmasına izin verilebilir)
            // isVisible = isVisible && baseScreenPosition.x >= 0 && baseScreenPosition.x <= Screen.width &&
            //             baseScreenPosition.y >= 0 && baseScreenPosition.y <= Screen.height;

            data.TmpText.enabled = isVisible;

            if (isVisible)
            {
                // *** EKRAN UZAYINDA OFSET UYGULA ***
                Vector3 finalScreenPosition = baseScreenPosition + new Vector3(0, screenSpaceVerticalOffset, 0);

                // Canvas türüne göre pozisyon ayarla
                if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Screen Space Overlay için doğrudan ekran koordinatını kullanabiliriz
                    data.TmpText.rectTransform.position = finalScreenPosition;
                }
                else // Screen Space - Camera veya World Space (Camera modu için daha uygun)
                {
                    // Diğer modlar için ekran noktasını Canvas'ın local pozisyonuna çevirmemiz gerekir
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        targetCanvas.transform as RectTransform,
                        finalScreenPosition, // Ofset eklenmiş ekran pozisyonunu kullan
                        targetCanvas.worldCamera, // Canvas'a atanmış kamera (Camera modu için gerekli)
                        out Vector2 localPoint);
                    data.TmpText.rectTransform.localPosition = localPoint;
                }
            }
        }
        else
        {
            // Pozisyon bulunamadıysa (target transform da null ise vb.)
            data.TmpText.enabled = false;
        }
    }

    // ... (OnApplicationQuit, OnDestroy önceki gibi) ...
    public void OnApplicationQuit() { applicationIsQuitting = true; }
    void OnDestroy()
    {
        applicationIsQuitting = true;
        if (_instance == this)
        {
            _instance = null;
        }

        // Aktif nickname'leri temizle
        foreach (var kvp in activeNicknames)
        {
            if (kvp.Value.TmpText != null)
                Destroy(kvp.Value.TmpText.gameObject);
        }
        activeNicknames.Clear();

        // Havuzu temizle
        while (nicknamePool.Count > 0)
        {
            TextMeshProUGUI pooledItem = nicknamePool.Dequeue();
            if (pooledItem != null)
                Destroy(pooledItem.gameObject);
        }
        nicknamePool.Clear();
    }
}