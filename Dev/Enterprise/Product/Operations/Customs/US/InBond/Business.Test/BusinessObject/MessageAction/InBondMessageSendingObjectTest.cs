using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMessageSendingObject))]
	sealed class InBondMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUS_MasterBillNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			var moveDetail = header.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.B9_BM = moveHeader.PK;
			bill.B0_MasterBillNumber = "MTB00000001";
			bill.B0_HouseBillNumber = ZString.Empty;
			var sendingObject = new InBondMessageSendingObject(moveHeader, null, InBondMessageType.AirInBondAdd);
			AssertEquals(ZString.Empty, sendingObject.US_MasterBillNumber);
			sendingObject = new InBondMessageSendingObject(moveHeader, bill, InBondMessageType.AirInBondAdd);
			AssertEquals(bill.B0_MasterBillNumber, sendingObject.US_MasterBillNumber);
			sendingObject = new InBondMessageSendingObject(moveHeader, bill, InBondMessageType.AirBillDelete);
			AssertEquals(bill.B0_MasterBillNumber, sendingObject.US_MasterBillNumber);
			bill.B0_HouseBillNumber = "HSB00000001";
			AssertEquals("MTB00000001/HSB00000001", sendingObject.US_MasterBillNumber);
		}

		public void TestNoMaxLengthExceptionOnUS_HouseBillNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC#$000075911111222223333344444";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();

			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A62123456789   ABC          00000100123456789012 N                            
20ABC 40                                     4701040115                         
30A 0001777 39011302        NJC000000758                                        
328888XJ501                                                                     
30A 0002777 39011302        NJC000075911                                        
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAdd Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)             :A
 Inbond Entry Type (4-5)       :62
 Inbond Number (6-17)          :123456789
 Carrier Code (18-21)          :ABC
 Value (31-38)                 :100
 Inbond Carrier I D (39-50)    :123456789012
 B T A F D A Indicator (52-52) :N

