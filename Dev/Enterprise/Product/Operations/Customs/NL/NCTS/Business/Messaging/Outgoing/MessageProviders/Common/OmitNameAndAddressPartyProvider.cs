using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class OmitNameAndAddressPartyProvider : PartyProvider
{
	public OmitNameAndAddressPartyProvider(JobDocAddress jobDocAddress) : base(jobDocAddress)
	{
	}

	protected override bool OmitNameAndAddressWhenIDIsFound => true;

	protected override ZString GetCustomsCodeCore() => docAddress.Organisation.GetIdentificationNumber();

	public static OmitNameAndAddressPartyProvider New(JobDocAddress jobDocAddress) => jobDocAddress == null ? null : new OmitNameAndAddressPartyProvider(jobDocAddress);
}
