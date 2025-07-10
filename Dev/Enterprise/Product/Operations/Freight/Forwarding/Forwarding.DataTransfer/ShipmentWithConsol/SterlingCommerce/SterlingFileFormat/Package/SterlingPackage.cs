using System.Globalization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingPackage : SterlingRecord
	{
		public SterlingPackage()
		{
		}

		#region Source
		public Xsd.Package Source
		{
			get
			{
				return fSource;
			}
			set
			{
				fSource = value;
			}
		}
		Xsd.Package fSource;
		#endregion

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "PKG";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(PackType);
			AddField(NumberOfPacks);
			AddField(Weight);
			AddField(WeightDimensionType);
			AddField(Length);
			AddField(LengthDimensionType);
			AddField(Width);
			AddField(WidthDimensionType);
			AddField(Height);
			AddField(HeightDimensionType);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region PackType

		public ZString PackType
		{
			get
			{
				return Source.PackType;
			}
		}

		#endregion

		#region NumberOfPacks

		public ZString NumberOfPacks
		{
			get
			{
				return Source.NumberOfPacks.ToString(CultureInfo.InvariantCulture);
			}
		}

		#endregion

		#region Weight

		public ZString Weight
		{
			get
			{
				return Source.Weight.Value.ToString();
			}
		}

		#endregion

		#region WeightDimensionType

		public ZString WeightDimensionType
		{
			get
			{
				return Source.Weight.DimensionType;
			}
		}

		#endregion

		#region Length

		public ZString Length
		{
			get
			{
				return Source.Length.Value.ToString();
			}
		}

		#endregion

		#region LengthDimensionType

		public ZString LengthDimensionType
		{
			get
			{
				return Source.Length.DimensionType;
			}
		}

		#endregion

		#region Width

		public ZString Width
		{
			get
			{
				return Source.Width.Value.ToString();
			}
		}

		#endregion

		#region WidthDimensionType

		public ZString WidthDimensionType
		{
			get
			{
				return Source.Width.DimensionType;
			}
		}

		#endregion

		#region Height

		public ZString Height
		{
			get
			{
				return Source.Height.Value.ToString();
			}
		}

		#endregion

		#region HeightDimensionType

		public ZString HeightDimensionType
		{
			get
			{
				return Source.Height.DimensionType;
			}
		}

		#endregion

		#endregion

	}
}