----------------INBQP20-----------------
 Carrier Code (3-6)                           :ABC
 Mode Of Transport M O T Code (7-8)           :40
 Port Of Importing Conveyance Arrival (46-49) :4701
 Estimated Date Of Arrival (50-55)            :01-Apr-15

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000758

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0002
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000075911

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", messageText);
		}

		public void TestUS_HouseBillNumber()
		{
			using (Customs.Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				var moveHeader = header.MovementHeaders.AddNew();
				var bill = header.Bills.AddNew();
				var moveDetail = header.MovementDetails.AddNew();
				moveDetail.B9_B0 = bill.PK;
				moveDetail.B9_BM = moveHeader.PK;
				bill.B0_MasterBillNumber = "MTB00000001";
				bill.B0_IssuerCode = "APLU";
				bill.B0_HouseBillNumber = "HB00000001";
				bill.B0_HouseBillIssuerCode = "BPLU";
				var sendingObject = new InBondMessageSendingObject(moveHeader, bill, InBondMessageType.BillOfLadingLevelArrival);
				AssertEquals(bill.B0_HouseBillNumber, sendingObject.US_HouseBillNumber);
				AssertEquals(bill.B0_HouseBillIssuerCode, sendingObject.US_HouseBillIssuer);
				header.BH_ImportTransportMode = TransportModeCodes.Codes.AirContainer;
				sendingObject = new InBondMessageSendingObject(moveHeader, bill, InBondMessageType.BillOfLadingLevelArrival);
				AssertEquals(ZString.Empty, sendingObject.US_HouseBillNumber);
				AssertEquals(ZString.Empty, sendingObject.US_HouseBillIssuer);
			}
		}

		public void TestBuildArriveContainerAtDestinationMessage()
		{
			using (Customs.Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				var moveHeader = header.MovementHeaders.AddNew();
				var bill = header.Bills.AddNew();
				var moveDetail = header.MovementDetails.AddNew();
				moveDetail.B9_B0 = bill.PK;
				moveDetail.B9_BM = moveHeader.PK;
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "Container1";
				moveHeader.InBondNumber = "123456789";
				moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
				moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
				moveHeader.BM_DestinationPortCode = "1234";
				moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
				moveHeader.BM_FIRMS = "XXXX";
				header.BH_CarrierSCAC = "ABC";
				header.BH_VoyageNumber = "1234A";
				header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
				bill.B0_MasterBillNumber = "12345678901";
				bill.B0_HouseBillNumber = "123456789012";
				bill.B0_HouseBillIssuerCode = "ABCD";
				Factory.Save();
				var sendingObject = new InBondMessageSendingObject(moveHeader, bill, container, InBondMessageType.ContainerLevelArrival);
				sendingObject.US_DiversionPortCode = "1401";
				sendingObject.US_DiversionDate = new ZDateTime(2019, 02, 18, 18, 20, 10);
				sendingObject.US_DiversionInBondCarrierID = "123";
				sendingObject.US_DiversionCarrierSCAC = "DRF";
				var message = sendingObject.US_MessageContents;
				AssertMultilineASCIIEquals("Diversion Message generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)                             :3
 Inbond Number (4-15)                          :123456789
 Master Bill Of Lading (20-31)                 :12345678901
 F I R M S Location On In Bond Arrival (48-51) :XXXX
 Container Number (64-77)                      :CONTAINER1

----------------INBWP20-----------------
 Date (3-8)              :01-Sep-11
 Time (9-14)             :171000
 Port Of Arrival (15-18) :1234
 Export M O T (56-57)    :10

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
			}
		}

		public void TestBuildExportContainerAtDestinationMessage()
		{
			using (Customs.Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				var moveHeader = header.MovementHeaders.AddNew();
				var bill = header.Bills.AddNew();
				var moveDetail = header.MovementDetails.AddNew();
				moveDetail.B9_B0 = bill.PK;
				moveDetail.B9_BM = moveHeader.PK;
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "Container1";
				moveHeader.InBondNumber = "123456789";
				moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
				moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
				moveHeader.BM_DestinationPortCode = "1234";
				moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
				moveHeader.BM_FIRMS = "XXXX";
				header.BH_CarrierSCAC = "ABC";
				header.BH_VoyageNumber = "1234A";
				header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
				bill.B0_MasterBillNumber = "12345678901";
				bill.B0_HouseBillNumber = "123456789012";
				Factory.Save();
				var sendingObject = new InBondMessageSendingObject(moveHeader, bill, container, InBondMessageType.ContainerLevelExportation);
				sendingObject.US_DiversionPortCode = "1401";
				sendingObject.US_DiversionDate = new ZDateTime(2019, 02, 18, 18, 20, 10);
				sendingObject.US_DiversionInBondCarrierID = "123";
				sendingObject.US_DiversionCarrierSCAC = "DRF";
				var message = sendingObject.US_MessageContents;
				AssertMultilineASCIIEquals("Diversion Message generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)                             :7
 Inbond Number (4-15)                          :123456789
 Master Bill Of Lading (20-31)                 :12345678901
 F I R M S Location On In Bond Arrival (48-51) :XXXX
 Container Number (64-77)                      :CONTAINER1

----------------INBWP20-----------------
 Date (3-8)              :31-Aug-10
 Time (9-14)             :160050
 Port Of Arrival (15-18) :1234
 Export M O T (56-57)    :10

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
			}
		}

		public void TestFieldsReadonly()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.BillOfLadingLevelArrival);
			Assert(sendingObject.US_ArrivalDateInfo.ReadOnly);
			Assert(sendingObject.US_ExportDateInfo.ReadOnly);
			Assert(sendingObject.US_ExportConveyanceInfo.ReadOnly);
			Assert(sendingObject.US_ExportTransportModeInfo.ReadOnly);
			Assert(sendingObject.US_ArrivalFirmsCodeInfo.ReadOnly);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelArrival);
			Assert(!sendingObject.US_ArrivalDateInfo.ReadOnly);
			Assert(!sendingObject.US_ExportDateInfo.ReadOnly);
			Assert(!sendingObject.US_ExportConveyanceInfo.ReadOnly);
			Assert(!sendingObject.US_ExportTransportModeInfo.ReadOnly);
			Assert(!sendingObject.US_ArrivalFirmsCodeInfo.ReadOnly);
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveHeader3 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew(bill1.PK);
			var moveDetail2 = moveHeader2.MovementDetails.AddNew(bill2.PK);
			var moveDetail3 = moveHeader3.MovementDetails.AddNew(bill3.PK);
			Factory.Save();
			var messageType = InBondMessageType.BillOfLadingLevelArrival;
			var master = new InBondMessageSendingHeaderObject(header.PK, messageType, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var sendingObjects = master.SendingObjects;
			AssertEquals(3, sendingObjects.Count);
			Assert(!sendingObjects[0].US_ShouldSendInfo.ReadOnly);
			Assert(!sendingObjects[1].US_ShouldSendInfo.ReadOnly);
			Assert(!sendingObjects[2].US_ShouldSendInfo.ReadOnly);
			sendingObjects[0].US_ShouldSend = true;
			Assert(!sendingObjects[0].US_ShouldSendInfo.ReadOnly);
			Assert(!sendingObjects[1].US_ShouldSendInfo.ReadOnly);
			Assert(!sendingObjects[2].US_ShouldSendInfo.ReadOnly);
		}

		public void TestDiversionMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_Code = "A1";
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DiversionRequest);
			sendingObject.US_DiversionDate = ZDateTime.BrettsBirthday;
			sendingObject.US_OA_DiversionInBondCarrier = orgHeader.MainAddress.PK;
			sendingObject.US_DiversionCarrierSCAC = "TWAD";
			sendingObject.US_DiversionInBondCarrierID = "10210";
			sendingObject.US_DiversionPortCode = "1101";
			var iInbondMessageSendingData = sendingObject as IInbondMessageSendingData;
			AssertEquals(ZDateTime.BrettsBirthday, iInbondMessageSendingData.DiversionDateTime);
			AssertEquals("1101", iInbondMessageSendingData.PortCode);
			AssertEquals("TWAD", iInbondMessageSendingData.InBondCarrierCode);
			AssertEquals("10210", iInbondMessageSendingData.BondedCarrierID);
			moveHeader.BM_DestinationPortCode = "1103";
			moveHeader.BM_OA_InBondCarrier = orgHeader.MainAddress.PK;
			moveHeader.BM_InBondCarrierID = "10210";
			moveHeader.BM_InBondCarrierSCAC = "TWAD";
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DiversionRequest);
			Assert(!sendingObject.US_DiversionDate.IsEmpty);
			AssertEquals("10210", sendingObject.US_DiversionInBondCarrierID);
			AssertEquals(orgHeader.MainAddress.PK, sendingObject.US_OA_DiversionInBondCarrier);
			AssertEquals("TWAD", sendingObject.US_DiversionCarrierSCAC);
			sendingObject.US_ShouldSend = true;
			sendingObject.US_DiversionDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_DiversionDateInfo, MandatoryValidation.YouHaveNotEntered);
			sendingObject.US_DiversionDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(sendingObject.US_DiversionDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateForDiversionPortCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1103", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DiversionRequest);
			sendingObject.US_ShouldSend = true;
			sendingObject.ValidateUS_DiversionPortCode();
			AssertHasMessageErrorContaining(sendingObject.US_DiversionPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			sendingObject.US_DiversionPortCode = "1103";
			AssertNoMessageErrorContaining(sendingObject.US_DiversionPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			sendingObject.US_DiversionPortCode = "*1";
			AssertHasMessageError(sendingObject.US_DiversionPortCodeInfo, ListValidation.InvalidCodeMessageError);
			sendingObject.US_DiversionPortCode = "1103";
			AssertNoMessageError(sendingObject.US_DiversionPortCodeInfo, ListValidation.InvalidCodeMessageError);
			var outgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = moveHeader;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = @"B013901SV9QP                                               HYEDUSCMT_193715     10A62400002072   FDX 1401428990000500071-056200300YN                            Y  3901SV9QP00015";
			Factory.Save();
			moveHeader.Reload();
			moveHeader.Messages.Load();
			sendingObject.US_DiversionPortCode = "1401";
			AssertHasMessageErrorContaining(sendingObject.US_DiversionPortCodeInfo, "Port Code must be different than original US Port of Destination");
			sendingObject.US_DiversionPortCode = "1103";
			AssertNoMessageErrorContaining(sendingObject.US_DiversionPortCodeInfo, "Port Code must be different than original US Port of Destination");
		}

		public void TestBulidDiversionRequestMessage()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_FIRMS = "XXXX";
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "123456789012";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DiversionRequest);
			sendingObject.US_DiversionPortCode = "1401";
			sendingObject.US_DiversionDate = new ZDateTime(2019, 02, 18, 18, 20, 10);
			sendingObject.US_DiversionInBondCarrierID = "123";
			sendingObject.US_DiversionCarrierSCAC = "DRF";
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("Diversion Message generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)    :Z
 Inbond Number (4-15) :123456789

----------------INBWP20-----------------
 Date (3-8)                  :18-Feb-19
 Time (9-14)                 :182010
 Port Of Arrival (15-18)     :1401
 Inbond Carrier Code (19-22) :DRF
 Bonded Carrier I D (23-34)  :123

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
			sendingObject.US_DiversionPortCode = "1101";
			message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("Should re-generated message context", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)    :Z
 Inbond Number (4-15) :123456789

----------------INBWP20-----------------
 Date (3-8)                  :18-Feb-19
 Time (9-14)                 :182010
 Port Of Arrival (15-18)     :1101
 Inbond Carrier Code (19-22) :DRF
 Bonded Carrier I D (23-34)  :123

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestInBondNumberHolderReplace()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000759";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			Factory.Save();
			AssertNotContains(MQEDIMessage.AirInBondNumberPlaceHolder, message.EM_MessageText);
		}

		[TestDate(2016, 11, 16)]
		public void TestPieceCountUnderContainer1()
		{
			var moveHeader = SetPieceCountInContainerAndCommodities(105, 0, 0);
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("InBondAdd generated with PieceCount1", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A61001002492   TOWE1101     0000000120-064032900 N                            
20TOWE61  ADMIRAL BULKER         123         1101111516                         
30A 0001A01 AAAAA                                                   0000000105  
328888XJ501                                                                     
40999990000000105BAG  0000000005KG                                              
65NC                                                                            
703920995000 000000250000000020KG                                               
710000000105INSTRUMENT STAND                             BAG                    
721_MARK                                                                        
709403200030 000000850000000080KG                                               
71          BARIATRIC STEP STOOL                         BAG                    
722_MARK                                                                        
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 11, 16)]
		public void TestPieceCountUnderContainer2()
		{
			var moveHeader = SetPieceCountInContainerAndCommodities(0, 85, 20);
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("InBondAdd generated with PieceCount2", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A61001002492   TOWE1101     0000000120-064032900 N                            
20TOWE61  ADMIRAL BULKER         123         1101111516                         
30A 0001A01 AAAAA                                                   0000000105  
328888XJ501                                                                     
40999990000000105BAG  0000000005KG                                              
65NC                                                                            
703920995000 000000250000000020KG                                               
710000000085INSTRUMENT STAND                             BAG                    
721_MARK                                                                        
709403200030 000000850000000080KG                                               
710000000020BARIATRIC STEP STOOL                         BAG                    
722_MARK                                                                        
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 11, 16)]
		public void TestPieceCountUnderContainer3()
		{
			var moveHeader = SetPieceCountInContainerAndCommodities(105, 85, 20);
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("InBondAdd generated with PieceCount3", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A61001002492   TOWE1101     0000000120-064032900 N                            
20TOWE61  ADMIRAL BULKER         123         1101111516                         
30A 0001A01 AAAAA                                                   0000000105  
328888XJ501                                                                     
40999990000000105BAG  0000000005KG                                              
65NC                                                                            
703920995000 000000250000000020KG                                               
710000000105INSTRUMENT STAND                             BAG                    
721_MARK                                                                        
709403200030 000000850000000080KG                                               
71          BARIATRIC STEP STOOL                         BAG                    
722_MARK                                                                        
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
		}

		CusInBondMoveHeader SetPieceCountInContainerAndCommodities(int pcContainer, int pcCommodity1, int pcCommodity2)
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.InBondNumber = "001002492";
			moveHeader.BM_InBondCarrierSCAC = "TOWE";
			moveHeader.BM_MonetaryValue = 1;
			moveHeader.BM_InBondCarrierID = "20-064032900";
			moveHeader.BM_DestinationPortCode = "1101";
			header.BH_ImportConveyanceName = "ADMIRAL BULKER";
			header.BH_VoyageNumber = "123";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ImportConveyanceName = "ADMIRAL BULKER";
			header.BH_ImportTransportMode = InbondCommonTypeList.Codes._1ImmediateTransport;
			header.BH_PortUnladingDCode = "1101";
			header.BH_ETA = new ZDateTime(2016, 11, 15);
			header.BH_CarrierSCAC = "TOWE";
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill1.B0_IssuerCode = "A01";
			bill1.B0_MasterBillNumber = "AAAAA";
			bill1.B0_ManifestQty = 105;
			bill1.B0_ManifestUQ = InBondManifestUQList.Codes.BAG;
			bill1.B0_Weight = 5;
			bill1.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			bill1.B0_PortOfLadingKCode = "99999";
			moveDetail.B9_B0 = bill1.PK;
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			container.BC_PieceCount = pcContainer;
			var commodity1 = container.Commodities.AddNew();
			commodity1.BY_MonetaryValue = 25;
			commodity1.BY_GrossWeight = 20;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			commodity1.BY_HarmonisedTariff = "3920.99.5000";
			commodity1.BY_Description = "INSTRUMENT STAND";
			commodity1.BY_MarksAndNumbers = "1_MARK";
			commodity1.BY_PieceCount = pcCommodity1;
			var commodity2 = container.Commodities.AddNew();
			commodity2.BY_MonetaryValue = 85;
			commodity2.BY_GrossWeight = 80;
			commodity2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			commodity2.BY_HarmonisedTariff = "9403.20.0030";
			commodity2.BY_Description = "BARIATRIC STEP STOOL";
			commodity2.BY_MarksAndNumbers = "2_MARK";
			commodity2.BY_PieceCount = pcCommodity2;
			Factory.Save();
			return moveHeader;
		}

		public void TestAirBondAddAfterMigrationDate()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000759";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A62123456789   ABC          00000100123456789012 N                            
20ABC 40                                     4701040115                         
30A 0001777 39011302        NJC000000758                                        
328888XJ501                                                                     
30A 0002777 39011302        NJC000000759                                        
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAdd Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)             :A
 Inbond Entry Type (4-5)       :62
 Inbond Number (6-17)          :123456789
 Carrier Code (18-21)          :ABC
 Value (31-38)                 :100
 Inbond Carrier I D (39-50)    :123456789012
 B T A F D A Indicator (52-52) :N

