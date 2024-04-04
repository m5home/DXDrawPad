using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GraphicsText
{
    private Graphics _graphics;

    public GraphicsText()
    {

    }

    public Graphics Graphics
    {
        get { return _graphics; }
        set { _graphics = value; }
    }

    /// <summary>  
    /// 绘制根据矩形旋转文本  
    /// </summary>  
    /// <param name="s">文本</param>  
    /// <param name="font">字体</param>  
    /// <param name="brush">填充</param>  
    /// <param name="layoutRectangle">局部矩形</param>  
    /// <param name="format">布局方式</param>  
    /// <param name="angle">角度</param>  
    public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format, float angle)
    {
        // 求取字符串大小  
        SizeF size = _graphics.MeasureString(s, font);

        // 根据旋转角度，求取旋转后字符串大小  
        SizeF sizeRotate = ConvertSize(size, angle);

        // 根据旋转后尺寸、布局矩形、布局方式计算文本旋转点  
        PointF rotatePt = GetRotatePoint(sizeRotate, layoutRectangle, format);

        // 重设布局方式都为Center  
        StringFormat newFormat = new StringFormat(format);
        newFormat.Alignment = StringAlignment.Center;
        newFormat.LineAlignment = StringAlignment.Center;

        // 绘制旋转后文本  
        DrawString(s, font, brush, rotatePt, newFormat, angle);
    }

    /// <summary>  
    /// 绘制根据点旋转文本，一般旋转点给定位文本包围盒中心点  
    /// </summary>  
    /// <param name="s">文本</param>  
    /// <param name="font">字体</param>  
    /// <param name="brush">填充</param>  
    /// <param name="point">旋转点</param>  
    /// <param name="format">布局方式</param>  
    /// <param name="angle">角度</param>  
    public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format, float angle)
    {
        // Save the matrix  
        Matrix mtxSave = _graphics.Transform;

        Matrix mtxRotate = _graphics.Transform;
        mtxRotate.RotateAt(angle, point);

        _graphics.Transform = mtxRotate;

        _graphics.DrawString(s, font, brush, point, format);

        // Reset the matrix  
        _graphics.Transform = mtxSave;
    }

    private SizeF ConvertSize(SizeF size, float angle)
    {
        Matrix matrix = new Matrix();
        matrix.Rotate(angle);

        // 旋转矩形四个顶点  
        PointF[] pts = new PointF[4];
        pts[0].X = -size.Width / 2f;
        pts[0].Y = -size.Height / 2f;
        pts[1].X = -size.Width / 2f;
        pts[1].Y = size.Height / 2f;
        pts[2].X = size.Width / 2f;
        pts[2].Y = size.Height / 2f;
        pts[3].X = size.Width / 2f;
        pts[3].Y = -size.Height / 2f;
        matrix.TransformPoints(pts);

        // 求取四个顶点的包围盒  
        float left = float.MaxValue;
        float right = float.MinValue;
        float top = float.MaxValue;
        float bottom = float.MinValue;

        foreach (PointF pt in pts)
        {
            // 求取并集  
            if (pt.X < left)
                left = pt.X;
            if (pt.X > right)
                right = pt.X;
            if (pt.Y < top)
                top = pt.Y;
            if (pt.Y > bottom)
                bottom = pt.Y;
        }

        SizeF result = new SizeF(right - left, bottom - top);
        return result;
    }

    private PointF GetRotatePoint(SizeF size, RectangleF layoutRectangle, StringFormat format)
    {
        PointF pt = new PointF();

        switch (format.Alignment)
        {
            case StringAlignment.Near:
                pt.X = layoutRectangle.Left + size.Width / 2f;
                break;
            case StringAlignment.Center:
                pt.X = (layoutRectangle.Left + layoutRectangle.Right) / 2f;
                break;
            case StringAlignment.Far:
                pt.X = layoutRectangle.Right - size.Width / 2f;
                break;
            default:
                break;
        }

        switch (format.LineAlignment)
        {
            case StringAlignment.Near:
                pt.Y = layoutRectangle.Top + size.Height / 2f;
                break;
            case StringAlignment.Center:
                pt.Y = (layoutRectangle.Top + layoutRectangle.Bottom) / 2f;
                break;
            case StringAlignment.Far:
                pt.Y = layoutRectangle.Bottom - size.Height / 2f;
                break;
            default:
                break;
        }

        return pt;
    }
}