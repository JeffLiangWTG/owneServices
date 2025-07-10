using CargoWise.Types;

using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingHeader : SterlingRecord
	{
		public SterlingHeader(SterlingCommerceConsolAndShipmentExporter master)
			: base(master) { }

		public Xsd.InterchangeInfo Source
		{
			get
			{
				return Master.Interchange;
			}
		}

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "H01";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(SenderId);
			AddField(ReciverID);
			AddField(DOCID);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region SenderID

		public ZString SenderId
		{
			get
			{
				return Source.Source.SenderCode;
			}
		}

		#endregion

		#region ReceiverID

		public ZString ReciverID
		{
			get
			{
				return Source.Target.ReceiverCode;
			}
		}

		#endregion

		#region DOCID

		public ZString DOCID
		{
			get
			{
				return Source.Source.Purpose;
			}
		}

		#endregion

		#endregion
	}
}
