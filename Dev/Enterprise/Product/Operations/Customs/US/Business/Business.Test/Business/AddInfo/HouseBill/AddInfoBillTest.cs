using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoBill))]
	sealed class AddInfoBillTest : AddInfoAbstractTest
	{
		public override void TestIsExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var houseBill = declaration.Bills.AddNew();
			var addInfo = new AddInfoBill(houseBill.CU_AddInfoInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExport", false, addInfo.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExport", true, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			var houseBill = declaration.Bills.AddNew();
			var addInfo = new AddInfoBill(houseBill.CU_AddInfoInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsDrawback", !addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert("IsDrawback", addInfo.IsDrawback);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoBillLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoBillValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var housebill = Factory.New<JobDeclaration>().Bills.AddNew();
			return new AddInfoBill(housebill.CU_AddInfoInfo);
		}
	}
}
