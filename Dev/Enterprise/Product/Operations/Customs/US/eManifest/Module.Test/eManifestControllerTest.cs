using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestController))]
	sealed class eManifestControllerBasherTest : ZControllerBasherTest
	{
		public void TestEditFormWhenInActive()
		{
			var trip = Factory.New<Trip>();
			trip.BH_IsActive = false;
			Factory.Save();
			using (var form = Controller.ShowEditForm(trip))
			{
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.eManifest;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			return trip;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
