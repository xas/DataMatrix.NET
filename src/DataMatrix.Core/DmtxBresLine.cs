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

namespace DataMatrix.Core
{
    internal struct DmtxBresLine
    {
        #region Constructors
        internal DmtxBresLine(DmtxBresLine orig)
        {
            Error = orig.Error;
            Loc = new DmtxPixelLoc { X = orig.Loc.X, Y = orig.Loc.Y };
            Loc0 = new DmtxPixelLoc { X = orig.Loc0.X, Y = orig.Loc0.Y };
            Loc1 = new DmtxPixelLoc { X = orig.Loc1.X, Y = orig.Loc1.Y };
            Outward = orig.Outward;
            Steep = orig.Steep;
            Travel = orig.Travel;
            XDelta = orig.XDelta;
            XOut = orig.XOut;
            XStep = orig.XStep;
            YDelta = orig.YDelta;
            YOut = orig.YOut;
            YStep = orig.YStep;
        }

        internal DmtxBresLine(DmtxPixelLoc loc0, DmtxPixelLoc loc1, DmtxPixelLoc locInside)
        {
            int cp;
            DmtxPixelLoc locBeg, locEnd;


            /* Values that stay the same after initialization */
            Loc0 = loc0;
            Loc1 = loc1;
            XStep = (loc0.X < loc1.X) ? +1 : -1;
            YStep = (loc0.Y < loc1.Y) ? +1 : -1;
            XDelta = Math.Abs(loc1.X - loc0.X);
            YDelta = Math.Abs(loc1.Y - loc0.Y);
            Steep = (YDelta > XDelta);

            /* Take cross product to determine outward step */
            if (Steep)
            {
                /* Point first vector up to get correct sign */
                if (loc0.Y < loc1.Y)
                {
                    locBeg = loc0;
                    locEnd = loc1;
                }
                else
                {
                    locBeg = loc1;
                    locEnd = loc0;
                }
                cp = (((locEnd.X - locBeg.X) * (locInside.Y - locEnd.Y)) -
                      ((locEnd.Y - locBeg.Y) * (locInside.X - locEnd.X)));

                XOut = (cp > 0) ? +1 : -1;
                YOut = 0;
            }
            else
            {
                /* Point first vector left to get correct sign */
                if (loc0.X > loc1.X)
                {
                    locBeg = loc0;
                    locEnd = loc1;
                }
                else
                {
                    locBeg = loc1;
                    locEnd = loc0;
                }
                cp = (((locEnd.X - locBeg.X) * (locInside.Y - locEnd.Y)) -
                      ((locEnd.Y - locBeg.Y) * (locInside.X - locEnd.X)));

                XOut = 0;
                YOut = (cp > 0) ? +1 : -1;
            }

            /* Values that change while stepping through line */
            Loc = loc0;
            Travel = 0;
            Outward = 0;
            Error = (Steep) ? YDelta / 2 : XDelta / 2;
        }
        #endregion

        #region Methods
        internal bool GetStep(DmtxPixelLoc target, ref int travel, ref int outward)
        {
            /* Determine necessary step along and outward from Bresenham line */
            if (Steep)
            {
                travel = (YStep > 0) ? target.Y - Loc.Y : Loc.Y - target.Y;
                Step(travel, 0);
                outward = (XOut > 0) ? target.X - Loc.X : Loc.X - target.X;
                if (YOut != 0)
                {
                    throw new InvalidOperationException("Invald yOut value for bresline step!");
                }
            }
            else
            {
                travel = (XStep > 0) ? target.X - Loc.X : Loc.X - target.X;
                Step(travel, 0);
                outward = (YOut > 0) ? target.Y - Loc.Y : Loc.Y - target.Y;
                if (XOut != 0)
                {
                    throw new InvalidOperationException("Invald xOut value for bresline step!");
                }
            }

            return true;
        }


        internal bool Step(int travel, int outward)
        {
            int i;

            if (Math.Abs(travel) >= 2)
            {
                throw new ArgumentException("Invalid value for 'travel' in BaseLineStep!");
            }

            /* Perform forward step */
            if (travel > 0)
            {
                Travel++;
                if (Steep)
                {
                    Loc = new DmtxPixelLoc() { X = Loc.X, Y = Loc.Y + YStep };
                    Error -= XDelta;
                    if (Error < 0)
                    {
                        Loc = new DmtxPixelLoc() { X = Loc.X + XStep, Y = Loc.Y };
                        Error += YDelta;
                    }
                }
                else
                {
                    Loc = new DmtxPixelLoc() { X = Loc.X + XStep, Y = Loc.Y };
                    Error -= YDelta;
                    if (Error < 0)
                    {
                        Loc = new DmtxPixelLoc() { X = Loc.X, Y = Loc.Y + YStep };
                        Error += XDelta;
                    }
                }
            }
            else if (travel < 0)
            {
                Travel--;
                if (Steep)
                {
                    Loc = new DmtxPixelLoc() { X = Loc.X, Y = Loc.Y - YStep };
                    Error += XDelta;
                    if (Error >= YDelta)
                    {
                        Loc = new DmtxPixelLoc() { X = Loc.X - XStep, Y = Loc.Y };
                        Error -= YDelta;
                    }
                }
                else
                {
                    Loc = new DmtxPixelLoc() { X = Loc.X - XStep, Y = Loc.Y };
                    Error += YDelta;
                    if (Error >= XDelta)
                    {
                        Loc = new DmtxPixelLoc() { X = Loc.X, Y = Loc.Y - YStep };
                        Error -= XDelta;
                    }
                }
            }

            for (i = 0; i < outward; i++)
            {
                /* Outward steps */
                Outward++;
                Loc = new DmtxPixelLoc() { X = Loc.X + XOut, Y = Loc.Y + YOut };
            }

            return true;
        }
        #endregion

        #region Properties
        internal int XStep { get; private set; }

        internal int YStep { get; private set; }

        internal int XDelta { get; private set; }

        internal int YDelta { get; private set; }

        internal bool Steep { get; private set; }

        internal int XOut { get; private set; }

        internal int YOut { get; private set; }

        internal int Travel { get; private set; }

        internal int Outward { get; private set; }

        internal int Error { get; private set; }

        internal DmtxPixelLoc Loc { get; private set; }


        internal DmtxPixelLoc Loc0 { get; private set; }

        internal DmtxPixelLoc Loc1 { get; private set; }
        #endregion
    }
}
