using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI
{
	internal class SignatureDrawer
	{
		public SignatureDrawer(PictureBox pictureBox, bool enablePopup)
			: this(pictureBox, enablePopup, new TransportCommon.Shared.SignatureDrawer(null))
		{
		}

		public SignatureDrawer(PictureBox pictureBox, bool enablePopup, ISignatureDrawer signatureDrawer)
		{
			Argument.NotNull(pictureBox, "pictureBox");
			this.signatureDrawer = signatureDrawer;

			Picture = pictureBox;
			Picture.Paint += new PaintEventHandler(Picture_Paint);

			if (enablePopup)
			{
				Picture.Click += delegate
				{ PopupSignature(); };
			}
		}

		readonly ISignatureDrawer signatureDrawer;

		void PopupSignature()
		{
			if (Leg != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new SignatureForm(Leg));
			}
		}

		public CommonCartageLeg Leg
		{
			get { return leg; }
			set
			{
				leg = value;
				Graphics g = Graphics.FromHwnd(Picture.Handle);
				DrawSignature(g);
			}
		}
		CommonCartageLeg leg;

		void Prepare()
		{
			_lines = new List<Point[]>(linesCount);

			if (Signature != null && Signature.Length > 0)
			{
				int bitsIndex = 0;

				signatureWidth = GetInt32(Signature, ref bitsIndex);
				signatureHeight = GetInt32(Signature, ref bitsIndex);
				linesCount = GetInt32(Signature, ref bitsIndex);

				for (int line = 0; line < linesCount; line++)
				{
					Int32 pointsCount = GetInt32(Signature, ref bitsIndex);
					Point[] points = new Point[pointsCount];

					for (int point = 0; point < pointsCount; point++)
					{
						ControlDpiScalingHelper.SetX(ref points[point], GetInt32(Signature, ref bitsIndex), false);
						ControlDpiScalingHelper.SetY(ref points[point], GetInt32(Signature, ref bitsIndex), false);
					}

					_lines.Add(points);
				}
			}
		}

		void Picture_Paint(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			Prepare();
#if !WINZOR
			DrawSignature(g);
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "Exception in ZArch.Core when IconTypes is changed. Refactor later.")]
		void DrawSignature(Graphics g)
		{
			try
			{
				if (g != null && Leg != null)
				{
					g.Clear(Color.White);

					var data = Leg.Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Owner, Leg.PK));
					if (data != null)
					{
						Signature = data.SD_BinaryValue;
						Prepare();
						g.SmoothingMode = SmoothingMode.AntiAlias;

						using (var matrix = new Matrix())
						{
							matrix.Scale(Picture.Width / (float)signatureWidth, Picture.Height / (float)signatureHeight);
							g.Transform = matrix;
						}

						if (_lines != null)
						{
							signatureDrawer.DrawSignature(_lines, g, Pens.Firebrick);
						}
					}
					else
					{
						var rm = new ResourceManager("Enterprise.Freight.LocalCartage.GUI.Images", System.Reflection.Assembly.GetExecutingAssembly());
						var img = rm.GetObject("SignatureNotRecorded") as Image; // Exception in ZArch.Core when IconTypes is changed. Refactor later.

						using (var matrix = new Matrix())
						{
							matrix.Scale(Picture.Width / (float)img.Width, Picture.Height / (float)img.Height);
							g.Transform = matrix;
						}
						g.DrawImage(img, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0));
					}
				}
			}
			catch (ArgumentException)
			{
				// Parameter is not valid. Possibly caused by OOM/GDI object leak/faulty .NET installation/bad codec/video driver/???.
				// Or sometimes the Resources in resx fail to load
			}
			catch (ExternalException)
			{
				// An exception is caused by external reference, e.g., 
				// external device missing, or network, etc. See http://social.msdn.microsoft.com/Forums/vstudio/en-US/cfa576f6-0df4-4aa7-bbe7-42c1bcb590dc/bitmapint32-int32-pixelformat-throws-argumentexception?forum=csharpgeneral
				// and http://social.msdn.microsoft.com/Forums/zh/winforms/thread/08eedbf7-db24-4106-9b7a-cd17a792015d
			}
		}

		Int32 linesCount;
		IList<Point[]> _lines;
		Int32 signatureWidth;
		Int32 signatureHeight;

		Int32 GetInt32(byte[] bits, ref int bitsIndex)
		{
			Int32 data = BitConverter.ToInt32(bits, bitsIndex);
			bitsIndex += Marshal.SizeOf(data);
			return data;
		}

		byte[] Signature
		{
			get;
			set;
		}

		PictureBox Picture
		{
			get;
			set;
		}
	}
}
