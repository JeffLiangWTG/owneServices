using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WarehouseFactsHelper
	{
		public static OrganisationFact GetOrCreateOrganisationFact(
			Dictionary<ZGuid, OrganisationFact> orgFacts,
			ZGuid orgPk,
			Func<OrgHeader> getOrg)
		{
			if (!orgFacts.TryGetValue(orgPk, out var orgFact))
			{
				var org = getOrg();
				if (org != null)
				{
					orgFacts[orgPk] = orgFact = GetOrganisationFact(org);
				}
			}
			return orgFact;
		}

		public static OrganisationFact GetOrganisationFact(OrgHeader organisation)
		{
			Argument.NotNull(organisation, nameof(organisation));

			var isProxyOrgOfCurrentCompany = organisation.IsProxyOrg(GlbCompany.CurrentCompany);
			return new OrganisationFact(organisation.PK.ToGuid(), organisation.OH_Code, isProxyOrgOfCurrentCompany || organisation.IsProxyOrgOfAnyCompany(), isProxyOrgOfCurrentCompany);
		}

		public static DocAddressFact GetOrCreateDocAddressFact(
			Dictionary<ZGuid, DocAddressFact> docAddressFacts,
			Dictionary<ZGuid, OrganisationFact> orgFacts,
			JobDocAddress jobDocAddress)
		{
			if (!docAddressFacts.TryGetValue(jobDocAddress.PK, out var docAddressFact))
			{
				if (!docAddressFacts.TryGetValue(jobDocAddress.E2_OA_Address, out docAddressFact))
				{
					var pk = jobDocAddress.E2_AddressOverride ? jobDocAddress.PK : jobDocAddress.E2_OA_Address;
					var orgFact = jobDocAddress.E2_AddressOverride ? null : GetOrCreateOrganisationFact(orgFacts, jobDocAddress.OrganisationPK, () => jobDocAddress.Organisation);
					docAddressFact = GetDocAddressFact(pk, jobDocAddress, orgFact);
					docAddressFacts.Add(pk, docAddressFact);
				}
			}
			return docAddressFact;
		}

		static DocAddressFact GetDocAddressFact(ZGuid pk, JobDocAddress jobDocAddr, OrganisationFact orgFact)
		{
			// for overrriden address jobDocAddress.pk.toGuid...for non-overrriden addresses OrgAddress.PK
			return new DocAddressFact(pk.ToGuid(), jobDocAddr.E2_CompanyName, jobDocAddr.E2_RN_NKCountryCode, jobDocAddr.E2_State, jobDocAddr.E2_City, jobDocAddr.E2_Postcode, orgFact);
		}
	}
}
