using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVDeclarationModule))]
	public class CusUSLVDeclarationModuleTest : ZModuleBasherTest
	{
		public void TestOverrideProperties()
		{
			using (var module = new CusUSLVDeclarationModule())
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				CombineAssertions(() =>
				{
					AssertEquals(ControllerIDs.Customs.US.USLowValueEntriesDeclaration, module.GetNewController().ID);
					AssertEquals(ModuleIDs.Customs.US.USLowValueEntriesDeclaration, module.ID);
					AssertEquals(true, module.SupportsWorkflow);
					AssertEquals(Env.Security.USLVConsignment, module.SecurityCheckpoint);
					AssertType<CusUSLVConsignmentFilterControl>(filterControl);
				});
			}
		}

		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert("The module doesn't have an enquiry screen, nothing is grid loaded.", true);
		}

		public override void TestModuleShowsAndCanSearch()
		{
			Assert("This module is not yet visible and no searches can be performed.", true);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USLowValueEntriesDeclaration;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
