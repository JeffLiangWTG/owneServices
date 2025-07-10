using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationExtensionMethodsForMergeTest : TestCaseWithFactory
	{
		public void TestGetLineCalculator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals(typeof(FTZLineDutyFeeCalculator), declaration.GetLineCalculator().GetType());
			AssertEquals(typeof(FTZDutyFeeCalculator), declaration.GetCalculationManager().GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(LineDutyFeeCalculator), declaration.GetLineCalculator().GetType());
			AssertEquals(typeof(DutyFeeCalculationManager), declaration.GetCalculationManager().GetType());
		}
	}
}
