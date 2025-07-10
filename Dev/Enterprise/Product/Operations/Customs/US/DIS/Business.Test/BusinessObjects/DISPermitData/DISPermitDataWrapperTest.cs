using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISPermitDataWrapperTest : TestCaseWithFactory
	{
		public void TestIDISPermit()
		{
			var permit = new DISPermitData(Factory);
			permit.PermitNumber = "23897";
			permit.ApprovalNumber = "A5427890";
			permit.PermitType = "T";
			permit.Statement = "S";
			permit.StartDate = ZDateTime.Today.AddYears(-1);
			permit.EndDate = ZDateTime.Today;
			var iPermit = (IDISPermit)new DISPermitDataWrapper(permit, "19-4239784-0");
			AssertEquals("23897", iPermit.Number);
			AssertEquals("19-4239784-0", iPermit.ImporterOfRecord);
			AssertEquals("A5427890", iPermit.ApprovalNumber);
			AssertEquals("T", iPermit.Type);
			AssertEquals("S", iPermit.Statement);
			AssertEquals(ZDateTime.Today.AddYears(-1), iPermit.StartDate);
			AssertEquals(ZDateTime.Today, iPermit.EndDate);
		}
	}
}
