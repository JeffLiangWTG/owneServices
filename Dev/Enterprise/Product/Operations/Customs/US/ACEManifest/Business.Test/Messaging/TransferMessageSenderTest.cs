using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class TransferMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendRequestTransfer()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2021, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = bill1.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_CarrierID = "13-150279800";
			transfer1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = bill1.PK;
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("BILL190801", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FRC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Bill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Bill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FRC
SFOVOG
SHA-12345678-BILL190801
WBL/SYD/T3/K5/THINGS
ARR/VOG1234A/22JUL-A
TRN/LAX-D/13-150279800/1234
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferSent, transferBill1.ATB_MessageStatus);
		}

		public void TestSendRequestTransferWhenHeaderIsExpressCourier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			header.IsExpressCourier = true;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2021, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = bill1.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_CarrierID = "13-150279800";
			transfer1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = bill1.PK;
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("BILL190801", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FXC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Bill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Bill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FXC
SFOVOG
SHA-12345678-BILL190801
WBL/SYD/T3/K5/THINGS
ARR/VOG1234A/22JUL-A
TRN/LAX-D/13-150279800/1234
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferSent, transferBill1.ATB_MessageStatus);
		}

		public void TestSendRequestTransfer_MAWB()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			var masterBill = (AsycudaBill)ManifestHeader.MasterBill;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = masterBill.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_OnwardCarrier = "US01";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = masterBill.PK;
			transferBill1.AllocateInBondNumber("TST0001");
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("SHA-123456789", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FRC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.TableName, message1.EM_LinkTable);
			var expectedMessage = $@"FRC
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T3/K5/CONSOLIDATION/23JUN
ARR/VOG1234A/22JUL-A
TRN/LAX-D/US01/{transferBill1.InBondNumber}
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferSent, transferBill1.ATB_MessageStatus);
		}

		public void TestSendRequestTransfer_MAWBWhenHeaderIsExpressCourier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			header.IsExpressCourier = true;
			var masterBill = (AsycudaBill)ManifestHeader.MasterBill;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = masterBill.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_OnwardCarrier = "US01";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = masterBill.PK;
			transferBill1.AllocateInBondNumber("TST0001");
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("SHA-123456789", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FXC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.TableName, message1.EM_LinkTable);
			var expectedMessage = $@"FXC
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T3/K5/CONSOLIDATION/23JUN
ARR/VOG1234A/22JUL-A
TRN/LAX-D/US01/{transferBill1.InBondNumber}
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferSent, transferBill1.ATB_MessageStatus);
		}

		public void TestSendCancelTransfer()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2021, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = bill1.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_CarrierID = "13-1502798000";
			transfer1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = bill1.PK;
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1, true);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("BILL190801", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FRC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Bill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Bill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FRC
SFOVOG
SHA-12345678-BILL190801
WBL/SYD/T3/K5/THINGS
ARR/VOG1234A/22JUL-A
TRN/000
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferCancelled, transferBill1.ATB_MessageStatus);
		}

		public void TestSendCancelTransferWhenHeaderIsExpressCourier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			header.IsExpressCourier = true;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2021, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = bill1.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_CarrierID = "13-1502798000";
			transfer1.ATF_DestinationWarehouseID = "1234";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = bill1.PK;
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1, true);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("BILL190801", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FXC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Bill.", bill1.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Bill.", bill1.TableName, message1.EM_LinkTable);
			var expectedMessage = @"FXC
