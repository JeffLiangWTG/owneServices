using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI.Test
{
	public class SingleSelectModuleDecisionProviderTest : TestCaseWithFactory
	{
		public void TestHandleDefaultAction()
		{
			var bizo = Factory.New<AccChargeCode>();
			var provider = new SingleSelectModuleDecisionProvider();
			provider.HandleDefaultAction(new[] { bizo });
			AssertEquals(bizo, provider.SelectedBusinessObject);
		}

		public void TestHandleDefaultAction_MultipleSelect()
		{
			var biz1 = Factory.New<AccChargeCode>();
			var biz2 = Factory.New<AccChargeCode>();
			var provider = new SingleSelectModuleDecisionProvider();
			provider.HandleDefaultAction(new[] { biz1, biz2 });
			AssertNull(provider.SelectedBusinessObject);
		}

		public void TestHandleDefaultAction_Errors()
		{
			var biz1 = Factory.New<AccChargeCode>();
			biz1.AddRowError("Some Error");
			biz1.AddRowError("Another Error");
			var provider = new SingleSelectModuleDecisionProvider();
			provider.HandleDefaultAction(new[] { biz1 });
			AssertEquals("Some Error", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertNull(provider.SelectedBusinessObject);
		}
	}
}