----------------INBQP20-----------------
 Carrier Code (3-6)                           :ABC
 Mode Of Transport M O T Code (7-8)           :40
 Port Of Importing Conveyance Arrival (46-49) :4701
 Estimated Date Of Arrival (50-55)            :01-Apr-15

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000758

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0002
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000759

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", messageText);
		}

		public void TestAirBondAddAfterMigrationDateWithFTZ()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			header.BH_FTZMove = true;
			header.BH_FIRMS = "A2";
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000759";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A62123456789   ABC          00000100123456789012YN                            
20ABC 40                                     4701040115A2                       
30A 0001777 39011302        NJC000000758                                        
328888XJ501                                                                     
30A 0002777 39011302        NJC000000759                                        
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAdd Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)                              :A
 Inbond Entry Type (4-5)                        :62
 Inbond Number (6-17)                           :123456789
 Carrier Code (18-21)                           :ABC
 Value (31-38)                                  :100
 Inbond Carrier I D (39-50)                     :123456789012
 Foreign Trade Zone Warehouse Indicator (51-51) :Y
 B T A F D A Indicator (52-52)                  :N

----------------INBQP20-----------------
 Carrier Code (3-6)                           :ABC
 Mode Of Transport M O T Code (7-8)           :40
 Port Of Importing Conveyance Arrival (46-49) :4701
 Estimated Date Of Arrival (50-55)            :01-Apr-15
 Foreign Trade Zone F I R M S Code (56-59)    :A2

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000758

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0002
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000759

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", messageText);
		}

		public void TestAirBondDeteleAfterMigrationDate()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000758";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondDelete);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondDelete generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10D62123456789   ABC          00000000                                          
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAmend);
			message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondAmend generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10D62123456789   ABC          00000000                                          
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAmend Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)       :D
 Inbond Entry Type (4-5) :62
 Inbond Number (6-17)    :123456789
 Carrier Code (18-21)    :ABC
 Value (31-38)           :0

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01
", messageText);
		}

		public void TestNonAirBondWithMoveFromWHS()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = "30";
			header.BH_PortUnladingDCode = "4701";
			header.BH_ETA = new ZDate(2019, 01, 30);
			header.BH_FTZMove = true;
			header.BH_FIRMS = "W235";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "RDWY";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "11-987654321";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "AABB";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000758";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("DepartureAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A61123456789   RDWY         0000010011-987654321YN                            
30A 0001RDWY77739011302                                                         
328888XJ501                                                                     
30A 0002AABB77739011302                                                         
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			bill1.B0_IssuerCode = ZString.Empty;
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			message = sendingObject.Send();
			AssertMultilineASCIIEquals("DepartureAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A61123456789   RDWY         0000010011-987654321YN                            
30A 0001RDWY77739011302                                                         
328888XJ501                                                                     
30A 0002AABB77739011302                                                         
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
		}

		public void TestAmendMessageAfterMigrationDate()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000758";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			sendingObject.Send();
			moveHeader.Reload();
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAmend);
			sendingObject.Send();
			moveHeader.GeneratePendingOriginalForAmendment(InBondMessageType.AirInBondAmend);
			moveHeader.Messages.Load();
			AssertEquals(3, moveHeader.Messages.Count);
			var hasQXMessage = moveHeader.Messages.Cast<EDIMessage>().Any(x => x.GetMessageBlocks<US.Messaging.Business.MessageBuildingBlocks.Input.AIRQX10>().Count > 0);
			Assert("Should not has any QX messages", !hasQXMessage);
			var countQPMessage = moveHeader.Messages.Cast<EDIMessage>().Count(x => x.GetMessageBlocks<US.Messaging.Business.MessageBuildingBlocks.ACE.Input.INBQP10>().Count > 0);
			Assert("Should send QP messages", countQPMessage == 3);
		}

		public void TestAirBondDeteleAfterMigrationDateButAddByQX()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "T1T";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "T1T";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000758";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			sendingObject.Send();
			moveHeader.Reload();
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondDelete);
			var message = sendingObject.Send();
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAmend Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)       :D
 Inbond Entry Type (4-5) :62
 Inbond Number (6-17)    :123456789
 Carrier Code (18-21)    :ABC
 Value (31-38)           :0

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01
", messageText);
		}

		public void TestSendWithAirEntireInBondArrivalAfterMigrationDate()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			bill.B0_MasterBillNumber = "77739011302";
			bill.B0_HouseBillNumber = "NJC000000758";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondArrival);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirEntireInBondArrival generated", @"B  8888XJ5WP                                01             <<MSGNO PLACEHOLDER>>
