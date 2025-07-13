# 【Debugx】
***阅读中文文档 >[中文](README.md)***\
***日本語のドキュメントを読む >[日本語](README_ja.md)***\
***Read this document in >[English](README_en.md)***

## 【User Manual】
**阅读中文用户手册 >[中文用户手册](Documents/UserManual_cn.md)**\
**日本語のユーザーマニュアルを読む >[日本語ユーザーマニュアル](Documents/UserManual_ja.md)**\
**Read this User Manual in >[English User Manual](Documents/UserManual_en.md)**

## 【UPM】
允许通过 Package Manager 将插件加载到 Unity 项目中。\
打开 Unity 菜单，进入 Window > Package Manager。\
点击窗口左上角的 + 按钮，然后选择 Add package from git URL...\
粘贴以下链接，即可将插件作为包安装到项目中：\
https://github.com/BlurFeng/Debugx.git?path=DebugxDemo/Assets/Plugins/Debugx

## 【概要】
这是一个Unity引擎的插件。\
用于按调试成员管理DebugLog，并输出log文件到本地。使用宏"DEBUG_X"来开启功能。

在你的代码中，直接使用Debugx.Log()就能简单得进行打印。\
不同的成员使用不同的key，可以方便的按成员打印，并快速找到对应代码的负责人。\
![](Documents/Images/DebugxCode.png)

在UnityDOTS的Burst中。我们必须使用DebugxBurst而不是Debugx，因为很多方法和字段在Burst中将不可用。\
但因为UnityDOTS更新的非常频繁，在不同DOTS版本下，此方法不能保证完全的可靠性。\
![](Documents/Images/DebugxBurst.png)