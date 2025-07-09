# 西游记幸存者 - 场景设置指南

本文档提供了如何设置场景并启动游戏的详细步骤说明。

## 快速场景设置（推荐方式）

1. **打开SampleScene场景**
   - 导航到 `Assets/Scenes/SampleScene.unity` 并双击打开

2. **添加GameSceneInitializer组件**
   - 在Hierarchy面板中，点击"Create"按钮并选择"Create Empty"
   - 将新对象命名为"SceneInitializer"
   - 在Inspector面板中，点击"Add Component"按钮
   - 搜索并添加"GameSceneInitializer"组件

3. **配置预制体引用**
   - 如果您已使用PrefabCreationTool创建了预制体，请将它们拖拽到相应字段中：
     - 玩家预制体
     - 游戏控制器预制体
     - 敌人管理器预制体
     - 商店管理器预制体
     - 协同效应管理器预制体
     - UI管理器预制体
     - 相机预制体
     - 环境预制体
     - 灯光预制体
     - UI预制体

4. **设置场景**
   - 在Inspector面板中，点击"设置游戏场景"按钮
   - 这将创建所有必要的场景对象和生成点

5. **设置启动选项**
   - 找到场景中的"SceneSetup"对象（如果没有，将由上一步创建）
   - 在其Inspector面板中，勾选"Start Game Immediately"选项以便游戏启动时自动开始

6. **运行游戏**
   - 点击Unity顶部的播放按钮
   - 游戏将自动初始化并启动

## 手动场景设置（高级用户）

如果您需要更精细的控制，可以按照以下步骤手动设置场景：

1. **创建核心管理器**
   - 使用预制体创建工具创建管理器预制体
   - 在Hierarchy面板中，创建一个空对象并命名为"Managers"
   - 将以下管理器添加为子对象：
     - GameController
     - SynergyManager
     - EnemyManager 
     - ShopManager
     - UIManager

2. **设置玩家和生成点**
   - 创建一个空对象并命名为"PlayerSpawnPoint"
   - 将其位置设为(0, 0, 0)或您希望玩家开始的位置
   - 创建一个空对象并命名为"EnemySpawnPoints"
   - 添加多个子对象作为敌人生成点，并将它们放置在地图周围

3. **创建环境**
   - 添加一个平面作为地板
   - 添加其他环境元素（障碍物、装饰等）

4. **设置用户界面**
   - 添加Canvas对象
   - 添加必要的UI元素（状态面板、生命条、经验条等）

5. **添加SceneSetup组件**
   - 创建一个空对象并命名为"SceneSetup"
   - 添加SceneSetup组件
   - 设置所有必要的预制体引用
   - 勾选"Start Game Immediately"以自动启动游戏

6. **配置GameController**
   - 将playerSpawnPoint设置为您创建的生成点
   - 将playerPrefab设置为您的玩家角色预制体
   - 设置其他游戏参数

7. **运行游戏**
   - 点击Unity顶部的播放按钮
   - 游戏将初始化并启动

## 预制体创建工具使用方法

1. **打开预制体创建工具**
   - 在Unity顶部菜单中，选择"西游记幸存者 > 预制体创建工具"

2. **创建角色预制体**
   - 设置角色名称（如"唐僧"）
   - 选择种族（如Human）和派别（如Buddhist）
   - 可选：提供模型作为模板

3. **创建管理器预制体**
   - 勾选您需要创建的管理器

4. **创建UI预制体**
   - 勾选您需要创建的UI组件

5. **设置输出路径**
   - 选择预制体保存的文件夹

6. **创建预制体**
   - 点击"创建预制体"按钮
   - 所有选定的预制体将被创建并保存到指定位置

## 游戏启动流程

当场景启动时，以下流程会自动执行：

1. SceneSetup组件初始化场景
2. GameController被创建并初始化
3. 各个管理器被创建并初始化
4. 玩家角色被生成在玩家生成点
5. 如果设置了"Start Game Immediately"，游戏会自动开始第一波
6. 否则，会显示教程（如果启用）或等待玩家手动开始

## 调试提示

- 如果游戏未自动启动，请检查SceneSetup组件中的"Start Game Immediately"选项
- 如果玩家未生成，请检查GameController中的playerPrefab引用
- 如果遇到空引用错误，请确保所有必要的预制体引用都已正确设置
- 使用控制台查看初始化过程中的日志消息

## 添加自定义西游记角色

1. 使用预制体创建工具创建基本角色
2. 修改角色预制体，添加自定义模型、技能和属性
3. 将角色添加到ShopManager中的可用角色池中
