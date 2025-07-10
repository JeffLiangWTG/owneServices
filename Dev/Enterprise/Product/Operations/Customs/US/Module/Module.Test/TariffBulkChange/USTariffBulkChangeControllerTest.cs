using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USTariffBulkChangeController))]
	sealed class USTariffBulkChangeControllerTest : ZSingletonControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USTariffBulkChange;
	}
}
