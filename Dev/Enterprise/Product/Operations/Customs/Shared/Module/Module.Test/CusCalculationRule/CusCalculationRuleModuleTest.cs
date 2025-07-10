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
	[TestedType(typeof(CusCalculationRulesModule))]
	public abstract class CusCalculationRulesModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				using (var control = module.GetNewFilterControl())
				{
					AssertType<CusCalculationRulesFilterControl>(control);
				}
			}
		}

		public virtual void TestGetNewFilterBusinessObject()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertType<CusCalculationRulesFilterBusinessObject>(module.GetNewFilterBusinessObject());
			}
		}

		public virtual void TestGetNewGridCollection()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertType<CusCalculationRuleCollection<CusCalculationRule>>(module.GetNewGridCollection());
			}
		}

		public virtual void TestGetNewController()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertType<CusCalculationRulesController>(module.GetNewController());
			}
		}

		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new CusCalculationRulesModuleForTest())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.Broker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.CusCalculationRules, module.SecurityCheckpoint);
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

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CusCalculationRules;

		protected override bool HasController() => true;

		sealed class CusCalculationRulesModuleForTest : CusCalculationRulesModule
		{
			public new IFilterControl GetNewFilterControl() => base.GetNewFilterControl();
			public new FilterBusinessObject GetNewFilterBusinessObject() => base.GetNewFilterBusinessObject();
			public new IBusinessObjectCollection GetNewGridCollection() => base.GetNewGridCollection();
		}
	}
}
