using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MarketingManager.Module.Testing
{
	public abstract class ValueAnalysisModuleTest : ZModuleBasherTest
	{
		public void TestFilterControl()
		{
			using (var module = GetValueAnalysisModuleForTest())
			using (var control = module.GetNewFilterControl())
			{
				Assert(control is ValueAnalysisFilterControl);
			}
		}

		public void TestGridCollection()
		{
			using (var module = GetValueAnalysisModuleForTest())
			{
				var collection = module.GetNewGridCollection();
				Assert(collection is ViewValueAnalysisCollection);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = GetValueAnalysisModuleForTest())
			{
				var businessObject = module.GetNewFilterBusinessObject();
				Assert(businessObject is ValueAnalysisFilterBusinessObject);
			}
		}

		public void TestAllowNewViewEditAndDelete()
		{
			using (var module = GetValueAnalysisModuleForTest())
			{
				AssertEquals("AllowNew", false, module.AllowNew);
				AssertEquals("AllowView", false, module.AllowView);
				AssertEquals("AllowEdit", false, module.AllowEdit);
				AssertEquals("AllowDelete", false, module.AllowDelete);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = GetValueAnalysisModuleForTest())
			{
				AssertEquals("Licence", Env.Licence.SalesValueAnalysis, module.LicenceCheckpoint);
			}
		}

		protected abstract IValueAnalysisModuleForTest GetValueAnalysisModuleForTest();

		protected override bool HasDefaultController()
		{
			return false;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}
	}
}
