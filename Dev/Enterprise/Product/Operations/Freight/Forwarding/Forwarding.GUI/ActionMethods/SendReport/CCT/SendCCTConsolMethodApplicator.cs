using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class SendCCTConsolMethodApplicator : DocDataObjectSendingMessageMethodApplicator
	{
		public SendCCTConsolMethodApplicator(DocDataObjectSendingMessageSettings settings, BusinessObjectFactory factory)
			: base(settings, factory)
		{
		}

		protected override ZString NoSelectedErrorMessage => Res.GetString("5C90FB18-D1A0-4B8D-A9DD-73A32EB738C5", "No consols selected.");

		protected override LogHyperlink GetHyperlink(BusinessObject bizObj)
		{
			if (bizObj is ForwardingConsol consol)
			{
				return consol.Hyperlink();
			}

			return null;
		}

		protected override DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider()
		{
			return new CCTConsolReportSendingProvider(Factory);
		}
	}
}
