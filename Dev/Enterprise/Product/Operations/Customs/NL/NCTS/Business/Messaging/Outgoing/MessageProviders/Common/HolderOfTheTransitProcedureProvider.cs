using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class HolderOfTheTransitProcedureProvider : PartyProvider, IHolderOfTheTransitProcedure
{
	public HolderOfTheTransitProcedureProvider(JobDocAddress principal) : base(principal)
	{
	}

	public string TIRHolderIdentificationNumber => docAddress.Organisation.GetCustomsRegNoIgnoringCountryPrefixed(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers);

	protected override bool OmitNameAndAddressWhenIDIsFound => true;

	public static HolderOfTheTransitProcedureProvider New(JobDocAddress principal) => principal == null ? null : new HolderOfTheTransitProcedureProvider(principal);
}
