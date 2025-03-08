//依赖:

//SharpDX.4.2.0
//SharpDX.Direct2D1.4.2.0
//SharpDX.DXGI.4.2.0

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;

/// <summary>
/// 基于D2D的绘图控件.
/// <para>总体思路:</para>
/// 
/// </summary>
public partial class DXDrawPad : UserControl
{
    SharpDX.Direct2D1.Factory2 oDXFactory = new SharpDX.Direct2D1.Factory2();
    WindowRenderTarget oRenderTarget;

    Control oInvoke;
    /* todo: 将字典的Key转化为int，通过 String.GetHashCode() 获取。
     * Dictionary 在使用 string 作为 Key 时，会通过二进制比较来确定是否相等。
     * 相比之下，通过int比较，性能会有明显的提升。
     * 但是此操作会影响到调用的难易度，需要封装，为了可读性考虑，此设计暂不实施。
     *
    */
    Dictionary<string, cDrawObject> oDrawObjects = new Dictionary<string, cDrawObject>();
    int mFpsCount = 0;
    cDrawObject mObjFPSDraw;

    Font mFontNumber = new Font("YaHei", 10);

    int mFPS = 0, mFPSSet = 60;
    int mFPSDelay, mFPSTick;

    bool mShowFPS = false;
    bool mIsDrawObject = false;

    int mInReSize = 0;

    public DXDrawPad()
    {
        InitializeComponent();

        var _renderProp = new HwndRenderTargetProperties
        {
            Hwnd = pictureBox1.Handle,
            PixelSize = new Size2(pictureBox1.Width, pictureBox1.Height),
            PresentOptions = PresentOptions.None
        };

        oRenderTarget = new WindowRenderTarget(oDXFactory, new RenderTargetProperties(), _renderProp);

        mFPSDelay = 900 / mFPSSet;
        AddDrawObject(cDrawObject.FPSKey, 100, 20, 0, 0);
        mObjFPSDraw = oDrawObjects[cDrawObject.FPSKey];

        tmrLoop.Enabled = true;
        GDIDrawPad_Resize(null, null);
    }

    private void GDIDrawPad_Load(object sender, EventArgs e)
    {
        oInvoke = this.Parent;
    }

    private void tmrLoop_Tick(object sender, EventArgs e)
    {
        if (mInReSize == 0)
        {
            #region FPS计算
            mFpsCount++;
            if (Environment.TickCount > mFPSTick)
            {
                mFPSTick = Environment.TickCount + 999;
                mFPS = mFpsCount;
                mFpsCount = 0;
                mObjFPSDraw.graphics.Clear(mObjFPSDraw.colorBack);
                mObjFPSDraw.graphics.DrawString("FPS:" + mFPS.ToString(), mFontNumber, Brushes.Yellow, 0, 0);
                mObjFPSDraw.RequireConvert = true;
                oDrawObjects[mObjFPSDraw.ObjectName] = mObjFPSDraw;
            }
            #endregion
            if (StartDraw())
            {
                DrawObjects();
                EndDraw();
            }
        }
        else //为改变大小做准备
        {
            if (mInReSize == 1) { mInReSize = 2; }
        }
    }

    private bool StartDraw()
    {
        if (mIsDrawObject)
        {
            if (oRenderTarget == null) { return false; }
            oRenderTarget.BeginDraw();
            oRenderTarget.Clear(new RawColor4(0, 0, 0, 0));
            return true;
        }
        return false;
    }

    private void EndDraw()
    {
        oRenderTarget.EndDraw();
    }

