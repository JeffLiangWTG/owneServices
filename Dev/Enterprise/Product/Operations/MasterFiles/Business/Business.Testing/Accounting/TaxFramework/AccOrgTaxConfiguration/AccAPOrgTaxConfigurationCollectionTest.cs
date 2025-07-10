using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAPOrgTaxConfigurationCollection))]
	sealed class AccAPOrgTaxConfigurationCollectionTest : AccOrgTaxConfigurationCollectionByLedgerTest
	{
		protected override ZString ExpectedLedger => LedgerTypes.AccountsPayable;

		protected override AccOrgTaxConfigurationCollection GetCollectionToTest(OrgCompanyData parent)
		{
			return new AccAPOrgTaxConfigurationCollection(parent);
		}
	}
}
