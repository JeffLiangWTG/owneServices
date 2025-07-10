using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaTransferHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckATF_RL_NKDestinationPortCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			var arrival = header.ArrivalHeaders.AddNew();
			var transferHeader = arrival.TransferHeaders.AddNew();
			transferHeader.ATF_RL_NKDestinationPortCode = "USCHI";
			transferHeader.ATF_RL_NKDestinationPortCode = ZString.Empty;
			AssertHasMessageErrorContaining(transferHeader.ATF_RL_NKDestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			transferHeader.ATF_RL_NKDestinationPortCode = "USLAX";
			AssertNoMessageErrorContaining(transferHeader.ATF_RL_NKDestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestInbondCarrierExclusivity()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertNoWarnings("No warning if Onward Carrier and CarrierID are not entered", transferHeader.ATF_OnwardCarrierInfo);
				transferHeader.ATF_OnwardCarrier = "ABCD";
				AssertNoWarnings("No warning if Onward Carrier is entered", transferHeader.ATF_OnwardCarrierInfo);
				transferHeader.ATF_CarrierID = "12-123456789";
				AssertHasWarnings("Warning if Onward Carrier and CarrierID are entered", transferHeader.ATF_OnwardCarrierInfo);
				transferHeader.ATF_OnwardCarrier = "";
				AssertNoWarnings("No warning if Onward Carrier is not entered", transferHeader.ATF_OnwardCarrierInfo);
			});
		}

		public void TestCheckATF_CarrierID()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			CombineAssertions(() =>
			{
				AssertNoMessageErrors("Empty carrier id with no error", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "12-345678901";
				AssertNoMessageErrors($"Correct format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "12-3456789AB";
				AssertNoMessageErrors($"Correct format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "123-45-6789";
				AssertNoMessageErrors($"Correct format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "123456-78901";
				AssertNoMessageErrors($"Correct format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "12345678901";
				AssertHasMessageErrors($"Wrong format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "12-34567890b";
				AssertHasMessageErrors($"Wrong format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "1234567-8901";
				AssertHasMessageErrors($"Wrong format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "12-3456789012";
				AssertHasMessageErrors($"Wrong format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
				transferHeader.ATF_CarrierID = "12-3456789ab";
				AssertHasMessageErrors($"Wrong format:{transferHeader.ATF_CarrierID}", transferHeader.ATF_CarrierIDInfo);
			});
		}

		public void TestCheckATF_DestinationWarehouseID()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferBill1 = transferHeader.TransferBills.AddNew();
			var transferBill2 = transferHeader.TransferBills.AddNew();
			CombineAssertions(() =>
			{
				AssertNoWarnings("No warning if Bonded Premise ID and In-Bond Number are not entered", transferHeader.ATF_DestinationWarehouseIDInfo);
				transferHeader.ATF_DestinationWarehouseID = "AAA";
				AssertNoWarnings("No warning if Bonded Premise ID is entered", transferHeader.ATF_DestinationWarehouseIDInfo);
				transferBill1.InBondNumber = "123";
				transferHeader.Validation.ValidateAll();
				AssertHasWarnings("Warning if Bonded Premise ID and the first In-Bond Number are are entered", transferHeader.ATF_DestinationWarehouseIDInfo);
				transferBill1.InBondNumber = "";
				transferHeader.Validation.ValidateAll();
				AssertNoWarnings("No warning if In-Bond Number is not entered", transferHeader.ATF_DestinationWarehouseIDInfo);
				transferBill2.InBondNumber = "456";
				transferHeader.Validation.ValidateAll();
				AssertHasWarnings("Warning if Bonded Premise ID and the second In-Bond Number are are entered", transferHeader.ATF_DestinationWarehouseIDInfo);
				transferHeader.ATF_DestinationWarehouseID = "";
				AssertNoWarnings("No warning if Bonded Premise ID is deleted", transferHeader.ATF_DestinationWarehouseIDInfo);
			});
		}

		public void TestExistBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			CombineAssertions(() =>
			{
				transferHeader.Validation.ValidateATF_TransferType();
				AssertHasMessageErrors("No bill message error", transferHeader.ATF_TransferTypeInfo);
				transferHeader.TransferBills.AddNew();
				transferHeader.Validation.ValidateATF_TransferType();
				AssertNoMessageErrors("No message error with transfer bill", transferHeader.ATF_TransferTypeInfo);
				transferHeader.TransferBills.AddNew();
				transferHeader.Validation.ValidateATF_TransferType();
				AssertNoMessageErrors("No message error with more than one transfer bill", transferHeader.ATF_TransferTypeInfo);
			});
		}

		public void TestCheckInBondCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-150279800", Core.Constants.CountryCodes.UnitedStates);
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();

			CombineAssertions(() =>
			{
				transferHeader.InBondCarrierOrgPK = carrier1.PK;
				AssertNoErrors("No error when carrier has valid code to populate In-Bond Carrier ID", transferHeader.InBondCarrierOrgPKInfo);
				AssertNoErrors("No error when carrier has valid code to populate In-Bond Carrier ID", transferHeader.ATF_OA_CarrierInfo);
				transferHeader.InBondCarrierOrgPK = carrier2.PK;
				AssertHasError("Error when carrier doesn't have valid code to populate In-Bond Carrier ID", transferHeader.InBondCarrierOrgPKInfo, "The Carrier does not have valid code to populate In-Bond Carrier ID.");
				AssertHasError("Error when carrier doesn't have valid code to populate In-Bond Carrier ID", transferHeader.ATF_OA_CarrierInfo, "The Carrier does not have valid code to populate In-Bond Carrier ID.");
			});
		}

		public void TestCheckOnwardCarrier()
		{
			var carrier1 = Factory.NewWithValidTestData<USCarrierCombined>();
			carrier1.UI_Code = "TST1";
			carrier1.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;
			var carrier2 = Factory.NewWithValidTestData<USCarrierCombined>();
			carrier2.UI_Code = "TST2";
			carrier2.UI_ModeOfTransportation = TransportModeCodes.Codes.TruckNonContainer;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();

			CombineAssertions(() =>
			{
				transferHeader.ATF_OnwardCarrier = "TST1";
				AssertNoMessageErrors("No message error when transport mode is 40", transferHeader.ATF_OnwardCarrierInfo);
				transferHeader.ATF_OnwardCarrier = "TST2";
				AssertHasMessageError("Message error when transport mode is not 40", transferHeader.ATF_OnwardCarrierInfo, "The Transportation Mode of Onward Carrier should be '40'.");
			});
		}
	}
}