101123456789                                                                    
201109011710001234                                     40                       
Y  8888XJ5WP                                01", message.EM_FormattedMessageText);
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirEntireInBondArrival Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)    :1
 Inbond Number (4-15) :123456789

----------------INBWP20-----------------
 Date (3-8)              :01-Sep-11
 Time (9-14)             :171000
 Port Of Arrival (15-18) :1234
 Export M O T (56-57)    :40

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", messageText);
		}

		public void TestAirBondAddWithoutWarning()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			try
			{
				var sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAmend);
				header.BH_PostDepartureOnly = true;
				sender.US_ShouldSend = true;
				AssertNoWarning(sender.US_ShouldSendInfo, Enterprise.Customs.US.InBond.Business.ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance(sender.US_InBondNumber).ToString());
				header.BH_PostDepartureOnly = false;
				sender.US_ShouldSend = true;
				AssertHasWarning(sender.US_ShouldSendInfo, Enterprise.Customs.US.InBond.Business.ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance(sender.US_InBondNumber).ToString());
				moveHeader.LogManager.AddAClearLogIfNecessary(ZString.Empty, ImportMessageStatusList.Codes.ClearDepartureOriginal);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoWarning(sender.US_ShouldSendInfo, Enterprise.Customs.US.InBond.Business.ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance(sender.US_InBondNumber).ToString());
			}
			finally
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		public void TestValidateUS_ShouldSend_InBondNumberAllocation()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			try
			{
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				var moveHeaderInDifferentFactory = factory2.Load<CusInBondMoveHeader>(moveHeader.PK);
				try
				{
					var senderInDifferentFactory = new InBondMessageSendingObject(moveHeaderInDifferentFactory, InBondMessageType.DepartureAdd);
					foreach (var messageType in new[] { InBondMessageType.DepartureAdd, InBondMessageType.DepartureAmend, InBondMessageType.AirInBondAdd, InBondMessageType.AirInBondAmend, InBondMessageType.BondedWarehouseUpdate })
					{
						senderInDifferentFactory.US_ShouldSend = true;
						var sender = new InBondMessageSendingObject(moveHeader, messageType);
						sender.US_ShouldSend = true;
						var message = ValidationConstants.MoveHeader.InBondNumberAllocationIsInProgress(moveHeader.GetInBondNumberAllocationMutexLockInfo());
						AssertHasError(messageType.ToString(), sender.US_ShouldSendInfo, message);
						sender.US_ShouldSend = false;
						AssertNoError(messageType.ToString(), sender.US_ShouldSendInfo, message);
						senderInDifferentFactory.US_ShouldSend = false;
						sender.US_ShouldSend = true;
						AssertNoError(messageType.ToString(), sender.US_ShouldSendInfo, message);
						sender.US_ShouldSend = false;
						AssertNoError(messageType.ToString(), sender.US_ShouldSendInfo, message);
					}
				}
				finally
				{
					moveHeaderInDifferentFactory.UnLockInBondNumberAllocationMutex();
				}
			}
			finally
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		public void TestUS_WarehouseAddressDetail()
		{
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.BondedWarehouseUpdate);
			AssertEquals(helper.Warehouse.MainAddress.AddressAsASingleLine, sender.US_WarehouseAddressDetail);
		}

		public void TestValidatingForWarehouseMessageType()
		{
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var importer = helper.Importer;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "EN31";
			var moveHeader = header.MovementHeaders.AddNew();
			try
			{
				helper.Warehouse.OH_RL_NKClosestPort = "AUSYD";
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				var sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.BondedWarehouseUpdate);
				sender.US_ShouldSend = true;
				AssertHasErrorContaining(sender.US_ShouldSendInfo, moveHeader.GetWarehouseShouldBeInsideHeaderCountryMessage());
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				helper.Warehouse.OH_RL_NKClosestPort = "USLAX";
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoErrorContaining(sender.US_ShouldSendInfo, moveHeader.GetWarehouseShouldBeInsideHeaderCountryMessage());
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				var commodity = container.Commodities.AddNew();
				sender.ValidateUS_ShouldSend();
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_PartNumber = "SDF";
				sender.ValidateUS_ShouldSend();
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_InvoiceQuantity = 10m;
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_WarehouseEntryNumber = "EN31";
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_WarehouseEntryLineNo = 1;
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_WarehouseEntryNumber = "";
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				commodity.BY_WarehouseEntryNumber = "EN31";
				var commodity2 = container.Commodities.AddNew();
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AProductIsRequiredForBondedWarehousingCommodity);
				USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				AssertHasErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AProductIsRequiredForBondedWarehousingCommodity);
				commodity2.BY_PartNumber = helper.Part.OP_PartNum;
				sender.ValidateUS_ShouldSend();
				AssertNoErrorContaining(sender.US_ShouldSendInfo, ValidationConstants.BondedWarehouse.AProductIsRequiredForBondedWarehousingCommodity);
			}
			finally
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		public void TestValidatingForBM_MoveToFTZIsRequired()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@", WarehouseTypes.Codes.FreeTradeZone);
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			try
			{
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "EN31";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				Factory.Save();
				var sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.BondedWarehouseUpdate);
				sender.US_ShouldSend = true;
				var messageError = ValidationConstants.MoveHeader.MoveToFTZIndicatorIsRequired.ToString();
				AssertHasErrorContaining(sender.US_ShouldSendInfo, messageError);
				moveHeader.BM_MoveToFTZ = YesNoDefaultList.Codes.Yes;
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoErrorContaining(sender.US_ShouldSendInfo, messageError);
			}
			finally
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		public void TestSendWhsTransaction()
		{
			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var importer = helper.Importer;
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer()
			{ ThrowExceptionOnInvalidOperation = false };
			var header = Factory.New<CusInBondHeader>();
			header.MessageInitiator = messageInitiator;
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			try
			{
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.BondedWarehouseCancel);
				sender.US_ShouldSend = true;
				sender.SendWhsTransaction();
				AssertEquals(0, messageInitiator.PastYesNoQuestionsAsked.Count);
				AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", messageInitiator.Warning);
				AssertEquals("", moveHeader.InBondNumber);
				moveHeader.UnLockInBondNumberAllocationMutex();
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.BondedWarehouseUpdate);
				sender.US_ShouldSend = true;
				sender.SendWhsTransaction();
				AssertEquals(0, messageInitiator.PastYesNoQuestionsAsked.Count);
				AssertEquals(@"Error - Cannot Import Order
This Order has no Lines.", messageInitiator.InvalidOperationText);
				AssertNotEquals("", moveHeader.InBondNumber);
			}
			finally
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		public void TestSendWithAirInBondAdd()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABCD";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 12345678;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_PortUnladingDCode = "1234";
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "12345";
			header.BH_ETA = new ZDate(2011, 09, 01);
			bill1.B0_MasterBillNumber = "12345678901";
			bill1.B0_HouseBillNumber = "123456789012";
			moveDetail1.B9_PreviousITNumber = "12345678901";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_MasterBillNumber = "98765432109";
			bill2.B0_HouseBillNumber = "987654321098";
			moveDetail2.B9_PreviousITNumber = "98765432109";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A62123456789   ABCD12341234512345678123456789012 N                            
