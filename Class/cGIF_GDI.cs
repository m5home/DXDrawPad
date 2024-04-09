using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class cGIF_GDI
{
    Image mImage;
    FrameDimension mFrameDimension;
    int mFrameCount;
    int mFrameIndex;
    int[] mFrameDelays;

    public cGIF_GDI()
    {
    }

    public bool Load(string filename)
    {
        try
        {
            mImage = Image.FromFile(filename);
            mFrameDimension = new FrameDimension(mImage.FrameDimensionsList[0]);
            mFrameCount = mImage.GetFrameCount(mFrameDimension);
            mFrameDelays = new int[mFrameCount];

            #region 获取帧延迟
            int _i = 0;

            foreach (var _property in mImage.PropertyItems)
            {
                if (_property.Id == 0x5100)     //此魔法数代表帧延迟
                {
                    for (int i = 0; i < _property.Value.Length; i+=4)
                    {
                        mFrameDelays[_i] = _property.Value[i + 3] * 0x1000000 + _property.Value[i + 3] * 0x10000 + _property.Value[i + 3] * 0x100 + _property.Value[i + 0];
                        mFrameDelays[_i] *= 10;     //转化为ms
                        _i++;
                    }
                    break;
                }
            }
            #endregion

            MoveFirst();
            return true;
        }
        catch
        {
        }
        return false; 
    }

    public void MoveFirst()
    {
        mFrameIndex = 0;
        mImage.SelectActiveFrame(mFrameDimension, 0);
    }

    public bool MoveNext()
    {
        mFrameIndex++;
        if (mFrameIndex >= mFrameCount)
        {
            return false;
        }
        mImage.SelectActiveFrame(mFrameDimension, mFrameIndex);
        return true;
    }

    public bool GetFrame(int frameIndex, out Image outImage)
    {
        mImage.SelectActiveFrame(mFrameDimension, frameIndex);
        outImage = mImage;
        mImage.SelectActiveFrame(mFrameDimension, mFrameIndex);
        return true;
    }

    public Image GetNextImage()
    {
        if (!MoveNext())
        {
            MoveFirst();
        }
        return mImage;
    }

    public Size Size
    {
        get { return mImage.Size; }
    }

    public int FrameDelay
    {
        get { return mFrameDelays[mFrameIndex]; }
    }
}