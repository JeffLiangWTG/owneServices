using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgDebtorGroupBankCurrentOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPB_RX_NKCurrency()
		{
			var testFactory = new BusinessObjectFactory();
			var bankAccount = testFactory.NewWithValidTestData<AccBankAccount>();
			var currency1 = testFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, "AUD"));
			var currency2 = testFactory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, currency1.RX_Code)).Where(currency => currency.RX_IsActive).FirstOrDefault();
			var debtorGroupBankCurrentOverrideCollection = new OrgDebtorGroupBankCurrentOverrideCollection(testFactory.New<OrgDebtorGroupBankDefault>());
			debtorGroupBankCurrentOverrideCollection.AddNew();
			debtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency = ZString.Empty;
			debtorGroupBankCurrentOverrideCollection[0].PB_AB = bankAccount.PK;
			Assert("PB_RX_NKCurrency should not be correct, not expecting errors.", debtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrencyInfo.HasNotifications());

			debtorGroupBankCurrentOverrideCollection.AddNew();
			debtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency = currency1.RX_Code;
			debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrency = currency1.RX_Code;
			debtorGroupBankCurrentOverrideCollection[1].PB_AB = bankAccount.PK;

			var notification = debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrencyInfo.Notifications.GetFirstMessage();
			Assert("PB_RX_NKCurrency should not be correct you cannot have 2 identical Currencies, expecting errors. " + notification, debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrencyInfo.HasNotifications());

			debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrency = currency2.RX_Code;
			var notification1 = debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrencyInfo.Notifications.GetFirstMessage();
			Assert("PB_RX_NKCurrency should be correct you should have different currencies on different records, not expecting errors. " + notification1, !debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrencyInfo.HasNotifications());

			debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrency = "yX3";
			var notification2 = debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrencyInfo.Notifications.GetFirstMessage();
			Assert("PB_RX_NKCurrency should not be correct random curren, expecting errors. " + notification2, debtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrencyInfo.HasNotifications());
		}
	}
}
