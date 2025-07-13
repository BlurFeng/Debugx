# 【Debugx】
***阅读中文文档 >[中文](README.md)***\
***日本語のドキュメントを読む >[日本語](README_ja.md)***\
***Read this document in >[English](README_en.md)***

## 【User Manual】
**阅读中文用户手册 >[中文用户手册](Documents/UserManual_cn.md)**\
**日本語のユーザーマニュアルを読む >[日本語ユーザーマニュアル](Documents/UserManual_ja.md)**\
**Read this User Manual in >[English User Manual](Documents/UserManual_en.md)**

## 【UPM】
Unity プロジェクトに Package Manager 経由でプラグインを導入できます。\
Unity メニューの Window > Package Manager を開きます。\
ウィンドウ左上の + ボタンをクリックし、Add package from git URL... を選択します。\
以下のリンクを貼り付けると、パッケージとしてプラグインをインストールできます：\
https://github.com/BlurFeng/Debugx.git?path=DebugxDemo/Assets/Plugins/Debugx

## 【概要】
こちらはUnity用プラグインです。\
メンバーによってデバッグログを管理し、ローカルにログファイルを出力することができます。\
マクロ「DEBUG_X」で機能をオンにします。

あなたのコードには、Debugx.Log()を使って簡単に印刷することができます。\
メンバーによって区別して異なるKeyを使用することで、メンバーごとに印刷することができ、対応するコードの担当者を素早く見つけることができます。\
![](Documents/Images/DebugxCode.png)

UnityDOTSのBurstには、Debugxの代わりにDebugxBurstを使用になります。なぜなら、多くのファンクションはBurstでは使えないからです。\
しかし、UnityDOTSは頻繁に更新されているため、ざまざまなDOTSのバージョンで、Debugxのファンクションは完全な信頼性を保証することはできません。\
![](Documents/Images/DebugxBurst.png)