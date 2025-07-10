using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class Origin : IOrigin
	{
		public Origin(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		public ZString CountryCode => invoiceLine.JI_CountryOfOrigin;

		public IAdditionalDocument AdditionalDocument => new AdditionalDocumentWrapper(invoiceLine.CertificateOfOriginNumber, invoiceLine.CertificateOfOriginNumberItemNumber);

		readonly JobComInvoiceLine invoiceLine;
	}
}