30A 0001    12345678901                                 12345678901             
328888XJ501                                                                     
30A 0002    98765432109                                 98765432109             
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
		}

		public void TestSendWithAirInBondDelete()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABCD";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 12345678;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_PortUnladingDCode = "1234";
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "12345";
			header.BH_ETA = new ZDate(2011, 09, 01);
			bill1.B0_MasterBillNumber = "12345678901";
			bill1.B0_HouseBillNumber = "123456789012";
			moveDetail1.B9_PreviousITNumber = "12345678901";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_MasterBillNumber = "98765432109";
			bill2.B0_HouseBillNumber = "987654321098";
			moveDetail2.B9_PreviousITNumber = "98765432109";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondDelete);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondDelete generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10D62123456789   ABCD         00000000                                          
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAmend);
			message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondDelete generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10D62123456789   ABCD         00000000                                          
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
		}

		public void TestSendWithAirEntireInBondArrival()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "123456789012";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondArrival);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirEntireInBondArrival InBondMessage generated", @"B  8888XJ5WP                                01             <<MSGNO PLACEHOLDER>>
101123456789                                                                    
201109011710001234                                     40                       
Y  8888XJ5WP                                01", message.EM_FormattedMessageText);
		}

		public void TestSendWithAirEntireInBondExportation()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
			moveHeader.BM_DestinationPortCode = "4321";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "123456789012";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondExportation);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirEntireInBondExportation InBondMessage generated", @"B  8888XJ5WP                                01             <<MSGNO PLACEHOLDER>>
105123456789                                                                    
201008311600504321                                     40                       
Y  8888XJ5WP                                01", message.EM_FormattedMessageText);
		}

		public void TestMessageContentsWithAirInBondAdd()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABCD";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 12345678;
			moveHeader.BM_InBondCarrierID = "123456789012";
			moveHeader.BM_PedimentoNumber = "15632456328965";
			header.BH_PortUnladingDCode = "1234";
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "12345";
			header.BH_ETA = new ZDate(2011, 09, 01);
			bill1.B0_MasterBillNumber = "12345678901";
			bill1.B0_HouseBillNumber = "123456789012";
			moveDetail1.B9_PreviousITNumber = "12345678901";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_MasterBillNumber = "98765432109";
			bill2.B0_HouseBillNumber = "987654321098";
			moveDetail2.B9_PreviousITNumber = "98765432109";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAdd message contents", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)                   :A
 Inbond Entry Type (4-5)             :62
 Inbond Number (6-17)                :123456789
 Carrier Code (18-21)                :ABCD
 U S Port Of Destination (22-25)     :1234
 Port Of Foreign Destination (26-30) :12345
 Value (31-38)                       :12345678
 Inbond Carrier I D (39-50)          :123456789012
 B T A F D A Indicator (52-52)       :N

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Sequence Of Master Bill Of Lading (13-24) :12345678901
 Previous Inbond Number (57-68)                   :12345678901

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0002
 Issuer Sequence Of Master Bill Of Lading (13-24) :98765432109
 Previous Inbond Number (57-68)                   :98765432109

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
			header.BH_VoyageNumber = "QF7589";
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAdd message contents", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)                   :A
 Inbond Entry Type (4-5)             :62
 Inbond Number (6-17)                :123456789
 Carrier Code (18-21)                :ABCD
 U S Port Of Destination (22-25)     :1234
 Port Of Foreign Destination (26-30) :12345
 Value (31-38)                       :12345678
 Inbond Carrier I D (39-50)          :123456789012
 B T A F D A Indicator (52-52)       :N

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Sequence Of Master Bill Of Lading (13-24) :12345678901
 Previous Inbond Number (57-68)                   :12345678901

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0002
 Issuer Sequence Of Master Bill Of Lading (13-24) :98765432109
 Previous Inbond Number (57-68)                   :98765432109

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestMessageContentWithAirInBondDeleteAndAmend()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABCD";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 12345678;
			moveHeader.BM_InBondCarrierID = "123456789012";
			moveHeader.BM_PedimentoNumber = "15632456328965";
			header.BH_PortUnladingDCode = "1234";
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "12345";
			header.BH_ETA = new ZDate(2011, 09, 01);
			bill1.B0_MasterBillNumber = "12345678901";
			bill1.B0_HouseBillNumber = "123456789012";
			moveDetail1.B9_PreviousITNumber = "12345678901";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_MasterBillNumber = "98765432109";
			bill2.B0_HouseBillNumber = "987654321098";
			moveDetail2.B9_PreviousITNumber = "98765432109";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondDelete);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondDelete generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)       :D
 Inbond Entry Type (4-5) :62
 Inbond Number (6-17)    :123456789
 Carrier Code (18-21)    :ABCD
 Value (31-38)           :0

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAmend);
			message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondDelete generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)       :D
 Inbond Entry Type (4-5) :62
 Inbond Number (6-17)    :123456789
 Carrier Code (18-21)    :ABCD
 Value (31-38)           :0

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestArrivalFieldsValidationOnlyForMoveHeaderToBeSent()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "123456789012";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "123456789";
			moveHeader1.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader1, InBondMessageType.AirEntireInBondArrival);
			sendingObject.US_ShouldSend = true;
			sendingObject.US_ArrivalFirmsCode = ZString.Empty;
			sendingObject.US_ArrivalDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_ArrivalFirmsCodeInfo, "You have not entered an Arrival Firms Code.");
			AssertHasMessageErrorContaining(sendingObject.US_ArrivalDateInfo, "You have not entered an Arrival Date.");
			sendingObject.US_ArrivalDate = DateTime.Today.AddDays(2);
			AssertHasMessageError(sendingObject.US_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
			sendingObject.US_ArrivalFirmsCode = "XXX";
			AssertHasMessageErrorContaining(sendingObject.US_ArrivalFirmsCodeInfo, ListValidation.InvalidCodeMessageError);
			sendingObject.US_ShouldSend = false;
			sendingObject.US_ArrivalFirmsCode = ZString.Empty;
			AssertNoMessageErrorContaining(sendingObject.US_ArrivalFirmsCodeInfo, "You have not entered an Arrival Firms Code.");
			sendingObject.US_ArrivalDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(sendingObject.US_ArrivalDateInfo, "You have not entered an Arrival Date.");
			sendingObject.US_ArrivalFirmsCode = "XXX";
			AssertNoMessageErrorContaining(sendingObject.US_ArrivalDateInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestExportationFieldsValidationOfOnlyforMoveHeadersToBeSent()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "123456789";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader1, InBondMessageType.AirEntireInBondExportation);
			sendingObject.US_ShouldSend = true;
			sendingObject.US_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_ExportDateInfo, "You have not entered an Export Date.");
			moveHeader1.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			sendingObject.US_ExportConveyance = ZString.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_ExportConveyanceInfo, "You have not entered an Export Conveyance.");
			sendingObject.US_ExportConveyance = "ADB";
			sendingObject.US_ExportTransportMode = "Z";
			AssertHasMessageErrorContaining(sendingObject.US_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			sendingObject.US_ExportTransportMode = ZString.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_ExportTransportModeInfo, "You have not entered an Export Transport Mode.");
			sendingObject.US_ShouldSend = false;
			sendingObject.US_ExportDate = ZDateTime.Empty;
			sendingObject.US_ExportTransportMode = ZString.Empty;
			sendingObject.US_ExportConveyance = ZString.Empty;
			AssertNoMessageErrorContaining(sendingObject.US_ExportDateInfo, "You have not entered an Export Date.");
			AssertNoMessageErrorContaining(sendingObject.US_ExportConveyanceInfo, "You have not entered an Export Conveyance.");
			AssertNoMessageErrorContaining(sendingObject.US_ExportTransportModeInfo, "You have not entered an Export Transport Mode.");
		}

		public void TestExportConveyanceFieldsValidationOfOnlyforMoveHeadersToBeSent()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "123456789";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader1, InBondMessageType.AirEntireInBondExportation);
			sendingObject.US_ShouldSend = true;
			moveHeader1.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			sendingObject.US_ExportConveyance = ZString.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_ExportConveyanceInfo, "You have not entered an Export Conveyance.");
			sendingObject.US_ExportConveyance = "AABBCCDD";
			AssertNoWarningContaining(sendingObject.US_ExportConveyanceInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
			sendingObject.US_ExportConveyance = "AAAAABBBBBCCCCCDDDDDEEEEEFFFFF";
			AssertHasWarningContaining(sendingObject.US_ExportConveyanceInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
			sendingObject.US_ShouldSend = false;
			sendingObject.US_ExportConveyance = "AAAAABBBBBCCCCCDDDDDEEEEEFFFFFMAB";
			AssertNoWarningContaining(sendingObject.US_ExportConveyanceInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
		}

		public void TestTOLFieldsValidationOfOnlyforMoveHeadersToBeSent()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "123456789";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader1, InBondMessageType.InBondLevelTransferOfLiability);
			sendingObject.US_ShouldSend = true;
			sendingObject.US_TOLDate = ZDateTime.Empty;
			sendingObject.US_TOLCarrierCode = ZString.Empty;
			sendingObject.US_TOLCarrierID = ZString.Empty;
			sendingObject.US_TOLCityName = ZString.Empty;
			AssertHasMessageErrorContaining(sendingObject.US_TOLDateInfo, "You have not entered a TOL Date.");
			AssertHasMessageErrorContaining(sendingObject.US_TOLCarrierCodeInfo, "You have not entered a TOL Carrier Code.");
			AssertHasMessageErrorContaining(sendingObject.US_TOLCarrierIDInfo, "You have not entered a TOL Carrier ID.");
			AssertHasMessageErrorContaining(sendingObject.US_TOLCityNameInfo, "You have not entered a TOL City Name.");
			sendingObject.US_ShouldSend = false;
			sendingObject.US_TOLDate = ZDateTime.Empty;
			sendingObject.US_TOLCarrierCode = ZString.Empty;
			sendingObject.US_TOLCarrierID = ZString.Empty;
			sendingObject.US_TOLCityName = ZString.Empty;
			AssertNoMessageErrorContaining(sendingObject.US_TOLDateInfo, "You have not entered a TOL Date.");
			AssertNoMessageErrorContaining(sendingObject.US_TOLCarrierCodeInfo, "You have not entered a TOL Carrier Code.");
			AssertNoMessageErrorContaining(sendingObject.US_TOLCarrierIDInfo, "You have not entered a TOL Carrier ID.");
			AssertNoMessageErrorContaining(sendingObject.US_TOLCityNameInfo, "You have not entered a TOL City Name.");
			sendingObject.US_TOLStateCode = "12";
			AssertHasMessageErrorContaining(sendingObject.US_TOLStateCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestMessageContentsWithAirEntireInBondArrival()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_FIRMS = "XXXX";
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "123456789012";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondArrival);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirEntireInBondArrival InBondMessage generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)                             :1
 Inbond Number (4-15)                          :123456789
 F I R M S Location On In Bond Arrival (48-51) :XXXX

