# Coin Rush V1.0

> 王昱宁、苏菩涵 | Unity 2022.3.8f1c1 | 2025–2026

---

## 开发踩坑记录

这个项目一开始只是个课程作业，没想到做到后面坑越踩越多。最大的麻烦是**跨场景单例对象失效**——GameManager、ScoreManager 这些用 `DontDestroyOnLoad` 保留下来的对象，换场景之后场景里的 UI 引用全丢了，分数文本直接变空白。最后解决办法是在 `OnSceneLoaded` 里手动重新扫描 Canvas 下的 Text，按关键词（coin / score）重新绑定，才勉强稳住。

另一个坑是**光标锁定**。第一人称/第三人称关卡里需要隐藏鼠标、锁定光标，但一按 Esc 打开暂停菜单，光标解开后有时候就锁不回去了。后来发现是 `CursorLockMode` 和 `Cursor.visible` 不同步，加了判断 `if (Cursor.lockState != CursorLockMode.Locked)` 再重新锁一次才解决。

汽车关卡的相机也比较头疼。一开始直接用 Camera.main 跟着车跑，结果车转向的时候相机一顿一顿的。后来换成 LateUpdate 里用 `Quaternion.Slerp` 平滑插值，再限制一下俯仰角 `Clamp(-maxPitchAngle, maxPitchAngle)`，手感才舒服点。还加了按住 `LeftAlt` 可以临时解锁光标，方便截图和调试。

音效方面，素材网站找了一圈没找到合适的，干脆自己写了个 `SimpleSFXGenerator`，用代码生成 AudioClip。金币音效参考了马里奥那种双音调上升，三角波混正弦波；过关失败分别用 C 大调和 G-E-C 下行音阶。虽然简陋，但好歹不用依赖外部资源。

UI 做暂停菜单的时候也折腾了一阵。一开始直接用场景里已有的 Canvas 加按钮，结果跟关卡里的 Back 按钮层级打架，点不到。后来干脆代码里动态创建一个独立的 PauseCanvas，`sortingOrder = 100`，确保永远在最上层，再自己铺半透明遮罩和三个按钮。这样每个场景都能统一复用，省得每个场景都手动搭一遍。

Level1 到 Level4 分别用了不同角色：汽车、人形、潜艇、UFO。每个角色的移动逻辑和相机跟踪都不一样，所以拆成了 `VehicleController`、`PlayerController`、`ufoController` 几个脚本，没有硬塞到一个文件里。不过这也意味着每个关卡都要挂不同的脚本，场景设置起来有点麻烦。

## 项目结构

```
Assets/
├── Scenes/
│   ├── Main Menu.unity        # 主菜单，Start/Levels/Tips/Quit
│   ├── Level1.unity           # 汽车驾驶（城镇）
│   ├── Level2.unity           # 人形行走（森林）
│   ├── Level3.unity           # 潜艇水下
│   ├── Level4.unity           # UFO 太空
│   ├── Success.unity          # 过关结算
│   └── Gameover.unity         # 失败结算
├── Scripts/
│   ├── GameManager.cs         # 关卡切换、计时器、胜负判定
│   ├── ScoreManager.cs        # 金币计数 + 最高分存档（PlayerPrefs）
│   ├── AudioManager.cs        # BGM / SFX 管理，含代码生成音效
│   ├── PauseMenu.cs           # 全局暂停菜单（代码动态创建 UI）
│   ├── LevelPauseMenu.cs      # 部分关卡的独立暂停处理
│   ├── MainMenuController.cs  # 主菜单按钮事件绑定
│   ├── PlayerController.cs    # 人形角色（含 IK 脚部适配）
│   ├── VehicleController.cs   # 汽车操控 + 跟随相机
│   ├── CharacterMover.cs      # 简化版人形移动（另一套动画逻辑）
│   ├── ufoController.cs       # UFO 移动
│   ├── ufoCameraController.cs # UFO 第三人称相机
│   ├── Coin.cs                # 金币自转、浮动、触发加分
│   ├── CountdownUI.cs         # 倒计时显示（时间不足变红闪烁）
│   ├── GameResultUI.cs        # 结算界面分数显示
│   ├── BackToMenuButton.cs    # 返回主菜单按钮通用脚本
│   ├── ShowCursorOnResult.cs  # 结算场景强制显示鼠标
│   ├── RuntimeInitializer.cs  # 编辑器直接运行时自动初始化单例
│   └── SimpleSFXGenerator.cs  # 纯代码生成 8-bit 风格音效
```

## 运行方式

1. 用 **Unity 2022.3.8f1c1** 打开项目。
2. 在 `File -> Build Settings` 里确认 `Main Menu` 和 4 个 Level 场景已加入 Build。
3. 直接 Play 运行，或 Build 成 Standalone / WebGL。

WebGL 版本构建后，把 `Team13_project/index.html` 和 `Build/` 目录一起部署即可。

## 操作说明

| 按键 | 功能 |
|------|------|
| `WASD` / 方向键 | 移动 |
| `鼠标` | 旋转视角 |
| `Esc` | 打开/关闭暂停菜单 |
| `Left Alt` | 临时解锁鼠标 |
| `Left Ctrl` | 切换走/跑（人形关卡） |
| `Left Shift` | 冲刺（人形关卡） |

## 游戏截图

**图 1**：主菜单界面。Coin Rush 标题、Start / Levels / Tips / Quit 四个按钮，背景是 Lowpoly 风格的树林和小屋。

![主菜单](./screenshots/screenshot1.jpg)

**图 2**：游戏暂停界面。半透明黑色遮罩，显示 PAUSED 标题和 Resume / Restart / Main Menu 三个按钮。

![暂停界面](./screenshots/screenshot2.jpg)

**图 3**：Level 1 汽车驾驶关卡。玩家操控绿色小汽车在城镇道路上行驶，收集路边的金币。

![汽车关卡](./screenshots/screenshot3.jpg)

**图 4**：Level 2 角色行走关卡。人形角色在草地森林场景中行走，金币散布在小屋附近。

![行走关卡](./screenshots/screenshot4.jpg)

**图 5**：Level 3 水下场景关卡。黄色潜艇在浅蓝色海洋中航行，周围有岩石和珊瑚。

![水下关卡](./screenshots/screenshot5.jpg)

**图 6**：Level 4 太空场景关卡。UFO 在紫色星空中飞行，背景有带光环的星球和小行星。

![太空关卡](./screenshots/screenshot6.jpg)

---

## 软著信息

- **游戏名称**：Coin Rush V1.0
- **开发完成日期**：2026 年
- **第一著作人（著作权人）**：王昱宁
- **著作人**：王昱宁、苏菩涵

### 分工说明

**王昱宁（第一著作人）**

负责软件整体架构设计、核心游戏逻辑开发与功能实现。主要完成：
- 游戏主体框架搭建（单例管理器、跨场景持久化）
- 玩家角色运动与控制系统（人形走跑跳 + IK、汽车驾驶、UFO 飞行）
- 碰撞检测与物理逻辑（金币触发、刚体移动）
- 关卡规则设计（4 关难度递进、过关/失败判定）
- 计分与胜负判定系统（实时计分、最高分存档）
- 核心 UI 交互功能开发（主菜单、暂停菜单、结算界面）
- 代码调试优化与整体功能整合
- 主导项目全流程开发与版本迭代

**苏菩涵**

负责游戏美术资源与视听效果制作。主要完成：
- 游戏场景建模与 Lowpoly 风格美术调整
- 游戏界面素材适配与美化
- 配乐制作与音效资源制作导入配置
- 基础动画效果调试（金币旋转浮动、UI 过渡）
