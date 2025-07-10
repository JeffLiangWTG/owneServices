using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccComplianceSequenceModule))]
	sealed class AccComplianceSequenceModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccComplianceSequence;
		}

		protected override string CountryCode
		{
			get { return "PE"; }
		}

		public void TestCheckpoints()
		{
			using (AccComplianceSequenceModule module = new AccComplianceSequenceModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ComplianceSequences, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewActionMenuItems()
		{
			using (var module = new AccComplianceSequenceModule())
			{
				var menuItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Bulk Create Compliance Sequence Books");
				AssertNotNull(menuItem);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccComplianceSequenceModuleForTest module = new AccComplianceSequenceModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccComplianceSequenceFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccComplianceSequenceModuleForTest module = new AccComplianceSequenceModuleForTest())
			{
				IBusinessObjectCollection complianceSequencesCollection = module.NewGridCollection;
				Assert("Invalid type", complianceSequencesCollection is AccComplianceSequenceCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccComplianceSequenceModuleForTest module = new AccComplianceSequenceModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccComplianceSequenceFilterBusinessObject);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (AccComplianceSequenceModuleForTest module = new AccComplianceSequenceModuleForTest())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Deactivate"));
			}
		}

		public void TestSupportsWorkflow()
		{
			using (AccComplianceSequenceModuleForTest module = new AccComplianceSequenceModuleForTest())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#endregion
	}
}
