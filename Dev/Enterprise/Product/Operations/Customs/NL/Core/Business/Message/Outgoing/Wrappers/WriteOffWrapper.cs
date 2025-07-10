using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class WriteOffWrapper : IWriteOff
{
	public WriteOffWrapper(SupportingDocument supportingDocument)
	{
		this.supportingDocument = supportingDocument;
	}
	readonly SupportingDocument supportingDocument;

	public decimal QuantityQuantity => supportingDocument?.CSI_Quantity ?? 0m;
	public string UnitCode => supportingDocument?.CSI_UnitOfQuantity;
	public decimal Amount => supportingDocument?.CSI_Value ?? 0m;
	public string CurrencyId => supportingDocument?.CSI_RX_NKCurrency;
	public IPackaging Packaging => null;
}
