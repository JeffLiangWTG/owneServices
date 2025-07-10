using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public class CartageXmlMessageDeliver : XmlMessageDeliver
	{
		public CartageXmlMessageDeliver(BusinessObject bizObjToLogAgainst, MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IJobNumber jobNumberSource, IValueObjectDataAdapter dataAdapter, XmlInterchange interchange)
			: base(modes, bizObjToDeliver, jobNumberSource, dataAdapter, interchange, null)
		{
			this.bizObjToLogAgainst = bizObjToLogAgainst;
		}

		readonly BusinessObject bizObjToLogAgainst;

		protected override DeliveryContext GetDeliveryContext()
		{
			return new DeliveryContext(bizObjToLogAgainst.Factory)
			{
				ParentInfo = EntityInfo.New(bizObjToLogAgainst),
			};
		}
	}
}
