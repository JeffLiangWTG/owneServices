using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(TranshipmentRequestValidation))]
	sealed class TranshipmentRequestValidationTest : Customs.Business.Testing.CusUnderbondValidationTest
	{
		public override void TestC4_OutturnedValidation()
		{
			Assert("TranshipmentRequest is not linked to an Outturn", condition: true);
		}

		public void TestIsATranshipmentRequest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			Assert(!decITR.Validation.IsATranshipmentRequest);

			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			Assert(decITR.Validation.IsATranshipmentRequest);

			decITR.C4_ModeOfMovement = ZString.Empty;
			Assert(!decITR.Validation.IsATranshipmentRequest);

			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			Assert(decITR.Validation.IsATranshipmentRequest);

			var testMawb = Factory.New<CusMAWB>();
			var testHawb = testMawb.ChildBills.AddNew();
			TranshipmentRequest.Create(testHawb);
			var hawbITR = testHawb.TranshipmentRequest;
			Assert(!hawbITR.Validation.IsATranshipmentRequest);

			hawbITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			Assert("Transhipment Request can be created on a CusHAWB", hawbITR.Validation.IsATranshipmentRequest);

			hawbITR.C4_ModeOfMovement = ZString.Empty;
			Assert(!hawbITR.Validation.IsATranshipmentRequest);

			hawbITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			Assert(hawbITR.Validation.IsATranshipmentRequest);
		}

		public void TestMovementReason()
		{
			var testMawb = Factory.New<CusMAWB>();
			var testHawb = testMawb.ChildBills.AddNew();
			TranshipmentRequest.Create(testHawb);
			var hawbITR = testHawb.TranshipmentRequest;
			hawbITR.C4_MovementReason = "";
			hawbITR.Validation.ValidateC4_MovementReason();
			AssertHasMessageErrorContaining(hawbITR.C4_MovementReasonInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(hawbITR.C4_MovementReasonInfo, ListValidation.InvalidCodeMessageError);

			hawbITR.C4_MovementReason = "ABC";
			AssertHasMessageError(hawbITR.C4_MovementReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(hawbITR.C4_MovementReasonInfo, MandatoryValidation.YouHaveNotEntered);

			hawbITR.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			AssertNoMessageError(hawbITR.C4_MovementReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(hawbITR.C4_MovementReasonInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestModeOfMovement()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = "9";
			AssertHasMessageError(decITR.C4_ModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(decITR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);

			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			AssertNoMessageError(decITR.C4_ModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(decITR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);

			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			decITR.Validation.ValidateC4_ModeOfMovement();
			AssertHasMessageError("1 - Sea is not in list for Import write-off", decITR.C4_ModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(decITR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaCV;
			AssertNoMessageError(decITR.C4_ModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(decITR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);

			decITR.C4_ModeOfMovement = "";
			AssertNoMessageErrorContaining(decITR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);

			decITR.C4_TranshipModeOfMovement = "1";
			decITR.Validation.ValidateC4_ModeOfMovement();
			AssertHasMessageErrorContaining(decITR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestModeOfMovementForDTR()
		{
			var testMawb = Factory.New<CusMAWB>();
			var testHawb = testMawb.ChildBills.AddNew();
			TranshipmentRequest.Create(testHawb);
			var hawbDTR = testHawb.TranshipmentRequest;
			hawbDTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			hawbDTR.Validation.ValidateC4_ModeOfMovement();
			AssertHasMessageErrorContaining("DTR requires validation of ModeOfMovement independantly", hawbDTR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);

			hawbDTR.C4_ModeOfMovement = "8";
			AssertHasMessageError(hawbDTR.C4_ModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(hawbDTR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);

			hawbDTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			AssertNoMessageError(hawbDTR.C4_ModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(hawbDTR.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestTranshipModeOfMovement()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_TranshipModeOfMovement = "9";
			AssertHasMessageError(decITR.C4_TranshipModeOfMovementInfo, ListValidation.InvalidCodeMessageError);

			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertNoMessageError(decITR.C4_TranshipModeOfMovementInfo, ListValidation.InvalidCodeMessageError);

			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			decITR.Validation.ValidateC4_TranshipModeOfMovement();
			AssertHasMessageError("3 - Road is not in list for Import write-off", decITR.C4_TranshipModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			AssertNoMessageError(decITR.C4_TranshipModeOfMovementInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckC4_FlightNo()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "QF117", "QF117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = LowValueEntryTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decITR.C4_ArrivalDate = ZDateTime.Today.AddDays(-14);
			decITR.C4_FlightNo = "QF117";

			decITR.Validation.ValidateC4_FlightNo();
			AssertNoMessageError(decITR.C4_FlightNoInfo, FlightNotOnCustomsSupportedList);

			decITR.C4_FlightNo = "blah";
			decITR.Validation.ValidateC4_FlightNo();
			AssertHasMessageError(decITR.C4_FlightNoInfo, FlightNotOnCustomsSupportedList);

			declaration.JE_MessageType = LowValueEntryTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			decITR.C4_FlightNo = "";
			AssertNoMessageErrors("Import movement request does not require flight details", decITR.C4_FlightNoInfo);
		}

		public void TestC4_TranshipBySeaVessel()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "BUNGA BIDARA", "BUNGA BIDARA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_ArrivalDate = ZDateTime.Today.AddDays(-2);
			decITR.C4_TranshipBySeaVessel = "BUNGA BIDARA";
			decITR.C4_TranshipBySeaVoyage = "716W";

			decITR.Validation.ValidateC4_TranshipBySeaVessel();
			AssertNoMessageError(decITR.C4_TranshipBySeaVesselInfo, VesselNotOnCustomsSupportedList);

			decITR.C4_TranshipBySeaVessel = "blah";
			decITR.Validation.ValidateC4_TranshipBySeaVessel();
			AssertHasMessageError(decITR.C4_TranshipBySeaVesselInfo, VesselNotOnCustomsSupportedList);

			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decITR.Validation.ValidateC4_TranshipBySeaVessel();
			AssertNoMessageError(decITR.C4_TranshipBySeaVesselInfo, VesselNotOnCustomsSupportedList);
		}

		public void TestC4_TranshipBySeaVessel_VesselNotFound()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Precondition: There are no Vessels named " + vesselName, 0, RefVessel.LookupVesselByName(vesselName, Factory).Length);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			TranshipmentRequest.Create(declaration);
			var transhipment = declaration.TranshipmentRequest;
			transhipment.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			transhipment.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			transhipment.C4_TranshipBySeaVessel = vesselName;
			transhipment.C4_TranshipBySeaLloydsIMONum = "998877";
			AssertHasWarning(transhipment.C4_TranshipBySeaVesselInfo, "A Vessel with this Name and Lloyds Number cannot be found.");

			var vesselA = Factory.New<RefVessel>();
			vesselA.RV_Code = vesselName;
			vesselA.RV_LloydsNumber = "";
			transhipment.Validation.ValidateC4_TranshipBySeaVessel();
			AssertHasWarning(transhipment.C4_TranshipBySeaVesselInfo, "A Vessel with this Name and Lloyds Number cannot be found.");

			var vesselB = Factory.New<RefVessel>();
			vesselB.RV_Code = vesselName;
			vesselB.RV_LloydsNumber = "998877";
			transhipment.Validation.ValidateC4_TranshipBySeaVessel();
			AssertNoWarning(transhipment.C4_TranshipBySeaVesselInfo, "A Vessel with this Name and Lloyds Number cannot be found.");
		}

		public void TestCheckC4_ArrivalDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = LowValueEntryTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_ArrivalDate = ZDateTime.Today.AddDays(5);
			decITR.C4_TranshipBySeaVessel = "BUNGA BIDARA";
			decITR.C4_TranshipBySeaVoyage = "716W";

			decITR.Validation.ValidateC4_ArrivalDate();
			AssertHasMessageError(decITR.C4_ArrivalDateInfo, ArrivalDateCannotBeAfterDepartureDate);
			AssertNoMessageError(decITR.C4_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("Arrival Date"));

			decITR.C4_ArrivalDate = ZDateTime.Today.AddDays(-2);
			decITR.Validation.ValidateC4_ArrivalDate();
			AssertNoMessageError(decITR.C4_ArrivalDateInfo, ArrivalDateCannotBeAfterDepartureDate);
			AssertNoMessageError(decITR.C4_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("Arrival Date"));

			decITR.C4_ArrivalDate = ZDateTime.Today;
			decITR.Validation.ValidateC4_ArrivalDate();
			AssertNoMessageError(decITR.C4_ArrivalDateInfo, ArrivalDateCannotBeAfterDepartureDate);
			AssertNoMessageError(decITR.C4_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("Arrival Date"));

			decITR.C4_ArrivalDate = ZDateTime.Empty;
			AssertHasMessageError(decITR.C4_ArrivalDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("Arrival Date"));
		}

		public void TestDepartureDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_TranshipDepartureDate = ZDateTime.Today.AddDays(-5);
			decITR.C4_TranshipBySeaVessel = "BUNGA BIDARA";
			decITR.C4_TranshipBySeaVoyage = "716W";

			decITR.Validation.ValidateC4_TranshipDepartureDate();
			AssertHasMessageError(decITR.C4_TranshipDepartureDateInfo, DepartureDateCannotBeBeforeArrivalDate);

			decITR.C4_TranshipDepartureDate = ZDateTime.Today.AddDays(2);
			decITR.Validation.ValidateC4_TranshipDepartureDate();
			AssertNoMessageError(decITR.C4_TranshipDepartureDateInfo, DepartureDateCannotBeBeforeArrivalDate);

			decITR.C4_TranshipDepartureDate = ZDateTime.Today;
			decITR.Validation.ValidateC4_TranshipDepartureDate();
			AssertNoMessageError(decITR.C4_TranshipDepartureDateInfo, DepartureDateCannotBeBeforeArrivalDate);

			decITR.C4_TranshipDepartureDate = ZDateTime.Empty;
			AssertHasMessageError(decITR.C4_TranshipDepartureDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("Departure date"));

			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decITR.Validation.ValidateC4_TranshipDepartureDate();
			AssertNoMessageError(decITR.C4_TranshipDepartureDateInfo, MandatoryValidation.YouHaveNotEnteredMessage("Departure date"));
		}

		public void TestTranshipBySeaVoyage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decITR.C4_TranshipBySeaVoyage = "";
			AssertNoMessageError(decITR.C4_TranshipBySeaVoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage("Voyage number"));

			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_TranshipBySeaVoyage = "";
			AssertHasMessageError(decITR.C4_TranshipBySeaVoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage("Voyage number"));

			decITR.C4_TranshipBySeaVoyage = "716W";
			AssertNoMessageError(decITR.C4_TranshipBySeaVoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage("Voyage number"));
		}

		public void TestCheckC4_OA_DestinationAddress()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD  address";
			transitDestOrgAddr.OA_Code = "TD1";
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = LowValueEntryTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;

			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			AssertHasMessageError("Address Org without specific CCP code", decITR.C4_OA_DestinationAddressInfo, TransitDestinationNotLinkedToCode);

			transitDestOrgAddr.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1795K", Core.Constants.CountryCodes.NewZealand);
			decITR.C4_OA_DestinationAddress = ZGuid.Empty;
			decITR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			AssertNoMessageError("Address Org with specific CCP code", decITR.C4_OA_DestinationAddressInfo, TransitDestinationNotLinkedToCode);

			var transitDestOrgAddrWithoutCCP = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddrWithoutCCP.OA_Address1 = "TD2 address";
			transitDestOrgAddrWithoutCCP.OA_Code = "TD2";
			Factory.Save();

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var container = oceanBill.Containers.AddNew();
			var containerDTRs = container.ContainerDTRs;
			var dtr1 = containerDTRs.AddNew();
			dtr1.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			dtr1.C4_OA_DestinationAddress = transitDestOrgAddrWithoutCCP.PK;
			dtr1.Validation.ValidateC4_OA_DestinationAddress();
			AssertHasMessageError("Address Org without specific CCP code", dtr1.C4_OA_DestinationAddressInfo, TransitDestinationNotLinkedToCode);

			var dtr2 = containerDTRs.AddNew();
			dtr2.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			dtr2.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			dtr2.Validation.ValidateC4_OA_DestinationAddress();
			AssertNoMessageError("Address Org with specific CCP code", dtr2.C4_OA_DestinationAddressInfo, TransitDestinationNotLinkedToCode);
		}

		public void TestDestinationAddress()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_OA_DestinationAddress = ZGuid.Empty;
			decITR.C4_RL_NKTranshipDestPort = "";
			var expectedError = string.Format(PremiseCodeRequiredForSea, decITR.C4_MovementReason);
			AssertHasMessageError(decITR.C4_OA_DestinationAddressInfo, expectedError);

			decITR.C4_RL_NKTranshipDestPort = "NZWLG";
			decITR.Validation.ValidateC4_OA_DestinationAddress();
			AssertNoMessageError(decITR.C4_OA_DestinationAddressInfo, expectedError);

			expectedError = string.Format(PremiseCodeRequiredForAir, decITR.C4_MovementReason);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decITR.C4_RL_NKTranshipDestPort = "";
			decITR.C4_OA_DestinationAddress = ZGuid.Empty;
			AssertHasMessageError(decITR.C4_OA_DestinationAddressInfo, expectedError);

			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";
			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD  address";
			transitDestOrgAddr.OA_Code = "TD1";
			decITR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			AssertNoMessageError(decITR.C4_OA_DestinationAddressInfo, expectedError);
		}

		public void TestDestinationAddressForDTR()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			TranshipmentRequest.Create(declaration);
			var decTranshipment = declaration.TranshipmentRequest;
			decTranshipment.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			decTranshipment.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decTranshipment.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decTranshipment.C4_OA_DestinationAddress = ZGuid.Empty;
			decTranshipment.C4_RL_NKTranshipDestPort = "";
			var expectedError = string.Format(PremiseCodeRequiredForAir, decTranshipment.C4_MovementReason);
			AssertHasMessageError(decTranshipment.C4_OA_DestinationAddressInfo, expectedError);

			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";
			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD  address";
			transitDestOrgAddr.OA_Code = "TD1";

			decTranshipment.C4_OA_DestinationAddress = ZGuid.Empty;
			decTranshipment.C4_OA_OriginAddress = ZGuid.Empty;
			AssertNoMessageError(decTranshipment.C4_OA_DestinationAddressInfo, DestinationAddressIsSameWithOriginAddress);

			decTranshipment.C4_OA_OriginAddress = transitDestOrgAddr.PK;
			decTranshipment.C4_OA_DestinationAddress = transitDestOrgAddr.PK;
			AssertHasMessageError(decTranshipment.C4_OA_DestinationAddressInfo, DestinationAddressIsSameWithOriginAddress);
		}

		public void TestCheckC4_OA_OriginAddress()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			TranshipmentRequest.Create(declaration);

			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";
			var transitDestOrgAddrWithoutCCP = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddrWithoutCCP.OA_Address1 = "TD  address";
			transitDestOrgAddrWithoutCCP.OA_Code = "TD1";

			var dtr = declaration.TranshipmentRequest;
			dtr.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			dtr.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			dtr.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;

			dtr.C4_OA_DestinationAddress = ZGuid.Empty;
			dtr.C4_OA_OriginAddress = ZGuid.Empty;
			AssertNoMessageError(dtr.C4_OA_OriginAddressInfo, DestinationAddressIsSameWithOriginAddress);

			dtr.C4_OA_DestinationAddress = transitDestOrgAddrWithoutCCP.PK;
			dtr.C4_OA_OriginAddress = transitDestOrgAddrWithoutCCP.PK;
			AssertHasMessageError(dtr.C4_OA_OriginAddressInfo, DestinationAddressIsSameWithOriginAddress);

			var transitDestOrgAddrHasCCP = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddrHasCCP.OA_Address1 = "TD2 address";
			transitDestOrgAddrHasCCP.OA_Code = "TD2";
			transitDestOrgAddrHasCCP.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1795K", Core.Constants.CountryCodes.NewZealand);
			Factory.Save();

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var container = oceanBill.Containers.AddNew();
			var containerDTRs = container.ContainerDTRs;
			var dtr1 = containerDTRs.AddNew();
			dtr1.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			dtr1.C4_OA_OriginAddress = transitDestOrgAddrWithoutCCP.PK;
			dtr1.Validation.ValidateC4_OA_OriginAddress();
			AssertHasMessageError("Address Org without specific CCP code", dtr1.C4_OA_OriginAddressInfo, OriginDestinationNotLinkedToCode);

			var dtr2 = containerDTRs.AddNew();
			dtr2.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			dtr2.C4_OA_OriginAddress = transitDestOrgAddrHasCCP.PK;
			dtr2.Validation.ValidateC4_OA_OriginAddress();
			AssertNoMessageError("Address Org With specific CCP code", dtr2.C4_OA_OriginAddressInfo, OriginDestinationNotLinkedToCode);
		}

		public void TestTranshipDestPort()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			TranshipmentRequest.Create(declaration);
			var decITR = declaration.TranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_OA_DestinationAddress = ZGuid.Empty;
			decITR.C4_RL_NKTranshipDestPort = "";
			var expectedError = string.Format(PremiseCodeRequiredForSea, decITR.C4_MovementReason);
			AssertHasMessageError(decITR.C4_RL_NKTranshipDestPortInfo, expectedError);

			decITR.C4_RL_NKTranshipDestPort = "FJSUV";
			AssertNoMessageError(decITR.C4_RL_NKTranshipDestPortInfo, expectedError);
			AssertHasMessageError(decITR.C4_RL_NKTranshipDestPortInfo, TransitPortMustBeNZ);

			decITR.C4_RL_NKTranshipDestPort = "NZWLG";
			AssertNoMessageError(decITR.C4_RL_NKTranshipDestPortInfo, TransitPortMustBeNZ);
		}

		protected override CusUnderbond CreateNewUnderbond() => Factory.New<TranshipmentRequest>();

		const string ArrivalDateCannotBeAfterDepartureDate = "Transhipment request Arrival Date cannot be after the declaration Departure Date";
		const string DepartureDateCannotBeBeforeArrivalDate = "Transhipment request Departure Date cannot be before the declaration Arrival Date";
		const string DestinationAddressIsSameWithOriginAddress = "Transhipment request Transit Destination cannot be same with Origin/Goods Location";
		const string FlightNotOnCustomsSupportedList = "Flight number of the import craft is not in the list of valid Flights supported by NZ Customs.";
		const string VesselNotOnCustomsSupportedList = "Vessel name of the import craft is not in the list of valid Vessel names supported by NZ Customs.";
		const string OriginDestinationNotLinkedToCode = "The address selected for the Origin/Goods Destination organisation does not have a Controlled Premise ID code (CCP) associated with it.\r\nPlease update the Organisation > Config grid to add the relevant CCP code for this address.";
		const string TransitDestinationNotLinkedToCode = "The address selected for the Transit Destination organisation does not have a Controlled Premise ID code (CCP) associated with it.\r\nPlease update the Organisation > Config grid to add the relevant CCP code for this address.";
		const string PremiseCodeRequiredForSea = "Transit Destination Premise code is mandatory for {0}.\r\nEnter an organisation with a Premise code issued by TSW or optionally, if destined to a New Zealand port, enter the Transit Dest. Port";
		const string PremiseCodeRequiredForAir = "Transit Destination Premise code is mandatory for {0}.\r\nEnter an organisation with a valid Premise code issued by TSW";
		const string TransitPortMustBeNZ = "Transit Destination Port must be a New Zealand port code.";
	}
}
