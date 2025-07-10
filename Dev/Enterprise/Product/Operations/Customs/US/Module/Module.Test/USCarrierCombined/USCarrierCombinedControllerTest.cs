using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCarrierCombinedController))]
	sealed class USCarrierCombinedControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(USCarrierCombinedController);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.USCarrierCombined;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "#@!@";
			Factory.Save();
			return carrier;
		}
	}
}
