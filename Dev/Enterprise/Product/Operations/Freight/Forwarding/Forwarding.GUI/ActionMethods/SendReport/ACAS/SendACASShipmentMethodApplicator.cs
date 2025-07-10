using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class SendACASShipmentMethodApplicator : DocDataObjectSendingMessageMethodApplicator
	{
		public SendACASShipmentMethodApplicator(DocDataObjectSendingMessageSettings settings, BusinessObjectFactory factory)
			: base(settings, factory)
		{
		}

		protected override ZString NoSelectedErrorMessage => Res.GetString("378B7C21-5262-4BF8-AF86-51DCA488C042", "No shipments selected.");

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
			return new ACASShipmentReportSendingProvider(Factory);
		}
	}
}