----------------INBWP20-----------------
 Date (3-8)              :01-Sep-11
 Time (9-14)             :171000
 Port Of Arrival (15-18) :1234
 Export M O T (56-57)    :40

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestMessageContentsWithAirEntireInBondExportation()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_ArrivalDate = new ZDateTime(2011, 09, 01, 17, 10, 00);
			moveHeader.BM_ExportDate = new ZDateTime(2010, 08, 31, 16, 00, 50);
			moveHeader.BM_DestinationPortCode = "4321";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_HouseBillNumber = "123456789012";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondExportation);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirEntireInBondExportation InBondMessage generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBWP10-----------------
 Action Code (3-3)    :5
 Inbond Number (4-15) :123456789

----------------INBWP20-----------------
 Date (3-8)              :31-Aug-10
 Time (9-14)             :160050
 Port Of Arrival (15-18) :4321
 Export M O T (56-57)    :40

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestMessageWithUseFirmsCode()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "ABC";
			moveHeader.BM_MonetaryValue = 100;
			moveHeader.BM_InBondCarrierID = "123456789012";
			header.BH_ImportTransportMode = "40";
			header.BH_PortUnladingDCode = "4701";
			header.BH_CarrierSCAC = "ABC";
			header.BH_ETA = new ZDate(2015, 04, 01);
			header.BH_FTZMove = true;
			header.BH_FIRMS = "W235";
			var usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "T1T";
			usCarrier.UI_ModeOfTransportation = "40";
			usCarrier.UI_Name = "Test Carrier";
			usCarrier.UI_AirwayBillPrefix = "777";
			bill1.B0_IssuerCode = "W235";
			bill1.B0_MasterBillNumber = "77739011302";
			bill1.B0_HouseBillNumber = "NJC000000758";
			moveDetail1.B9_B0 = bill1.PK;
			bill2.B0_IssuerCode = "W235";
			bill2.B0_MasterBillNumber = "77739011302";
			bill2.B0_HouseBillNumber = "NJC000000759";
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirInBondAdd);
			var message = sendingObject.Send();
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B  8888XJ5QP                                01             <<MSGNO PLACEHOLDER>>
10A62123456789   ABC          00000100123456789012YN                            
20ABC 40                                     4701040115W235                     
30A 0001777 39011302        NJC000000758                                        
328888XJ501                                                                     
30A 0002777 39011302        NJC000000759                                        
328888XJ501                                                                     
Y  8888XJ5QP                                01", message.EM_FormattedMessageText);
			var messageText = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirInBondAdd Text", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)                              :A
 Inbond Entry Type (4-5)                        :62
 Inbond Number (6-17)                           :123456789
 Carrier Code (18-21)                           :ABC
 Value (31-38)                                  :100
 Inbond Carrier I D (39-50)                     :123456789012
 Foreign Trade Zone Warehouse Indicator (51-51) :Y
 B T A F D A Indicator (52-52)                  :N

