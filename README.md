# Debugx
This is a plugin for Unity engine.<br />
The debug log is managed according to its members.use macro "DEBUG_X" open the functional.<br />
这是一个Unity引擎的插件。<br />
此插件用于以成员的方式管理调试日志。使用宏"DEBUG_X"来开启功能。<br />

![框架图](./Assets/Debugx/Docs/Debugx框架图.jpeg)

Configure debug members through the edit window.<br />
通过编辑窗口来配置调试成员。<br />
![成员配置编辑窗口](./Assets/Debugx/Docs/DebugxMemberWindow.png)

Add the debug manager to the project scenario and set the switch through the inspector. <br />
You can also set the switch dynamically in code.<br />
添加调试管理器到项目场景，并通过检视面板设置开关。你也可以在代码中动态的设置开关。<br />
![调试管理器](./Assets/Debugx/Docs/DebugxManager.png)

In your code, use debugx.log () to print like this.<br />
在你的代码中，像这样去使用Debugx.Log()来进行打印。<br />
![Debugx代码](./Assets/Debugx/Docs/DebugxCode.png)

This is the Log used in the Burst of UnityDOTS. <br />
We must use DebugxBurst instead of Debugx because many methods and fields will not be available in Burst.<br />
这是在UnityDOTS的Burst中使用的Log。我们必须使用DebugxBurst而不是Debugx，因为很多方法和字段在Burst中将不可用。<br />
![DebugxBurst代码](./Assets/Debugx/Docs/DebugxBurst.png)