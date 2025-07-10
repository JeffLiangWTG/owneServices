using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVClearanceModule))]
	public class CusUSLVClearanceModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USLowValueEntries;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		public void TestOverrides()
		{
			using (var module = new CusUSLVClearanceModule())
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				CombineAssertions(() =>
				{
					AssertEquals(ControllerIDs.Customs.US.USLowValueEntries, module.GetNewController().ID);
					AssertEquals(ModuleIDs.Customs.US.USLowValueEntries, module.ID);
					AssertEquals(Env.Licence.CoreCustomsModule, module.LicenceCheckPoint);
					AssertEquals(WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode, module.WorkflowType);
					AssertEquals(false, module.AllowUniversalCopy);
					AssertType<CusUSLVClearanceFilterBusinessObject>(module.FilterBusinessObject);
					AssertType<CusUSLVClearanceFilterControl>(filterControl);
					Assert(module.SupportsWorkflow);
				});
			}
		}

		public void TestModuleDescription()
		{
			using (var module = new CusUSLVClearanceModule())
			{
				AssertEquals("Low Value Entries", module.ID.Description);
			}
		}
	}
}
