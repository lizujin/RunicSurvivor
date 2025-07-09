# LoseUI 组件设置指南

## 概述

LoseUI 是一个处理游戏失败状态的UI组件，它提供了一个游戏结束面板，显示游戏统计信息，并允许玩家重新开始游戏或退出。与现有的 UIManager 一起使用，可以提供更专注于失败状态的用户体验。

## 设置步骤

### 1. 创建 LoseUI 预制体

1. 在 Hierarchy 窗口右键点击，选择 **UI > Canvas** 创建一个新的 Canvas
2. 将这个 Canvas 重命名为 "LoseUICanvas"
3. 确保 Canvas 设置为 "Screen Space - Overlay" 模式
4. 右键点击 Canvas，选择 **UI > Panel** 创建一个面板
5. 将面板重命名为 "LosePanel"
6. 设置面板大小填满整个屏幕，背景颜色设置为半透明黑色 (RGBA: 0, 0, 0, 0.8)

### 2. 添加 UI 元素

为 LosePanel 添加以下 UI 元素：

1. **游戏结束文本**
   - 创建 Text 组件 (UI > Text)
   - 重命名为 "GameOverText"
   - 设置文本为 "游戏结束"
   - 字体大小设置为 72
   - 颜色设置为红色
   - 锚点设置在面板上部

2. **分数文本**
   - 创建 Text 组件
   - 重命名为 "ScoreText"
   - 设置为 "击杀数: 0"
   - 字体大小设置为 36
   - 颜色设置为白色
   - 锚点设置在面板中上部

3. **波次文本**
   - 创建 Text 组件
   - 重命名为 "WaveText"
   - 设置为 "波次: 0/30"
   - 字体大小设置为 36
   - 颜色设置为白色
   - 锚点设置在分数文本下方

4. **生存时间文本**
   - 创建 Text 组件
   - 重命名为 "SurvivalTimeText"
   - 设置为 "生存时间: 00:00"
   - 字体大小设置为 36
   - 颜色设置为白色
   - 锚点设置在波次文本下方

5. **重新开始按钮**
   - 创建 Button 组件 (UI > Button)
   - 重命名为 "RestartButton"
   - 设置文本为 "重新开始"
   - 按钮颜色设置为蓝色
   - 锚点设置在面板下部

6. **退出按钮**
   - 创建 Button 组件
   - 重命名为 "QuitButton"
   - 设置文本为 "退出游戏"
   - 按钮颜色设置为红色
   - 锚点设置在重新开始按钮下方

### 3. 添加 LoseUI 组件

1. 选中 LoseUICanvas
2. 点击 **Add Component**
3. 搜索并添加 "LoseUI" 脚本

### 4. 配置 LoseUI 组件

在 Inspector 窗口中为 LoseUI 组件设置以下引用：

1. **Lose Panel**: 拖拽 LosePanel 到此字段
2. **Game Over Text**: 拖拽 GameOverText 到此字段
3. **Score Text**: 拖拽 ScoreText 到此字段
4. **Wave Text**: 拖拽 WaveText 到此字段
5. **Survival Time Text**: 拖拽 SurvivalTimeText 到此字段
6. **Restart Button**: 拖拽 RestartButton 到此字段
7. **Quit Button**: 拖拽 QuitButton 到此字段

### 5. 设置动画参数（可选）

在 LoseUI 组件中，可以设置以下参数：

- **Fade In Time**: 设置淡入动画的时长（默认为 1.0 秒）
- **Use Animation**: 勾选此项以启用淡入动画

### 6. 创建预制体

1. 将 LoseUICanvas 拖拽到 Project 窗口中的 Prefabs 文件夹
2. 这将创建一个 LoseUICanvas 预制体，可重复使用

### 7. 将 LoseUI 添加到场景中

1. 将 LoseUICanvas 预制体拖拽到游戏场景的 Hierarchy 中
2. 确保 LosePanel 初始状态设置为隐藏（取消勾选 Inspector 中的 "Active" 复选框）

## 与 GameManager 集成

LoseUI 组件会自动在启动时寻找 GameManager 并注册游戏结束事件。当 GameManager 触发游戏结束事件，且 isVictory 为 false 时，LoseUI 将显示失败界面。

## 测试

要测试 LoseUI 组件，可以：

1. 在游戏运行时，按下 ESC 键进入调试模式
2. 选中 LoseUICanvas 对象
3. 在 Inspector 中点击 LoseUI 组件上的 "Trigger Game Loss" 按钮

这将模拟游戏失败，并显示 LoseUI 界面。

## 与 UIManager 协同工作

LoseUI 可以与现有的 UIManager 协同工作。UIManager 处理一般的 UI 状态，包括游戏结束，而 LoseUI 专注于提供更丰富的失败体验。

若要确保 LoseUI 和 UIManager 不冲突，可以在 UIManager 的 HandleGameOver 方法中，当 victory 为 false 时，不显示游戏结束面板，让 LoseUI 接管。
