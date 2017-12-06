# LitEngine
LitEngine 基于ILRuntime实现的多App开发架构.

测试工程演示了如何创建App,以及资源管理,脚本外部调用等.


目前使用的unity版本为5.52.如需不同版本,请自行替换UnityEngine.dll引用后,重新生成

 **支持多App结构.类似腾讯游戏大厅的设计.但应避免多个App同时启动.因为Unity的资源加载问题,内容相同的文件,即使目录不同,也会认为是同一个文件**

 **Proto导出工具见名下 ProtoToCS 工程** 

**关于CLR绑定**

 	CodeTool_LS可以添加外部CLR绑定委托.在创建GameCore时,会自动调用.例:
	//设置CLR绑定委托
    LitEngine.CodeTool_LS.CLRBindingDelegate = ILRuntime.Runtime.Generated.CLRBindings.Initialize;

    生成CLR绑定类在CreatCLRBindingFile.cs文件中有示例.复制到Unity工程下的Editor下去掉注释即可.

**关于委托的使用**

	ILRuntime中的委托,不经过注册是无法使用的.
	CodeTool_LS初始化时默认注册了一些用到的委托.如果有额外的需要,可以使用外部委托方法自行添加.例:
	
```
	LitEngine.CodeTool_LS.RegisterDelegate = TestRegister.RegisterDelegate;

	public TestRegister
	{
		static public void RegisterDelegate(ILRuntime.Runtime.Enviorment.AppDomain _app)
		{
			_app.DelegateManager.RegisterMethodDelegate<float>();
		}
	}

```
	

 **说明:** 

	1.ILRuntime封装完毕.通过一个入口可以方便的进行使用.提供了对应的交互接口.见测试工程.

	2.封装了Unity接口,完成了与脚本层的对接.接口会自动注册的方法包括"Start","Awake","OnDestroy"等.详情见ScriptInterface命名空间.

	3.多App结构.可以包含多个App同时发布,数据不互通,定义不互通.

	4.资源一键导出到指定目录.一键移动到指定目录.

	5.资源加载模块.同步模式可以像加载Resources下的资源一样,通过名字直接加载. 一次Load对应一次Release.不推荐直接Remove.会删除掉可能正在使用的资源.

	6.封装好的TCP模块,已适配IPV6.数据高低位可设置.

	7.Update管理器.所有的Update通过管理器运行.已实现的接口会自动判断脚本层是否有对应的方法,实现自动注册和释放.每个Update默认会带有0.1s的间隔.如需要无间隔,只需设置对应的MaxTime.请谨慎设置间隔.Update,FixedUpdate,LateUpdate,OnGUI只保留了无参数方法的检测.

	8.Zip模块.UnZipTask.UnZipFileAsync 异步解压.使用方法见测试工程.

	9.DownLoad模块.DownLoadTask.DownLoadFileAsync 异步下载.使用方法见测试工程.

	10.轻量级Xml加载.

	11.支持ILRuntime的Protobuf转换工具.

	12.工具类.包括DLog日志输出类.LogToFile日志存储文件.

	13.如果需要更新ILRuntime库,请删除IL目录下的ILRuntime代码,放入新的,重新编译.需要修改AppDomin和DelegateManager类为Partail


 **注意事项:** 

	1.不推荐函数重载,尤其起参数个数一样的重载,会找不到对应的方法.

	2.委托不能随意定义和使用,请使用系统默认的.部分默认注册的请查看CodeTool_LS.cs.如需其他类型,请自行注册.

	3.资源请注意释放.资源管理方式采用的是Retain + Release方式管理.计数为0会自动删除.用过OC的同学可能比较熟悉.这么做是为了避开Unity资
	源管理的问题.及时释放资源.详细的加载释放过程,请查看测试工程代码.

  	4.AppCore.DestroyGameCore删除App会清除所有基于App加载的资源.包括所有ILRuntime脚本对象.App间互相切换建议使用AppCore.AppChange方法.
  	因为当调用DestroyGameCore方法后,之后的下一句代码已然不会再执行了.C#层次的不受限制.当然,也可以自行再C#层实现相关切换方法.

  	5.GameCore.DontDestroyOnLoad 请使用指定的方法添加DontDestroyOnLoad对象.同时提供了删除方法.这样就可以再删除App时同时删除所有对象了.


 **命名空间 LitEngine** 

 **ScriptInterface** 命名空间下定义了与unity对象的接口

 **Loader** 命名空间下为资源加载模块

 **ProtoCSLS** 命名空间下为protobuf解析模块.(转换工具更新后,不再需要此模块了.暂时保留)

 **NetTool** 网络模块,包含TCP,UDP,WWW的一些封装 UDP没有进行网络适配

 **ReaderAndWriterTool** 比较简单的加密

 **XmlLoad** XML加载模块

 **DownLoad** 下载

 **UnZip** 解压缩

 **管理器类** 

**AppCore**  App核心

**GameCore** App下的游戏核心

**ScriptManager** 脚本管理

**LoaderManager** 资源管理

**GameUpdateManager** 更新逻辑处理

 **使用方法:** 
测试工程已上传,在名下另一个工程

示例:

```
using LitEngine;

//取得editor下的工程目录 载入dll库适用editor模式,其他平台调用其它接入方法
string tapppath = System.IO.Directory.GetCurrentDirectory() + "\\";
//app1的dll文件,无后缀
string tpath = tapppath + "dllproject/testproj/testproj/bin/Release/testproj";
AppCore.CreatGameCore("testapp1").SManager.LoadProject(tpath);//app加载dll文件
//初始化app2
string tpathapp2 = tapppath+"testapp2/testapp2/bin/Release/testapp2";
AppCore.CreatGameCore("testapp").SManager.LoadProject(tpathapp2);//app2创建

//app1创建一个入口对象 1个整型参数,
object tobj = AppCore.App["testapp1"].SManager.CodeTool.GetCSLEObjectParmas("TestA", 2);

//app1 的对象 调用log入口方法 
AppCore.App["testapp1"].SManager.CodeTool.CallMethodByName("log", tobj);

//详细示例查看名下示例工程


```