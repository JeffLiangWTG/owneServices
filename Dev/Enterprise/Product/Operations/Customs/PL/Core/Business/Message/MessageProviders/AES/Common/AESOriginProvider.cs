using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESOriginProvider : IOrigin
{
	public AESOriginProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	public string CountryOfOrigin => invoiceLine.JI_CountryOfOrigin;

	public string RegionOfDispatch => null; // future use
}
