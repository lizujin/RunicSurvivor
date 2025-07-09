// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 扩展UIManager类的功能
/// </summary>
public static class UIExtensions
{
    // 预制体缓存
    private static GameObject damageNumberPrefab;
    private static GameObject floatingTextPrefab;

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public static void ShowDamageNumber(this UIManager uiManager, float amount, Vector3 position)
    {
        // 确保有Canvas
        Canvas canvas = uiManager.GetComponentInChildren<Canvas>();
        if (canvas == null) return;
        
        // 如果没有预制体，创建一个
        if (damageNumberPrefab == null)
        {
            damageNumberPrefab = CreateDamageNumberPrefab();
        }
        
        // 实例化伤害数字
        GameObject damageObj = GameObject.Instantiate(damageNumberPrefab, canvas.transform);
        
        // 设置位置
        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 将3D世界坐标转换为屏幕坐标
            Vector2 screenPos = Camera.main.WorldToScreenPoint(position);
            rectTransform.position = screenPos;
        }
        
        // 设置文本
        Text textComponent = damageObj.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = "-" + Mathf.RoundToInt(amount).ToString();
            textComponent.color = new Color(1f, 0.3f, 0.3f); // 红色
        }
        
        // 添加动画组件
        FloatingTextAnimation anim = damageObj.AddComponent<FloatingTextAnimation>();
        anim.Initialize(1.0f, new Vector2(0, 50), 0.5f);
    }
    
    /// <summary>
    /// 显示浮动文本
    /// </summary>
    public static void ShowFloatingText(this UIManager uiManager, string text, Vector3 position, Color color)
    {
        // 确保有Canvas
        Canvas canvas = uiManager.GetComponentInChildren<Canvas>();
        if (canvas == null) return;
        
        // 如果没有预制体，创建一个
        if (floatingTextPrefab == null)
        {
            floatingTextPrefab = CreateFloatingTextPrefab();
        }
        
        // 实例化浮动文本
        GameObject textObj = GameObject.Instantiate(floatingTextPrefab, canvas.transform);
        
        // 设置位置
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 将3D世界坐标转换为屏幕坐标
            Vector2 screenPos = Camera.main.WorldToScreenPoint(position);
            rectTransform.position = screenPos;
        }
        
        // 设置文本
        Text textComponent = textObj.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
        }
        
        // 添加动画组件
        FloatingTextAnimation anim = textObj.AddComponent<FloatingTextAnimation>();
        anim.Initialize(1.5f, new Vector2(0, 30), 1.0f);
    }
    
    /// <summary>
    /// 创建伤害数字预制体
    /// </summary>
    private static GameObject CreateDamageNumberPrefab()
    {
        GameObject prefab = new GameObject("DamageNumber");
        
        // 添加RectTransform
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 30);
        
        // 添加文本组件
        Text text = prefab.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(1f, 0.3f, 0.3f);
        
        // 设为预制体
        prefab.SetActive(false);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建浮动文本预制体
    /// </summary>
    private static GameObject CreateFloatingTextPrefab()
    {
        GameObject prefab = new GameObject("FloatingText");
        
        // 添加RectTransform
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(150, 30);
        
        // 添加文本组件
        Text text = prefab.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 16;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Bold;
        
        // 设为预制体
        prefab.SetActive(false);
        
        return prefab;
    }
}

/// <summary>
/// 浮动文本的动画组件
/// </summary>
public class FloatingTextAnimation : MonoBehaviour
{
    private float duration;
    private Vector2 moveAmount;
    private float fadeDelay;
    private float elapsed = 0f;
    private RectTransform rectTransform;
    private Text text;
    private Vector2 startPosition;
    private Color startColor;
    
    public void Initialize(float duration, Vector2 moveAmount, float fadeDelay)
    {
        this.duration = duration;
        this.moveAmount = moveAmount;
        this.fadeDelay = fadeDelay;
        
        rectTransform = GetComponent<RectTransform>();
        text = GetComponent<Text>();
        
        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
        }
        
        if (text != null)
        {
            startColor = text.color;
        }
        
        // 激活游戏对象
        gameObject.SetActive(true);
    }
    
    private void Update()
    {
        elapsed += Time.deltaTime;
        
        if (elapsed >= duration)
        {
            // 动画结束
            Destroy(gameObject);
            return;
        }
        
        // 移动
        if (rectTransform != null)
        {
            float progress = elapsed / duration;
            rectTransform.anchoredPosition = startPosition + moveAmount * progress;
        }
        
        // 淡出
        if (text != null && elapsed > fadeDelay)
        {
            float fadeProgress = (elapsed - fadeDelay) / (duration - fadeDelay);
            Color color = text.color;
            color.a = Mathf.Lerp(startColor.a, 0f, fadeProgress);
            text.color = color;
        }
    }
}
