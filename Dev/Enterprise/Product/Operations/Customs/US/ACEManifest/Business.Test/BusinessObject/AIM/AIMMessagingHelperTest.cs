using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	sealed class AIMMessagingHelperTest : TestCaseWithFactory
	{
		public void TestSendFRCFRXMessage()
		{
			var header = ManifestHeader;
			var items = header.Bills.Cast<ISelectionItem>();
			var messageParents = header.Bills.Cast<IMessageParent>().ToList();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRC);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var sender = new AIMMessagingHelper();
			var result = sender.SendBillMessage(header, chooser.MessageType, messageParents, chooser);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("1 Air Import Message(s) Created.", result.Message);
			AssertEquals("Should update the message status of bill to SNT as the message type is FRC.", MessageStatusCodeList.Codes.Sent, bill.ABL_MessageStatus);
			var message = bill.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRC, message.EM_MessageSubType);
			AssertEquals("BILL190801", message.EM_ApplicationReference);
			var frcMessage = @"FRC
SHA-12345678-BILL190801
WBL/SYD/T3/L8.3/THINGS
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888
CSD/AU/9527-USD/1001110000
RFA/01";
			AssertMultilineASCIIEquals(frcMessage, message.EM_MessageText);
			bill.ABL_MessageStatus = string.Empty;
			chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRX);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			result = sender.SendBillMessage(header, chooser.MessageType, messageParents, chooser);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("1 Air Import Message(s) Created.", result.Message);
			AssertEquals("Should update the message status of bill to CAN as the message type is FRX.", MessageStatusCodeList.Codes.Cancel, bill.ABL_MessageStatus);
			var message2 = bill.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).LastOrDefault();
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message2.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRX, message2.EM_MessageSubType);
			AssertEquals("BILL190801", message2.EM_ApplicationReference);
			var frxMessage = @"FRX
SHA-12345678-BILL190801
RFA/01";
			AssertMultilineASCIIEquals(frxMessage, message2.EM_MessageText);
		}

		public void TestSendFXCFXXMessage()
		{
			var header = ManifestHeader;
			var items = header.Bills.Cast<ISelectionItem>();
			var messageParents = header.Bills.Cast<IMessageParent>().ToList();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FXC);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var sender = new AIMMessagingHelper();
			var result = sender.SendBillMessage(header, chooser.MessageType, messageParents, chooser);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("1 Air Import Message(s) Created.", result.Message);
			AssertEquals("Should update the message status of bill to SNT as the message type is FXC.", MessageStatusCodeList.Codes.Sent, bill.ABL_MessageStatus);
			var message = bill.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FXC, message.EM_MessageSubType);
			AssertEquals("BILL190801", message.EM_ApplicationReference);
			var fxcMessage = @"FXC
SHA-12345678-BILL190801
WBL/SYD/T3/L8.3/THINGS
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888
CSD/AU/9527-USD/1001110000
RFA/01";
			AssertMultilineASCIIEquals(fxcMessage, message.EM_MessageText);
			bill.ABL_MessageStatus = string.Empty;
			chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FXX);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			result = sender.SendBillMessage(header, chooser.MessageType, messageParents, chooser);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("1 Air Import Message(s) Created.", result.Message);
			AssertEquals("Should update the message status of bill to CAN as the message type is FXX.", MessageStatusCodeList.Codes.Cancel, bill.ABL_MessageStatus);
			var message2 = bill.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).LastOrDefault();
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message2.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FXX, message2.EM_MessageSubType);
			AssertEquals("BILL190801", message2.EM_ApplicationReference);
			var fxxMessage = @"FXX
SHA-12345678-BILL190801
CED/000
RFA/01";
			AssertMultilineASCIIEquals(fxxMessage, message2.EM_MessageText);
		}

		public void TestSendFRIMessage()
		{
			var header = ManifestHeader;
			var items = header.Bills.Cast<ISelectionItem>();
			var messageParents = header.Bills.Cast<IMessageParent>().ToList();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRI);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var sender = new AIMMessagingHelper();
			var result = sender.SendBillMessage(header, chooser.MessageType, messageParents, chooser);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("1 Air Import Message(s) Created.", result.Message);
			AssertEquals("Should update the message status of bill to SNT as the message type is FRI.", MessageStatusCodeList.Codes.Sent, bill.ABL_MessageStatus);
			var message = bill.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRI, message.EM_MessageSubType);
			AssertEquals("BILL190801", message.EM_ApplicationReference);
			var expectedMessage = @"FRI
