using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class SignatureData
	{
		public SignatureData(CommonCartageLeg leg)
			: this(leg, new SignatureDrawer(null))
		{
		}

		public SignatureData(CommonCartageLeg leg, ISignatureDrawer signatureDrawer)
		{
			CargoWise.Common.Argument.NotNull(leg, "leg", "don't pass null to SignatureData");
			this.leg = leg;
			this.signatureDrawer = signatureDrawer;
		}

		readonly ISignatureDrawer signatureDrawer;

		public Image Image
		{
			get { return image ?? (image = GetBitmap()); }
		}

		Image image;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Values not used for rendering")]
		public Bitmap GetBitmap()
		{
			if (!ShowSignature)
			{
				return null;
			}

			var data = GetSignatureData();
			byte[] signature = data.SD_BinaryValue;

			var bitsIndex = 0;
			Lines = new List<Point[]>();

			this.Width = GetInt32(signature, ref bitsIndex);
			this.Height = GetInt32(signature, ref bitsIndex);

			// number of line segments
			var linesCount = GetInt32(signature, ref bitsIndex);

			// loop through each line segment and get points
			for (var line = 0; line < linesCount; line++)
			{
				// number of points in this segment
				var pointsCount = GetInt32(signature, ref bitsIndex);
				var points = new Point[pointsCount];

				// get all points in this segment
				for (int point = 0; point < pointsCount; point++)
				{
					points[point].X = GetInt32(signature, ref bitsIndex);
					points[point].Y = GetInt32(signature, ref bitsIndex);
				}

				// add line segment to list
				Lines.Add(points);
			}

			var result = new Bitmap(Width, Height);

			var g = Graphics.FromImage(result);

			signatureDrawer.DrawSignature(Lines, g, Pens.Black);

			return result;
		}

		public StmData GetSignatureData()
		{
			if (signatureData == null || signatureData.IsDeleted)
			{
				image = null;
				signatureData = GetSignatureDataCore();
			}
			return signatureData;
		}
		StmData signatureData;

		protected StmData GetSignatureDataCore()
		{
			//SD_Type = BIN
			//SD_Name = Signature
			return leg.Factory.LoadTop1<StmData>(new ZQuery(Enterprise.ZArchitecture.Schema.StmDataSchema.SD_Owner, leg.PK));
		}

		public void SetSignatureData(byte[] signature, Guid departmentGuid)
		{
			var data = GetSignatureData();
			if (signature != null && signature.Length != 0)
			{
				if (data == null)
				{
					data = leg.Factory.New<StmData>();
					data.SD_Owner = leg.PK;
				}

				data.SD_DepartmentGuid = departmentGuid;
				data.SD_BinaryValue = new ZBlob(signature);
			}
			else if (data != null)
			{
				data.Delete();
			}
		}

		readonly CommonCartageLeg leg;

		public int Height
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public List<Point[]> Lines
		{
			get;
			private set;
		}

		public bool ShowSignature
		{
			get { return TransportRegistry.Instance.ShowLegSignatures.Value && HasValidSignature; }
		}

		public bool HasValidSignature
		{
			get
			{
				var data = GetSignatureData();
				return data != null && !data.SD_BinaryValue.IsEmpty;
			}
		}

		Int32 GetInt32(byte[] bits, ref int bitsIndex)
		{
			var data = BitConverter.ToInt32(bits, bitsIndex);
			bitsIndex += Marshal.SizeOf(data);
			return data;
		}
	}
}
