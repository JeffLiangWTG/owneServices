using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrganizationRegistryProviderTest : TestCaseWithFactory
	{
		public void TestEnableImportFromCreditReports()
		{
			var provider = new OrganizationRegistryProvider();
			OrganisationRegistry.Instance.EnableImportFromCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true);
			AssertEquals(true, provider.EnableImportFromCreditReports);
			OrganisationRegistry.Instance.EnableImportFromCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, false);
			AssertEquals(false, provider.EnableImportFromCreditReports);
			provider.EnableImportFromCreditReports = true;
			AssertEquals(true, OrganisationRegistry.Instance.EnableImportFromCreditReports.Value);
			provider.EnableImportFromCreditReports = false;
			AssertEquals(false, OrganisationRegistry.Instance.EnableImportFromCreditReports.Value);
		}
	}
}
