/*
DataMatrix.Net

DataMatrix.Net - .net library for decoding DataMatrix codes.
Copyright (C) 2009/2010 Michael Faschinger

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.
You can also redistribute and/or modify it under the terms of the
GNU Lesser General Public License as published by the Free Software
Foundation; either version 3.0 of the License or (at your option)
any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
General Public License or the GNU Lesser General Public License 
for more details.

You should have received a copy of the GNU General Public
License and the GNU Lesser General Public License along with this 
library; if not, write to the Free Software Foundation, Inc., 
51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

Contact: Michael Faschinger - michfasch@gmx.at
 
*/

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DataMatrix.net
{
    public class DmtxImageDecoder
    {
        /// <summary>
        /// returns a list of DataMatrix codes in the image provided that can be
        /// found in the given time span, but no more than maxResultCount codes
        /// (useful, if you e.g. expect only one code to be in the image)
        /// </summary>
        public List<string> DecodeImageMosaic(SKBitmap image, int maxResultCount = int.MaxValue, TimeSpan timeOut = default)
        {
            return DecodeImage(image, maxResultCount, timeOut, true);
        }

        /// <summary>
        /// returns a list of DataMatrix codes in the image provided that can be
        /// found in the given time span, but no more than maxResultCount codes
        /// (useful, if you e.g. expect only one code to be in the image)
        /// </summary>
        public List<string> DecodeImage(SKBitmap image, int maxResultCount = int.MaxValue, TimeSpan timeOut = default, bool isMosaic = false)
        {
            if (timeOut == default)
            {
                timeOut = TimeSpan.MaxValue;
            }
            List<string> result = [];
            byte[] rawImg = image.Bytes;
            var pad = image.RowBytes - image.Width * image.BytesPerPixel;
            DmtxImage dmtxImg = new(rawImg, image.Width, image.Height, DmtxPackOrder.DmtxPack32bppRGBX)
            {
                RowPadBytes = pad
            };
            DmtxDecode decode = new(dmtxImg, 1);
            Stopwatch stopWatch = new();
            stopWatch.Start();
            while (true)
            {
                if (stopWatch.Elapsed > timeOut)
                {
                    break;
                }
                DmtxRegion region = decode.RegionFindNext(timeOut);
                if (region != null)
                {
                    DmtxMessage msg = isMosaic ? decode.MosaicRegion(region, -1) : decode.MatrixRegion(region, -1);
                    string message = Encoding.ASCII.GetString(msg.Output, 0, msg.Output.Length);
                    message = message.Substring(0, message.IndexOf('\0'));
                    if (!result.Contains(message))
                    {
                        result.Add(message);
                        if (result.Count >= maxResultCount)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return result;
        }
    }
}
