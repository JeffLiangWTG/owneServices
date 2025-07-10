using System;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVOriginLoadListValidationTest : BusinessObjectValidationTestCase
	{
		#region HVL_OA_OriginDepot

		public void TestValidate_OriginDepot()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = ZGuid.Empty;

			AssertHasError(loadList.HVL_OA_OriginDepotInfo, "Please enter an Origin Depot.");

			loadList.HVL_OA_OriginDepot = originDepot.PK;
			AssertNoErrors(loadList.HVL_OA_OriginDepotInfo);
		}

		#endregion

		#region HVL_OA_DestinationDepot

		public void TestValidate_DestinationDepot()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = ZGuid.Empty;

			AssertHasError(loadList.HVL_OA_DestinationDepotInfo, "Please enter a Destination Depot.");

			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			AssertNoErrors(loadList.HVL_OA_DestinationDepotInfo);
		}

		#endregion

		#region HVL_RL_NKOrigin

		public void TestValidateHVL_RL_NKOrigin()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();

			loadList.Validation.ValidateHVL_RL_NKOrigin();
			AssertHasError(loadList.HVL_RL_NKOriginInfo, "Please enter an Origin Port.");

			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.Validation.ValidateHVL_RL_NKOrigin();
			AssertNoErrors(loadList.HVL_RL_NKOriginInfo);
		}

		#endregion

		#region HVL_RL_NKDestination

		public void TestValidateHVL_RL_NKDestination()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();

			loadList.Validation.ValidateHVL_RL_NKDestination();
			AssertHasError(loadList.HVL_RL_NKDestinationInfo, "Please enter a Destination Port.");

			loadList.HVL_RL_NKDestination = "AUSYD";
			loadList.Validation.ValidateHVL_RL_NKDestination();
			AssertNoErrors(loadList.HVL_RL_NKDestinationInfo);
		}

		#endregion

		#region HVL_TransportMode

		public void TestValidate_TransportMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;

			loadList.HVL_TransportMode = TransportModes.Air;
			AssertNoErrors(loadList.HVL_TransportModeInfo);

			loadList.HVL_TransportMode = ZString.Empty;
			AssertHasError(loadList.HVL_TransportModeInfo, "Please enter a Transport Mode.");
		}

		public void TestValidate_TransportMode_WhenInvalidCode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = "ABC";
			AssertHasError(loadList.HVL_TransportModeInfo, "Enter a valid Transport Mode.");

			loadList.HVL_TransportMode = TransportModes.Air;
			AssertNoErrors(loadList.HVL_TransportModeInfo);
		}

		#endregion

		#region HVL_OH_Carrier

		public void TestValidate_Carrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
			loadList.HVL_OH_Carrier = ZGuid.Empty;

			AssertHasError(loadList.HVL_OH_CarrierInfo, "Please enter a Carrier.");

			loadList.HVL_OH_Carrier = carrier.PK;
			AssertNoErrors(loadList.HVL_OH_CarrierInfo);
		}

		#endregion

		#region

		public void TestValidate_ServiceLevel()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
			loadList.HVL_RS_NKServiceLevel = ZString.Empty;

			AssertHasError(loadList.HVL_RS_NKServiceLevelInfo, "Please enter a Service Level.");
		}

		public void TestValidate_ServiceLevel_WhenInvalidCode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_RS_NKServiceLevel = "XXX";

			AssertHasError(loadList.HVL_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

			loadList.HVL_RS_NKServiceLevel = "DIR";
			AssertNoErrors(loadList.HVL_RS_NKServiceLevelInfo);
		}

		#endregion

		#region HVL_Status

		public void TestValidate_Status_WhenInvalidCode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = "XXX";

			AssertHasError(loadList.HVL_StatusInfo, "Enter a valid Status.");
		}

		public void TestWhenStatusIsOpenOrClosed_SomePropertiesAreNotMandatory()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			loadList.RunPreSaveValidation();

			CombineAssertions("Properties are mandatory when HVL_Status is Lodged", () =>
			{
				AssertHasErrors(loadList.HVL_TransportModeInfo);
				AssertHasErrors(loadList.HVL_MasterBillNumberInfo);
				AssertHasErrors(loadList.HVL_OH_CarrierInfo);
				AssertHasErrors(loadList.HVL_RS_NKServiceLevelInfo);
				AssertHasWarnings(loadList.HVL_RC_ContainerTypeInfo);
				AssertHasWarnings(loadList.HVL_ContainerNumberInfo);
			});

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.RunPreSaveValidation();

			CombineAssertions("Properties are not mandatory when HVL_Status is Open", () =>
			{
				AssertNoErrors(loadList.HVL_TransportModeInfo);
				AssertNoErrors(loadList.HVL_MasterBillNumberInfo);
				AssertNoErrors(loadList.HVL_OH_CarrierInfo);
				AssertNoErrors(loadList.HVL_RS_NKServiceLevelInfo);
				AssertNoWarnings(loadList.HVL_RC_ContainerTypeInfo);
				AssertNoWarnings(loadList.HVL_ContainerNumberInfo);
			});

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Closed;
			loadList.RunPreSaveValidation();

			CombineAssertions("Properties are not mandatory when HVL_Status is Closed", () =>
			{
				AssertNoErrors(loadList.HVL_TransportModeInfo);
				AssertNoErrors(loadList.HVL_MasterBillNumberInfo);
				AssertNoErrors(loadList.HVL_OH_CarrierInfo);
				AssertNoErrors(loadList.HVL_RS_NKServiceLevelInfo);
				AssertNoWarnings(loadList.HVL_RC_ContainerTypeInfo);
				AssertNoWarnings(loadList.HVL_ContainerNumberInfo);
			});
		}

		public void TestHVL_StatusHasOperationActionReadOnlyMemberAttribute()
		{
			AssertHasCustomAttribute<OperationActionReadOnlyMemberAttribute>(typeof(HVLVOriginLoadList), HVLVOriginLoadListSchema.Constants.HVL_Status, false, attribute => attribute.PropertyName == nameof(HVLVOriginLoadList.HVL_Status_OperationActionReadOnly));
		}

		public void TestGivenLoadListNotLodgedOrConsolidated_EditStatus()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.RunPreSaveValidation();

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.RunPreSaveValidation();

			AssertNoErrors(loadList.HVL_StatusInfo);
			Assert(loadList.HVL_Status == HVLVOriginLoadListStatus.Codes.Open);

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Pending;
			loadList.RunPreSaveValidation();

			AssertNoErrors(loadList.HVL_StatusInfo);
			Assert(loadList.HVL_Status == HVLVOriginLoadListStatus.Codes.Pending);

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Closed;
			loadList.RunPreSaveValidation();

			AssertNoErrors(loadList.HVL_StatusInfo);
			Assert(loadList.HVL_Status == HVLVOriginLoadListStatus.Codes.Closed);

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Failed;
			loadList.RunPreSaveValidation();

			AssertNoErrors(loadList.HVL_StatusInfo);
			Assert(loadList.HVL_Status == HVLVOriginLoadListStatus.Codes.Failed);
		}

		public void TestStatusCannotBeLDG_UnlessInTestingMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;

			AssertHasError(loadList.HVL_StatusInfo, "HVLV Origin Load List status cannot be updated to LDG.");

			using (HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
				loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
				AssertNoError("Should not add error when it's testing mode", loadList.HVL_StatusInfo, "HVLV Origin Load List status cannot be updated to LDG.");
			}
		}

		public void TestStatusCannotBeCON_UnlessInTestingMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;

			AssertHasError(loadList.HVL_StatusInfo, "HVLV Origin Load List status cannot be updated to CON.");

			using (HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
				loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
				AssertNoError("Should not add error when it's testing mode", loadList.HVL_StatusInfo, "HVLV Origin Load List status cannot be updated to CON.");
			}
		}

		#endregion

		#region HVL_MasterBillNumber

		public void TestValidate_MasterBill()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
			loadList.HVL_MasterBillNumber = ZString.Empty;

			AssertHasError(loadList.HVL_MasterBillNumberInfo, "Please enter a Master Bill/BOL.");
		}

		public void TestCheckHVL_MasterBillNumber_WhenAirModeAndMasterBillIsValid_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;

			loadList.HVL_MasterBillNumber = "12345678905";
			AssertNoNotifications("Valid master bill number, not expecting errors.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "12399999992";
			AssertNoNotifications("Valid master bill number, not expecting errors.", loadList.HVL_MasterBillNumberInfo);
		}

		public void TestCheckHVL_MasterBillNumber_WhenAirModeAndMasterBillIsInvalid_HasErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;

			loadList.HVL_MasterBillNumber = "123";
			AssertHasNotifications("The MAWB should contain 11 digits.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "XX";
			AssertHasNotifications("The MAWB should contain 11 digits.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "99999999999";
			AssertHasNotifications("Invalid check digit. The last digit should be '2'", loadList.HVL_MasterBillNumberInfo);
		}

		public void TestCheckHVL_MasterBillNumber_WhenNotAirModeAndMasterBillIsValid_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;

			loadList.HVL_MasterBillNumber = "Something";
			AssertNoNotifications("Valid master bill number, not expecting errors.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "123";
			AssertNoNotifications("Valid master bill number, not expecting errors.", loadList.HVL_MasterBillNumberInfo);
		}

		public void TestCheckHVL_MasterBillNumber_WhenNotAirModeAndMasterBillIsInvalid_HasErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;

			loadList.HVL_MasterBillNumber = "123,123";
			AssertHasNotifications("Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "123 123";
			AssertHasNotifications("Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "123:123";
			AssertHasNotifications("Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = "123;123";
			AssertHasNotifications("Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.", loadList.HVL_MasterBillNumberInfo);

			loadList.HVL_MasterBillNumber = " 123";
			AssertHasMessageError(loadList.HVL_MasterBillNumberInfo, "Ocean Bill should not contain leading spaces.");
		}

		public void TestCheckHVL_MasterBillNumber_WhenAvaliableNumberLessEqualsnLeftNumber_HasWarningMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var carrier = Factory.New<OrgHeader>();
			var cusCodes = carrier.CustomsCodes.AddNew();
			var orgFountains = carrier.OrgFountains.AddNew();
			carrier.OH_Code = "Org";
			cusCodes.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			cusCodes.OK_CustomsRegNo = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			orgFountains.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			orgFountains.SN_MaximumValue = 1;
			orgFountains.SN_MinimumValue = 1;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCodes.UnitedStates;
			loadList.HVL_RL_NKOrigin = CountryCodes.UnitedStates;
			loadList.HVL_OH_Carrier = carrier.PK;
			loadList.HVL_TransportMode = TransportModes.Road;

			loadList.HVL_MasterBillNumber = "12345678905";
			AssertHasWarning(loadList.HVL_MasterBillNumberInfo,
				"The BOL number range allocated by Forward Air is about to expire. You have 1 numbers left. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges.");
		}

		public void TestCheckHVL_MasterBillNumber_WhenAvaliableNumberGreaterThanLeftNumber_HasWarningMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var carrier = Factory.New<OrgHeader>();
			var cusCodes = carrier.CustomsCodes.AddNew();
			var orgFountains = carrier.OrgFountains.AddNew();
			carrier.OH_Code = "Org";
			cusCodes.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			cusCodes.OK_CustomsRegNo = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			orgFountains.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			orgFountains.SN_MaximumValue = 10000;
			orgFountains.SN_MinimumValue = 10000;
			orgFountains.SN_Count = 1;
			orgFountains.SN_Value = 10001;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCodes.UnitedStates;
			loadList.HVL_RL_NKOrigin = CountryCodes.UnitedStates;
			loadList.HVL_OH_Carrier = carrier.PK;
			loadList.HVL_TransportMode = TransportModes.Road;

			loadList.HVL_MasterBillNumber = "13205234585";
			AssertHasWarning(loadList.HVL_MasterBillNumberInfo,
				"The BOL number range allocated by Forward Air is to expire. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges.");
		}

		public void TestCheckHVL_MasterBillNumber_WhenReadOnly_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var carrier = Factory.New<OrgHeader>();
			var cusCodes = carrier.CustomsCodes.AddNew();
			var orgFountains = carrier.OrgFountains.AddNew();
			carrier.OH_Code = "Org";
			cusCodes.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			cusCodes.OK_CustomsRegNo = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			orgFountains.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			orgFountains.SN_MaximumValue = 10000;
			orgFountains.SN_MinimumValue = 10000;
			orgFountains.SN_Count = 1;
			orgFountains.SN_Value = 10001;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCodes.UnitedStates;
			loadList.HVL_RL_NKOrigin = CountryCodes.UnitedStates;
			loadList.HVL_OH_Carrier = carrier.PK;
			loadList.HVL_TransportMode = TransportModes.Road;
			loadList.HVL_IsNeutralMaster = true;

			AssertNoNotifications(loadList.HVL_MasterBillNumberInfo);
		}

		#endregion

		#region VoyageFlight

		public void TestValidate_VoyageFlight_WhenSeaTransportMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_VoyageFlight = ZString.Empty;

			AssertHasError(loadList.HVL_VoyageFlightInfo, "Please enter a Voyage/Flight.");

			loadList.HVL_VoyageFlight = "Ship001";
			AssertNoErrors(loadList.HVL_VoyageFlightInfo);
		}

		public void TestValidate_VoyageFlight_WhenAirTransportMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_VoyageFlight = ZString.Empty;

			AssertHasError(loadList.HVL_VoyageFlightInfo, "Please enter a Voyage/Flight.");

			loadList.HVL_VoyageFlight = "Flight001";
			AssertNoErrors(loadList.HVL_VoyageFlightInfo);
		}

		public void TestCheckHVL_VoyageFlight_WhenAirModeAndVoyageFlightIsInvalid_HasErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;

			loadList.HVL_VoyageFlight = "12345678";
			AssertHasNotifications("Flight number has more than 7 characters, error expected.", loadList.HVL_VoyageFlightInfo);
			AssertHasWarning("Flight numbers have a specific set of rules which are followed by all airlines.",
				loadList.HVL_VoyageFlightInfo, "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter.");

			loadList.HVL_VoyageFlight = "123456";
			AssertHasWarning("Flight numbers have a specific set of rules which are followed by all airlines.",
				loadList.HVL_VoyageFlightInfo, "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter.");
			AssertHasNotifications("Either or both of the first two characters of the Flight number are not letters, error expected.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "AAA456";
			AssertHasWarning("Flight numbers have a specific set of rules which are followed by all airlines.",
				loadList.HVL_VoyageFlightInfo, "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter.");
			AssertHasNotifications("One of the characters 3-6 of the Flight number are letters, error expected.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "A2A456";
			AssertHasWarning("Flight numbers have a specific set of rules which are followed by all airlines.",
				loadList.HVL_VoyageFlightInfo, "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter.");
			AssertHasNotifications("Either or both of the first two characters of the Flight number are not letters, error expected.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "A234!6";
			AssertHasWarning("Flight numbers have a specific set of rules which are followed by all airlines.",
				loadList.HVL_VoyageFlightInfo, "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter.");
			AssertHasNotifications("Flight number are contains non-alphanumeric character, error expected.", loadList.HVL_VoyageFlightInfo);
		}

		public void TestCheckHVL_VoyageFlight_WhenAirModeAndVoyageFlightIsValid_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;

			loadList.HVL_VoyageFlight = "QF43";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "F43";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "1F1";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "QF1234";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "QF1234A";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);
		}

		public void TestCheckHVL_VoyageFlight_WhenSeaModeAndVoyageFlightIsInvalid_HasErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;

			var anotherLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			anotherLoadList.HVL_TransportMode = TransportModes.Sea;

			var fakeVessel = Factory.New<RefVessel>();
			fakeVessel.RV_Code = "FakeVessel";

			loadList.HVL_VoyageFlight = "V100";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", loadList.HVL_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically.");

			loadList.HVL_VoyageFlight = "V.100";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", loadList.HVL_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically.");

			loadList.HVL_VoyageFlight = "100";
			loadList.HVL_VesselName = fakeVessel.RV_Code;
			anotherLoadList.HVL_VesselName = loadList.HVL_VesselName;
			anotherLoadList.HVL_VoyageFlight = loadList.HVL_VoyageFlight;
			AssertHasWarning(anotherLoadList.HVL_VoyageFlightInfo, "Another Sailing Schedule already exists for the given Vessel/Voyage Number combination.");
		}

		public void TestCheckHVL_VoyageFlight_WhenSeaModeAndVoyageFlightIsValid_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;

			loadList.HVL_VoyageFlight = "100";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "D100";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "VV1234";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);
		}

		public void TestCheckHVL_VoyageFlight_WhenRailModeAndVoyageFlightIsInvalid_HasErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Rail;

			var anotherLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			anotherLoadList.HVL_TransportMode = TransportModes.Rail;

			var fakeVessel = Factory.New<RefVessel>();
			fakeVessel.RV_Code = "FakeVessel";

			loadList.HVL_VoyageFlight = "100";
			loadList.HVL_VesselName = fakeVessel.RV_Code;
			anotherLoadList.HVL_VesselName = loadList.HVL_VesselName;
			anotherLoadList.HVL_VoyageFlight = loadList.HVL_VoyageFlight;
			AssertHasError("Journey number must be unique to a Journey name and cannot be repeated.", anotherLoadList.HVL_VoyageFlightInfo, "Journey number must be unique to a Journey name and cannot be repeated.");
		}

		public void TestCheckHVL_VoyageFlight_WhenRailModeAndVoyageFlightIsValid_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Rail;

			var uniqueVessel = Factory.New<RefVessel>();
			uniqueVessel.RV_Code = "FakeVessel";

			loadList.HVL_VoyageFlight = "100";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "AAA";
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);

			loadList.HVL_VoyageFlight = "AA100";
			loadList.HVL_VesselName = uniqueVessel.RV_Code;
			AssertNoNotifications("Valid flight number, not expecting errors.", loadList.HVL_VoyageFlightInfo);
		}

		#endregion

		#region Vessel

		public void TestValidate_Vessel_WhenSeaTransportMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_VesselName = ZString.Empty;

			AssertHasError(loadList.HVL_VesselNameInfo, "Please enter a Vessel.");

			loadList.HVL_VesselName = "Enterprise";
			AssertHasNotifications("Enter a valid Vessel.", loadList.HVL_VesselNameInfo);

			loadList.HVL_VesselName = "BANOWATI";
			AssertNoErrors(loadList.HVL_VesselNameInfo);
		}

		public void TestValidate_Vessel_WhenAirTransportMode()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_VesselName = ZString.Empty;

			AssertNoErrors(loadList.HVL_VesselNameInfo);
		}

		public void TestCheckHVL_RV_NKVessel_WhenSeaModeAndVesselIsInvalid_HasErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_VoyageFlight = "QF12";
			loadList.HVL_VesselName = "alkdjlafladkj";
			AssertHasErrors("Invalid code, error expected", loadList.HVL_VesselNameInfo);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			loadList.HVL_OH_Carrier = carrier.PK;
			loadList.HVL_VesselName = "BANOWATI";

			var anotherLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			anotherLoadList.HVL_TransportMode = TransportModes.Sea;
			anotherLoadList.HVL_VoyageFlight = loadList.HVL_VoyageFlight;
			anotherLoadList.HVL_OH_Carrier = loadList.HVL_OH_Carrier;

			anotherLoadList.HVL_VesselName = loadList.HVL_VesselName;
			AssertHasError(anotherLoadList.HVL_VesselNameInfo, "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings.");
		}

		public void TestCheckHVL_RV_NKVessel_WhenSeaModeAndVesselIsValid_HasNoErrorMessage()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "LOOSHAAADKAAAAA";
			vessel.RV_LloydsNumber = "9308390";
			loadList.HVL_VesselName = vessel.RV_FK;

			AssertNoWarnings(loadList.HVL_VesselNameInfo);
			AssertNoErrors("Vessel not empty with correct code, no error expected", loadList.HVL_VesselNameInfo);
		}

		#endregion

		#region Container Type

		public void TestValidationWarning_ContainerType()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_RC_ContainerType = ZGuid.Empty;

			AssertHasWarning(loadList.HVL_RC_ContainerTypeInfo, "You have not entered a Container Type.");

			loadList.HVL_RC_ContainerType = ref20GP.PK;
			AssertNoWarnings(loadList.HVL_RC_ContainerTypeInfo);
		}

		public void TestValidate_ContainerType_WhenInvalidCode()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_RC_ContainerType = ZGuid.NewZGuid();

			AssertHasWarning(loadList.HVL_RC_ContainerTypeInfo, "Enter a valid Container Type.");

			loadList.HVL_RC_ContainerType = ref20GP.PK;
			AssertNoWarnings(loadList.HVL_RC_ContainerTypeInfo);
		}

		#endregion

		#region Container Number

		public void TestValidate_ContainerNumber()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_ContainerNumber = ZString.Empty;

			AssertHasWarning(loadList.HVL_ContainerNumberInfo, "You have not entered a Container Number.");

			loadList.HVL_ContainerNumber = "CNT001";
			AssertNoWarnings(loadList.HVL_ContainerNumberInfo);
		}

		#endregion

		#region Dep/Arv

		public void TestDepartureDateTimeValidation()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_E_Arv = DateTime.Now;
			loadList.HVL_E_Dep = DateTime.Now.AddDays(1);

			AssertHasError(loadList.HVL_E_DepInfo, "The departure date must be before the arrival date.");

			loadList.HVL_E_Arv = DateTime.Now.AddDays(1);
			loadList.HVL_E_Dep = DateTime.Now;

			AssertNoErrors(loadList.HVL_E_ArvInfo);
			AssertNoErrors(loadList.HVL_E_DepInfo);
		}

		public void TestArrivalDateTimeValidation()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_E_Dep = DateTime.Now;
			loadList.HVL_E_Arv = DateTime.Now.AddDays(-1);

			AssertHasError(loadList.HVL_E_ArvInfo, "The arrival date must be after the departure date.");

			loadList.HVL_E_Arv = DateTime.Now;
			loadList.HVL_E_Dep = DateTime.Now.AddDays(-1);

			AssertNoErrors(loadList.HVL_E_ArvInfo);
			AssertNoErrors(loadList.HVL_E_DepInfo);
		}

		#endregion
	}
}
