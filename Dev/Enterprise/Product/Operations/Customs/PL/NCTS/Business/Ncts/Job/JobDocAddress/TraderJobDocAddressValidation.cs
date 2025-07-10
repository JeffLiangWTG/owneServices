using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TraderJobDocAddressValidation : NctsJobDocAddressValidation
{
	readonly string traderType;

	public TraderJobDocAddressValidation(JobDocAddress addressToValidate, string traderType, NctsCommonCargoDesc goodsItem) : base(addressToValidate, goodsItem)
	{
		this.traderType = Argument.NotNull(traderType, nameof(traderType));
	}

	public TraderJobDocAddressValidation(JobDocAddress addressToValidate, string traderType, NctsHeader header) : base(addressToValidate, header)
	{
		this.traderType = Argument.NotNull(traderType, nameof(traderType));
	}

	protected ZPropertyInfo OrganisationPKInfo => Parent.OrganisationPKInfo;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();
		if (Header.IsDepartureMovement)
		{
			if (RepresentativeAndPrincipalAreNotSelected)
			{
				var errorMessage = ZString.Format(Res.GetString("6BD70171-2579-4B7A-A32E-3A0E315C2998", "[NR0017] You have not entered a {0}."), traderType);
				OrganisationPKInfo.AddMessageError(errorMessage);
			}
			else if (RepresentativeAndPrincipalHaveEmptyEori)
			{
				var errorMessage = Res.GetString("DEDDFA68-A781-4D67-870B-1D55BD4E4D71", "[NR0016] EORI number of the Representative or Principal is required for generation of valid LRN.");
				OrganisationPKInfo.AddMessageError(errorMessage);
			}
		}
	}

	bool RepresentativeAndPrincipalAreNotSelected => Header.MovementHeader?.Representative.Organisation == null && Header.Principal.Organisation == null;

	bool RepresentativeAndPrincipalHaveEmptyEori => EuEoriResolver.GetRegNoWithCountryCode(Header.MovementHeader?.Representative.Organisation).IsEmpty
		&& EuEoriResolver.GetRegNoWithCountryCode(Header.Principal.Organisation).IsEmpty;
}
