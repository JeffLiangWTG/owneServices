using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDebtorGroupBankCurrentOverride))]
	sealed class OrgDebtorGroupBankCurrentOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public new void TestBizObjectFields()
		{
			OrgDebtorGroupBankCurrentOverride test = Factory.NewWithValidTestData<OrgDebtorGroupBankCurrentOverride>();
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			test.PB_RX_NKCurrency = currency.RX_Code;
			AssertEquals("This Descriptions must be equal", test.CurrencyDescription, currency.RX_Desc);
		}
	}
}
