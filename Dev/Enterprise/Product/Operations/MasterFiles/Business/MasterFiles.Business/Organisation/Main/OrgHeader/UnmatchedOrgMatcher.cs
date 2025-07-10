using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class UnmatchedOrgMatcher
	{
		public static void GetUnmatchedOrgPatternIfRegistryConfigurationAllowsIt(OrgPatternMatchCollection collection)
		{
			if (collection != null && UseUnmatchedOrganisationForMatching)
			{
				collection.Load(new ZQuery(OrgPatternMatchSchema.OS_OH, OrgHeader.UnmatchedOrganisationPK));
			}
		}

		public static OrgAddress GetUnmatchedOrgAddressIfRegistryConfigurationAllowsIt(BusinessObjectFactory factory, ISimpleLogger logger = null)
		{
			OrgAddress result = null;

			if (UseUnmatchedOrganisationForMatching)
			{
				var unmatchedOrgPattern = factory.LoadTop1<OrgPatternMatch>(new ZQuery(OrgPatternMatchSchema.OS_OH, OrgHeader.UnmatchedOrganisationPK));
				result = unmatchedOrgPattern?.Address;
				if (result != null)
				{
					logger?.Log(LogType.Information, Res.GetString("E99C8E3E-DA79-4F19-BCF2-BDF17DEB2338", "No match found - Assigned to UNMATCHED organization (Code: {0})", OrgHeader.UnmatchedOrganisationCode));
				}
			}

			return result;
		}

		static bool UseUnmatchedOrganisationForMatching => OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
	}
}
