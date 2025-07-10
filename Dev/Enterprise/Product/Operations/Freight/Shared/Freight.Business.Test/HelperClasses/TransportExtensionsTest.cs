using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Test
{
	class TransportExtensionsTest : TestCaseWithFactory
	{
		public void TestGetScreeningPartiesFromVessel_TransportMode_Sea()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Name = "DUMMY_VESSEL";

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "DUMMY_ORG";
			refVessel.RV_OH = organization.PK;

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = refVessel.RV_Name;

			var parties = transport.GetScreeningPartiesFromVessel(consol);
			AssertContainsExactElementsInAnyOrder("Linked vessel", new BusinessObject[] { refVessel, organization }, parties.Select(u => u.ScreeningEntity));
			Assert("Consol added to parent", parties.First().Parents.Contains(consol));

			refVessel.RV_Name = ZString.Empty;
			transport.JW_Vessel = ZString.Empty;
			AssertEquals("Not include empty name vessel", 0, transport.GetScreeningPartiesFromVessel(consol).Count());

			transport.JW_Vessel = "DUMMY_User_Entered";
			AssertContainsExactElementsInAnyOrder("Not linked vessel", new[] { transport }, transport.GetScreeningPartiesFromVessel(consol).Select(u => u.ScreeningEntity));
		}

		public void TestGetScreeningPartiesFromVessel_TransportMode_InlandWaterwayTransport()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Name = "DUMMY_VESSEL";

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "DUMMY_ORG";
			refVessel.RV_OH = organization.PK;

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			transport.JW_Vessel = refVessel.RV_Name;
			AssertEquals("Not include vessel when trasport mode is not SEA and allow InlandWaterwayTransport is false", 0, transport.GetScreeningPartiesFromVessel(consol).Count());
			AssertEquals("Include vessel when trasport mode is IWT and allow InlandWaterwayTransport is true", 2, transport.GetScreeningPartiesFromVessel(consol, allowInlandWaterwayTransport: true).Count());
		}

		public void TestGetScreeningPartiesFromVessel_TransportMode_Not_Sea()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Name = "DUMMY_VESSEL";

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "DUMMY_ORG";
			refVessel.RV_OH = organization.PK;

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_Vessel = refVessel.RV_Name;
			AssertEquals("Not include vessel when trasport mode is not SEA", 0, transport.GetScreeningPartiesFromVessel(consol).Count());
		}
	}
}
