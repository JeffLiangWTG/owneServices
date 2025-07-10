using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLegExtensionsTests : TestCaseWithFactory
	{
		public void TestRunSheetSequenceHelper_Split()
		{
			var leg1 = Factory.New<CommonCartageLeg>();
			var leg2 = Factory.New<CommonCartageLeg>();
			var helper = new RunSheetSequenceHelper(leg1);
			var errors = helper.Split(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals(null, errors);

			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			errors = helper.Split(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals("Selected legs should all have the same sequence number.", errors);
		}

		public void TestOtherLegsInSameGroup()
		{
			CommonCartageLeg leg1 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg2 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg3 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg4 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg5 = Factory.New<CommonCartageLeg>();

			ZGuid runsheetPK1 = Factory.New<CommonWorkSheet>().PK;
			ZGuid runsheetPK2 = Factory.New<CommonWorkSheet>().PK;

			leg1.JU_EY_RunSheet = runsheetPK1;
			leg2.JU_EY_RunSheet = runsheetPK1;
			leg3.JU_EY_RunSheet = runsheetPK1;

			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 1;

			leg4.JU_EY_RunSheet = runsheetPK2;
			leg4.JU_RunSheetSequence = 1;

			leg5.JU_RunSheetSequence = 2;
			leg5.JU_EY_RunSheet = runsheetPK1;

			AssertEquals("should find correct number of legs", 2, leg1.OtherLegsInSameGroup().Length);
			AssertEquals("should find correct number of legs", 2, leg2.OtherLegsInSameGroup().Length);
			AssertEquals("should find correct number of legs", 2, leg3.OtherLegsInSameGroup().Length);
			AssertEquals("should find correct number of legs", 0, leg4.OtherLegsInSameGroup().Length);
			AssertEquals("should find correct number of legs", 0, leg5.OtherLegsInSameGroup().Length);
		}
	}
}
