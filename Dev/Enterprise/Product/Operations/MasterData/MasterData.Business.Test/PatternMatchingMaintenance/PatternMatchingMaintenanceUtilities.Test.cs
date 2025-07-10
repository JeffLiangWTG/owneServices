using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingMaintenanceUtilitiesTest : TestCaseWithFactory
	{
		public void TestCreateOrUpdatePatternRecord_ReturnsFalse_WhenHashedValue_EqualZero()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var webUrl = org.OrgWebURLs.AddNew();

			var webUrlMaintenance = new PatternMatchingOrgWebURLMaintenance(webUrl);
			var maintenance = new PatternMatchingMaintenanceUtilities<PatternMatchingDomain>(webUrl, false, 0, org.PK, Guid.Empty, "AU");

			var result = maintenance.CreateOrUpdatePatternRecord(PatternMatchingDomainSchema.PMD_ParentId, webUrl.PK);

			Assert("Should not have created any patterns", !result);

			var domainPattern = Factory.LoadTop1<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, webUrl.PK));
			AssertNull(domainPattern);
		}
	}
}
