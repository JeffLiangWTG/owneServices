using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.TransportCommon.Shared
{
	public class SignatureDrawer : ISignatureDrawer
	{
		#region Constructor

		public SignatureDrawer(ZBlob signature)
		{
			Argument.NotNull(signature, "signature");

			this.Signature = signature;
		}

		readonly ZBlob Signature;

		#endregion

		#region Image

		public Image Image
		{
			get { return image ?? (image = CreateImage()); }
		}
		Image image;

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using GDI to draw an image, not WinForms code")]
		public void DrawSignature(IEnumerable<Point[]> lines, Graphics graphics, Pen pen)
		{
			foreach (var line in lines)
			{
				if (line != null && line.Length > 0)
				{
					if (line.Length > 2)
					{
						graphics.DrawCurve(pen, line, 1.0F);
					}
					else if (line.Length == 2)
					{
						graphics.DrawLines(pen, line);
					}
					else
					{
						// Drawing a point seems to not be supported on the graphics object, so go to the side
						var point = new Point(line[0].X + 1, line[0].Y);
						graphics.DrawLine(pen, line[0], point);
					}
				}
			}
		}

		#region Implementation

		#region CreateImage

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using GDI to draw an image, not WinForms code")]
		Bitmap CreateImage()
		{
			if (Signature.IsEmpty)
			{
				return null;
			}

			var binaryValue = Signature;
			var bitsIndex = 0;
			var lines = new List<Point[]>();
			var width = GetInt32(binaryValue, ref bitsIndex);
			var height = GetInt32(binaryValue, ref bitsIndex);
			int linesCount = GetInt32(binaryValue, ref bitsIndex); // number of line segments

			// loop through each line segment and get points
			for (int line = 0; line < linesCount; line++)
			{
				// number of points in this segment
				var pointsCount = GetInt32(binaryValue, ref bitsIndex);
				var points = new Point[pointsCount];

				// get all points in this segment
				for (int point = 0; point < pointsCount; point++)
				{
					points[point].X = GetInt32(binaryValue, ref bitsIndex);
					points[point].Y = GetInt32(binaryValue, ref bitsIndex);
				}

				// add line segment to list
				lines.Add(points);
			}

			var result = new Bitmap(width, height);
			var g = Graphics.FromImage(result);
			g.Clear(Color.White);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			DrawSignature(lines, g, Pens.Black);

			return result;
		}

		#endregion

		#region GetInt32

		Int32 GetInt32(byte[] bits, ref int bitsIndex)
		{
			var data32 = BitConverter.ToInt32(bits, bitsIndex);
			bitsIndex += Marshal.SizeOf(data32);
			return data32;
		}

		#endregion

		#endregion
	}
}
