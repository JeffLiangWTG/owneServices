using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	sealed class TwoStepDeclarationHelperTest : TestCaseWithFactory
	{
		public void TestIsTwoStepClearingValid()
		{
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				Assert(TwoStepDeclarationHelper.IsTwoStepClearingValid);
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				Assert(!TwoStepDeclarationHelper.IsTwoStepClearingValid);
			}
		}
	}
}
