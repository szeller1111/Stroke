Stroke 是一款鼠标手势程序。它允许你通过划动鼠标来执行特定的操作。你可以使用 Stroke.Configure 来帮助你轻松地完成相关的设定。

首先你需要了解的是“动作”和“动作包”的概念，动作包是若干动作的集合，这里的动作指的是通过特定的手势执行特定的操作，这些操作需要你编写 C# 代码来实现，你的代码最终会在程序运行时插入到一个临时创建的方法体中。为了方便使用，你可以自行编写动态链接库（dll），其命名空间建议使用“Stroke”，否则在编写脚本的时候你需要指定其所在的命名空间。另外，大多数常用的 .NET Framework 的命名空间已经被引入，你可以在脚本中直接使用。动作包主要是为了匹配操作环境而设计的，这里的操作环境指的是当前被操作的窗体，你需要在动作包的代码区域填写正则表达式来匹配窗体所属程序的路径，每行填写一条模式字符串，若路径与某条模式字符串之间存在匹配成功的部分则动作包里的动作才有可能被触发。每次使用鼠标划出手势最多只能触发一个动作，且动作包的匹配顺序是从后往前的，换句话说，如果在后面的动作包中有动作匹配成功了，那么其他的所有在它前面动作包内的动作都将不会再被触发。因此，建议将全局类动作包放在靠前的位置，这样就不会影响特定程序的动作匹配了。

## Base 库

为了方便你编写实用的脚本，我提供了 Base.dll，以下介绍这个库所提供的功能：

- Base.Data：是一个 Dictionary<string, object> 类型的对象，它能够解决脚本中只能声明局部变量的问题。

- Base.KeyDown(Keys key)：按下键盘上的某个键。Keys 定义在 [System.Windows.Forms.Keys](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys) 中。

- Base.KeyUp(Keys key)：弹起键盘上的某个键。

- Base.PressKeys(string keys)：允许你执行一串按键序列操作。以下列出该函数所支持的所有字符（不区分大小写）及其含义：
  
  - 所有英文字母和数字：按下并弹起对应的键。
  
  - 修饰键：
    
    - (：按下 Ctrl 键。
    - )：弹起 Ctrl 键。
    - \[：按下 Shift 键。
    - \]：弹起 Shift 键。
    - {：按下 Alt 键。
    - }：弹起 Alt 键。
    - \<：按下 Win 键。
    - \>：弹起 Win 键。
  
  - 其他：
    
    - \t：Tab 键。
    - \r：Return(Enter) 键。
    - \e：Escape 键。
    - \s：Space 键。
    - \b：Backspace 键。
    - \i：Insert 键。
    - \d：Delete 键。

- Base.WindowState：它是关于窗口状态的枚举类型，有以下四种：
  
  - Normal：正常。
  
  - Minimize：最小化。
  
  - Maximize：最大化。
  
  - Close：关闭。

- Base.SetWindowState(WindowState state)：设置当前活动窗体的状态。

- Base.GetWindowState()：获取当前活动窗体的状态，返回类型为 Base.WindowState。

- Base.Run(string fileName, string arguments = "", string workingDirectory = "")：启动指定的应用程序或文件。


