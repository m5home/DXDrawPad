using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjGDIDrawPad
{
    public partial class Form1 : Form
    {
        ST_DRAW_OBJECT mCirItem = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mPosMoveItem = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mPosItem = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mK_MoveItem = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mK_Item = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mTank_Item = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mLaser_Item = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mFFF_Item = new ST_DRAW_OBJECT();
        ST_DRAW_OBJECT mBackImage = new ST_DRAW_OBJECT();

        Font mFontNumber = new Font("Consolas", 9);

        cSharpDXImage oGif_K_Move;
        cSharpDXImage oGif_K;
        cSharpDXImage oGif_Tank;
        cSharpDXImage oGif_Laser;
        cSharpDXImage oGif_FFF;
        cSharpDXImage oImageBack;

        public Form1()
        {
            InitializeComponent();

            oGif_K_Move = new cSharpDXImage(dxDrawPad1.RenderTarget);
	        oGif_K = new cSharpDXImage(dxDrawPad1.RenderTarget);
	        oGif_Tank = new cSharpDXImage(dxDrawPad1.RenderTarget);
	        oGif_Laser = new cSharpDXImage(dxDrawPad1.RenderTarget);
	        oGif_FFF = new cSharpDXImage(dxDrawPad1.RenderTarget);
            oImageBack = new cSharpDXImage(dxDrawPad1.RenderTarget);

			oGif_K_Move.Load(Application.StartupPath + @"\k-upper.gif");
            oGif_K.Load(Application.StartupPath + @"\K站立.gif");
            oGif_Tank.Load(Application.StartupPath + @"\tank.gif");
            oGif_Laser.Load(Application.StartupPath + @"\laser.gif");
            oGif_FFF.Load(Application.StartupPath + @"\fff.gif");
			oImageBack.Load(Application.StartupPath + @"\back.png");

            dxDrawPad1.IsShowFPS = true;

            #region 添加子画布, 后添加的最后绘制, 因此是在最上层.
            dxDrawPad1.AddDrawObject("backimg", oImageBack.Size.Width, oImageBack.Size.Height, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("backimg", out mBackImage);

			mBackImage.graphics.Clear(mBackImage.colorBack);
			mBackImage.graphics.DrawImage(oImageBack.GetNextImage(), 0, 0);
			dxDrawPad1.UpdateDrawObject(mBackImage);

			dxDrawPad1.AddDrawObject("Cir", 200, 200, 100, 100, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("Cir", out mCirItem);

            dxDrawPad1.AddDrawObject("PosMove", 200, 20, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("PosMove", out mPosMoveItem);

            dxDrawPad1.AddDrawObject("Pos", 200, 20, 0, dxDrawPad1.Height - 20, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("Pos", out mPosItem);

            dxDrawPad1.AddDrawObject("K_Move", oGif_K_Move.Size.Width, oGif_K_Move.Size.Height, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("K_Move", out mK_MoveItem);

            dxDrawPad1.AddDrawObject("K", oGif_K.Size.Width, oGif_K.Size.Height, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("K", out mK_Item);

            dxDrawPad1.AddDrawObject("Tank", oGif_Tank.Size.Width, oGif_Tank.Size.Height, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("Tank", out mTank_Item);

            dxDrawPad1.AddDrawObject("Laser", oGif_Laser.Size.Width, oGif_Laser.Size.Height, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("Laser", out mLaser_Item);

            dxDrawPad1.AddDrawObject("FFF", oGif_FFF.Size.Width, oGif_FFF.Size.Height, 0, 0, Color.Green, Color.Transparent, Color.Red);
            dxDrawPad1.GetDrawObject("FFF", out mFFF_Item);
            #endregion

            dxDrawPad1.IsDrawObject = true;

            timer1.Start();
            timer2.Start();
            timer3.Start();
            timer4.Start();
            timer5.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mCirItem.graphics.Clear(mCirItem.colorBack);
            mCirItem.graphics.FillEllipse(Brushes.Yellow, 1, 1, 198, 198);
            mCirItem.graphics.DrawLine(Pens.Red, 0, 0, 150, 150);
            mCirItem.graphics.DrawString("第一个子画布,在最下层", mFontNumber, Brushes.Black, 0, 100);
            dxDrawPad1.UpdateDrawObject(mCirItem);
		}

		private void dxDrawPad1_Load(object sender, EventArgs e)
		{
		}

        private void dxDrawPad1_MouseMove(object sender, MouseEventArgs e)
        {
            #region 随鼠标动的
            mPosMoveItem.graphics.Clear(mPosMoveItem.colorBack);
            mPosMoveItem.graphics.DrawString("随鼠标动的 - " + e.Location.ToString(), mFontNumber, Brushes.Blue, 0, 0);
            mPosMoveItem.LocationGDI.X = e.Location.X;             //修改画布的位置
            mPosMoveItem.LocationGDI.Y = e.Location.Y + 20;

            dxDrawPad1.UpdateDrawObject(mPosMoveItem);
            #endregion

            #region 防空的K!
            mK_MoveItem.LocationGDI.X = e.X;
            mK_MoveItem.LocationGDI.Y = e.Y;

            dxDrawPad1.UpdateDrawObject(mK_MoveItem);
            #endregion

            #region 激光!!
            mLaser_Item.LocationGDI.X = e.X - mLaser_Item.imageGDI.Width;
            mLaser_Item.LocationGDI.Y = e.Y - mLaser_Item.imageGDI.Height;

			dxDrawPad1.UpdateDrawObject(mLaser_Item);
            #endregion

            #region 飞机丢炸弹!!
            mFFF_Item.LocationGDI.X = e.X;
            mFFF_Item.LocationGDI.Y = e.Y + mFFF_Item.imageGDI.Height;

			dxDrawPad1.UpdateDrawObject(mFFF_Item);
            #endregion

            #region 坦克!!
            mTank_Item.LocationGDI.X = e.X - mTank_Item.imageGDI.Width;
            mTank_Item.LocationGDI.Y = e.Y;

			dxDrawPad1.UpdateDrawObject(mTank_Item);
            #endregion

            #region 仅绘图,不更新画布位置,则不需要更新回去
            mPosItem.graphics.Clear(mPosItem.colorBack);
            mPosItem.graphics.DrawString("固定的 - "+ e.Location.ToString(), mFontNumber, Brushes.Gold, 0, 0);

            dxDrawPad1.UpdateDrawObject(mPosItem);
            #endregion
        }

		#region gif动画的处理
		private void timer1_Tick(object sender, EventArgs e)
        {
            mK_MoveItem.graphics.Clear(mK_MoveItem.colorBack);
            mK_MoveItem.graphics.DrawImage(oGif_K_Move.GetNextImage(), 0, 0);
			dxDrawPad1.UpdateDrawObject(mK_MoveItem);

			timer1.Interval = oGif_K_Move.FrameDelay / 2;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            mK_Item.graphics.Clear(mK_Item.colorBack);
            mK_Item.graphics.DrawImage(oGif_K.GetNextImage(), 0, 0);
			dxDrawPad1.UpdateDrawObject(mK_Item);

			timer2.Interval = oGif_K.FrameDelay / 2;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            mTank_Item.graphics.Clear(mTank_Item.colorBack);
            mTank_Item.graphics.DrawImage(oGif_Tank.GetNextImage(), 0, 0);
			dxDrawPad1.UpdateDrawObject(mTank_Item);

			timer3.Interval = oGif_Tank.FrameDelay / 2;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            mLaser_Item.graphics.Clear(mLaser_Item.colorBack);
            mLaser_Item.graphics.DrawImage(oGif_Laser.GetNextImage(), 0, 0);
			dxDrawPad1.UpdateDrawObject(mLaser_Item);

			timer4.Interval = oGif_Laser.FrameDelay / 2;
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            mFFF_Item.graphics.Clear(mFFF_Item.colorBack);
            mFFF_Item.graphics.DrawImage(oGif_FFF.GetNextImage(), 0, 0);
			dxDrawPad1.UpdateDrawObject(mFFF_Item);

			timer5.Interval = oGif_FFF.FrameDelay / 2;
        }
		#endregion
	}
}
