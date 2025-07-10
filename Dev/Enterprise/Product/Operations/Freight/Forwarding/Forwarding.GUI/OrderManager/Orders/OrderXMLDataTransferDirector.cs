using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class OrderXMLDataTransferDirector : XmlDataTransferDirector
	{
		public OrderXMLDataTransferDirector(IValueObjectDataAdapter adapter, bool checkForLicence)
			: base(adapter, checkForLicence)
		{
		}

		public override Enterprise.DataTransfer.Xml.XmlValueObjectSerializer Serializer
		{
			get
			{
				return new OrderXMLValueObjectSerializer();
			}
		}
	}
}
