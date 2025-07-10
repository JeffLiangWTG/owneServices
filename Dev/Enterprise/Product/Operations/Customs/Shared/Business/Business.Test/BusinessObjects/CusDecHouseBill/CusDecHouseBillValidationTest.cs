using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class CusDecHouseBillValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestExWarehouseDecDoesNotHaveMasterBillValidated()
		{
			Bill houseBill = Factory.New<Bill>();
			houseBill.CU_JE = BaseJobDeclaration.New(Factory).PK;
			houseBill.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			houseBill.CU_MasterBill = "";
			AssertNoNotifications("ExWarehouse doesn't care about master bills", houseBill.CU_MasterBillInfo);
		}

		public void TestHouseBillNumberIsNotMandatoryWithoutCustomsNeedToKnowPackDetails()
		{
			Bill houseBill = Factory.New<Bill>();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "";
			AssertEquals("Housebill not mandatory", false, houseBill.CU_BillNumInfo.HasMessageErrors());

			houseBill.CU_BillNum = "123455";
			AssertEquals("Housebill not mandatory", false, houseBill.CU_BillNumInfo.HasMessageErrors());
		}

		[ExpectNoExceptions()]
		public void TestCU_HouseBillValidationDoesntBlowWithANullDeclaration()
		{
			Bill houseBill = Factory.New<Bill>();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "123455";
		}

		#region Implementation

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		#endregion
	}
}
