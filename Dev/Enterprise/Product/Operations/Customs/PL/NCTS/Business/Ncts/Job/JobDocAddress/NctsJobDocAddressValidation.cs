using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsJobDocAddressValidation : EU.NCTS.Business.NctsJobDocAddressValidation
{
	public NctsJobDocAddressValidation(JobDocAddress addressToValidate, NctsCommonCargoDesc goodsItem) : base(addressToValidate, goodsItem)
	{ }

	public NctsJobDocAddressValidation(JobDocAddress addressToValidate, NctsHeader header) : base(addressToValidate, header)
	{ }

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		ValidateJobDocAddressContactsPhones();
		ValidateJobDocAddressEmployerIdentificationNumber();
	}

	static readonly HashSet<DocAddressType> DocAddressTypesToValidateNonEmptyContactPhones = new HashSet<DocAddressType>
		{ DocAddressType.Principal, DocAddressType.ConsignorDocumentaryAddress, DocAddressType.Representative, DocAddressType.Carrier };

	void ValidateJobDocAddressContactsPhones()
	{
		var parent = Parent;

		if (Header.IsPhase5Departure
			&& DocAddressTypesToValidateNonEmptyContactPhones.Contains(parent.DocAddressType)
			&& parent.Address?.Header is OrgHeader orgHeader
			&& OrgContactsHelper.TryGetContactForMessage(orgHeader, out var contactForMessage)
			&& !contactForMessage.OC_ContactName.IsEmpty
			&& !contactForMessage.HasNonEmptyPhone())
		{
			parent.OrganisationPKInfo.AddMessageError(Res.GetString("3DEF02FB-24AC-4BE3-80CD-A16A2327D7D1",
				"Contact Name is present. However, Phone number is missing which is required. Hence, Contact information will be skipped in Customs Edi message."));
		}
	}

	void ValidateJobDocAddressEmployerIdentificationNumber()
	{
		var parent = Parent;

		if (Header.IsPhase5Arrival
			&& parent.Address?.Header is OrgHeader orgHeader
			&& orgHeader.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).IsEmpty)
		{
			parent.OrganisationPKInfo.AddMessageError(Res.GetString("B0AAB507-AC75-4736-B75B-8FA2E156FB38", "EORI not declared for Destination Trader. Please update EORI for Destination Trader in Organization master."));
		}
	}
}
