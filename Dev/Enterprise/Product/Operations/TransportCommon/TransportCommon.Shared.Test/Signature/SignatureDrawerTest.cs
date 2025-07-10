using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.TransportCommon.Shared.Testing
{
	public class SignatureDrawerTest : TestCaseWithFactory
	{
		#region TestImage

		public void TestBlankImage()
		{
			var signatureDrawer = new SignatureDrawer(ZBlob.Empty);
			AssertNull(signatureDrawer.Image);
		}

		public void TestImageWithSinglePointDoublePointAndTriplePointLines()
		{
			var signature = GetSignature();
			var signatureDrawer = new SignatureDrawer(signature);

			var image = signatureDrawer.Image;
			AssertNotNull(image);
			AssertEquals(200, image.Width);
			AssertEquals(100, image.Height);

			var map = new Bitmap(image);
			int width = image.Width;
			int height = image.Height;
			Color pixel;
			var dots = new List<Tuple<int, int>>();
			for (var row = 0; row < height; row++)
			{
				for (var col = 0; col < width; col++)
				{
					pixel = map.GetPixel(col, row);
					if (pixel.ToArgb() == Color.Black.ToArgb())
					{
						dots.Add(new Tuple<int, int>(row, col));
					}
				}
			}

			var expected = new Tuple<int, int>[] { new Tuple<int, int>(21, 20), new Tuple<int, int>(21, 50), new Tuple<int, int>(22, 20), new Tuple<int, int>(22, 50), new Tuple<int, int>(23, 20), new Tuple<int, int>(23, 50), new Tuple<int, int>(24, 20), new Tuple<int, int>(24, 50), new Tuple<int, int>(25, 50), new Tuple<int, int>(26, 50), new Tuple<int, int>(27, 50),
				new Tuple<int, int>(28, 50), new Tuple<int, int>(29, 50), new Tuple<int, int>(30, 50), new Tuple<int, int>(31, 50), new Tuple<int, int>(32, 50), new Tuple<int, int>(33, 50), new Tuple<int, int>(34, 50), new Tuple<int, int>(35, 50), new Tuple<int, int>(36, 50), new Tuple<int, int>(37, 50), new Tuple<int, int>(38, 50), new Tuple<int, int>(39, 50),
				new Tuple<int, int>(40, 50), new Tuple<int, int>(41, 50), new Tuple<int, int>(42, 50), new Tuple<int, int>(43, 50), new Tuple<int, int>(44, 50), new Tuple<int, int>(45, 50), new Tuple<int, int>(46, 19), new Tuple<int, int>(46, 50), new Tuple<int, int>(47, 19), new Tuple<int, int>(47, 50), new Tuple<int, int>(48, 50), new Tuple<int, int>(49, 50),
				new Tuple<int, int>(50, 50), new Tuple<int, int>(51, 50), new Tuple<int, int>(52, 50), new Tuple<int, int>(53, 50), new Tuple<int, int>(54, 50), new Tuple<int, int>(55, 50), new Tuple<int, int>(56, 50), new Tuple<int, int>(57, 50), new Tuple<int, int>(58, 50), new Tuple<int, int>(59, 50), new Tuple<int, int>(60, 50), new Tuple<int, int>(61, 50),
				new Tuple<int, int>(62, 50), new Tuple<int, int>(63, 50), new Tuple<int, int>(64, 50), new Tuple<int, int>(65, 50), new Tuple<int, int>(66, 50), new Tuple<int, int>(67, 50), new Tuple<int, int>(68, 50), new Tuple<int, int>(69, 50), new Tuple<int, int>(70, 19), new Tuple<int, int>(70, 50), new Tuple<int, int>(71, 19), new Tuple<int, int>(71, 50),
				new Tuple<int, int>(72, 50), new Tuple<int, int>(73, 50), new Tuple<int, int>(74, 50), new Tuple<int, int>(75, 50), new Tuple<int, int>(76, 50), new Tuple<int, int>(77, 50), new Tuple<int, int>(78, 50), new Tuple<int, int>(79, 50) };
			AssertArrayEqualsByElements(expected, dots.ToArray());

			AssertEquals("Background pixel should be white", Color.White.ToArgb(), map.GetPixel(0, 0).ToArgb());

			AssertNotEquals("Single point line should have at least some visibility, even if it is not the same colour as a double or triple point line", Color.White.ToArgb(), map.GetPixel(199, 20).ToArgb());
		}

		#endregion

		public ZBlob GetSignature()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(200); // width
				writer.Write(100); // height
				writer.Write(3); // number of lines

				writer.Write(3); // number of points for 1st line

				writer.Write(20); // line 1, point 1, x
				writer.Write(20); // line 1, point 1, y

				writer.Write(20); // line 1, point 2, x
				writer.Write(80); // line 1, point 2, y

				writer.Write(30); // ...
				writer.Write(80);

				writer.Write(2); // number of points for 2nd line

				writer.Write(50);
				writer.Write(20);

				writer.Write(50);
				writer.Write(80);

				writer.Write(1); // number of points for 3rd line

				writer.Write(199);
				writer.Write(20);

				return new ZBlob(stream.ToArray());
			}
		}
	}
}
