# 【Debugx】
***阅读中文文档[中文](README.md)***\
***日本語のドキュメントを読む[日本語](README_ja.md)***\
***Read this document in [English](README_en.md)***\

## 【用户手册】
**阅读中文用户手册[中文用户手册](Documents/UserManual_cn.md)**\
**日本語のユーザーマニュアルを読む[日本語ユーザーマニュアル](Documents/UserManual_ja.md)**\
**Read this User Manual in[English User Manual](Documents/UserManual_en.md)**\

## 【概要】
This is a plugin for Unity engine.\
The debug log is managed according to its members. Later export the log file to the local. use macro "DEBUG_X" open the functional.\

In your code, use debugx.log () to print like this.\
![Debugx代码](./Docs/DebugxCode.png)

In the Burst of UnityDOTS. We must use DebugxBurst instead of Debugx because many methods and fields will not be available in Burst.\
![DebugxBurst代码](./Docs/DebugxBurst.png)