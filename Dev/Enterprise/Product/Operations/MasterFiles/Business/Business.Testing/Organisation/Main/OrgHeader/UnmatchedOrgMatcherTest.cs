using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UnmatchedOrgMatcherTest : TestCaseWithFactory
	{
		public void TestMatchUnmatchedOrgPattern()
		{
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				AssertEquals(0, header.SimilarOrgMatches.Count);

				UnmatchedOrgMatcher.GetUnmatchedOrgPatternIfRegistryConfigurationAllowsIt(header.SimilarOrgMatches);
				AssertEquals(1, header.SimilarOrgMatches.Count);
				AssertEquals(unmatchedOrganisation.Code, header.SimilarOrgMatches[0].OH_Code);
			}

			unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = false };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				AssertEquals(0, header.SimilarOrgMatches.Count);

				UnmatchedOrgMatcher.GetUnmatchedOrgPatternIfRegistryConfigurationAllowsIt(header.SimilarOrgMatches);
				AssertEquals(0, header.SimilarOrgMatches.Count);
			}
		}

		public void TestUnmatchedOrgAddress()
		{
			var unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = true };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var address = UnmatchedOrgMatcher.GetUnmatchedOrgAddressIfRegistryConfigurationAllowsIt(Factory);
				AssertEquals("UNMATCHED ORGANISATION", address.Addressee);
			}

			unmatchedOrganisation = new UnmatchedOrganisation(Factory) { IsEnabled = false };
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation))
			{
				var address = UnmatchedOrgMatcher.GetUnmatchedOrgAddressIfRegistryConfigurationAllowsIt(Factory);
				AssertNull(address);
			}
		}
	}
}
