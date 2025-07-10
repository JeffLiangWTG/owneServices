using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class CC583ExitCarrierProvider : ICC583CExitCarrier
{
	readonly JobDeclaration declaration;

	public CC583ExitCarrierProvider(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}

	public string IdentificationNumber => EuEoriResolver.GetRegNoWithCountryCode(declaration.ShippingLine);

	public string Name => declaration.ShippingLine?.OH_FullName;

	public IAddress Address => CachedValueHelper.GetValue(ref address, () => declaration.ShippingLine is null ? null : new AddressProvider(declaration.ShippingLine.MainAddress));
	CachedValue<IAddress> address;
}