    private void DrawObjects()
    {
        cDrawObject _FPSItem;
        string[] _keys = oDrawObjects.Keys.ToArray();

        #region 绘制用户对象
        //for (int i = 0; i < _keys.Length; i++)
        //{
        //    oDrawObjects[_keys[i]].UpdateImageDX(ConvertGdiBitmap(oDrawObjects[_keys[i]].imageGDI));
        //    if (item.Value.ObjectName != ST_DRAW_OBJECT.FPSKey)
        //    {
        //        oRenderTarget.DrawBitmap(item.Value.imageDX, item.Value.RectDX, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
        //    }
        //}
        foreach (var item in oDrawObjects)
        {
            oDrawObjects[item.Value.ObjectName].UpdateImageDX();
            if (item.Value.ObjectName != cDrawObject.FPSKey)
            {
                oRenderTarget.DrawBitmap(item.Value.imageDX, item.Value.RectDX, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            }
        }
        #endregion
        #region 绘制FPS
        if (mShowFPS)
        {
            _FPSItem = oDrawObjects[cDrawObject.FPSKey];
            oRenderTarget.DrawBitmap(_FPSItem.imageDX, _FPSItem.RectDX, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
        }
        #endregion
    }

    private void GDIDrawPad_Resize(object sender, EventArgs e)
    {
        mInReSize = 1;

        #region 等待主绘图过程停止绘制
        while (true)
        {
            if (mInReSize == 2) break;
            Application.DoEvents();
        }
        #endregion

        #region 重新关联Graphics对象
        oRenderTarget?.Resize(new Size2(pictureBox1.Width, pictureBox1.Height));
        #endregion

        #region 坐标系变换
        //Matrix transformMatrix = new Matrix();
        //transformMatrix.Translate(0, this.Height); // 移动原点到左下角
        //transformMatrix.Scale(1, -1); // 反转Y轴方向
        //oGpMain.Transform = transformMatrix;
        #endregion

        mInReSize = 0;
    }

    /// <summary>
    /// 添加一个子画布对象
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="objPosX"></param>
    /// <param name="objPosY"></param>
    public void AddDrawObject(string keyName, int width, int height, int objPosX = -1, int objPosY = -1)
    {
        if (objPosX == -1 && objPosY == -1)
        {
            AddDrawObject(keyName, width, height, (this.Width - width) / 2, (this.Height - height) / 2, Color.Yellow, Color.Transparent, Color.Red);
        }
        else
        {
            AddDrawObject(keyName, width, height, objPosX, objPosY, Color.Yellow, Color.Transparent, Color.Red);
        }
    }

    /// <summary>
    /// 添加一个子画布对象
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="objPosX"></param>
    /// <param name="objPosY"></param>
    /// <param name="drawColor"></param>
    /// <param name="backColor"></param>
    /// <param name="selectedColor"></param>
    public void AddDrawObject(string keyName, int width, int height, int objPosX, int objPosY, Color drawColor, Color backColor, Color selectedColor)
    {
        cDrawObject _item = new cDrawObject(keyName);

        _item.initObject(oRenderTarget);
        _item.imageGDI = new Bitmap(width, height);
        _item.graphics = Graphics.FromImage(_item.imageGDI);

        #region 设置画布坐标变化
        //_item.graphics.ScaleTransform(1f, -1f);
        //_item.graphics.TranslateTransform(0, -height);
        #endregion

        _item.color = drawColor;
        _item.colorBack = backColor;
        _item.colorSelected = selectedColor;
        _item.graphics.Clear(_item.colorBack);
        _item.LocationGDI = new Point(objPosX, objPosY);
        _item.RequireConvert = true;

        oDrawObjects.Add(keyName, _item);
    }
    /// <summary>
    /// 获取一个子画布对象.
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="outDrawObject"></param>
    /// <returns></returns>
    public bool GetDrawObject(string keyName, out cDrawObject outDrawObject)
    {
        return oDrawObjects.TryGetValue(keyName, out outDrawObject);
    }

    /// <summary>
    /// 把修改过参数的子画布对象更新到列表
    /// </summary>
    /// <param name="drawObject"></param>
    /// <returns></returns>
    public bool UpdateDrawObject(cDrawObject drawObject)
    {
        if (oDrawObjects.TryGetValue(drawObject.ObjectName, out var obj))
        {
            lock (oDrawObjects)
            {
                drawObject.RequireConvert = true;
                if (!obj.IsSameInstance(drawObject))
                {
                    oDrawObjects[drawObject.ObjectName] = drawObject;
                    obj.Dispose();
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 返回当前坐标所对应的 <see cref="cDrawObject"/> 元素合集。
    /// </summary>
    /// <param name="point">当前的坐标。</param>
    /// <returns>所有包含目标坐标的 <see cref="cDrawObject"/> 实例。</returns>
    public IEnumerable<cDrawObject> GetDrawObjectAt(Point point)
    {
        cDrawObject obj;
        PointF itemPoint;
        foreach (var item in oDrawObjects)
        {
            obj = item.Value;
            itemPoint = obj.LocationGDI;
            if (itemPoint.X <= point.X &&
                itemPoint.Y <= point.Y &&
                itemPoint.X + obj.imageGDI.Width >= point.X &&
                 itemPoint.Y + obj.imageGDI.Height >= point.Y)
            {
                yield return obj;
            }
        }
        yield break;
    }

    /// <summary>
    /// 移除目标实例。
    /// </summary>
    /// <param name="keyName">目标实例的Key</param>
    /// <returns>成功返回 <c>true</c>, 否则返回 <c>false</c></returns>
    public bool RemoveDrawObject(string keyName)
    {
        if (oDrawObjects.TryGetValue(keyName, out var value))
        {
            lock (oDrawObjects)
            {
                oDrawObjects.Remove(keyName);
                value.Dispose();
            }
            return true;
        }
        return false;
    }

    #region 鼠标事件触发
    private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
    {
        OnMouseDown(e);
    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
    {
        OnMouseMove(e);
    }

    private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
    {
        OnMouseUp(e);
    }
    #endregion

    private void RaiseEvent(Delegate eventName, params object[] Params)
    {
        try { if (eventName != null) oInvoke.Invoke(eventName, Params); }
        catch { }
    }

    #region 属性
    /// <summary>
    /// 是否绘制对象
    /// </summary>
    public bool IsDrawObject
    {
        get { return mIsDrawObject; }
        set { mIsDrawObject = value; }
    }
    /// <summary>
    /// 是否显示FPS
    /// </summary>
    public bool IsShowFPS
    {
        get { return mShowFPS; }
        set { mShowFPS = value; }
    }

    public RenderTarget RenderTarget => oRenderTarget;
    #endregion
}

public class cDrawObject : IEquatable<cDrawObject>, IDisposable
{
    /// <summary>
    /// 默认的 <see cref="cDrawObject"/> 实例。
    /// </summary>
    public static readonly cDrawObject Default = new();
    /// <summary>
    /// 统一使用GUID作为FPS的Key，避免和调用者添加的Key冲突。此Key由绘图板占用，调用者不应该使用此Key。
    /// </summary>
    public static readonly string FPSKey = Guid.NewGuid().ToString();
    /// <summary>
    /// 用于ARGB转换的只读属性配置
    /// </summary>
    private static readonly BitmapProperties s_defaultBitmapProperties = new(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));

    public readonly string ObjectName;
    /// <summary>
    /// 当前对象的GDI画布
    /// </summary>
    public Image imageGDI;
    /// <summary>
    /// 当前对象的DX图象
    /// </summary>
    public SharpDX.Direct2D1.Bitmap imageDX;
    /// <summary>
    /// 当前对象画布关联的Graphics对象
    /// </summary>
    public Graphics graphics;
    /// <summary>
    /// 当前画布要绘制在主画布上的坐标,中心原点
    /// </summary>
    public PointF LocationGDI;
    /// <summary>
    /// 当前画布要绘制在主画布上的区域,绝对坐标.
    /// <para>由LocationGDI与imageGDI中的大小计算得来.</para>
    /// </summary>
    public RawRectangleF RectDX;
    /// <summary>
    /// 画布上绘制的对象默认颜色
    /// </summary>
    public Color color;
    /// <summary>
    /// 画布上绘制的对象被选中时的颜色
    /// </summary>
    public Color colorSelected;
    /// <summary>
    /// 画布上背景色
    /// </summary>
    public Color colorBack;
    /// <summary>
    /// 画布对象是否被选中
    /// </summary>
    public bool Selected;
    /// <summary>
    /// 当前对象是否需要从GDI转换为DX
    /// </summary>
    public bool RequireConvert;
    private bool disposedValue;

    WindowRenderTarget oRenderTarget;

    private cDrawObject()
    {
        ObjectName = string.Empty;
    }

    /// <summary>
    /// 初始化 <see cref="cDrawObject"/> 的新实例，并指定其 <see cref="cDrawObject.ObjectName"/>.
    /// </summary>
    /// <param name="objectName"><see cref="cDrawObject.ObjectName"/>, 用于作为此实例的Key.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <remarks>
    /// <see cref="cDrawObject.ObjectName"/> 作为内部字典的Key使用。
    /// 使用构造函数同意赋值，可以避免此Key为 <c>null</c>.
    /// </remarks>
    public cDrawObject(string objectName)
    {
        ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        if (objectName == string.Empty)
        {
            throw new ArgumentException("objectName是空字符串.", nameof(objectName));
        }
    }

    public void initObject(WindowRenderTarget objRenderTarget)
    {
        this.oRenderTarget = objRenderTarget;
    }

    public bool IsSameInstance(cDrawObject other) => Object.ReferenceEquals(this, other);

    public override int GetHashCode() => ObjectName.GetHashCode();

    public override bool Equals(object obj) => obj is cDrawObject stObj && Equals(stObj);

    public override string ToString() => $"{ObjectName}: {LocationGDI}";

    public bool Equals(cDrawObject other) => other != null && ObjectName == other.ObjectName;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                imageDX?.Dispose();
                graphics?.Dispose();
            }
            imageDX = null;
            imageGDI = null;
            graphics = null;
            disposedValue = true;
        }
    }

    public void UpdateDrawObject()
    {
        this.RequireConvert = true;
    }

    public void UpdateImageDX()
    {
        #region 更新绘制位置,每次都更新
        this.RectDX.Left = this.LocationGDI.X;
        this.RectDX.Top = this.LocationGDI.Y;
        this.RectDX.Right = this.LocationGDI.X + this.imageGDI.Width;
        this.RectDX.Bottom = this.LocationGDI.Y + this.imageGDI.Height;
        #endregion

        if (this.RequireConvert)
        {
            this.RequireConvert = false;
            ConvertGdiBitmap();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// GDI的Bitmap转换到DX的Bitmap
    /// </summary>
    /// <remarks>
	/// 相比于旧版本，此版本避免了一次转换所需的图像流复制，所以性能得到提升，
	/// 同时还有以下潜在的好处：
	/// <para>1.使用更高效的可复用非托管流 <see cref="DataStream"/>.</para>
	/// 2.手动更改像素颜色信息以提供SIMD友好的代码支持。
	/// </remarks>
    private void ConvertGdiBitmap()
    {
        imageDX?.Dispose();
        Bitmap bitmap = (Bitmap)imageGDI;
        int width = bitmap.Width;
        int height = bitmap.Height;
        Rectangle sourceArea = new(0, 0, width, height);
        Size2 size = new(width, height);
        int stride = width * sizeof(int);
        using DataStream tempStream = new(height * stride, true, true);
        BitmapData bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        ConvertGdiBitmapCore(tempStream, bitmapData);
        bitmap.UnlockBits(bitmapData);
        tempStream.Position = 0;
        imageDX = new SharpDX.Direct2D1.Bitmap(oRenderTarget, size, tempStream, stride, s_defaultBitmapProperties);
    }
    private void ConvertGdiBitmapCore(DataStream tempStream, BitmapData bitmapData)
    {

        int total = bitmapData.Width * bitmapData.Height;
        unsafe
        {
            uint* src = (uint*)bitmapData.Scan0.ToPointer();
            // 直接写入目标缓冲区，由于 SharpDX.Direct2D1.Bitmap 构造函数只关注起始指针。
            // 所以不需要关注 DataStream 的坐标等属性。
            // 这里使用 DataStream，只是基于性能考量。
            uint* dest = (uint*)tempStream.DataPointer;
            //经过基准测试，当图片像素数量大于512*512时，并行化可以取得优势。
            if (total >= 512 * 512)
            {
                ConvertGdiBitmapCoreParallel(total, src, dest);
            }
            else
            {
                ConvertGdiBitmapCoreSimd(total, src, dest);
            }
        }
    }
    private unsafe void ConvertGdiBitmapCoreSimd(int total, uint* src, uint* dest)
    {
        int remain = total;
        int i = 0;
        unsafe
        {
            if (IntPtr.Size == 8)
            {
                ulong* s = (ulong*)src;
                ulong* d = (ulong*)dest;
                while (remain >= 8)
                {
                    d[i] = (s[i] & 0xFF00FF00FF00FF00) | ((s[i] >> 16) & 0x000000FF000000FF) | ((s[i] & 0x000000FF000000FFF) << 16);
                    d[i + 1] = (s[i + 1] & 0xFF00FF00FF00FF00) | ((s[i + 1] >> 16) & 0x000000FF000000FF) | ((s[i + 1] & 0x000000FF000000FF) << 16);
                    d[i + 2] = (s[i + 2] & 0xFF00FF00FF00FF00) | ((s[i + 2] >> 16) & 0x000000FF000000FF) | ((s[i + 2] & 0x000000FF000000FF) << 16);
                    d[i + 3] = (s[i + 3] & 0xFF00FF00FF00FF00) | ((s[i + 3] >> 16) & 0x000000FF000000FF) | ((s[i + 3] & 0x000000FF000000FF) << 16);
                    remain -= 8;
                    i += 4;
                }
                while (remain >= 2)
                {
                    d[i] = (s[i] & 0xFF00FF00FF00FF00) | ((s[i] >> 16) & 0x000000FF000000FF) | ((s[i] & 0x000000FF000000FFF) << 16);
                    remain -= 2;
                    ++i;
                }
                i <<= 1;
            }
            else
            {
                while (remain >= 8)
                {
                    dest[i] = (src[i] & 0xFF00FF00) | ((src[i] >> 16) & 0xFF) | ((src[i] & 0xFF) << 16);
                    dest[i + 1] = (src[i + 1] & 0xFF00FF00) | ((src[i + 1] >> 16) & 0xFF) | ((src[i + 1] & 0xFF) << 16);
                    dest[i + 2] = (src[i + 2] & 0xFF00FF00) | ((src[i + 2] >> 16) & 0xFF) | ((src[i + 2] & 0xFF) << 16);
                    dest[i + 3] = (src[i + 3] & 0xFF00FF00) | ((src[i + 3] >> 16) & 0xFF) | ((src[i + 3] & 0xFF) << 16);
                    dest[i + 4] = (src[i + 4] & 0xFF00FF00) | ((src[i + 4] >> 16) & 0xFF) | ((src[i + 4] & 0xFF) << 16);
                    dest[i + 5] = (src[i + 5] & 0xFF00FF00) | ((src[i + 5] >> 16) & 0xFF) | ((src[i + 5] & 0xFF) << 16);
                    dest[i + 6] = (src[i + 6] & 0xFF00FF00) | ((src[i + 6] >> 16) & 0xFF) | ((src[i + 6] & 0xFF) << 16);
                    dest[i + 7] = (src[i + 7] & 0xFF00FF00) | ((src[i + 7] >> 16) & 0xFF) | ((src[i + 7] & 0xFF) << 16);
                    remain -= 8;
                    i += 8;
                }
            }
            while (i < total)
            {
                dest[i] = (src[i] & 0xFF00FF00) | ((src[i] >> 16) & 0xFF) | ((src[i] & 0xFF) << 16);
                ++i;
            }
        }
    }
    private unsafe void ConvertGdiBitmapCoreParallel(int total, uint* src, uint* dest)
    {
        int remain = total - total % 8;
        if (total >= 8)
        {
            int sub = total / 8;
            if (IntPtr.Size == 8)
            {
                ulong* s = (ulong*)src;
                ulong* d = (ulong*)dest;
                Parallel.For(0, sub, i =>
                {
                    int v = (i << 2);
                    d[v] = (s[v] & 0xFF00FF00FF00FF00) | ((s[v] >> 16) & 0x000000FF000000FF) | ((s[v] & 0x000000FF000000FF) << 16);
                    d[v + 1] = (s[v + 1] & 0xFF00FF00FF00FF00) | ((s[v + 1] >> 16) & 0x000000FF000000FF) | ((s[v + 1] & 0x000000FF000000FF) << 16);
                    d[v + 2] = (s[v + 2] & 0xFF00FF00FF00FF00) | ((s[v + 2] >> 16) & 0x000000FF000000FF) | ((s[v + 2] & 0x000000FF000000FF) << 16);
                    d[v + 3] = (s[v + 3] & 0xFF00FF00FF00FF00) | ((s[v + 3] >> 16) & 0x000000FF000000FF) | ((s[v + 3] & 0x000000FF000000FF) << 16);
                });
            }
            else
            {
                Parallel.For(0, sub, i =>
                {
                    int v = (i << 3);
                    dest[v] = (src[v] & 0xFF00FF00) | ((src[v] >> 16) & 0xFF) | ((src[v] & 0xFF) << 16);
                    dest[v + 1] = (src[v + 1] & 0xFF00FF00) | ((src[v + 1] >> 16) & 0xFF) | ((src[v + 1] & 0xFF) << 16);
                    dest[v + 2] = (src[v + 2] & 0xFF00FF00) | ((src[v + 2] >> 16) & 0xFF) | ((src[v + 2] & 0xFF) << 16);
                    dest[v + 3] = (src[v + 3] & 0xFF00FF00) | ((src[v + 3] >> 16) & 0xFF) | ((src[v + 3] & 0xFF) << 16);
                    dest[v + 4] = (src[v + 4] & 0xFF00FF00) | ((src[v + 4] >> 16) & 0xFF) | ((src[v + 4] & 0xFF) << 16);
                    dest[v + 5] = (src[v + 5] & 0xFF00FF00) | ((src[v + 5] >> 16) & 0xFF) | ((src[v + 5] & 0xFF) << 16);
                    dest[v + 6] = (src[v + 6] & 0xFF00FF00) | ((src[v + 6] >> 16) & 0xFF) | ((src[v + 6] & 0xFF) << 16);
                    dest[v + 7] = (src[v + 7] & 0xFF00FF00) | ((src[v + 7] >> 16) & 0xFF) | ((src[v + 7] & 0xFF) << 16);
                });
            }
        }
        Parallel.For(remain, total, i =>
        {
            dest[i] = (src[i] & 0xFF00FF00) | ((src[i] >> 16) & 0xFF) | ((src[i] & 0xFF) << 16);
        });
    }
}