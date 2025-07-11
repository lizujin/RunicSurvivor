using UnityEngine;
/// <summary>
/// 相机跟随脚本
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 10, -10);
    public bool lookAtTarget = true;
    public float updateInterval = 0.0f; // 更新间隔时间（0表示每帧更新）
    
    private Transform target;
    private float updateTimer;
    private Vector3 velocity = Vector3.zero;
    
    public void Start()
    {
        // 查找玩家
        FindAndSetTarget();
        
        // 立即将相机定位到玩家位置，避免初始化时相机跳跃
        if (target != null)
        {
            Vector3 initialPosition = target.position + offset;
            transform.position = initialPosition;
            
            if (lookAtTarget)
            {
                transform.LookAt(target);
            }
        }
    }
    
    /// <summary>
    /// 查找并设置目标
    /// </summary>
    private void FindAndSetTarget()
    {
        // 首先尝试查找玩家控制器
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 优先使用中心角色作为目标
            Character centerCharacter = player.CurrentCenterCharacter;
            if (centerCharacter != null)
            {
                target = centerCharacter.transform;
                Debug.Log("相机目标设置为中心角色: " + centerCharacter.name);
            }
            else
            {
                target = player.transform;
                Debug.Log("相机目标设置为玩家控制器");
            }
            
            // 订阅玩家的中心角色改变事件
            player.OnCenterChanged += HandleCenterCharacterChanged;
        }
        else
        {
            // 如果找不到玩家控制器，则尝试直接查找Player标签的对象
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                target = playerObject.transform;
                Debug.Log("相机目标设置为Player标签对象");
            }
            else
            {
                Debug.LogWarning("无法找到跟随目标，相机将保持静止");
            }
        }
    }
    
    /// <summary>
    /// 处理中心角色改变事件
    /// </summary>
    private void HandleCenterCharacterChanged(Character oldCenter, Character newCenter)
    {
        if (newCenter != null)
        {
            target = newCenter.transform;
            Debug.Log("相机目标更新为新的中心角色: " + newCenter.name);
        }
        else
        {
            // 如果新的中心角色为空，回退到玩家控制器
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
                Debug.Log("中心角色为空，相机目标回退到玩家控制器");
            }
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.OnCenterChanged -= HandleCenterCharacterChanged;
        }
    }
    
    public void LateUpdate()
    {
        if (target == null)
        {
            // 如果目标丢失，尝试重新查找
            FindAndSetTarget();
            return;
        }
        
        // 计算期望位置
        Vector3 desiredPosition = target.position + offset;
        
        // 使用SmoothDamp实现更平滑的相机跟随
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;
        
        // 可选：让相机看向目标
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
}
