// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理角色头顶的生命条，使其跟随角色移动
/// </summary>
public class CharacterHealthBar : MonoBehaviour
{
    [Header("设置")]
    public Transform target;              // 跟随的目标
    public Vector3 offset = new Vector3(0, 1.5f, 0);  // 与目标的偏移量
    public float smoothFollow = 0.15f;    // 跟随平滑度 (越小越平滑)
    public bool alwaysFaceCamera = true;  // 是否始终面向相机
    
    [Header("UI 元素")]
    public Image healthBarFill;           // 生命值填充图像
    public Image manaBarFill;             // 魔法值填充图像
    public Text nameText;                 // 名称文本
    public Text levelText;                // 等级文本
    
    // 内部变量
    private Character characterRef;       // 角色引用
    private Canvas worldCanvas;           // 世界空间画布
    private RectTransform canvasRect;     // 画布矩形变换
    private Camera mainCamera;            // 主相机
    
    private void Awake()
    {
        // 获取组件引用
        worldCanvas = GetComponentInChildren<Canvas>();
        if (worldCanvas != null)
        {
            canvasRect = worldCanvas.GetComponent<RectTransform>();
        }
        
        // 确保画布类型为世界空间
        if (worldCanvas != null && worldCanvas.renderMode != RenderMode.WorldSpace)
        {
            worldCanvas.renderMode = RenderMode.WorldSpace;
        }
    }
    
    private void Start()
    {
        // 获取主相机
        mainCamera = Camera.main;
        
        // 获取目标角色
        if (target != null)
        {
            characterRef = target.GetComponent<Character>();
        }
    }
    
    /// <summary>
    /// 初始化健康条与目标
    /// </summary>
    public void Initialize(Transform targetTransform, Character character)
    {
        target = targetTransform;
        characterRef = character;
        
        // 如果指定了角色，立即更新生命条
        if (characterRef != null)
        {
            UpdateHealthBar();
            UpdateInfo();
        }
    }
    
    /// <summary>
    /// 更新生命条
    /// </summary>
    public void UpdateHealthBar()
    {
        if (characterRef == null || healthBarFill == null) return;
        
        // 更新健康条填充
        healthBarFill.fillAmount = characterRef.health / characterRef.maxHealth;
        
        // 根据健康值变化颜色
        if (healthBarFill.fillAmount > 0.6f)
        {
            healthBarFill.color = Color.green;
        }
        else if (healthBarFill.fillAmount > 0.3f)
        {
            healthBarFill.color = Color.yellow;
        }
        else
        {
            healthBarFill.color = Color.red;
        }
        
        // 如果有魔法条，更新魔法条
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = characterRef.mana / characterRef.maxMana;
        }
    }
    
    /// <summary>
    /// 更新角色信息
    /// </summary>
    public void UpdateInfo()
    {
        if (characterRef == null) return;
        
        // 更新名称文本
        if (nameText != null)
        {
            nameText.text = characterRef.characterName;
        }
        
        // 更新等级文本
        if (levelText != null)
        {
            levelText.text = "Lv." + characterRef.tier.ToString();
        }
    }
    
    private void LateUpdate()
    {
        if (target == null || mainCamera == null) return;
        
        // 计算目标位置
        Vector3 targetPosition = target.position + offset;
        
        // 平滑跟随目标
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothFollow * Time.deltaTime);
        
        // 如果设置了始终面向相机
        if (alwaysFaceCamera && worldCanvas != null)
        {
            worldCanvas.transform.LookAt(
                worldCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward, 
                mainCamera.transform.rotation * Vector3.up
            );
        }
        
        // 每帧更新健康条
        UpdateHealthBar();
    }
}
