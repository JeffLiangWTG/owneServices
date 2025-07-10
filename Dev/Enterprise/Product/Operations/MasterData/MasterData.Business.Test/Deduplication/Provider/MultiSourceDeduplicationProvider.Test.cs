using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class MultiSourceDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeDuplicationProvider()
		{
			var provider = new DeduplicationProvider();
			var orgNameProvider = new MultiSourceDeduplicationProvider(provider, typeof(MultiSourceCompanyName), DeduplicationProvider.Constants.OrganisationNames);

			AssertEquals(DeduplicationDisplayMode.List, orgNameProvider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.OrganisationNames, orgNameProvider.GroupNameForType);
			AssertEquals(typeof(MultiSourceCompanyName), orgNameProvider.GlowType);

			var personNameProvider = new MultiSourceDeduplicationProvider(provider, typeof(MultiSourcePersonName), DeduplicationProvider.Constants.PersonNames);

			AssertEquals(DeduplicationDisplayMode.List, personNameProvider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.PersonNames, personNameProvider.GroupNameForType);
			AssertEquals(typeof(MultiSourcePersonName), personNameProvider.GlowType);

			var domainProvider = new MultiSourceDeduplicationProvider(provider, typeof(MultiSourceDomain), DeduplicationProvider.Constants.Domains);

			AssertEquals(DeduplicationDisplayMode.List, domainProvider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.Domains, domainProvider.GroupNameForType);
			AssertEquals(typeof(MultiSourceDomain), domainProvider.GlowType);

			var phoneProvider = new MultiSourceDeduplicationProvider(provider, typeof(MultiSourcePhoneNumber), DeduplicationProvider.Constants.PhoneNumbers);

			AssertEquals(DeduplicationDisplayMode.List, phoneProvider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.PhoneNumbers, phoneProvider.GroupNameForType);
			AssertEquals(typeof(MultiSourcePhoneNumber), phoneProvider.GlowType);
		}

		public void TestGetComparisonSource()
		{
			var provider = new DeduplicationProvider();
			var nameProvider = new MultiSourceDeduplicationProvider(provider, typeof(MultiSourceCompanyName), DeduplicationProvider.Constants.OrganisationNames);
			var orgInDB = Factory.NewWithValidTestData<OrgHeader>();

			orgInDB.OH_Code = "ABZ";
			Factory.Save();
			var dedupOrg = new DeduplicationOrgHeader(orgInDB);
			var source = nameProvider.GetComparisonSource(dedupOrg, orgInDB.PK.ToGuid(), new List<string> { "OH_FullName" });

			AssertNotNull("Source is not null", source);
			AssertEquals(typeof(DeduplicationOrgHeader), source.GetType());
			AssertEquals(orgInDB.PK, ((DeduplicationOrgHeader)source).OH_PK);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_Code = "CCA";
			dedupOrg = new DeduplicationOrgHeader(newOrg);
			source = nameProvider.GetComparisonSource(dedupOrg, newOrg.PK.ToGuid(), new List<string> { "OH_FullName" });

			AssertNotNull("Source is not null", source);
			AssertEquals(typeof(DeduplicationOrgHeader), source.GetType());
			AssertEquals(newOrg.PK, ((DeduplicationOrgHeader)source).OH_PK);

			source = nameProvider.GetComparisonSource(dedupOrg, newOrg.PK.ToGuid(), new List<string> { "ZZ_FullName" });

			AssertNull(source);
		}
	}
}
