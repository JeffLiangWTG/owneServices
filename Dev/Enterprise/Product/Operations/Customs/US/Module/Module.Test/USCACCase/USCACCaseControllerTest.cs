using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCACCaseController))]
	sealed class USCACCaseControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.US.USCACCase, new USCACCaseController().ModuleID);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USCACCase;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
