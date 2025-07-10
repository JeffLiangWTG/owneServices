using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class InvoiceInformationForTest : IInvoiceInformation
	{
		public ZString InvoiceNumber { get; set; }

		public ZDateTime InvoiceDate { get; set; }
	}
}
