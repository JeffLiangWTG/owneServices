using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class SendACASConsolMethodApplicator : DocDataObjectSendingMessageMethodApplicator
	{
		public SendACASConsolMethodApplicator(DocDataObjectSendingMessageSettings settings, BusinessObjectFactory factory)
			: base(settings, factory)
		{
		}

		protected override ZString NoSelectedErrorMessage => Res.GetString("30B4DB11-E759-49B6-BCEE-A826AA603B4F", "No consols selected.");

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
			return new ACASConsolReportSendingProvider(Factory);
		}
	}
}
