using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(USAMSModule))]
	sealed class USAMSModuleTest : ZModuleBasherTest
	{
		class USAMSModuleForTest : USAMSModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessOject)
			{
				return base.GetNewController(selectedBusinessOject);
			}
		}

		public void TestGetNewController()
		{
			using (var module = new USAMSModuleForTest())
			{
				var controller1 = module.GetNewController(AMS) as CusInBondHeaderController;
				AssertNotNull(controller1);
				var consol = Factory.New<ForwardingConsol>();
				AMS.BH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				AMS.BH_ParentID = consol.PK;
				var controller2 = module.GetNewController(AMS) as CusInBondHeaderConsolController;
				AssertNotNull(controller2);
			}
		}

		public void TestNewMenuItems()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			using (var module = new USAMSModule())
			{
				AssertNotNull(module.FormActionMenu);
				var menuItem = module.FormActionMenu.FindByText("New VOCC");
				menuItem.PerformClick();
				var controller = ((IFilterModuleInternalsForTesting)module).LastController;
				using (var form = (USAMSForm)controller.LastShownForm)
				{
					AssertEquals("form.BusinessEntity.IsNVOCCHeader", false, form.BusinessEntity.IsNVOCCHeader);
				}

				menuItem = module.FormActionMenu.FindByText("New NVOCC");
				menuItem.PerformClick();
				controller = ((IFilterModuleInternalsForTesting)module).LastController;
				using (var form = (USAMSForm)controller.LastShownForm)
				{
					AssertEquals("form.BusinessEntity.IsNVOCCHeader", true, form.BusinessEntity.IsNVOCCHeader);
				}
			}
		}

		public void TestSupportsWorkflow()
		{
			using (var module = new USAMSModule())
			{
				AssertEquals("WorkflowType", true, module.SupportsWorkflow);
				AssertEquals("WorkflowType", "AMS", module.WorkflowType);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.US.AMS;
		}

		protected override bool HasController()
		{
			return true;
		}

		CusInBondHeader AMS
		{
			get
			{
				if (ams == null)
				{
					ams = Factory.New<CusInBondHeader>();
				}

				return ams;
			}
		}

		CusInBondHeader ams;
	}
}
