**基于GDI的Graphics交互,使用DX(基于SharpDX)绘图的绘图控件,简单的图元管理方式与使用方式,比GDI更好的性能.**

因为工作中需要一个绘图框架,但找了不少库都不是很容易使用,要么接口复杂,要么使用麻烦,所以自己撸一个吧,毕竟我也勉强算个程序员?

这个控件的使用接口如下:

![image](https://github.com/m5home/DXDrawPad/blob/master/interface.png)

控件的工作思路很简单:

就是有一堆逻辑上的\[**子画布**\],用户在这些画布上绘图,贴图片什么的(使用的是**Graphics对象**),然后交由控件使用**Direct2D**来依顺序(**添加子画布,调用AddDrawObject的顺序**)绘制到控件中.

由于使用GDI的**Graphics**对象来交互,所以使用上很方便,没什么不一样.

绘制的内容,再由DX来绘制,这样效率就非常高.

由于是依顺序绘制的,所以也就有层的概念了,__先添加的子画布先绘制,即在Z顺序的底层.__

中间有一层从GDI的Bitmap转换为SharpDX的Bitmap过程(ConvertGdiBitmap函数),实现流程比较绕:

先用System.Drawing.Image把内容写入到Stream,再用SharpDX的BitmapDecoder从Stream中读回来,并最终转化为SharpDX.Direct2D1.Bitmap.

这应该是最低性能的一环了.

代码中的工程显示了一个大底图,并使用按钮绘制一个实心圆及线圈,以及几个GIF图象显示,有固定的也有跟随鼠标的.

别的就没啥了:\)
