using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class RepresentativeJobDocAddressValidation : JobDocAddressValidation
{
	public RepresentativeJobDocAddressValidation(JobDocAddress representative)
		: base(representative)
	{
	}

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		var parent = Parent;
		if (parent.Organisation is { } organisation && EuEoriResolver.GetRegNoWithCountryCode(organisation).IsEmpty)
		{
			parent.OrganisationPKInfo.AddWarning(Res.GetString("22557AFA-C25A-4424-A7E8-96504696B09D", "EORI for Trader Representative is not present."));
		}
	}
}
