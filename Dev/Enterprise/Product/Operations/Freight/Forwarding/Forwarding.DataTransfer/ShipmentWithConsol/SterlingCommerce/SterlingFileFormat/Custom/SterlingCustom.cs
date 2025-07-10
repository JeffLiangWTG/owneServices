using CargoWise.Types;

using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingCustom : SterlingRecord
	{
		public SterlingCustom(SterlingCommerceConsolAndShipmentExporter master)
			: base(master) { }

		public Xsd.ShipmentShipmentDetailsCustom Source
		{
			get
			{
				return Master.Shipment.ShipmentDetails.Custom;
			}
		}

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "CST";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(CustomAttribute1);
			AddField(CustomAttribute2);
			AddField(Date1);
			AddField(Date2);
			AddField(Decimal1);
			AddField(Decimal2);
			AddField(Flag1);
			AddField(Flag2);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region CustomAttribute1

		public ZString CustomAttribute1
		{
			get
			{
				return Source.CustomAttribute1;
			}
		}

		#endregion

		#region CustomAttribute2

		public ZString CustomAttribute2
		{
			get
			{
				return Source.CustomAttribute2;
			}
		}

		#endregion

		#region Date1

		public ZString Date1
		{
			get
			{
				return ToTimeFormat(Source.Date1);
			}
		}

		#endregion

		#region Date2

		public ZString Date2
		{
			get
			{
				return ToTimeFormat(Source.Date2);
			}
		}

		#endregion

		#region Decimal1

		public ZString Decimal1
		{
			get
			{
				return Source.Decimal1.ToString();
			}
		}

		#endregion

		#region Decimal2

		public ZString Decimal2
		{
			get
			{
				return Source.Decimal2.ToString();
			}
		}

		#endregion

		#region Flag1

		public ZString Flag1
		{
			get
			{
				return Source.Flag1.ToString();
			}
		}

		#endregion

		#region Flag2

		public ZString Flag2
		{
			get
			{
				return Source.Flag2.ToString();
			}
		}

		#endregion

		#endregion

	}
}
