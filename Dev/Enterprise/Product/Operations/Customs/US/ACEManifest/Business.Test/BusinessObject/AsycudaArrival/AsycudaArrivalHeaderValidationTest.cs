using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaArrivalHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckATH_VoyageFlightNo()
		{
			// validation reports flight number mismatch.  Input must meet (2-3AN)+(3N(N)(A))
			// does not show error for <CarrierCode>N or <CarrierCode>NN because these can be converted to 2AN+3N by prefixing zeros
			var flightNumberIsWrongLengthError = ValidationConstants.FlightNumberIsWrongLength;
			var flightNumberIsNonCompliantError = ValidationConstants.FlightNumberIsNonCompliant;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrival = header.ArrivalHeaders.AddNew();
			arrival.ATH_VoyageFlightNo = "Q";
			AssertHasMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsWrongLengthError);
			header.AMA_CarrierCode = "Q1";
			arrival.ATH_VoyageFlightNo = "Q123"; // Q1 is an indonesian airline carrier code, 23 will be converted to 023 in message.
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsWrongLengthError);
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "QF";
			arrival.ATH_VoyageFlightNo = "QF";
			AssertHasMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsWrongLengthError);
			arrival.ATH_VoyageFlightNo = "QF1";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsWrongLengthError);
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "QF01";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "";
			arrival.ATH_VoyageFlightNo = "QF001";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "081001";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "08101";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "0811";
			AssertHasMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "081";
			AssertHasMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "081";
			arrival.ATH_VoyageFlightNo = "08101";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "0811";
			AssertNoMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			arrival.ATH_VoyageFlightNo = "081";
			AssertHasMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
			header.AMA_CarrierCode = "QUAN";
			arrival.ATH_VoyageFlightNo = "QUAN01";
			AssertHasMessageErrorContaining(arrival.ATH_VoyageFlightNoInfo, flightNumberIsNonCompliantError);
		}

		public void TestFlightDetailsAreNotDuplicated()
		{
			var flightNumberIsDuplicated = ValidationConstants.FlightNumberIsDuplicated;
			var flightNo = "UA001";
			var flightDate = ZDateTime.Today;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = flightNo;
			arrivalHeader.ATH_ETAAtDischargePort = flightDate;
			Factory.Save();
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_VoyageFlightNo = flightNo;
			arrivalHeader2.ATH_ETAAtDischargePort = flightDate;
			AssertHasError(arrivalHeader2.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			arrivalHeader2.ATH_VoyageFlightNo = "UA010";
			AssertNoError(arrivalHeader2.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
		}

		public void TestDuplicateFlightsAlreadyInDatabaseAreAllowed()
		{
			var flightNumberIsDuplicated = ValidationConstants.FlightNumberIsDuplicated;
			var flightNo = "UA001";
			var flightDate = ZDateTime.Today;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = flightNo;
			arrivalHeader1.ATH_ETAAtDischargePort = flightDate;
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_ETAAtDischargePort = flightDate;
			Factory.Save();
			// Dummy up second arrival to match existing arrival in the db
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			var arrivalUpdateSql = string.Format("UPDATE dbo.AsycudaArrivalHeader SET ATH_VoyageFlightNo = '{0}', ATH_SystemLastEditTimeUtc = GETUTCDATE(), ATH_SystemLastEditUser = '{1}' WHERE ATH_PK = '{2}'", flightNo, currentUser, arrivalHeader2.PK.ToString());
			Db.Connection.ExecuteScalar(arrivalUpdateSql); // Required to have duplicate values in db for test case.
			var newArrivalHeader = header.ArrivalHeaders.AddNew();
			newArrivalHeader.ATH_VoyageFlightNo = flightNo;
			newArrivalHeader.ATH_ETAAtDischargePort = flightDate;
			AssertHasError("This is a newly entered duplicate arrival, give error message", newArrivalHeader.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			AssertNoError("This arrival is already in the db, do not give error message", arrivalHeader1.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			AssertNoError("This arrival is also already in the db, so it will not give an error message", arrivalHeader2.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
		}

		public void TestFlightsAlreadyInDatabaseCannotBeChangedToDuplicate()
		{
			var flightNumberIsDuplicated = ValidationConstants.FlightNumberIsDuplicated;
			var flightNo = "UA001";
			var flightDate = ZDateTime.Today;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader1 = header.ArrivalHeaders.AddNew();
			arrivalHeader1.ATH_VoyageFlightNo = flightNo;
			arrivalHeader1.ATH_ETAAtDischargePort = flightDate;
			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			arrivalHeader2.ATH_ETAAtDischargePort = flightDate;
			var arrivalHeader3 = header.ArrivalHeaders.AddNew();
			arrivalHeader3.ATH_VoyageFlightNo = "UA100";
			arrivalHeader3.ATH_ETAAtDischargePort = flightDate;
			Factory.Save();
			// Dummy up second arrival to match existing arrival in the db
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			var arrivalUpdateSql = string.Format("UPDATE dbo.AsycudaArrivalHeader SET ATH_VoyageFlightNo = '{0}', ATH_SystemLastEditTimeUtc = GETUTCDATE(), ATH_SystemLastEditUser = '{1}' WHERE ATH_PK = '{2}'", flightNo, currentUser, arrivalHeader2.PK.ToString());
			Db.Connection.ExecuteScalar(arrivalUpdateSql); // Required to have duplicate values in db for test case.
			var newArrivalHeader = header.ArrivalHeaders.AddNew();
			newArrivalHeader.ATH_VoyageFlightNo = flightNo;
			newArrivalHeader.ATH_ETAAtDischargePort = flightDate;
			AssertHasError("This is a newly entered duplicate arrival, give error message", newArrivalHeader.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			AssertNoError("This arrival is already in the db, do not give error message", arrivalHeader1.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			AssertNoError("This arrival is also already in the db, so it will not give an error message", arrivalHeader2.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			AssertNoError("This arrival is not a duplicate", arrivalHeader3.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
			arrivalHeader3.ATH_VoyageFlightNo = flightNo;
			AssertHasError("This arrival is already in the db, but is being changed and will now be a duplicate, so error.", arrivalHeader3.ATH_VoyageFlightNoInfo, flightNumberIsDuplicated);
		}
	}
}
