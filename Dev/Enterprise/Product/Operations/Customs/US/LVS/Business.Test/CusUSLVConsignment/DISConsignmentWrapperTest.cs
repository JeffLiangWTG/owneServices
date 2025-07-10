using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class DISConsignmentWrapperTest : TestCaseWithFactory
	{
		public void TestIUSDISDefaultValues()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "FFF";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "ENS";
			entryNumber.CE_RN_NKCountryCode = "US";
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryLineReference = "AABBCCDD";
			entryNumber.CE_ParentID = consignment.PK;
			entryNumber.CE_EntryNum = "SMILE";

			ZString msgText = @"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120596RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         10  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", "");

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = msgText;

			consignment.Messages.Add(message);

			var consignmentWrapper = new DISConsignmentWrapper(consignment);
			var shipment = consignment.Shipment;
			var usDISdefaultvalues = consignmentWrapper as IUSDISDefaultValues;

			AssertEquals(shipment.ULH_EntryFilerCode, usDISdefaultvalues.PreparerID);

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, shipment.Branch.GB_GC.ToGuid(), Guid.Empty, "2704");
			AssertEquals("2704", usDISdefaultvalues.PreparerSiteCode);

			AssertEquals(shipment.ULH_DischargeDate, usDISdefaultvalues.ArrivalDate);
			AssertEquals(shipment.ULH_PortOfDischarge, usDISdefaultvalues.PortOfUnlading);
			AssertEquals(shipment.ULH_PortOfEntry, usDISdefaultvalues.PortOfEntry);
			AssertEquals(shipment.ULH_IORReference, usDISdefaultvalues.ImporterOfRecordID);

			foreach (var trade in usDISdefaultvalues.DefaultTradeTransactions)
			{
				AssertEquals(TradeTransactionType.Entry, trade.Type);
				AssertEquals("SMILE", consignment.CE_EntryNum, trade.Number);
				AssertEquals("FFF", shipment.ULH_EntryFilerCode, trade.FilerOrSCAC);
				AssertEquals("JobNumber -NO24WOSZ1J2BDQXU6G1AO4VP07MBXLDXUTK", shipment.ULH_JobNumber, trade.ReferenceNumber);
			}

			foreach (IDISCBPRequestDefault soMessage in usDISdefaultvalues.DefaultCBPRequests)
			{
				if (soMessage.ID == "96-012016")
				{
					AssertEquals(new ZDate(2016, 1, 20), soMessage.RequestDate);
					AssertEquals("Requested on 01-20-16 (RELEASED)", soMessage.Description);
				}
				else if (soMessage.ID == "10-012016")
				{
					AssertEquals(new ZDate(2016, 1, 20), soMessage.RequestDate);
					AssertEquals("Requested on 012016 (DATA UNDER PGA REVIEW)", soMessage.Description);
				}
			}
		}
	}
}