SFOVOG
SHA-12345678-BILL190801
WBL/SYD/T3/K5/THINGS
ARR/VOG1234A/22JUL-A
TRN/000
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferCancelled, transferBill1.ATB_MessageStatus);
		}

		public void TestSendCancelTransfer_MAWB()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			var masterBill = (AsycudaBill)ManifestHeader.MasterBill;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = masterBill.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_OnwardCarrier = "US01";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = masterBill.PK;
			transferBill1.AllocateInBondNumber("TST0001");
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1, true);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("SHA-123456789", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FRC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.TableName, message1.EM_LinkTable);
			var expectedMessage = $@"FRC
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T3/K5/CONSOLIDATION/23JUN
ARR/VOG1234A/22JUL-A
TRN/000
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferCancelled, transferBill1.ATB_MessageStatus);
		}

		public void TestSendCancelTransfer_MAWBWhenHeaderIsExpressCourier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var address2 = carrier.Addresses.AddNew();
			address2.OA_Address1 = "Park Lane";
			address2.OA_RL_NKRelatedPortCode = "USSFO";
			address2.OA_City = "San Francisco";
			address2.OA_State = "CA";
			var header = ManifestHeader;
			header.IsExpressCourier = true;
			var masterBill = (AsycudaBill)ManifestHeader.MasterBill;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_Reference = "A";
			arrHeader1.ATH_ETAAtDischargePort = new ZDateTime(2020, 07, 22);
			arrHeader1.ATH_VoyageFlightNo = "VOG1234A";
			var arrLine1 = arrHeader1.ArrivalDetails.AddNew();
			arrLine1.ATL_ABL_AsycudaBill = masterBill.PK.ToGuid();
			arrLine1.ATL_Reference = "A";
			var transfer1 = arrHeader1.TransferHeaders.AddNew();
			transfer1.ATF_RL_NKDestinationPortCode = "USLAX";
			transfer1.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transfer1.ATF_OA_Carrier = address2.PK;
			transfer1.ATF_OnwardCarrier = "US01";
			var transferBill1 = transfer1.TransferBills.AddNew();
			transferBill1.ATB_ABL_Bill = masterBill.PK;
			transferBill1.AllocateInBondNumber("TST0001");
			var sender = new TransferMessageSender();
			sender.SendMessage(transferBill1, true);
			header.Messages.Reload(false);
			var message1 = header.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.USAMA, message1.EM_ApplicationCode);
			AssertEquals("SHA-123456789", message1.EM_ApplicationReference);
			AssertEquals(EDIMessageTypeList.Codes.FHL, message1.EM_MessageType);
			AssertEquals(AIMMessageSubTypes.FXC, message1.EM_MessageSubType);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.PK, message1.EM_LinkUniqueID);
			AssertEquals("Should attach on the Manifest.", ManifestHeader.TableName, message1.EM_LinkTable);
			var expectedMessage = $@"FXC
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T3/K5/CONSOLIDATION/23JUN
ARR/VOG1234A/22JUL-A
TRN/000
RFA/19";
			AssertMultilineASCIIEquals(expectedMessage, message1.EM_MessageText);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferCancelled, transferBill1.ATB_MessageStatus);
		}

		#region Implementation
		OrgAddress consignee;
		OrgAddress shipper;
		AsycudaBill bill1;
		AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (fManifestHeader == null)
				{
					consignee = Factory.NewWithValidTestData<OrgAddress>();
					consignee.Header.OH_FullName = "FullName";
					consignee.OA_Address1 = "Address1";
					consignee.OA_Address2 = "Address2";
					consignee.OA_City = "LOS ANGELES";
					consignee.OA_State = "CA";
					consignee.OA_PostCode = "90210";
					consignee.OA_Phone = "+00123456888";
					consignee.OA_RN_NKCountryCode = "US";
					shipper = Factory.NewWithValidTestData<OrgAddress>();
					shipper.Header.OH_FullName = "FullName";
					shipper.OA_Address1 = "Address1";
					shipper.OA_Address2 = "Address2";
					shipper.OA_City = "SYDNEY";
					shipper.OA_State = "NSW";
					shipper.OA_PostCode = "2001";
					shipper.OA_Phone = "+00123456888";
					shipper.OA_RN_NKCountryCode = "AU";
					fManifestHeader = Factory.New<AsycudaManifestHeader>();
					fManifestHeader.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
					fManifestHeader.AMA_MasterBill = "SHA-123456789";
					fManifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
					fManifestHeader.AMA_RL_NKPortOfFirstArrival = "USSFO";
					fManifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";
					fManifestHeader.AMA_CarrierCode = "VOG";
					fManifestHeader.AMA_Voyage = "VOG1234A";
					fManifestHeader.AMA_E_ARV = new ZDateTime(2020, 06, 23);
					bill1 = fManifestHeader.Bills.AddNew();
					bill1.ABL_BillNumber = "BILL190801";
					bill1.ABL_ShipmentType = "IMP";
					bill1.ABL_RL_NKOrigin = "AUSYD";
					bill1.ABL_RL_NKFinalDestination = "USJFK";
					bill1.ABL_OA_Consignee = consignee.PK;
					bill1.ABL_OA_Shipper = shipper.PK;
					bill1.ABL_MessageStatus = string.Empty;
					bill1.ABL_GoodsDescription = "THINGS";
					bill1.ABL_ManifestQty = 3;
					bill1.ABL_ManifestUQ = "BOX";
					bill1.ABL_GrossWeight = 5m;
					bill1.ABL_GrossWeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
				}

				return fManifestHeader;
			}
		}

		AsycudaManifestHeader fManifestHeader;
		#endregion
	}
}
