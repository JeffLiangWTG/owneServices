using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.US.Module.Testing
{
	[CargoWise.Data.Testing.UseSnapshotProtection]
	abstract class NumberModuleTest : ZPopupModuleBasherTest
	{
		public void TestController()
		{
			using (var module = (NumberModule)ZModuleFactory.Instance.Create(ExpectedModuleID))
			{
				module.Show();
				var lastController = ((IPopupModuleInternalsForTesting)module).LastController;
				AssertEquals(ExpectedControllerType, lastController.GetType());
				if (lastController.LastShownForm != null)
				{
					lastController.LastShownForm.Dispose();
				}
			}
		}

		public virtual void TestLicenceCheckPoint()
		{
			using (var module = (NumberModule)ZModuleFactory.Instance.Create(ExpectedModuleID))
			{
				module.Show();
				AssertEquals("LicenceCheckPoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				module.CloseFormForTestingOnly();
			}
		}

		protected abstract ModuleIdentifier ExpectedModuleID { get; }

		protected abstract Type ExpectedControllerType { get; }

		protected override ModuleIdentifier GetModuleID() => ExpectedModuleID;

		protected override Form GetFormToBashCore(ZPopupModule module)
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			return base.GetFormToBashCore(module);
		}
	}
}
