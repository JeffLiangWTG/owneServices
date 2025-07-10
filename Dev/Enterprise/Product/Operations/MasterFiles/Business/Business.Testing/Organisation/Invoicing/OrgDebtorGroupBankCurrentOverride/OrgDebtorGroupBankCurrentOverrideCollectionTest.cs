using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDebtorGroupBankCurrentOverrideCollection))]
	sealed class OrgDebtorGroupBankCurrentOverrideCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgDebtorGroupBankDefault masterDebtorGroupBankDefault = Factory.NewWithValidTestData<OrgDebtorGroupBankDefault>();
			return new OrgDebtorGroupBankCurrentOverrideCollection(masterDebtorGroupBankDefault);
		}
	}
}
