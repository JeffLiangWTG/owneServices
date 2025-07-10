using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAROrgTaxConfigurationCollection))]
	sealed class AccAROrgTaxConfigurationCollectionTest : AccOrgTaxConfigurationCollectionByLedgerTest
	{
		protected override ZString ExpectedLedger => LedgerTypes.AccountsReceivable;

		protected override AccOrgTaxConfigurationCollection GetCollectionToTest(OrgCompanyData parent)
		{
			return new AccAROrgTaxConfigurationCollection(parent);
		}
	}
}
