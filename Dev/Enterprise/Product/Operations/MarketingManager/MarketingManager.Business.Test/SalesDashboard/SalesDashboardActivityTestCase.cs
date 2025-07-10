using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	abstract class SalesDashboardActivityTestCase : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot insert or delete on a view", true);
		}

		public virtual void TestHumanReadableName()
		{
			var activity = GetNewBusinessObject();
			AssertNotEquals("", activity.HumanReadableName);
		}

		public void TestOverallActivityDispositionDescription_NoExceptionWhenParentIsNull()
		{
			var activity = GetNewBusinessObject() as SalesDashboardActivity;
			activity.VSA_ParentId = new CargoWise.Types.ZGuid();

			AssertNoExceptionThrown(() =>
			{
				_ = activity.OverallActivityDispositionDescription;
			});
		}
	}
}
