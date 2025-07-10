using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusDecHouseBillLookupsBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCU_BillTypeList()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			declarationMock.Protected().Setup<bool>("ShouldIncludeSubBillInBillTypeList").Returns(false);
			var bill = declaration.Bills.AddNew();
			AssertEquals(2, bill.Lookups.CU_BillTypeList.Count);

			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			AssertEquals("IsPost", true, declaration.IsPost);
			AssertEquals(1, bill.Lookups.CU_BillTypeList.Count);
			AssertEquals(BillTypeList.Codes.HouseBill, ((ICodeDescription)bill.Lookups.CU_BillTypeList[0]).Code);
		}
	}
}
