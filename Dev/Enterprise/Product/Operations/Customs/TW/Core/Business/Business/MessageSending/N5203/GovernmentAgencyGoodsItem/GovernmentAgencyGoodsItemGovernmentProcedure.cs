using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class GovernmentProcedure : IGovernmentProcedure
	{
		public GovernmentProcedure(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		public ZString TransportTypeCode => ZString.Empty;

		public ZString CurrentCode => invoiceLine.JI_Procedure;

		readonly JobComInvoiceLine invoiceLine;

		public ZString Description => ZString.Empty;
	}
}
