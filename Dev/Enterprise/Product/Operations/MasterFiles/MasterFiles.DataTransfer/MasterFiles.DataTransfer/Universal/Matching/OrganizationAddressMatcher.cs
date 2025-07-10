using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Matching
{
	public class OrganizationAddressMatcher : IOrganizationAddressMatcher
	{
		public IOrgHeader GetMatchingOrgHeader(OrganizationAddress organisationData, BusinessObjectFactory factory)
		{
			var matcher = new OrganisationMatcher(factory, IfUnmatched.ReturnNull);
			var matchedOrgHeader = matcher.GetMatchingOrganization(organisationData);
			return matchedOrgHeader;
		}

		public ZString GetMatchedOrganisationCode(IOrganizationAddress addressData, IXmlImportLogger logger)
		{
			var addressDataConcrete = (OrganizationAddress)addressData;

			var matchedOrgAddress = new OrganisationDataObjectReader(addressDataConcrete, logger, new UniversalObjectFactory()).GetMatched();

			if (matchedOrgAddress != null)
			{
				return matchedOrgAddress.Header.OH_Code;
			}

			return ZString.Empty;
		}
	}
}
