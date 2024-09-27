using NUnit.Framework;
using SkiaSharp;
using System.Collections;

namespace DataMatrixTest
{
    internal static class DataMatrixTestClass
    {
        internal static IEnumerable TestGS1Cases
        {
            get
            {
                yield return new TestCaseData("gs1DataMatrix1.png", "10AC3454G3", SKEncodedImageFormat.Png);
                yield return new TestCaseData("gs1DataMatrix2.gif", "010761234567890017100503", SKEncodedImageFormat.Gif);
            }
        }
    }
}
