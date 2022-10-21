# fa

**注：fa语言新编译器重构所有内容，对特性及偏向性有较大程度修改，暂未开源，开发出成品之后开源。此仓库暂时保留老编译器项目，待新编译器完成后替换**

[![license](https://img.shields.io/github/license/fa-org/fa?color=09f)](./LICENSE)
[![csharp](https://img.shields.io/lgtm/alerts/github/fa-org/fa?logo=lgtm)](https://lgtm.com/projects/g/fa-org/fa)
[![csharp](https://img.shields.io/lgtm/grade/csharp/github/fa-org/fa?logo=lgtm)](https://lgtm.com/projects/g/fa-org/fa)
<!--[![download](https://img.shields.io/github/downloads/fa-org/fa/total.svg)](https://github.com/fa-org/fa/releases)-->

fa语言是一款不容易误解、简单易学的编程语言。目前处于测试阶段，可以编译简单的功能代码。

### **qq交流群：1018390466 欢迎一起讨论语法/实现/轮子**

## 目标

- 极简（编译器控制变量位于栈上、堆上、是否引用计数、是否原子化等）（暂未实现）
- 安全（船新错误处理方案，确保能编译的代码无异常。ps:系统异常除外）（暂未实现）
- 元编程（变量加上 "@" 前缀即代表编译期计算）（暂未实现）
- 高性能（llvm ir后端输出，原生指令运行，simd优化）（暂未实现）
- 所有权及释放（通通编译器处理）（暂未实现）
- 强制开发范式（比如强制实现DDD模型）（暂未实现）

<!--- 提前编译多种Native代码与SIMD（比如x86-64编译x86、amd64、MMX、SSE、AVX等），然后分别压缩-->
<!--- 编译出的程序运行后检查当前平台支持的最快速的SIMD指令集，解压指令集代码，并执行-->
<!--	+ 在加速效果非常明显的位置编译SIMD代码-->
<!--- 循环里检测是否所有路径都是同步，如果是，那么警告；循环加上@Sync那么压制警告；加上@Yield那么每次循环均yield-->

## TODO

<!--- .Ok ()、.Err () 函数的处理（理想处理方式：传lambda，接收正确结果或错误信息并执行代码）-->
- 自举
- 判断浮点数时误差处理
- 可空类型变量switch
- 类型switch
- 线程锁
- 标准库

## 进度

- ~~基于C++语言，Antlr前端，llvm binding API后端，实现初版fa语言编译器（废弃）~~
- 基于C#语言，Antlr前端，C#后端，实现初版fa语言编译器
	+ Hello World （√）
	+ 可空类型（√）
	+ 类，对象，表达式，循环（√）
	+ 枚举类型（√）
	+ switch（√）
- 基于fa语言，fa前端，llvm ir后端，实现自举fa语言编译器
	+ 元编程
	+ 函数降级
	+ 状态机
	+ 继承

## 特性

- 可选类型
	+ 任意类型后接问号表示，比如 `int?`
	+ 可以储存此类型的值（`void?` 除外），也可以储存一条错误信息
	+ 可选类型可与原始类型计算，结果为可选类型
	+ 通过它代替异常实现机制
	+ 当函数返回类型为可选类型时，代表计算出错自动返回调用者
		```
		int? func1 () {
			int? a = 5 / 0; // 此时a为除0异常
			int b = a + 3; // 异常结果b无法存放，异常信息在此处return，不执行后面代码
			return b;
		}
		int func2 () {
			int? a = 5 / 0; // 此时a为除0异常
			int b = a + 3; // 异常结果b无法存放，函数返回类型也无法接收，编译报错
			return b;
		}
		```
- 全局异步任务（暂未实现）
	+ 方法默认纯异步实现，方法体内无异步代码可转同步方式实现
	+ 无future、async、await等关键字或概念，用户只需要撸代码
	+ 通过独立线程实现所有异步锁的占用及控制

fa语言快排实现：

![compile](imgs/20211222001053.jpg)

<!--![stars](https://starchart.cc/fa-org/fa.svg)

## 当前进度

- [ ] 基本语法
	+ [x] 表达式
	+ [x] 类、对象、继承（暂不完全）
	+ [ ] 异步
	+ [ ] linq
- [ ] 编译系统
	+ [x] 支持Windows x86平台
	+ [ ] 支持交叉编译
	+ [ ] 支持Windows、Linux、Mac平台
- [ ] 速度、体积优化
	+ [ ] 并发优化
	+ [ ] SIMD支持
		* [ ] 计划支持：SSE4.2、AVX、AVX2、AVX512、neon
	+ [ ] 可执行代码压缩（待定）
- [ ] ...

## 基本语法特性
- 构造函数允许返回null，代表失败
- meta变量编译时求值
- 接近C#的语法
- 纯stackful
- 进程间调用
  - 创建子进程时，能共享对象（通过共享段或共享内存实现）
- 浮点数==比较为指定精度比较，精确比较使用===
- 文件夹代表命名空间
- 类变量没有赋予初值时，强制要求声明对象时赋初值
- 不区分栈变量、堆变量与引用变量，无需手工使用智能指针
- 函数自动微分

## 示例代码

```fa
// 声明类实例
public class TestClass {
	// 字段
	public string Value1;
	public string Value2 = "hello";
	public string Value3 { get; set; }
	public string Value4 { get; set; } = "hello";

	// 可以被赋予哪些类型的值
	public static TestClass From (int32 _n) {}
	public static TestClass From (double _n) {}
	public static TestClass From (string _s) {}

	// 定义函数
	public string func () {}
}
-->





<!--
编写LLVM Pass模块知识点梳理√
https://blog.csdn.net/u010940020/article/details/99721684



七妹要奈斯的LLVM专栏√
https://blog.csdn.net/qq_42570601/category_10200372.html



intel的SPMD优化方案
https://github.com/ispc/ispc



https://zhuanlan.zhihu.com/p/25959684
前言（就是本篇）
考不上三本也能给自己心爱的语言加上Coroutine（一） - 知乎专栏
考不上三本也能给自己心爱的语言加上Coroutine（二） - 知乎专栏
考不上三本也能给自己心爱的语言加上Coroutine（三） - 知乎专栏
考不上三本也能给自己心爱的语言加上Coroutine（四） - 知乎专栏
考不上三本也会实现数据绑定（一） - 知乎专栏
考不上三本也会实现数据绑定（二） - 知乎专栏
考不上三本也会实现数据绑定（三） （作者： @余生梦 ）
考不上三本也能实现C++编译器——前言
考不上三本也能懂系列——处理声明（一）
考不上三本也能懂系列——处理声明（二）
考不上三本也能懂系列——处理声明（三）（新！）
考不上三本也能懂系列——实现C++类型系统（一）
考不上三本也能懂系列——实现C++类型系统（二）
考不上三本也能懂系列——什么是C++的argument-dependent lookup



SIMD简介
https://zhuanlan.zhihu.com/p/55327037



LLVM新建全局变量
https://www.cnblogs.com/jourluohua/p/10813824.html



IR API(四)——操作IR的字符串、全局变量、全局常量及数组
https://blog.csdn.net/qq_42570601/article/details/108007986



创建结构体
https://llvm.org/doxygen/classllvm_1_1StructType.html#a7cf5280be35cd0c973f40c7d87a11acd



可微编程-自上而下的产品形态 5 Swift中的自动微分
https://zhuanlan.zhihu.com/p/133721083



LLVM新建全局变量
https://www.cnblogs.com/jourluohua/p/10813824.html



C++ IRBuilder::CreateStructGEP方法代码示例（有点水）
https://vimsky.com/examples/detail/cpp-ex---IRBuilder-CreateStructGEP-method.html



LLVM Language Reference Manual
https://llvm.org/docs/LangRef.html







//// MT
//@lib "libucrt.lib";
//@lib "libcmt.lib";
////@lib "libvcruntime.lib";

// MD
@lib "ucrt.lib";
@lib "msvcrt.lib";
//@lib "vcruntime.lib";

//@lib "kernel32.lib" "user32.lib" "gdi32.lib" "winspool.lib" "comdlg32.lib" "ole32.lib";
//@lib "advapi32.lib" "shell32.lib" "oleaut32.lib" "uuid.lib" "odbc32.lib" "odbccp32.lib";

-->
