using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class PartyProvider : INCTSParty
{
	public PartyProvider(JobDocAddress jobDocAddress)
	{
		docAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
	}
	protected readonly JobDocAddress docAddress;

	public string Id => docAddress.HasRealOrganisation ? GetCustomsCode() : docAddress.E2_GovRegNum;

	public string Name => IncludeNameAndAddress
			? docAddress.HasRealOrganisation ? docAddress.Organisation.OH_FullName : docAddress.E2_CompanyName
			: string.Empty;

	public INCTSAddress Address => IncludeNameAndAddress ? GetAddress() : null;

	public IContact Contact => ContactProvider.NewOrNull(docAddress);

	protected virtual INCTSAddress GetAddress() => docAddress.HasRealAddress ? new AddressProvider(docAddress.Address) : new AddressProvider(docAddress);

	protected virtual bool OmitNameAndAddressWhenIDIsFound => false;
	bool IncludeNameAndAddress => !OmitNameAndAddressWhenIDIsFound || string.IsNullOrEmpty(Id);

	ZString GetCustomsCode() => GetCustomsCodeCore();

	protected virtual ZString GetCustomsCodeCore() => docAddress.Organisation.GetEoriDetails();
}
