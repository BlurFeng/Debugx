# Debugx
This is a plugin for Unity engine.<br />
The debug log is managed according to its members. Later export the log file to the local. use macro "DEBUG_X" open the functional.<br />
这是一个Unity引擎的插件。<br />
用于按调试成员管理DebugLog，并输出log文件到本地。使用宏"DEBUG_X"来开启功能。<br />

In your code, use debugx.log () to print like this.<br />
在你的代码中，像这样去使用Debugx.Log()来进行打印。<br />
![Debugx代码](./Docs/DebugxCode.png)

This is the Log used in the Burst of UnityDOTS. <br />
We must use DebugxBurst instead of Debugx because many methods and fields will not be available in Burst.<br />
这是在UnityDOTS的Burst中使用的Log。我们必须使用DebugxBurst而不是Debugx，因为很多方法和字段在Burst中将不可用。<br />
![DebugxBurst代码](./Docs/DebugxBurst.png)
