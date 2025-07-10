using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class SendCCTShipmentMethodApplicator : DocDataObjectSendingMessageMethodApplicator
	{
		public SendCCTShipmentMethodApplicator(DocDataObjectSendingMessageSettings settings, BusinessObjectFactory factory)
			: base(settings, factory)
		{
		}
		protected override ZString NoSelectedErrorMessage => Res.GetString("6FA848E3-78D8-4B46-9577-798F7DEAC280", "No shipments selected.");

		protected override LogHyperlink GetHyperlink(BusinessObject bizObj)
		{
			if (bizObj is ForwardingShipment shipment)
			{
				return shipment.Hyperlink();
			}

			return null;
		}

		protected override DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider()
		{
			return new CCTShipmentReportSendingProvider(Factory);
		}
	}
}
