using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Graphics mGraphBitMap;
        PointF mPoint = new PointF();
        private Bitmap mBitmap;
        StringFormat mTextFormat = new StringFormat();
        
        public Form1()
        {
            InitializeComponent();

            mTextFormat.Alignment = StringAlignment.Center;
            mTextFormat.LineAlignment = StringAlignment.Center;

            mBitmap = new Bitmap(pictureBox1.ClientRectangle.Width, pictureBox1.ClientRectangle.Height);
            mGraphBitMap = Graphics.FromImage(mBitmap);
            pictureBox1.Image = mBitmap;

            // 坐标系变换
            Matrix transformMatrix = new Matrix();
            transformMatrix.Translate(0, pictureBox1.Height); // 移动原点到左下角
            transformMatrix.Scale(1, -1); // 反转Y轴方向
            mGraphBitMap.Transform = transformMatrix;
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            PointF[] mGraphPoint = new PointF[1] {e.Location};

            mGraphBitMap.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, mGraphPoint);
            mPoint = mGraphPoint[0];

            mGraphBitMap.Clear(Color.Gray);

            DrawGraphics();
            DrawMouseLocation();

            pictureBox1.Refresh();
        }

        private void DrawGraphics()
        {
            // 在mGraph坐标系中绘制一些示例图形
            Pen pen = new Pen(Color.Yellow);
            mGraphBitMap.DrawLine(pen, 0, 0, 100, 100);
            mGraphBitMap.DrawRectangle(pen, 50, 50, 100, 100);
            mGraphBitMap.DrawEllipse(pen, 100, 100, 150, 100);
        }

        private void DrawMouseLocation()
        {
            var _pt = mPoint;
            _pt.X -= 30;
            _pt.Y -= 10;
            mGraphBitMap.DrawImage(BulidImage($"({mPoint.X}, {mPoint.Y})", this.Font, Brushes.White), _pt);
        }

        private Image BulidImage(string text, Font font, Brush brush)
        {
            Image image = new Bitmap(60,20);        //实例化一个Image对象
            var g = Graphics.FromImage(image);     //创建画布


            g.Clear(Color.Transparent);                    //设置画布背景颜色
            //设置画布坐标变化
            g.ScaleTransform(1f, -1f);
            g.TranslateTransform(0, -20);

            //设置画布上文字效果，去除锯齿影响
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //获取绘制字体大小
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, 0, 0);
            g.DrawLine(new Pen(Color.Blue), 0, 0, 60, 20);
            return image;
        }
    }
}
