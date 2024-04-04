using SharpDX.Direct2D1;
using SharpDX.WIC;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

internal class cSharpDXImage
{
	ImagingFactory oImagingFactory = new ImagingFactory();
	RenderTarget oRender;

	SharpDX.WIC.BitmapDecoder oWICBitmap;
	BitmapFrameDecode oFrameBitmap;
	FormatConverter oFormatConverter;
	Stream oGdiBmpStream = new MemoryStream();

	Dictionary<int, SharpDX.Direct2D1.Bitmap> oAllFrame = new System.Collections.Generic.Dictionary<int, SharpDX.Direct2D1.Bitmap>();
	int mFrameCount;
	int mFrameIndex;

	int[] mFrameDelays;
	List<System.Drawing.Image> oAllFrameGDI = new List<System.Drawing.Image>();

	public cSharpDXImage(RenderTarget objRender)
	{
		oRender = objRender;
		oFormatConverter = new FormatConverter(oImagingFactory);
	}

	private SharpDX.Direct2D1.Bitmap ConvertGdiBitmap(System.Drawing.Bitmap objBitmapGDI)
	{
		oGdiBmpStream.Seek(0, SeekOrigin.Begin);
		objBitmapGDI.Save(oGdiBmpStream, ImageFormat.Png);
		oGdiBmpStream.SetLength(oGdiBmpStream.Length);

		SharpDX.WIC.BitmapDecoder Decoder = new BitmapDecoder(oImagingFactory, oGdiBmpStream, SharpDX.WIC.DecodeOptions.CacheOnLoad);
		BitmapFrameDecode oFrameBitmap = Decoder.GetFrame(0);
		FormatConverter Converter = new FormatConverter(oImagingFactory);
		Converter.Initialize(oFrameBitmap, SharpDX.WIC.PixelFormat.Format32bppPBGRA);
		return SharpDX.Direct2D1.Bitmap.FromWicBitmap(oRender, Converter);
	}

	public bool Load(string imagePath)
	{
		System.Drawing.Image _Image;
		FrameDimension _FrameDimension;

		mFrameCount = 1;
		try
		{
			_Image = System.Drawing.Image.FromFile(imagePath);
			_FrameDimension = new FrameDimension(_Image.FrameDimensionsList[0]);
			mFrameCount = _Image.GetFrameCount(_FrameDimension);
			mFrameDelays = new int[mFrameCount];

			#region 获取所有帧
			for (int i = 0; i < mFrameCount; i++)
			{
				_Image.SelectActiveFrame(_FrameDimension, i);
				oAllFrameGDI.Add((System.Drawing.Image)_Image.Clone());
			}

			#endregion

			#region 获取帧延迟
			int _i = 0;

			foreach (var _property in _Image.PropertyItems)
			{
				if (_property.Id == 0x5100)     //此魔法数代表帧延迟
				{
					for (int i = 0; i < _property.Value.Length; i += 4)
					{
						mFrameDelays[_i] = _property.Value[i + 3] * 0x1000000 + _property.Value[i + 3] * 0x10000 + _property.Value[i + 3] * 0x100 + _property.Value[i + 0];
						mFrameDelays[_i] *= 10;     //转化为ms
						_i++;
					}
					break;
				}
			}
			#endregion

			oWICBitmap = new BitmapDecoder(oImagingFactory, imagePath, SharpDX.IO.NativeFileAccess.Read, SharpDX.WIC.DecodeOptions.CacheOnLoad);

			oAllFrame.Clear();
			//mFrameCount = oWICBitmap.FrameCount;
			mFrameIndex = 0;
			Console.WriteLine(oWICBitmap.DecoderInfo);

			for (int i = 0; i < mFrameCount; i++)
			{
				oAllFrame.Add(i, _GetFrame(i));
			}
			return true;
		}
		catch { }

		return false;
	}
	public void MoveFirst()
	{
		mFrameIndex = 0;
	}

	public bool MoveNext()
	{
		mFrameIndex++;
		if (mFrameIndex >= mFrameCount)
		{
			return false;
		}
		return true;
	}

	private SharpDX.Direct2D1.Bitmap _GetFrame(int frameIndex)
	{
		oFrameBitmap = oWICBitmap.GetFrame(frameIndex);
		oFormatConverter = new FormatConverter(oImagingFactory);
		oFormatConverter.Initialize(oFrameBitmap, SharpDX.WIC.PixelFormat.Format32bppPBGRA);
		return SharpDX.Direct2D1.Bitmap.FromWicBitmap(oRender, oFormatConverter);
	}

	public SharpDX.Direct2D1.Bitmap GetFrame(int frameIndex)
	{
		if (frameIndex < 0 || frameIndex >= mFrameCount) { frameIndex = 0; }
		return oAllFrame[frameIndex];
	}

	public SharpDX.Direct2D1.Bitmap GetNextFrame()
	{
		mFrameIndex++;
		if (mFrameIndex < 0 || mFrameIndex >= mFrameCount) { mFrameIndex = 0; }
		return oAllFrame[mFrameIndex];
	}
	public System.Drawing.Image GetNextImage()
	{
		mFrameIndex++;
		if (mFrameIndex < 0 || mFrameIndex >= mFrameCount) { mFrameIndex = 0; }
		return oAllFrameGDI[mFrameIndex];
	}

	public Size Size
	{
		get 
		{
			BitmapFrameDecode _frame = oWICBitmap.GetFrame(0);

			return new Size(_frame.Size.Width, _frame.Size.Height);
		}
	}

	public int FrameDelay
	{
		get { return mFrameDelays[mFrameIndex]; }
	}

}