SHA-12345678-BILL190801
WBL/SYD/T3/L8.3/THINGS
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888
CSD/AU/9527-USD/1001110000";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		public void TestSendFXIMessage()
		{
			var header = ManifestHeader;
			var items = header.Bills.Cast<ISelectionItem>();
			var messageParents = header.Bills.Cast<IMessageParent>().ToList();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FXI);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var sender = new AIMMessagingHelper();
			var result = sender.SendBillMessage(header, chooser.MessageType, messageParents, chooser);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("1 Air Import Message(s) Created.", result.Message);
			AssertEquals("Should update the message status of bill to SNT as the message type is FXI.", MessageStatusCodeList.Codes.Sent, bill.ABL_MessageStatus);
			var message = bill.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FXI, message.EM_MessageSubType);
			AssertEquals("BILL190801", message.EM_ApplicationReference);
			var expectedMessage = @"FXI
SHA-12345678-BILL190801
WBL/SYD/T3/L8.3/THINGS
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888
CSD/AU/9527-USD/1001110000";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		public void TestSendFRIMessageForMAWB()
		{
			var header = ManifestHeader;
			var consignee2 = Factory.NewWithValidTestData<OrgAddress>();
			var shipper2 = Factory.NewWithValidTestData<OrgAddress>();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL190802";
			bill2.ABL_ShipmentType = "IMP";
			bill2.ABL_RL_NKOrigin = "AUSYD";
			bill2.ABL_RL_NKFinalDestination = "SGSIN";
			bill2.ABL_OA_Consignee = consignee2.PK;
			bill2.ABL_OA_Shipper = shipper2.PK;
			bill2.ABL_MessageStatus = string.Empty;
			bill2.ABL_GoodsDescription = "THINGS";
			bill2.ABL_ManifestQty = 2;
			bill2.ABL_ManifestUQ = "BOX";
			bill2.ABL_GrossWeight = 6.3m;
			bill2.ABL_GrossWeightUQ = "KG";
			AssertEquals(3.764817m, bill.MassInKilos);
			AssertEquals(6.3m, bill2.MassInKilos);
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA210";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 13);
			additionalMessageInformation.AM_IsConsolidation = true;
			var sender = new AIMMessagingHelper();
			var result = sender.SendManifestMessage(header, AIMMessageSubTypes.FRI, null, additionalMessageInformation);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("Air Import Manifest Message Created.", result.Message);
			var message = header.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("Should attach on the header.", header.PK, message.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message.EM_LinkTable);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRI, message.EM_MessageSubType);
			AssertEquals("SHA-123456789", message.EM_ApplicationReference);
			var expectedMessage = @"FRI
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T5/K10.1/CONSOLIDATION/23JUN
ARR/UA210/13SEP
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		public void TestSendFRCMessageForMAWB()
		{
			var header = ManifestHeader;
			var consignee2 = Factory.NewWithValidTestData<OrgAddress>();
			var shipper2 = Factory.NewWithValidTestData<OrgAddress>();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL190802";
			bill2.ABL_ShipmentType = "IMP";
			bill2.ABL_RL_NKOrigin = "AUSYD";
			bill2.ABL_RL_NKFinalDestination = "SGSIN";
			bill2.ABL_OA_Consignee = consignee2.PK;
			bill2.ABL_OA_Shipper = shipper2.PK;
			bill2.ABL_MessageStatus = string.Empty;
			bill2.ABL_GoodsDescription = "THINGS";
			bill2.ABL_ManifestQty = 2;
			bill2.ABL_ManifestUQ = "BOX";
			bill2.ABL_GrossWeight = 6.3m;
			bill2.ABL_GrossWeightUQ = "KG";
			AssertEquals(3.764817m, bill.MassInKilos);
			AssertEquals(6.3m, bill2.MassInKilos);
			var items = new ISelectionItem[] { header };
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRC);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA210";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 13);
			additionalMessageInformation.AM_IsConsolidation = true;
			var sender = new AIMMessagingHelper();
			var result = sender.SendManifestMessage(header, AIMMessageSubTypes.FRC, chooser, additionalMessageInformation);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("Air Import Manifest Message Created.", result.Message);
			var message = header.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("Should attach on the header.", header.PK, message.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message.EM_LinkTable);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRC, message.EM_MessageSubType);
			AssertEquals("SHA-123456789", message.EM_ApplicationReference);
			var expectedMessage = @"FRC
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T5/K10.1/CONSOLIDATION/23JUN
ARR/UA210/13SEP
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888
RFA/01";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		public void TestSendFRXMessageForMAWB()
		{
			var header = ManifestHeader;
			var consignee2 = Factory.NewWithValidTestData<OrgAddress>();
			var shipper2 = Factory.NewWithValidTestData<OrgAddress>();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL190802";
			bill2.ABL_ShipmentType = "IMP";
			bill2.ABL_RL_NKOrigin = "AUSYD";
			bill2.ABL_RL_NKFinalDestination = "SGSIN";
			bill2.ABL_OA_Consignee = consignee2.PK;
			bill2.ABL_OA_Shipper = shipper2.PK;
			bill2.ABL_MessageStatus = string.Empty;
			bill2.ABL_GoodsDescription = "THINGS";
			bill2.ABL_ManifestQty = 2;
			bill2.ABL_ManifestUQ = "BOX";
			bill2.ABL_GrossWeight = 6.3m;
			bill2.ABL_GrossWeightUQ = "KG";
			AssertEquals(3.764817m, bill.MassInKilos);
			AssertEquals(6.3m, bill2.MassInKilos);
			var items = new ISelectionItem[] { header };
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRX);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "BA730";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 19);
			additionalMessageInformation.AM_IsConsolidation = true;
			var sender = new AIMMessagingHelper();
			var result = sender.SendManifestMessage(header, AIMMessageSubTypes.FRX, chooser, additionalMessageInformation);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("Air Import Manifest Message Created.", result.Message);
			var message = header.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("Should attach on the header.", header.PK, message.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message.EM_LinkTable);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRX, message.EM_MessageSubType);
			AssertEquals("SHA-123456789", message.EM_ApplicationReference);
			var expectedMessage = @"FRX
