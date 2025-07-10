using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Module.Testing
{
	[TestedType(typeof(ETerminalReleaseManifestPortMessagingController))]
	sealed class ETerminalReleaseManifestPortMessagingControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn_VoyageIsSea_ReturnPlugIn()
		{
			var controller = new ETerminalReleaseManifestPortMessagingControllerForTesting();
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			using (var plugin = controller.GetPlugIn(voyage))
			{
				AssertType<ETerminalReleaseManifestPortMessagingPlugin>(plugin);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ETerminalReleaseManifestPortMessaging;
		}

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
		}

		ZString StoredCountry;

		class ETerminalReleaseManifestPortMessagingControllerForTesting : ETerminalReleaseManifestPortMessagingController
		{
			public new ZPlugIn GetPlugIn(IBusiness businessEntity)
			{
				return base.GetPlugIn(businessEntity);
			}
		}
	}
}
