using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	class PSCEntrySummaryDataTest : TestCaseWithFactory
	{
		public void TestPopulateData()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_MessageText = @"B  3902SV9AE                                               6009040              
10ASV9  10002564 3902B00005226   0310 XY     Y002 2041811                       
1191-01319900091-013199001                     041411       IL                  
20AAAA3902041411A101ADMIRALENGRACHT                                             
2156890                                                                         
2200000001PK                                                                    
23MAPLU879342                                                                   
318B 891                                                                        
40  001 HTHT041111        0000000009602670000000500    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
509802006000 0000000000 0000002000                                              
509032896015 0000000850 0000000500             NO                               
6250100000313                                                                   
40  002 HTHT041111        0000000041602670000000500    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
509802006000 0000000000 0000002000                                              
509032896015 0000017000 0000010000             NO                               
6250100001500                                                                   
8950100000001813                                                                
9000000017850 00000001813 00000000000 00000000000 00000000000                   
Y  3902SV9AE".Replace("\r\n", "");

			Assert(message.MessageBlock.MessageBlocks.Count > 0);

			var headerData = new PSCEntrySummaryData(message, null);

			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionADDCVD, headerData.EntryType);
			AssertEquals("IOR Number", "91-013199000", headerData.IORNumber);
			AssertEquals("ReconType", "002", headerData.ReconType);
			AssertEquals("FTA Recon", true, headerData.FTARecon);
			AssertEquals("PortofEntry", "3902", headerData.EntryPort);
			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, headerData.PaymentType);
			AssertEquals(ZString.Empty, headerData.ClientBranchDesig);
			AssertEquals(new ZDateTime(2011, 04, 18), headerData.PSD);
			AssertEquals(ZString.Empty, headerData.PSCMonth);

			var sTUMessage = Factory.New<MQEDIMessage>();
			sTUMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			sTUMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			sTUMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sTUMessage.EM_MessageNum = "26384";
			sTUMessage.EM_MessageText = "B011001267HP                                               26384                H4601267 0200744970817110209                                                    Y  1001267HP00001";
			sTUMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;

			headerData = new PSCEntrySummaryData(message, sTUMessage);

			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionADDCVD, headerData.EntryType);
			AssertEquals("IOR Number", "91-013199000", headerData.IORNumber);
			AssertEquals("ReconType", "002", headerData.ReconType);
			AssertEquals("FTA Recon", true, headerData.FTARecon);
			AssertEquals("PortofEntry", "3902", headerData.EntryPort);
			AssertEquals(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, headerData.PaymentType);
			AssertEquals("02", headerData.ClientBranchDesig);
			AssertEquals(new ZDateTime(2011, 08, 17), headerData.PSD);
			AssertEquals("09", headerData.PSCMonth);

			sTUMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.StatementUpdate;
			sTUMessage.EM_MessageText = "B011001267SU                                               26384                H4601267  0200744970817110209                                                   Y  1001267SU00001";

			headerData = new PSCEntrySummaryData(message, sTUMessage);

			AssertEquals("EntryType", EntryTypeList.Codes.ConsumptionADDCVD, headerData.EntryType);
			AssertEquals("IOR Number", "91-013199000", headerData.IORNumber);
			AssertEquals("ReconType", "002", headerData.ReconType);
			AssertEquals("FTA Recon", true, headerData.FTARecon);
			AssertEquals("PortofEntry", "3902", headerData.EntryPort);
			AssertEquals(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, headerData.PaymentType);
			AssertEquals("02", headerData.ClientBranchDesig);
			AssertEquals(new ZDateTime(2011, 08, 17), headerData.PSD);
			AssertEquals("09", headerData.PSCMonth);
		}

		public static MQEDIMessage CreateCustomsResponseFailed(BusinessObjectFactory factory, ZString messageNum)
		{
			var result = factory.New<MQEDIMessage>();
			result.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			result.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			result.EM_MessageText = "B018888XJ5ER                                               56895                10A888813-147927000103807-00049                 8         XJ5 7003823301037  IL E108888XJ5 70038233   48401   ULTIMATE CONSIGNEE NOT ON FILE           B001557249000000011750000000000001 00000000000000000000000000000250000000002500          E908888XJ5 70038233   52401   TRANSACTION DATA REJECTED                B00155724Y  8888XJ5ER00004000000011750                                                   ";
			result.EM_MessageNum = messageNum;
			return result;
		}

		public static MQEDIMessage CreateCustomsResponseSuccess(BusinessObjectFactory factory, ZString messageNum)
		{
			var result = factory.New<MQEDIMessage>();
			result.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			result.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			result.EM_MessageText = "B018888XJ5ER                                               56963                E08888XJ5 70038209B00155721PAPERLESS - FILER RETAIN RECORDS        0180857A     Y  8888XJ5ER00001000000069291                                                   ";
			result.EM_MessageNum = messageNum;
			result.EM_SystemCreateTimeUtc = ZDateTime.Now;
			return result;
		}
	}
}
