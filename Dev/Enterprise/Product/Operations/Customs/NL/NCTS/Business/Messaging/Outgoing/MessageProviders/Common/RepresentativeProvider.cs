using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class RepresentativeProvider : PartyProvider, INCTSRepresentative
{
	public static RepresentativeProvider New(JobDocAddress jobDocAddress) => jobDocAddress.IsValidAddress ? new RepresentativeProvider(jobDocAddress) : null;

	RepresentativeProvider(JobDocAddress jobDocAddress) : base(jobDocAddress)
	{
	}

	public int Status => CachedValueHelper.GetValue(ref status, () =>
	{
		var result = 2;
		var parent = docAddress?.Parent;

		if (parent is NctsDepartureMovementHeader movementHeader)
		{
			var holderOfTransitProcedure = movementHeader.Header.Principal;
			if (holderOfTransitProcedure != null && holderOfTransitProcedure.E2_OA_Address == docAddress.E2_OA_Address && holderOfTransitProcedure.OrganisationPK == docAddress.OrganisationPK)
			{
				result = 3;
			}
		}
		return result;
	});
	CachedValue<ZInt> status;

	protected override INCTSAddress GetAddress() => null;
}
