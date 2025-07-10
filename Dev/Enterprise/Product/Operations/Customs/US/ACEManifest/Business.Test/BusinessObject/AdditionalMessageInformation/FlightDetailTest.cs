using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(FlightDetail))]
	class FlightDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReadOnlyProperties()
		{
			var flightDetail = new FlightDetail(Factory);
			Assert(flightDetail.FlightNo_ReadOnly);
			Assert(flightDetail.FlightArrivalDate_ReadOnly);
			Assert(flightDetail.FlightReference_ReadOnly);
		}

		public void TestValidateSelected()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			header.AMA_MasterBill = "30102020012";
			header.AMA_Voyage = "VOG1234A";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			var flights = additionalMessageInformation.FlightArrivalDetails;
			AssertEquals(1, flights.Count);
			var flight = flights[0];
			AssertEquals("VOG1234A", flight.FlightNo);
			AssertNoError(flight.SelectedInfo, "More than 1 Arrival is selected");
			var flight2 = flights.AddNew();
			flight2.RunPreSaveValidation();
			AssertNoError(flight2.SelectedInfo, "More than 1 Arrival is selected");
			flight2.Selected = true;
			AssertHasError(flight2.SelectedInfo, "More than 1 Arrival is selected");
			flight2.Selected = false;
			AssertNoError(flight2.SelectedInfo, "More than 1 Arrival is selected");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FlightDetail(Factory);
		}
	}
}