SFOVOG
SHA-12345678-M
ARR/BA730/19SEP
RFA/01";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		public void TestSendFRIMessageForMAWBIncludesAgent()
		{
			var header = ManifestHeader;
			var consignee2 = Factory.NewWithValidTestData<OrgAddress>();
			var shipper2 = Factory.NewWithValidTestData<OrgAddress>();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL190802";
			bill2.ABL_ShipmentType = "IMP";
			bill2.ABL_RL_NKOrigin = "AUSYD";
			bill2.ABL_RL_NKFinalDestination = "SGSIN";
			bill2.ABL_OA_Consignee = consignee2.PK;
			bill2.ABL_OA_Shipper = shipper2.PK;
			bill2.ABL_MessageStatus = string.Empty;
			bill2.ABL_GoodsDescription = "THINGS";
			bill2.ABL_ManifestQty = 2;
			bill2.ABL_ManifestUQ = "BOX";
			bill2.ABL_GrossWeight = 6.3m;
			bill2.ABL_GrossWeightUQ = "KG";
			AssertEquals(3.764817m, bill.MassInKilos);
			AssertEquals(6.3m, bill2.MassInKilos);
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA210";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 13);
			additionalMessageInformation.AM_IsConsolidation = true;
			additionalMessageInformation.AM_Agent = "BCBT336";
			var sender = new AIMMessagingHelper();
			var result = sender.SendManifestMessage(header, AIMMessageSubTypes.FRI, null, additionalMessageInformation);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("Air Import Manifest Message Created.", result.Message);
			var message = header.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("Should attach on the header.", header.PK, message.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message.EM_LinkTable);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRI, message.EM_MessageSubType);
			AssertEquals("SHA-123456789", message.EM_ApplicationReference);
			var expectedMessage = @"FRI
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T5/K10.1/CONSOLIDATION/23JUN
ARR/UA210/13SEP
AGT/BCBT336
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		public void TestSendFRCMessageForMAWBIncludesAgent()
		{
			var header = ManifestHeader;
			var consignee2 = Factory.NewWithValidTestData<OrgAddress>();
			var shipper2 = Factory.NewWithValidTestData<OrgAddress>();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL190802";
			bill2.ABL_ShipmentType = "IMP";
			bill2.ABL_RL_NKOrigin = "AUSYD";
			bill2.ABL_RL_NKFinalDestination = "SGSIN";
			bill2.ABL_OA_Consignee = consignee2.PK;
			bill2.ABL_OA_Shipper = shipper2.PK;
			bill2.ABL_MessageStatus = string.Empty;
			bill2.ABL_GoodsDescription = "THINGS";
			bill2.ABL_ManifestQty = 2;
			bill2.ABL_ManifestUQ = "BOX";
			bill2.ABL_GrossWeight = 6.3m;
			bill2.ABL_GrossWeightUQ = "KG";
			AssertEquals(3.764817m, bill.MassInKilos);
			AssertEquals(6.3m, bill2.MassInKilos);
			var items = new ISelectionItem[] { header };
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRC);
			chooser.Reason = AIMReasonCodes.Codes.R01;
			chooser.SelectAll();
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA210";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 13);
			additionalMessageInformation.AM_IsConsolidation = true;
			additionalMessageInformation.AM_Agent = "GAZA742";
			var sender = new AIMMessagingHelper();
			var result = sender.SendManifestMessage(header, AIMMessageSubTypes.FRC, chooser, additionalMessageInformation);
			Assert("1 Air Import Message(s) Created.", result.IsSuccess);
			AssertEquals("Air Import Manifest Message Created.", result.Message);
			var message = header.Messages.Find(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.USAMA).FirstOrDefault();
			AssertEquals("Should attach on the header.", header.PK, message.EM_LinkUniqueID);
			AssertEquals("Should attach on the header.", header.TableName, message.EM_LinkTable);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.FHL, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", AIMMessageSubTypes.FRC, message.EM_MessageSubType);
			AssertEquals("SHA-123456789", message.EM_ApplicationReference);
			var expectedMessage = @"FRC
