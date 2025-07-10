using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCRuleController))]
	sealed class USCRuleControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(USCRuleController);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USCRule;
	}
}
