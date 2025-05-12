using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoyUI_ActiveItemsController : Singletroon<SoyUI_ActiveItemsController>
{
    #region HELMETS
    [Header("Helmets")]
    [SerializeField] private GameObject[] helmetIconPrefabs;
    private GameObject activeHelmetIcon;
    private List<TextMeshProUGUI> durabilityTexts = new();
    #endregion

    #region SPROKE
    [Header("Sproke")]
    [SerializeField] private GameObject sprokeIconPrefab;
    private GameObject activeSprokeIcon;
    private Image sprokeFillMask;
    private bool sprokeActive = false;
    private float lastSprokePickUpTime;
    #endregion

    #region GREENARROW
    [Header("GreenArrow")]

    [SerializeField] private GameObject greenArrowIconPrefab;
    private GameObject activeGreenArrowIcon;
    private List<TextMeshProUGUI> greenArrowAmmoTexts = new();
    [SerializeField] private GameObject greenArrowButton;
    #endregion

    #region SOYMILK
    [Header("SoyMilk")]
    [SerializeField] private GameObject soymilkIconPrefab;
    private GameObject activeSoymilkIcon;
    private Image soymilkFillMask;
    private bool soymilkActive = false;
    private float lastSoymilkPickUpTime;
    #endregion

    #region MAGNET
    [Header("Magnet")]
    [SerializeField] private GameObject magnetIconPrefab;
    private GameObject activeMagnetIcon;
    private Image magnetFillMask;
    private bool magnetActive = false;
    private float lastMagnetPickUpTime;
    #endregion

    #region OUCHIE
    [Header("Ouchie")]
    [SerializeField] private GameObject ouchieIconPrefab;
    private GameObject activeOuchieIcon;
    private Image ouchieFillMask;
    private bool ouchieActive = false;
    private float lastOuchiePickUpTime;
    [SerializeField] private GameObject ouchieStatus;
    [SerializeField] private List<TextMeshProUGUI> ouchieStatusTexts = new();
    #endregion


    #region KEBAB
    [Header("Kebab")]
    [SerializeField] private GameObject kebabIconPrefab;
    private GameObject activeKebabIcon;
    private Image kebabFillMask;
    private bool kebabActive = false;
    private float lastKebabPickUpTime;
    #endregion    

    #region NUT
    [Header("Nut")]
    [SerializeField] private GameObject nutIconPrefab;
    private GameObject activeNutIcon;
    private Image nutFillMask;
    private bool nutActive = false;
    private float lastNutPickUpTime;
    #endregion

    #region MEDS
    [Header("Meds")]
    [SerializeField] private GameObject medsIconPrefab;
    private GameObject activeMedsIcon;
    private Image medsFillMask;
    private bool medsActive = false;
    private float lastMedsPickUpTime;
    #endregion

    [SerializeField] GameObject touchScrenButtonsPanel;
    private RectTransform panel;
    private Dictionary<int, GameObject> helmetIconPrefabByDurability;
    private SoyGameController gameController;
    private SoyTimeController timeController;


    void Awake()
    {
        gameController = SoyGameController.Instance;
        timeController = SoyTimeController.Instance;
        panel = GetComponent<RectTransform>();
        helmetIconPrefabByDurability = new Dictionary<int, GameObject>()
        {
            { 1,  helmetIconPrefabs[0] },
            { 3,  helmetIconPrefabs[1] },
            { 5,  helmetIconPrefabs[2] },
            { 7,  helmetIconPrefabs[3] },
            { 10, helmetIconPrefabs[4] }
        };

        OnPickUpHelmet(5);
        touchScrenButtonsPanel.SetActive(ToweraldStatic.isMobile);
    }

    #region HELMET_METHODS
    public void OnPickUpHelmet(int remainingUses)
    {
        if (activeHelmetIcon != null)
        {
            Destroy(activeHelmetIcon);
            durabilityTexts.Clear();
        }

        if (helmetIconPrefabByDurability.TryGetValue(remainingUses, out GameObject prefab))
        {
            activeHelmetIcon = Instantiate(prefab, panel);
            durabilityTexts = new List<TextMeshProUGUI>(activeHelmetIcon.GetComponentsInChildren<TextMeshProUGUI>());
            UpdateHelmetDurability(remainingUses);
        }
    }

    public void UpdateHelmetDurability(int remainingUses)
    {
        foreach (var tmp in durabilityTexts)
        {
            tmp.text = remainingUses.ToString();
        }
    }

    public void OnLoseHelmet()
    {
        if (activeHelmetIcon != null)
        {
            Destroy(activeHelmetIcon);
            activeHelmetIcon = null;
            durabilityTexts.Clear();
        };
    }
    #endregion

    #region GREENARROW_METHODS
    public void OnPickUpGreenArrow(float ammo)
    {
        if (activeGreenArrowIcon == null)
        {
            activeGreenArrowIcon = Instantiate(greenArrowIconPrefab, panel);
            greenArrowAmmoTexts = new List<TextMeshProUGUI>(activeGreenArrowIcon.GetComponentsInChildren<TextMeshProUGUI>());
        }

        UpdateGreenArrow(ammo);
        if (ToweraldStatic.isMobile)
            greenArrowButton.SetActive(true);
    }

    public void UpdateGreenArrow(float ammo)
    {
        if (ammo <= 0)
            OnLoseGreenArrow();
        else
        {
            foreach (var tmp in greenArrowAmmoTexts)
                tmp.text = ammo.ToString(); 
        }
    }

    public void OnLoseGreenArrow()
    {
        if (activeGreenArrowIcon != null)
        {
            Destroy(activeGreenArrowIcon);
            activeGreenArrowIcon = null;
        }
        greenArrowButton.SetActive(false);
    }

    #endregion

    #region SPROKE_METHODS
    public void OnPickUpSproke()
    {
        if (activeSprokeIcon == null)
        {
            activeSprokeIcon = Instantiate(sprokeIconPrefab, panel);
            sprokeFillMask = activeSprokeIcon.transform.Find("FillMask").GetComponent<Image>();
        }

        lastSprokePickUpTime = Time.time;

        if (sprokeFillMask != null)
            sprokeFillMask.fillAmount = 1f;

        sprokeActive = true;
    }

    public void OnLoseSproke()
    {
        if (activeSprokeIcon != null)
        {
            Destroy(activeSprokeIcon);
            activeSprokeIcon = null;
        }

        sprokeActive = false;
    }
    #endregion

    #region SOYMILK_METHODS
    public void OnPickUpSoyMilk()
    {
        if (activeSoymilkIcon == null)
        {
            activeSoymilkIcon = Instantiate(soymilkIconPrefab, panel);
            soymilkFillMask = activeSoymilkIcon.transform.Find("FillMask").GetComponent<Image>();
        }

        lastSoymilkPickUpTime = Time.time;

        if (soymilkFillMask != null)
            soymilkFillMask.fillAmount = 1f;

        soymilkActive = true;
    }

    public void OnLoseSoyMilk()
    {
        if (activeSoymilkIcon != null)
        {
            Destroy(activeSoymilkIcon);
            activeSoymilkIcon = null;
        }

        soymilkActive = false;
    }
    #endregion

    #region MAGNET_METHODS
    public void OnPickUpMagnet()
    {
        if (activeMagnetIcon == null)
        {
            activeMagnetIcon = Instantiate(magnetIconPrefab, panel);
            magnetFillMask = activeMagnetIcon.transform.Find("FillMask").GetComponent<Image>();
        }

        lastMagnetPickUpTime = Time.time;

        if (magnetFillMask != null)
            magnetFillMask.fillAmount = 1f;

        magnetActive = true;
    }

    public void OnLoseMagnet()
    {
        if (activeMagnetIcon != null)
        {
            Destroy(activeMagnetIcon);
            activeMagnetIcon = null;
        }

        magnetActive = false;
    }
    #endregion

    #region OUCHIE_METHODS
    public void OnPickUpOuchie(float status)
    {
        ouchieStatus.SetActive(true);
        string formattedStatus = status % 1 == 0 ? $"{status:0}%" : $"{status:0.##}%";
        foreach (var tmp in ouchieStatusTexts)
            tmp.text = formattedStatus; 
        if (activeOuchieIcon == null)
        {
            activeOuchieIcon = Instantiate(ouchieIconPrefab, panel);
            ouchieFillMask = activeOuchieIcon.transform.Find("FillMask").GetComponent<Image>();
        }
        lastOuchiePickUpTime = Time.time;
        if (ouchieFillMask != null)
            ouchieFillMask.fillAmount = 1f;

        ouchieActive = true;
    }

    public void OnLoseOuchie()
    {
        if (activeOuchieIcon != null)
        {
            Destroy(activeOuchieIcon);
            activeOuchieIcon = null;
        }

        ouchieActive = false;
    }
    #endregion
    
    #region KEBAB_METHODS
    public void OnPickUpKebab()
    {
        if (activeKebabIcon == null)
        {
            activeKebabIcon = Instantiate(kebabIconPrefab, panel);
            kebabFillMask = activeKebabIcon.transform.Find("FillMask").GetComponent<Image>();
        }

        lastKebabPickUpTime = Time.time;

        if (kebabFillMask != null)
            kebabFillMask.fillAmount = 1f;

        kebabActive = true;
    }

    public void OnLoseKebab()
    {
        if (activeKebabIcon != null)
        {
            Destroy(activeKebabIcon);
            activeKebabIcon = null;
        }

        kebabActive = false;
    }
    #endregion

    #region NUT_METHODS
    public void OnPickUpNut()
    {
        if (activeNutIcon == null)
        {
            activeNutIcon = Instantiate(nutIconPrefab, panel);
            nutFillMask = activeNutIcon.transform.Find("FillMask").GetComponent<Image>();
        }

        lastNutPickUpTime = Time.time;

        if (nutFillMask != null)
            nutFillMask.fillAmount = 1f;

        nutActive = true;
    }

    public void OnLoseNut()
    {
        if (activeNutIcon != null)
        {
            Destroy(activeNutIcon);
            activeNutIcon = null;
        }

        nutActive = false;
    }
    #endregion

    #region MEDS_METHODS
    public void OnPickUpMeds()
    {
        if (activeMedsIcon == null)
        {
            activeMedsIcon = Instantiate(medsIconPrefab, panel);
            medsFillMask = activeMedsIcon.transform.Find("FillMask").GetComponent<Image>();
        }

        lastMedsPickUpTime = Time.time;

        if (medsFillMask != null)
            medsFillMask.fillAmount = 1f;

        medsActive = true;
    }

    public void OnLoseMeds()
    {
        if (activeMedsIcon != null)
        {
            Destroy(activeMedsIcon);
            activeMedsIcon = null;
        }

        medsActive = false;
    }
    #endregion

    private void UpdateFill(
        bool active,
        float startTime,
        float endTime,
        Image fillMask,
        ref bool activeFlag)
    {
        if (!active) return;

        float now = Time.time;
        float duration = endTime - startTime;
        float t = Mathf.Clamp01((now - startTime) / duration);
        float remaining = 1f - t;

        if (fillMask != null)
            fillMask.fillAmount = remaining;

        if (t >= 1f)
            activeFlag = false;
    }

    void Update()
    {
        if (gameController.IsPaused)
            return;

        UpdateFill(sprokeActive, lastSprokePickUpTime, timeController.GetSprokeEndTime(), sprokeFillMask, ref sprokeActive);
        UpdateFill(soymilkActive, lastSoymilkPickUpTime, gameController.GetTimedItemEndTime(Item.SoyMilk), soymilkFillMask, ref soymilkActive);
        UpdateFill(magnetActive, lastMagnetPickUpTime, gameController.GetTimedItemEndTime(Item.Magnet), magnetFillMask, ref magnetActive);
        UpdateFill(ouchieActive, lastOuchiePickUpTime, gameController.GetTimedItemEndTime(Item.Ouchie), ouchieFillMask, ref ouchieActive);
        UpdateFill(kebabActive, lastKebabPickUpTime, gameController.GetTimedItemEndTime(Item.Kebab), kebabFillMask, ref kebabActive);
        UpdateFill(nutActive, lastNutPickUpTime, gameController.GetTimedItemEndTime(Item.Nut), nutFillMask, ref nutActive);
        UpdateFill(medsActive, lastMedsPickUpTime, gameController.GetTimedItemEndTime(Item.Meds), medsFillMask, ref medsActive);
    }
}
