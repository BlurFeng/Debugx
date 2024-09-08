# 【Debugx】
***阅读中文文档[中文](README.md)***\
***日本語のドキュメントを読む[日本語](README_ja.md)***\
***Read this document in [English](README_en.md)***\

## 【用户手册】
**阅读中文用户手册[中文用户手册](Documents/UserManual_cn.md)**\
**日本語のユーザーマニュアルを読む[日本語ユーザーマニュアル](Documents/UserManual_ja.md)**\
**Read this User Manual in[English User Manual](Documents/UserManual_en.md)**\

## 【概要】
这是一个Unity引擎的插件。\
用于按调试成员管理DebugLog，并输出log文件到本地。使用宏"DEBUG_X"来开启功能。\

在你的代码中，直接使用Debugx.Log()就能简单得进行打印。\
不同的成员使用不同的key，方便我们区分和快速找到对应代码的负责人。\
![Debugx代码](./Docs/DebugxCode.png)

在UnityDOTS的Burst中。我们必须使用DebugxBurst而不是Debugx，因为很多方法和字段在Burst中将不可用。\
![DebugxBurst代码](./Docs/DebugxBurst.png)