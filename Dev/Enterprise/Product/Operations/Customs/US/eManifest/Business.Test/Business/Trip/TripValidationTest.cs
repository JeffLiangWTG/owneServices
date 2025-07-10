using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripValidationTest : BusinessObjectValidationTestCase
	{
		public void TestClientIsMandatory()
		{
			trip.BH_OA_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(trip.BH_OA_ImporterInfo, "enter");
			trip.BH_OA_Importer = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertNoMessageErrorContaining(trip.BH_OA_ImporterInfo, "enter");
		}

		public void TestCheckBH_CarrierSCAC()
		{
			var validCode = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, Trip.Truck)).UI_Code;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(trip.BH_CarrierSCACInfo, "??", validCode);
			const string warning = "There should be an organization with the carrier SCAC specified in order to print CBP Form 7533.";
			trip.BH_CarrierSCAC = validCode;
			AssertHasWarning(trip.BH_CarrierSCACInfo, warning);
			trip.BH_CarrierSCAC = ZString.Empty;
			AssertNoWarning(trip.BH_CarrierSCACInfo, warning);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, validCode, Core.Constants.CountryCodes.UnitedStates);
			trip.BH_CarrierSCAC = validCode;
			AssertNoWarning(trip.BH_CarrierSCACInfo, warning);
		}

		public void TestCheckBH_ETA()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(trip.BH_ETAInfo);
			trip.BH_ETA = ZDateTime.Today.AddDays(-10);
			const string error = "Estimated Date of Arrival cannot be a past date.";
			const string warning = "e-Manifest should be lodged at least 30 mins before arrival.";
			AssertHasError(trip.BH_ETAInfo, error);
			AssertNoWarning(trip.BH_ETAInfo, warning);
			trip.BH_ETA = ZDateTime.Now.AddMinutes(20);
			AssertNoError(trip.BH_ETAInfo, error);
			AssertHasWarning(trip.BH_ETAInfo, warning);
			trip.BH_ETA = ZDateTime.Now.AddMinutes(40);
			AssertNoError(trip.BH_ETAInfo, error);
			AssertNoWarning(trip.BH_ETAInfo, warning);
			trip.BH_ETA = ZDateTime.Today.AddDays(-10);
			Factory.Save();
			trip.Validation.ValidateBH_ETA();
			AssertNoError(trip.BH_ETAInfo, error);
			AssertNoWarning(trip.BH_ETAInfo, warning);
			trip.BH_ETA = ZDateTime.Today.AddDays(-5);
			AssertHasError(trip.BH_ETAInfo, error);
			AssertNoWarning(trip.BH_ETAInfo, warning);
			trip.BH_ETA = ZDateTime.Now.AddMinutes(20);
			AssertNoError(trip.BH_ETAInfo, error);
			AssertHasWarning(trip.BH_ETAInfo, warning);
		}

		public void TestCheckBH_ImportTransportMode()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(trip.BH_ImportTransportModeInfo, "??", TransportModes.Codes.Road);
		}

		public void TestCheckAtLeastResponsiblePartyEntered()
		{
			const string messageError = @"Responsible party is required on the Crew tab.
The responsible party may also be the driver, passenger, or crew member.
If this is the case, use RP in this data element to report that person.";
			trip.Validation.ValidateBH_JobReference();
			AssertNoMessageError("No child collections should be validated if validate all has not been run", trip.BH_JobReferenceInfo, messageError);
			trip.Validation.ValidateAll();
			AssertHasMessageError("Should validate as far as validate all has been run", trip.BH_JobReferenceInfo, messageError);
			trip.CrewMembers.AddNew().CP_Type = CrewTypes.Codes.CrewMember;
			trip.Validation.ValidateBH_JobReference();
			AssertHasMessageError("Crew member is not responsible party", trip.BH_JobReferenceInfo, messageError);
			trip.CrewMembers.AddNew().CP_Type = CrewTypes.Codes.ResponsibleParty;
			trip.Validation.ValidateBH_JobReference();
			AssertNoMessageError("Responsible party entered", trip.BH_JobReferenceInfo, messageError);
		}

		public void TestCheckBH_PortUnladingDCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var validCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "Test Name", startDate, endDate);
			newFactory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(trip.BH_PortUnladingDCodeInfo, "????", validCode.ZZD_Code);
		}

		public void TestCheckBH_RL_NKPortUnlading()
		{
			var validCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).RL_Code;
			ValidationTestHelper.AssertInvalidCodeMessageError(trip.BH_RL_NKPortUnladingInfo, "????", validCode);
		}

		public void TestCheckBH_TransitDirection()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(trip.BH_TransitDirectionInfo, "?", TransitDirectionCodes.Codes.Importation);
		}

		public void TestValidateAll()
		{
			using (trip.SuspendValidationTesting())
			{
				trip.BH_TransitDirection = "?";
				trip.ClearAllNotifications();
				AssertNoNotifications(trip.BH_TransitDirectionInfo);
				trip.Validation.ValidateAll();
				AssertHasNotifications(trip.BH_TransitDirectionInfo);
			}
		}

		public void TestValidateTripReferenceVoyageNumber()
		{
			AssertNoMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "ABC";
			AssertNoMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "X";
			AssertHasMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "1234567890";
			AssertNoMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "ABCDEFGHIJ";
			AssertNoMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "ABCDEFG123";
			AssertNoMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "ABCDEFG 12";
			AssertHasMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "ABCDEFG*12";
			AssertHasMessageErrorContaining(trip.BH_VoyageNumberInfo, "2-10 alphanumeric");
			trip.BH_VoyageNumber = "ABCDEFG123";
			trip.BH_JobReference = "Original";
			trip.BH_ETA = new ZDateTime(2017, 12, 05);
			var duplicateTrip = (Trip)trip.Clone();
			duplicateTrip.BH_VoyageNumber = "ABCDEFG123";
			duplicateTrip.BH_ETA = new ZDateTime(2016, 12, 05);
			AssertHasMessageErrorContaining(duplicateTrip.BH_VoyageNumberInfo, "already in use on job Original");
			duplicateTrip.BH_ETA = new ZDateTime(2018, 12, 05);
			AssertHasMessageErrorContaining(duplicateTrip.BH_VoyageNumberInfo, "already in use on job Original");
			duplicateTrip.BH_VoyageNumber = "New value";
			AssertNoMessageErrorContaining(duplicateTrip.BH_VoyageNumberInfo, "already in use on job Original");
			duplicateTrip.BH_ETA = new ZDateTime(2018, 12, 06);
			duplicateTrip.BH_VoyageNumber = "ABCDEFG123";
			AssertNoMessageErrorContaining(duplicateTrip.BH_VoyageNumberInfo, "already in use on job Original");
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip = Factory.New<Trip>();
		}

		Trip trip;
	}
}
