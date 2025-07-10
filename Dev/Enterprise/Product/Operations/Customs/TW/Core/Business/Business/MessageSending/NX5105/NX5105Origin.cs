using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105GovernmentAgencyGoodsItem_Origin : IOrigin
	{
		readonly JobComInvoiceLine invoiceLine;

		public NX5105GovernmentAgencyGoodsItem_Origin(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}

		ZString IOrigin.CountryCode => invoiceLine.JI_CountryOfOrigin;

		IAdditionalDocument IOrigin.AdditionalDocument
		{
			get
			{
				IAdditionalDocument additionalDocument = null;
				if (!invoiceLine.CertificateOfOriginNumber.IsEmpty || !invoiceLine.CertificateOfOriginNumberItemNumber.IsEmpty)
				{
					additionalDocument = new AdditionalDocumentWrapper(invoiceLine.CertificateOfOriginNumber, invoiceLine.CertificateOfOriginNumberItemNumber);
				}
				return additionalDocument;
			}
		}
	}
}