----------------INBQP20-----------------
 Carrier Code (3-6)                           :ABC
 Mode Of Transport M O T Code (7-8)           :40
 Port Of Importing Conveyance Arrival (46-49) :4701
 Estimated Date Of Arrival (50-55)            :01-Apr-15
 Foreign Trade Zone F I R M S Code (56-59)    :W235

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000758

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0002
 Issuer Code Of Master Bill Of Lading (9-12)      :777
 Issuer Sequence Of Master Bill Of Lading (13-24) :39011302
 House Bill Number (29-40)                        :NJC000000759

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11) :8888XJ501

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", messageText);
		}

		public void TestMessageContentsFullDataForDeparture()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1!10";
			subs.DG_Variant = "c";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.1";
			subs.DG_FlashPoint = "-10";
			subs.DG_PSN = "SD WHO WHERE";
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "OTT1";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 323.25m;
			moveHeader.BM_InBondCarrierID = "123456789012";
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_PortOfLadingKCode = "35987";
			bill.B0_ManifestQty = 436;
			bill.B0_ManifestUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			bill.B0_Weight = 974.64m;
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill.B0_Volume = 3.54m;
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicFeet;
			bill.B0_PlaceOfReceiptDCode = "9853";
			bill.ForeignShipper.E2_AddressOverride = ZBool.True;
			bill.ForeignShipper.E2_CompanyName = "BOB THE BUILDER";
			bill.ForeignShipper.E2_RN_NKCountryCode = ZString.Empty;
			bill.Consignee.E2_AddressOverride = ZBool.True;
			bill.Consignee.E2_CompanyName = "WENDY THE DESTROYER";
			bill.Consignee.E2_RN_NKCountryCode = ZString.Empty;
			bill.NotifyParty.E2_AddressOverride = ZBool.True;
			bill.NotifyParty.E2_CompanyName = "JACK THE NOBODY";
			bill.NotifyParty.E2_RN_NKCountryCode = ZString.Empty;
			var ref1 = bill.AdditionalReferences.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			ref1.BR_ReferenceNum = "2011CQ003948458";
			var ref2 = bill.AdditionalReferences.AddNew();
			ref2.BR_Qualifier = ReferenceQualifierList.Codes.FP;
			ref2.BR_ReferenceNum = "FP342332";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			moveDetail.B9_PreviousITType = EntryTypeList.Codes.ConsumptionFTZ;
			moveDetail.B9_PreviousITNumber = "IT3234232";
			moveDetail.B9_InBoundQty = 124;
			var snp1 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.First, "SNP1");
			var snp2 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Second, "SNP2");
			var snp3 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Third, "SNP3");
			var snp4 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Fourth, "SNP4");
			var refContType = Factory.New<RefContainer>();
			refContType.RC_Code = "RCD";
			refContType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TORU324234";
			container.BC_RC = refContType.PK;
			container.BC_Seal1 = "SL1";
			container.BC_Seal2 = "SL2";
			var commodity1 = container.Commodities.AddNew();
			commodity1.BY_HarmonisedTariff = "3020453123";
			commodity1.BY_MonetaryValue = 1500m;
			commodity1.BY_Description = "DESC 2342";
			commodity1.BY_GrossWeight = 42.43m;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			commodity1.BY_PieceCount = 492;
			commodity1.BY_MarksAndNumbers = "MARKS AND FUNNY NUMBERS";
			var commodity2 = container.Commodities.AddNew();
			commodity2.BY_HarmonisedTariff = "3020453124";
			commodity2.BY_MonetaryValue = 1600m;
			commodity2.BY_Description = "DESC 2343";
			commodity2.BY_GrossWeight = 42.43m;
			commodity2.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			commodity2.BY_PieceCount = 492;
			commodity2.BY_MarksAndNumbers = "MARKS AND FUNNY NUMBERS2";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "B!@34";
			org.OH_FullName = "THIS IS THE COMPANY";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB BROWN";
			var undg = container.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGFlashPoint = -10m;
			undg.DI_TechnicalName = "WHAT TECH NAME";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirEntireInBondExportation InBondMessage generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)                   :A
 Inbond Entry Type (4-5)             :62
 Inbond Number (6-17)                :123456789
 Carrier Code (18-21)                :OTT1
 U S Port Of Destination (22-25)     :1234
 Port Of Foreign Destination (26-30) :12345
 Value (31-38)                       :323
 Inbond Carrier I D (39-50)          :123456789012
 B T A F D A Indicator (52-52)       :Y

----------------INBQP20-----------------
 Carrier Code (3-6)                           :ABC
 Mode Of Transport M O T Code (7-8)           :11
 Importing Conveyance Name (11-33)            :APL VESSEL
 Voyage Flight Trip Number (34-38)            :1234A
 Port Of Importing Conveyance Arrival (46-49) :5687
 Estimated Date Of Arrival (50-55)            :02-Oct-12

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Sequence Of Master Bill Of Lading (13-24) :12345678901
 In Bond Quantity (69-78)                         :124

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11)   :8888XJ501
 Secondary Notify Party Code1 (12-20) :SNP2
 Secondary Notify Party Code2 (21-29) :SNP3
 Secondary Notify Party Code3 (30-38) :SNP4

----------------INBQP33-----------------
 Qualifier (3-5)             :FEN
 Reference Identifier (6-35) :2011CQ003948458

----------------INBQP33-----------------
 Qualifier (3-5)             :FP
 Reference Identifier (6-35) :FP342332

----------------INBQP40-----------------
 Foreign Port Of Lading (3-7) :35987
 Manifest Quantity (8-17)     :436
 Manifest Units (18-22)       :AE
 Weight (23-32)               :975
 Weight Unit (33-34)          :LB
 Volume (35-44)               :4
 Volume Unit (45-46)          :CF
 Place Of Prereceipt (47-63)  :9853

----------------INBQP65-----------------

----------------INBQP65-----------------
 Container Number (3-16)            :TORU324234
 Seal Number1 (17-31)               :SL1
 Seal Number2 (32-46)               :SL2
 Container Description Code (47-48) :40

----------------INBQP70-----------------
 Harmonized Number (3-12) :3020453123
 Value (14-21)            :1500
 Weight (22-31)           :43
 Weight Unit (32-33)      :LB

----------------INBQP71-----------------
 Piece Count (3-12)         :492
 Description (13-57)        :DESC 2342
 Manifest Unit Code (58-60) :AE

----------------INBQP72-----------------
 Marks And Numbers (3-47) :MARKS AND FUNNY NUMBERS

----------------INBQP70-----------------
 Harmonized Number (3-12) :3020453124
 Value (14-21)            :1600
 Weight (22-31)           :43
 Weight Unit (32-33)      :LB

----------------INBQP71-----------------
 Piece Count (3-12)         :492
 Description (13-57)        :DESC 2343
 Manifest Unit Code (58-60) :AE

----------------INBQP72-----------------
 Marks And Numbers (3-47) :MARKS AND FUNNY NUMBERS2

----------------INBQP75-----------------
 Hazardous Material Code (3-12)            :UN1!10
 Hazardous Material Class (13-16)          :2.1
 Hazardous Material Code Qualifier (17-17) :I
 Hazardous Material Description (18-47)    :SD WHO WHERE
 Hazardous Material Contact (48-71)        :BOB BROWN
 Flashpoint Temperature (72-74)            :10
 Unit Of Measure Code (75-76)              :CE
 Negative Indicator (77-77)                :N

----------------INBQP76-----------------
 Hazardous Material Classification (32-61) :WHAT TECH NAME

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestQP32_QP33_ForAMS()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "OTT1";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 323.25m;
			moveHeader.BM_InBondCarrierID = "123456789012";
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "12345678901";
			bill.B0_PortOfLadingKCode = "35987";
			bill.B0_ManifestQty = 436;
			bill.B0_ManifestUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			bill.B0_Weight = 974.64m;
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill.B0_Volume = 3.54m;
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicFeet;
			bill.B0_PlaceOfReceiptDCode = "9853";
			bill.ForeignShipper.E2_AddressOverride = ZBool.True;
			bill.ForeignShipper.E2_CompanyName = "BOB THE BUILDER";
			bill.Consignee.E2_AddressOverride = ZBool.True;
			bill.Consignee.E2_CompanyName = "WENDY THE DESTROYER";
			bill.NotifyParty.E2_AddressOverride = ZBool.True;
			bill.NotifyParty.E2_CompanyName = "JACK THE NOBODY";
			var ref1 = bill.AdditionalReferences.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			ref1.BR_ReferenceNum = "2011CQ003948458";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			moveDetail.B9_PreviousITType = EntryTypeList.Codes.ConsumptionFTZ;
			moveDetail.B9_PreviousITNumber = "IT3234232";
			moveDetail.B9_InBoundQty = 124;
			var snp1 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.First, "SNP1");
			var snp2 = moveDetail.SecondaryNotifyParties.AddNew(SecondaryNotifyPartyCodeList.Codes.Second, "SNP2");
			var refContType = Factory.New<RefContainer>();
			refContType.RC_Code = "RCD";
			refContType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TORU324234";
			container.BC_RC = refContType.PK;
			container.BC_Seal1 = "SL1";
			container.BC_Seal2 = "SL2";
			var commodity1 = container.Commodities.AddNew();
			commodity1.BY_HarmonisedTariff = "3020453123";
			commodity1.BY_MonetaryValue = 1500m;
			commodity1.BY_Description = "DESC 2342";
			commodity1.BY_GrossWeight = 42.43m;
			commodity1.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			commodity1.BY_PieceCount = 492;
			commodity1.BY_MarksAndNumbers = "MARKS AND FUNNY NUMBERS";
			Factory.Save();
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirEntireInBondExportation InBondMessage generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)                   :A
 Inbond Entry Type (4-5)             :62
 Inbond Number (6-17)                :123456789
 Carrier Code (18-21)                :OTT1
 U S Port Of Destination (22-25)     :1234
 Port Of Foreign Destination (26-30) :12345
 Value (31-38)                       :323
 Inbond Carrier I D (39-50)          :123456789012
 B T A F D A Indicator (52-52)       :Y