SFOVOG
SHA-12345678-M
WBL/SYDLAX/T5/K10.1/CONSOLIDATION/23JUN
ARR/UA210/13SEP
AGT/GAZA742
SHP/FULLNAME
/ADDRESS1
/SYDNEY/NSW
/AU/2001/00123456888
CNE/FULLNAME
/ADDRESS1
/LOS ANGELES/CA
/US/90210/00123456888
RFA/01";
			AssertMultilineASCIIEquals(expectedMessage, message.EM_MessageText);
		}

		OrgAddress consignee;
		OrgAddress shipper;
		AsycudaBill bill;
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
					bill = fManifestHeader.Bills.AddNew();
					bill.ABL_BillNumber = "BILL190801";
					bill.ABL_ShipmentType = "IMP";
					bill.ABL_RL_NKOrigin = "AUSYD";
					bill.ABL_RL_NKFinalDestination = "USJFK";
					bill.ABL_OA_Consignee = consignee.PK;
					bill.ABL_OA_Shipper = shipper.PK;
					bill.ABL_MessageStatus = string.Empty;
					bill.ABL_GoodsDescription = "THINGS";
					bill.ABL_ManifestQty = 3;
					bill.ABL_ManifestUQ = "BOX";
					bill.ABL_GrossWeight = 8.3m;
					bill.ABL_GrossWeightUQ = "LB";
					bill.ABL_RX_NKGoodsValueCurrency = "USD";
					bill.ABL_Tariff = "1001110000";
					bill.ABL_GoodsValue = 9527m;
				}

				return fManifestHeader;
			}
		}

		AsycudaManifestHeader fManifestHeader;
	}
}
