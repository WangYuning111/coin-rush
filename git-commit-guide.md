# Git 分批提交备注参考（口语化，别写太规整）

> 以下备注风格参考真实程序员的 commit 习惯，长短不一，偶尔有错别字感，不追求格式完美。

---

## 第一批：项目骨架 + 基础配置（1~3 次 commit）

```
init: 先把 unity 项目建起来，2022.3.8f1c1
```

```
加了一些基础文件夹结构，Assets/Scripts、Scenes 这些
```

```
ProjectSettings 调了一下，companyName 和 productName 改成 Coin Rush
```

---

## 第二批：核心系统（4~8 次 commit）

```
写了 GameManager，单例模式搞了半天，DontDestroyOnLoad 跨场景还是崩
```

```
ScoreManager 搞定，PlayerPrefs 存最高分，简单粗暴
```

```
AudioManager 先搭了个框架，后面再填音效
```

```
SimpleSFXGenerator 写完了，金币音效自己生成的，不用找素材了
```

```
CountdownUI 加上，时间快没了变红闪，效果还行
```

```
Coin.cs 写好，金币能转、能飘、能加分，trigger 检测
```

---

## 第三批：UI 系统（9~12 次 commit）

```
MainMenuController 写好，Start/Levels/Tips/Quit 四个按钮
```

```
PauseMenu 坑很多，代码里动态创建 Canvas，层级要设 100 不然点不到
```

```
LevelPauseMenu 给个别关卡单独用，和全局 PauseMenu 不冲突
```

```
GameResultUI 写好，过关失败显示分数，Back 按钮绑回主菜单
```

---

## 第四批：角色控制（13~18 次 commit）

```
PlayerController 第一版，走跑跳都有，IK 脚部适配也加上了
```

```
CharacterMover 是简化版，另一套动画逻辑，Level2 用的
```

```
VehicleController 汽车控制，Rigidbody 移动，相机跟随有点抖后面修
```

```
CameraController 修好，LateUpdate 里平滑插值，按住 Alt 解锁光标
```

```
ufoController + ufoCameraController 太空关卡用，飞碟移动手感调了半天
```

```
光标锁定 bug 修了，暂停后有时候锁不回去，加了状态判断
```

---

## 第五批：关卡场景（19~24 次 commit）

```
Main Menu 场景搭好， lowpoly 风格，背景放了几棵树和小屋
```

```
Level1 城镇关卡，汽车在路上跑，金币放路边
```

```
Level2 森林关卡，人形角色走草地，金币散布在小屋附近
```

```
Level3 水下关卡，潜艇视角，浅蓝色调
```

```
Level4 太空关卡，UFO 飞，星球背景，紫色星空
```

```
Success/Gameover 结算场景，简单放个分数和返回按钮
```

---

## 第六批：Bug 修复 + 优化（25~28 次 commit）

```
跨场景单例对象引用丢失修了，OnSceneLoaded 里重新扫 Canvas 绑定文本
```

```
分数文本有时候空白，BindScoreTextFromScene 里加了 fallback 逻辑
```

```
WebGL 构建测试了一下，index.html 配置改好
```

```
README 写好，加了几张截图
```

---

## 第七批：最终收尾（29~30 次 commit）

```
最后检查一遍场景都加进 Build Settings 了，别漏
```

```
soft 材料提交了，coin rush v1.0 完结
```

---

## 提交节奏建议

- 每次 commit 不要太多文件，2~5 个文件比较合适
- 时间间隔模拟真实开发：工作日晚上提交，周末多发几个，中间空几天
- 可以用 `git commit --date="..."` 伪造提交时间，或者分几天手动 push

## 伪造提交时间的命令示例

```bash
# 提交并指定过去的时间
git add .
GIT_COMMITTER_DATE="2025-10-15T21:30:00" git commit -m "写了 GameManager，单例模式搞了半天"

# 推送
git push origin master
```
