using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(FlightDetailCollection))]
	class FlightDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FlightDetailCollection>
	{
		public void TestInitialiseFlightArrival_FromHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var flightDetailCollection = new FlightDetailCollection(header);
			AssertEquals(0, flightDetailCollection.Count);
			flightDetailCollection.InitialiseFlightArrival();
			AssertEquals(1, flightDetailCollection.Count);
			var flightDetail = flightDetailCollection[0];
			AssertEquals("VOG1234A", flightDetail.FlightNo);
			AssertEquals("2020-07-25", flightDetail.FlightArrivalDate.ToISO8601ShortDateString());
			AssertEquals("", flightDetail.FlightReference);
			Assert(flightDetail.Selected);
			Assert(!flightDetail.IsArrival);
		}

		public void TestInitialiseFlightArrival_FromArrival()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var arrival = header.ArrivalHeaders.AddNew();
			arrival.ATH_VoyageFlightNo = "QF123";
			arrival.ATH_ETAAtDischargePort = new ZDateTime(2021, 8, 24, 04, 20, 00, 00);
			arrival.ATH_Reference = "A";
			var flightDetailCollection = new FlightDetailCollection(header);
			AssertEquals(0, flightDetailCollection.Count);
			flightDetailCollection.InitialiseFlightArrival();
			AssertEquals(1, flightDetailCollection.Count);
			var flightDetail = flightDetailCollection[0];
			AssertEquals("QF123", flightDetail.FlightNo);
			AssertEquals("2021-08-24", flightDetail.FlightArrivalDate.ToISO8601ShortDateString());
			AssertEquals("A", flightDetail.FlightReference);
			Assert(flightDetail.Selected);
			Assert(flightDetail.IsArrival);
		}

		public void TestInitialiseFlightArrival_FromArrivals()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var arrival1 = header.ArrivalHeaders.AddNew();
			arrival1.ATH_VoyageFlightNo = "QF123";
			arrival1.ATH_ETAAtDischargePort = new ZDateTime(2021, 8, 24, 04, 20, 00, 00);
			arrival1.ATH_Reference = "A";
			var arrival2 = header.ArrivalHeaders.AddNew();
			arrival2.ATH_VoyageFlightNo = "081001";
			arrival2.ATH_ETAAtDischargePort = new ZDateTime(2021, 9, 23, 02, 15, 00, 00);
			arrival2.ATH_Reference = "B";
			var flightDetailCollection = new FlightDetailCollection(header);
			flightDetailCollection.InitialiseFlightArrival();
			AssertEquals(2, flightDetailCollection.Count);
			var flightDetail1 = flightDetailCollection[0];
			AssertEquals("QF123", flightDetail1.FlightNo);
			AssertEquals("2021-08-24", flightDetail1.FlightArrivalDate.ToISO8601ShortDateString());
			AssertEquals("A", flightDetail1.FlightReference);
			Assert(!flightDetail1.Selected);
			Assert(flightDetail1.IsArrival);
			var flightDetail2 = flightDetailCollection[1];
			AssertEquals("081001", flightDetail2.FlightNo);
			AssertEquals("2021-09-23", flightDetail2.FlightArrivalDate.ToISO8601ShortDateString());
			AssertEquals("B", flightDetail2.FlightReference);
			Assert(!flightDetail2.Selected);
			Assert(flightDetail2.IsArrival);
		}

		public void TestGetSelectedFlights()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			header.AMA_E_ARV = new ZDateTime(2020, 7, 25, 06, 30, 00, 00);
			var arrival1 = header.ArrivalHeaders.AddNew();
			arrival1.ATH_VoyageFlightNo = "QF123";
			arrival1.ATH_ETAAtDischargePort = new ZDateTime(2021, 8, 24, 04, 20, 00, 00);
			arrival1.ATH_Reference = "A";
			var arrival2 = header.ArrivalHeaders.AddNew();
			arrival2.ATH_VoyageFlightNo = "081001";
			arrival2.ATH_ETAAtDischargePort = new ZDateTime(2021, 9, 23, 02, 15, 00, 00);
			arrival2.ATH_Reference = "B";
			var flightDetailCollection = new FlightDetailCollection(header);
			flightDetailCollection.InitialiseFlightArrival();
			AssertEquals(2, flightDetailCollection.Count);
			var flightDetail1 = flightDetailCollection[0];
			Assert(!flightDetail1.Selected);
			var flightDetail2 = flightDetailCollection[1];
			Assert(!flightDetail2.Selected);
			AssertEquals(0, flightDetailCollection.GetSelectedFlights().Count());
			flightDetail1.Selected = true;
			AssertEquals(1, flightDetailCollection.GetSelectedFlights().Count());
			flightDetail2.Selected = true;
			AssertEquals(2, flightDetailCollection.GetSelectedFlights().Count());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FlightDetail(Factory);
		}

		protected override FlightDetailCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new FlightDetailCollection(header);
		}
	}
}
