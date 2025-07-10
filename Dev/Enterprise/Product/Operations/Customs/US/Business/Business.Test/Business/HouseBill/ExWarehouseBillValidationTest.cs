using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExWarehouseBillValidationTest : TestCaseWithFactory
	{
		public void TestNoValidationExpectedForBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			var bill = declaration.Bills.AddNew();
			bill.RunPreSaveValidation();
			AssertEquals("no message erros expected", "", bill.Notifications.GetMessageErrors().ToUniqueMessageListString());
			AssertEquals("no warnings expected", "", bill.Notifications.GetWarnings().ToUniqueMessageListString());
		}
	}
}
