using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingConsolManyToManyCollection))]
	public class TrackingConsolManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			originalGlobalsIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		bool originalGlobalsIsWeb;

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
			Globals.IsWeb = originalGlobalsIsWeb;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			Factory.Save();
			return shipment.Consols;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingConsolManyToManyCollection), GetCollectionToTest().GetType());
		}

		public void TestLoadSetsSuppressFlightDetails()
		{
			TrackingConsolManyToManyCollection collection = (TrackingConsolManyToManyCollection)GetCollectionToTest();
			TrackingConsol consol1 = (TrackingConsol)collection.ParentShipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";

			Transport transport1 = consol1.Transports[0];
			transport1.JW_VoyageFlight = "ABC";
			transport1.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;

			TrackingConsol consol2 = (TrackingConsol)collection.ParentShipment.Consols.AddNew();
			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "USNYC";
			transport2.JW_VoyageFlight = "XYZ";

			AssertEquals(false, Suppression.EnabledForAnyOfFieldsWeb(consol1.FlightDetailsSuppressionBizO, new[] { SuppressFields.MasterBill }));
			AssertEquals(false, Suppression.EnabledForAnyOfFieldsWeb(consol2.FlightDetailsSuppressionBizO, new[] { SuppressFields.MasterBill }));

			collection.ParentShipment.JS_RL_NKOrigin = "AUMEL";
			collection.ParentShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			collection.ParentShipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			ZDateTime testDate = ZDateTime.Today.AddDays(10);

			transport1.JW_ETD = testDate;
			transport2.JW_ETD = testDate.AddDays(-5);
			collection.ParentShipment.JS_E_DEP = testDate;
			AssertEquals("Shipment should not suppress flight details", false, Suppression.EnabledForAnyOfFieldsWeb(collection.ParentShipment, new[] { SuppressFields.MasterBill }));

			AssertSame("Consol1 is the departure consol", consol1, collection.ParentShipment.DepartureConsol);

			collection.Load();

			AssertEquals("Should be set to false", false, Suppression.EnabledForAnyOfFieldsWeb(consol1.FlightDetailsSuppressionBizO, new[] { SuppressFields.MasterBill }));
			AssertEquals("Should be set to false", false, Suppression.EnabledForAnyOfFieldsWeb(consol2.FlightDetailsSuppressionBizO, new[] { SuppressFields.MasterBill }));

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);

			AssertEquals("Should be set to true", true, Suppression.EnabledForAnyOfFieldsWeb(consol1.FlightDetailsSuppressionBizO, new[] { SuppressFields.MasterBill }));
		}
	}
}