----------------INBQP30-----------------
 Action Code (3-3)                                :A
 Sequence Number (5-8)                            :0001
 Issuer Sequence Of Master Bill Of Lading (13-24) :12345678901
 In Bond Quantity (69-78)                         :124

----------------INBQP32-----------------
 Secondary Notify Party Code (3-11)   :8888XJ501
 Secondary Notify Party Code1 (12-20) :SNP2

----------------INBQP33-----------------
 Qualifier (3-5)             :FEN
 Reference Identifier (6-35) :2011CQ003948458

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
		}

		public void TestValidateUS_ShouldSend()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INBOND130812";
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB2";
			moveHeader2.LogManager.AddAClearLogIfNecessary(Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingDepartureOriginal, Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureOriginal);
			Factory.Save();
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(2, collection.Count);
			var sendingObject1 = collection.Cast<InBondMessageSendingObject>().FirstOrDefault(x => x.US_InBondNumber == "INBOND130812");
			AssertNotNull(sendingObject1);
			sendingObject1.US_ShouldSend = true;
			AssertHasWarning(sendingObject1.US_ShouldSendInfo, Enterprise.Customs.US.InBond.Business.ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance("INBOND130812").ToString());
			sendingObject1.US_ShouldSend = false;
			AssertNoWarning(sendingObject1.US_ShouldSendInfo, Enterprise.Customs.US.InBond.Business.ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance("INBOND130812").ToString());
			var sendingObject2 = collection.Cast<InBondMessageSendingObject>().FirstOrDefault(x => x.US_InBondNumber != "INBOND130812");
			AssertNotNull(sendingObject2);
			sendingObject2.US_ShouldSend = true;
			AssertNoWarnings(sendingObject2.US_ShouldSendInfo);
			sendingObject2.US_ShouldSend = false;
			AssertNoWarnings(sendingObject2.US_ShouldSendInfo);
			moveHeader2.LogManager.AddAClearLogIfNecessary(Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			Factory.Save();
			var collection2 = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(2, collection2.Count);
			var sendingObject21 = collection2[0];
			sendingObject21.US_ShouldSend = true;
			AssertNoWarnings(sendingObject21.US_ShouldSendInfo);
			sendingObject21.US_ShouldSend = false;
			AssertNoWarnings(sendingObject21.US_ShouldSendInfo);
			header.BH_PostDepartureOnly = true;
			moveHeader1.InBondNumber = "";
			moveHeader2.LogManager.AddAClearLogIfNecessary(Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureWithdraw);
		}

		public void TestValidateUS_ShouldSend_PostDepartureMessageOnly()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_PostDepartureOnly = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = "";
			Factory.Save();
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.InBondLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(1, collection.Count);
			var sendingObj = collection[0];
			sendingObj.US_ShouldSend = true;
			AssertHasMessageError(sendingObj.US_ShouldSendInfo, ValidationConstants.MoveHeader.InBondNumberIsRequired.ToString());
		}

		public void TestValidateUS_ShouldSend_AtLeastOneMovementDetailsExist()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			try
			{
				var sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
				var messageError = ValidationConstants.MoveHeader.AtLeastOneMovementDetailsIsRequired.ToString();
				header.BH_PostDepartureOnly = true;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				header.BH_PostDepartureOnly = false;
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertHasMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondArrival);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.AirEntireInBondExportation);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelArrival);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelExportation);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelTransferOfLiability);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				sender = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertHasMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
				moveHeader.MovementDetails.AddNew();
				sender.US_ShouldSend = false;
				sender.US_ShouldSend = true;
				AssertNoMessageErrorContaining(sender.US_ShouldSendInfo, messageError);
			}
			finally
			{
				moveHeader.UnLockInBondNumberAllocationMutex();
			}
		}

		public void TestSendINBQP10WithLongerInbondEntryType()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = "061";
			moveHeader.InBondNumber = "123456789";
			moveHeader.BM_InBondCarrierSCAC = "OTT1";
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "12345";
			moveHeader.BM_MonetaryValue = 323.25m;
			moveHeader.BM_InBondCarrierID = "123456789012";
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			var message = sendingObject.US_MessageContents;
			AssertMultilineASCIIEquals("AirEntireInBondExportation InBondMessage generated", @"---------------AABIInputB---------------
 Processing District Port Code (4-7)    :8888
 Filer Code (8-10)                      :XJ5
 Processing Filer Office Code (45-46)   :01
 Filer Preparers User Data Text (60-80) :<<MSGNO PLACEHOLDER>>

----------------INBQP10-----------------
 Action Code (3-3)               :A
 Inbond Entry Type (4-5)         :**
 Inbond Number (6-17)            :123456789
 Carrier Code (18-21)            :OTT1
 U S Port Of Destination (22-25) :1234
 Value (31-38)                   :323
 Inbond Carrier I D (39-50)      :123456789012
 B T A F D A Indicator (52-52)   :Y

---------------AABIInputY---------------
 Processing District Port Code (4-7)  :8888
 Filer Code (8-10)                    :XJ5
 Processing Filer Office Code (45-46) :01", message);
			AssertEquals("", CargoWise.Common.ErrorReporter.LastMessageReported);
		}

		public void TestEstimatedDateTimeShouldUseArrivalDateAsDefaultAndFallsBackToETADateWhenEmpty()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_ETA = new ZDate(2015, 04, 01);
			moveHeader.BM_ArrivalDate = ZDateTime.Empty;
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			AssertContains("Estimated Date Of Arrival (50-55)  :01-Apr-15", sendingObject.US_MessageContents);
			header.BH_ETA = new ZDate(2015, 04, 01);
			moveHeader.BM_ArrivalDate = new ZDate(2016, 11, 14);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			AssertContains("Estimated Date Of Arrival (50-55)  :14-Nov-16", sendingObject.US_MessageContents);
			header.BH_ETA = ZDate.Empty;
			moveHeader.BM_ArrivalDate = ZDate.Empty;
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			AssertNotContains("Estimated Date Of Arrival (50-55)", sendingObject.US_MessageContents);
			header.BH_ETA = ZDate.Empty;
			moveHeader.BM_ArrivalDate = new ZDate(2016, 11, 14);
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
			AssertContains("Estimated Date Of Arrival (50-55)  :14-Nov-16", sendingObject.US_MessageContents);
			sendingObject.US_ShouldSend = false;
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			DeclarationTestHelper.SetPreparerOfficeCode("01");
		}

		protected override void TearDown()
		{
			if (header != null)
			{
				header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}

			base.TearDown();
		}

		CusInBondHeader Header
		{
			get
			{
				return header ?? (header = Factory.New<CusInBondHeader>());
			}
		}

		CusInBondHeader header;
		protected override BusinessObject GetNewBusinessObject()
		{
			var moveHeader = Header.MovementHeaders.AddNew();
			return new InBondMessageSendingObject(moveHeader, InBondMessageType.DepartureAdd);
		}
		#endregion
	}
}
