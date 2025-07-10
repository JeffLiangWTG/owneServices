using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExWarehouseAddInfoBillValidationTest : TestCaseWithFactory
	{
		public void TestNoValidationExpectedForBill()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			Bill bill = declaration.Bills.AddNew();
			bill.RunPreSaveValidation();
			AssertEquals("no message erros expected", "", bill.Notifications.GetMessageErrors().ToUniqueMessageListString());
			AssertEquals("no warnings expected", "", bill.Notifications.GetWarnings().ToUniqueMessageListString());
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EnableENS = false;
			bill.RunPreSaveValidation();
			AssertEquals("no message erros expected", "", bill.Notifications.GetMessageErrors().ToUniqueMessageListString());
			AssertEquals("no warnings expected", "", bill.Notifications.GetWarnings().ToUniqueMessageListString());
		}
	}
}
