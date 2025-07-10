using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GuaranteesModule))]
	sealed class GuaranteesModuleTest : ZModuleBasherTest
	{
		public void TestGetNewFilterControl()
		{
			using (var module = new GuaranteesModuleForTest())
			{
				using (var control = module.GetNewFilterControl())
				{
					AssertType<GuaranteesFilterControl>(control);
				}
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new GuaranteesModuleForTest())
			{
				AssertType<GuaranteesFilterStripBusinessObject>(module.GetNewFilterBusinessObject());
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new GuaranteesModuleForTest())
			{
				AssertType<CusGuaranteeHeaderCollection>(module.GetNewGridCollection());
			}
		}

		public void TestGetNewController()
		{
			using (var module = new GuaranteesModuleForTest())
			{
				AssertType<GuaranteesController>(module.GetNewController());
			}
		}

		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new GuaranteesModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.Broker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.Guarantees, module.SecurityCheckpoint);
			}
		}

		public void TestGuaranteesModuleAllows()
		{
			using (var module = new GuaranteesModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
				AssertEquals("module.AllowView", true, module.AllowView);
				AssertEquals("module.AllowUniversalCopy", false, module.AllowUniversalCopy);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.Guarantees;

		protected override bool HasController() => true;

		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		sealed class GuaranteesModuleForTest : GuaranteesModule
		{
			public new IFilterControl GetNewFilterControl() => base.GetNewFilterControl();
			public new FilterBusinessObject GetNewFilterBusinessObject() => base.GetNewFilterBusinessObject();
			public new IBusinessObjectCollection GetNewGridCollection() => base.GetNewGridCollection();
		}
	}
}
