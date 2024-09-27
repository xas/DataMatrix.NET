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

using System;

namespace DataMatrix.net
{
    internal struct DmtxScanGrid
    {
        #region Constructors
        internal DmtxScanGrid(DmtxDecode dec)
        {
            int smallestFeature = dec.ScanGap;
            XMin = dec.XMin;
            XMax = dec.XMax;
            YMin = dec.YMin;
            YMax = dec.YMax;

            /* Values that get set once */
            int xExtent = XMax - XMin;
            int yExtent = YMax - YMin;
            int maxExtent = (xExtent > yExtent) ? xExtent : yExtent;

            if (maxExtent < 1)
            {
                throw new ArgumentException("Invalid max extent for Scan Grid: Must be greater than 0");
            }

            int extent = 1;
            MinExtent = extent;
            for (; extent < maxExtent; extent = ((extent + 1) * 2) - 1)
            {
                if (extent <= smallestFeature)
                {
                    MinExtent = extent;
                }
            }

            MaxExtent = extent ;

            XOffset = (XMin + XMax - MaxExtent) / 2;
            YOffset = (YMin + YMax - MaxExtent) / 2;

            /* Values that get reset for every level */
            Total = 1;
            Extent = MaxExtent;

            JumpSize = Extent + 1;
            PixelTotal = 2 * Extent - 1;
            StartPos = Extent / 2;
            PixelCount = 0;
            XCenter = YCenter = StartPos;

            SetDerivedFields();
        }
        #endregion


        #region Methods
        internal DmtxRange PopGridLocation(ref DmtxPixelLoc loc)
        {
            DmtxRange locStatus;

            do
            {
                locStatus = GetGridCoordinates(ref loc);

                /* Always leave grid pointing at next available location */
                PixelCount++;

            } while (locStatus == DmtxRange.DmtxRangeBad);

            return locStatus;
        }

        private DmtxRange GetGridCoordinates(ref DmtxPixelLoc locRef)
        {

            /* Initially pixelCount may fall beyond acceptable limits. Update grid
             * state before testing coordinates */

            /* Jump to next cross pattern horizontally if current column is done */
            if (PixelCount >= PixelTotal)
            {
                PixelCount = 0;
                XCenter += JumpSize;
            }

            /* Jump to next cross pattern vertically if current row is done */
            if (XCenter > MaxExtent)
            {
                XCenter = StartPos;
                YCenter += JumpSize;
            }

            /* Increment level when vertical step goes too far */
            if (YCenter > MaxExtent)
            {
                Total *= 4;
                Extent /= 2;
                SetDerivedFields();
            }

            if (Extent == 0 || Extent < MinExtent)
            {
                locRef.X = locRef.Y = -1;
                return DmtxRange.DmtxRangeEnd;
            }

            int count = PixelCount;

            if (count >= PixelTotal)
            {
                throw new InvalidOperationException("Scangrid is beyong image limits!");
            }

            DmtxPixelLoc loc = new DmtxPixelLoc();
            if (count == PixelTotal - 1)
            {
                /* center pixel */
                loc.X = XCenter;
                loc.Y = YCenter;
            }
            else
            {
                int half = PixelTotal / 2;
                int quarter = half / 2;

                /* horizontal portion */
                if (count < half)
                {
                    loc.X = XCenter + ((count < quarter) ? (count - quarter) : (half - count));
                    loc.Y = YCenter;
                }
                /* vertical portion */
                else
                {
                    count -= half;
                    loc.X = XCenter;
                    loc.Y = YCenter + ((count < quarter) ? (count - quarter) : (half - count));
                }
            }

            loc.X += XOffset;
            loc.Y += YOffset;

            locRef.X = loc.X;
            locRef.Y = loc.Y;

            if (loc.X < XMin || loc.X > XMax ||
                  loc.Y < YMin || loc.Y > YMax)
            {
                return DmtxRange.DmtxRangeBad;
            }

            return DmtxRange.DmtxRangeGood;
        }


        /// <summary>
        /// Update derived fields based on current state
        /// </summary>
        private void SetDerivedFields()
        {
            JumpSize = Extent + 1;
            PixelTotal = 2 * Extent - 1;
            StartPos = Extent / 2;
            PixelCount = 0;
            XCenter = YCenter = StartPos;
        }
        #endregion

        #region Properties
        /// <summary>
        ///  Smallest cross size used in scan
        /// </summary>
        internal int MinExtent { get; private set; }

        /// <summary>
        /// Size of bounding grid region (2^N - 1)
        /// </summary>
        internal int MaxExtent { get; private set; }

        /// <summary>
        /// Offset to obtain image X coordinate
        /// </summary>
        internal int XOffset { get; private set; }

        /// <summary>
        /// Offset to obtain image Y coordinate
        /// </summary>
        internal int YOffset { get; private set; }

        /// <summary>
        ///  Minimum X in image coordinate system
        /// </summary>
        internal int XMin { get; private set; }


        /// <summary>
        /// Maximum X in image coordinate system
        /// </summary>
        internal int XMax { get; private set; }

        /// <summary>
        ///  Minimum Y in image coordinate system
        /// </summary>
        internal int YMin { get; private set; }

        /// <summary>
        /// Maximum Y in image coordinate system
        /// </summary>
        internal int YMax { get; private set; }

        /// <summary>
        ///  Total number of crosses at this size
        /// </summary>
        internal int Total { get; private set; }

        /// <summary>
        ///  Length/width of cross in pixels
        /// </summary>
        internal int Extent { get; private set; }

        /// <summary>
        /// Distance in pixels between cross centers
        /// </summary>
        internal int JumpSize { get; private set; }

        /// <summary>
        ///  Total pixel count within an individual cross path
        /// </summary>
        internal int PixelTotal { get; private set; }

        /// <summary>
        /// X and Y coordinate of first cross center in pattern
        /// </summary>
        internal int StartPos { get; private set; }

        /// <summary>
        /// Progress (pixel count) within current cross pattern
        /// </summary>
        internal int PixelCount { get; private set; }

        /// <summary>
        /// X center of current cross pattern
        /// </summary>
        internal int XCenter { get; private set; }


        /// <summary>
        /// Y center of current cross pattern
        /// </summary>
        internal int YCenter { get; private set; }
        #endregion
    }
}
