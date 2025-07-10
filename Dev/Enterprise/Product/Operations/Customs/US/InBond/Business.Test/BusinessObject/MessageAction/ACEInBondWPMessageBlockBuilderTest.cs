using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class ACEInBondWPMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestInBondLevelMessagesAdd()
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
			var sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelArrival);
			var message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B         WP                                               <<MSGNO PLACEHOLDER>>
101123456789                                                                    
20            1234                                                              
Y         WP", message.EM_FormattedMessageText);
			moveHeader.BM_ExportDate = new ZDate(2011, 09, 13);
			moveHeader.BM_ExportLadenOn = "VESSEL 2";
			moveHeader.BM_ExportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelExportation);
			message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B         WP                                               <<MSGNO PLACEHOLDER>>
105123456789                                                                    
201109130000001234                                     40VESSEL 2               
Y         WP", message.EM_FormattedMessageText);
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SC1Z";
			moveHeader.BM_TOLCarrierCode = "SC1Z";
			moveHeader.BM_TOLCarrierID = "12-3456789XY";
			moveHeader.BM_TOLCityName = "LOS ANGELES";
			moveHeader.BM_TOLDate = new ZDate(2012, 05, 17);
			moveHeader.BM_TOLStateCode = "CA";
			sendingObject = new InBondMessageSendingObject(moveHeader, InBondMessageType.InBondLevelTransferOfLiability);
			message = sendingObject.Send();
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("AirInBondAdd generated", @"B         WP                                               <<MSGNO PLACEHOLDER>>
10A123456789                                                                    
20120517000000    SC1Z12-3456789XYLOS ANGELES        CA40VESSEL 2               
Y         WP", message.EM_FormattedMessageText);
		}
	}
}
