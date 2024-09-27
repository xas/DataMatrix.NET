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

using DataMatrix.net;
using NUnit.Framework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DataMatrixTest
{
    [TestFixture]
    class BarCodeTests
    {
        private static string testVal = "Hello World!";

        [Test]
        public void TestMatrixEnDecoder()
        {
            string fileName = "encodedImg.png";
            DmtxImageEncoder encoder = new();
            DmtxImageEncoderOptions options = new()
            {
                ModuleSize = 8,
                MarginSize = 4,
                BackColor = Color.White,
                ForeColor = Color.Green
            };
            SKBitmap encodedBitmap = encoder.EncodeImage(testVal, options);
            using (FileStream fs = new(fileName, FileMode.Create))
            {
                encodedBitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
            }

            string s = encoder.EncodeSvgImage("DataMatrix.net rocks!!one!eleven!!111!eins!!!!", 7, 7, Color.FromArgb(100, 255, 0, 0), Color.Turquoise);
            TextWriter tw = new StreamWriter("encodedImg.svg");
            tw.Write(s);
            tw.Flush();
            tw.Close();

            TestRawEncoder("HELLO WORLD");
            using (FileStream fs = new("helloWorld.png", FileMode.Create))
            {
                new DmtxImageEncoder().EncodeImage("HELLO WORLD").Encode(fs, SKEncodedImageFormat.Png, 100);
            }

            DmtxImageDecoder decoder = new();
            List<string> codes = decoder.DecodeImage(SKBitmap.Decode(fileName), 1, new TimeSpan(0, 0, 3));
            foreach (string code in codes)
            {
                Console.WriteLine("Decoded:\n" + code);
            }

            for (int i = 1; i < 10; i++)
            {
                var encodedData = Guid.NewGuid().ToString();
                SKBitmap source = encoder.EncodeImage(encodedData);
                var decodedData = decoder.DecodeImage(source);
                if (decodedData.Count != 1 || decodedData[0] != encodedData)
                    throw new InvalidOperationException("Encoding or decoding failed!");
            }
        }

        [Test]
        public void TestMosaicEnDecoder()
        {
            string fileName = "encodedMosaicImg.png";
            DmtxImageEncoder encoder = new();
            DmtxImageEncoderOptions options = new()
            {
                ModuleSize = 8,
                MarginSize = 4
            };
            SKBitmap encodedBitmap = encoder.EncodeImageMosaic(testVal, options);
            using (FileStream fs = new(fileName, FileMode.Create))
            {
                encodedBitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
            }

            DmtxImageDecoder decoder = new();
            List<string> codes = decoder.DecodeImageMosaic(SKBitmap.Decode(fileName), 1, new TimeSpan(0, 0, 3));
            Assert.That(codes.Count, Is.GreaterThan(0));
            foreach (string code in codes)
            {
                Console.WriteLine("Decoded:\n" + code);
            }
        }

        [Test]
        [TestCaseSource(typeof(DataMatrixTestClass), nameof(DataMatrixTestClass.TestGS1Cases))]
        public void TestGS1EnDecoder(string fileName, string gs1Code, SKEncodedImageFormat encodedFormat)
        {
            DmtxImageEncoder encoder = new();
            DmtxImageEncoderOptions options = new()
            {
                ModuleSize = 8,
                MarginSize = 30,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Scheme = DmtxScheme.DmtxSchemeAsciiGS1
            };
            SKBitmap encodedBitmap = encoder.EncodeImage(gs1Code, options);
            using (FileStream fs = new(fileName, FileMode.Create))
            {
                encodedBitmap.Encode(fs, encodedFormat, 100);
            }
            DmtxImageDecoder decoder = new();
            List<string> decodedCodes = decoder.DecodeImage(encodedBitmap, 1, new TimeSpan(0, 0, 5));
            Assert.That(decodedCodes, Is.Not.Null);
            Assert.That(decodedCodes.Count, Is.GreaterThan(0));
            Assert.That(decodedCodes[0], Is.EqualTo(gs1Code));
            Console.WriteLine("Encoded code 1: {0}, decoded code 1: {1}, codes are equal: {2}", gs1Code, decodedCodes[0], gs1Code.Equals(decodedCodes[0]));
        }

        private void TestRawEncoder(string text)
        {
            DmtxImageEncoder encoder = new();
            bool[,] rawData = encoder.EncodeRawData(text);
            Console.WriteLine("================");
            Console.WriteLine();
            for (int rowIdx = 0; rowIdx < rawData.GetLength(1); rowIdx++)
            {
                for (int colIdx = 0; colIdx < rawData.GetLength(0); colIdx++)
                {
                    Console.Write(rawData[colIdx, rowIdx] ? "X" : " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("================");
        }

    }
}
