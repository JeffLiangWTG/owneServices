using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class UpdateBillDispositionSourceTest : TestCaseWithFactory
	{
		public void TestUpdateDispositionSource()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec1.US_EnableCRL = true;
			dec1.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec1.US_EntryFilerCode = "SV9";
			var invoiceHeader = dec1.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			dec1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec1.ImportEntryNumber = "71002057";
			CreateStatusMessage("B003901SV9SO                                                                    "
				+ "SO103901SV9  71002057 0123-456789012CO                      3021 0909131        "
				+ "SO20CR B00160703                                                                "
				+ "SO40R    00591325206                                       00000150BL   00000000"
				+ "SO50013014103293BILL ON FILE                                                    "
				+ "SO60093013012597ADMISSIBLE                                                      "
				+ "Y  3901SV9SO00000                                                               ", dec1.ActiveEntryHeaders.SimplifiedEntry.Messages);
			var bill1_dec1 = dec1.Bills.AddNew();
			bill1_dec1.CU_BillNum = "Bill1";
			bill1_dec1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var disposition1 = bill1_dec1.DispositionCodes.AddNew();
			disposition1.US_Code = "93";
			disposition1.US_DispositionDate = new ZDateTime(2014, 01, 30, 10, 32, 0);
			var disposition2 = bill1_dec1.DispositionCodes.AddNew();
			disposition2.US_Code = "91";
			disposition2.US_DispositionDate = new ZDateTime(2014, 01, 30, 10, 32, 0);
			var bill2_dec1 = bill1_dec1.ChildBills.AddNew();
			bill2_dec1.CU_BillNum = "Bill2House";
			bill2_dec1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_EnableCRL = true;
			dec2.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec2.US_EntryFilerCode = "SV9";
			var invoiceHeader2 = dec2.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			dec2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec2.ImportEntryNumber = "71002057";
			var iSMessage = Factory.New<MQEDIMessage>();
			iSMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			iSMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			iSMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse;
			dec2.Messages.Add(iSMessage);
			iSMessage.EM_MessageText = "B018888XJ5IS                                               35310                "
				+ "R1                                       DLH                     0468 081709    "
				+ "SC0200468 08170902042166613 0000100000                                          "
				+ "SD0817091354381CENTERED, GENERAL EXAMINATION                                    "
				+ "SC0200468 08170902042166611           000RH0592968 0000100000                   "
				+ "SD04070910555910USDA CHANGE HOLD QUANTITY                                       "
				+ "SD0407091354261FLOCAL TRANSFER AUTHORIZED                                       "
				+ "SC0200468 08170902042166611           000RH0592968 0000100000                   "
				+ "SC0200468 08170902042166611           000RH0592968 0000100000                   "
				+ "Y  8888XJ5IN00001";
			var bill1_dec2 = dec2.Bills.AddNew();
			bill1_dec2.CU_BillNum = "Bill1Dec2";
			bill1_dec2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var disposition3 = bill1_dec2.DispositionCodes.AddNew();
			disposition3.US_Code = "1C";
			disposition3.US_DispositionDate = new ZDateTime(2009, 08, 17, 13, 54, 38);
			var bill2_dec2 = bill1_dec2.ChildBills.AddNew();
			bill2_dec2.CU_BillNum = "Bill2House";
			bill2_dec2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var disposition4 = bill2_dec2.DispositionCodes.AddNew();
			disposition4.US_Code = "10";
			disposition4.US_DispositionDate = new ZDateTime(2009, 04, 07, 10, 55, 59);
			var disposition5 = bill2_dec2.DispositionCodes.AddNew();
			disposition5.US_Code = "1F";
			disposition5.US_DispositionDate = new ZDateTime(2009, 04, 07, 13, 54, 26);
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec3.US_EnableCRL = true;
			dec3.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec3.US_EntryFilerCode = "SV9";
			var invoiceHeader3 = dec3.Invoices.AddNew();
			invoiceHeader3.InvoiceLines.AddNew();
			dec3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec3.ImportEntryNumber = "71002057";
			CreateStatusMessage("B003901SV9SO                                                                    "
				+ "SO103901SV9  71002057 0123-456789012CO                      3021 0909131        "
				+ "SO20CR B00160703                                                                "
				+ "SO40R    00591325206                                       00000150BL   00000000"
				+ "SO50013014103291BILL                                                            "
				+ "SO60093013012597ADMISSIBLE                                                      "
				+ "Y  3901SV9SO00000                                                               ", dec3.ActiveEntryHeaders.SimplifiedEntry.Messages);
			var iSMessage2 = Factory.New<MQEDIMessage>();
			iSMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			iSMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			iSMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse;
			dec3.Messages.Add(iSMessage2);
			iSMessage2.EM_MessageText = "B018888XJ5IS                                               35310                "
				+ "R1                                       DLH                     0468 081709    "
				+ "SC0200468 08170902042166613 0000100000                                          "
				+ "SD0817091354381CENTERED, GENERAL EXAMINATION                                    "
				+ "SC0200468 08170902042166611           000RH0592968 0000100000                   "
				+ "SD01301410310091TRANSFER OF LIAB FOR INBOND                                     "
				+ "SC0200468 08170902042166611           000RH0592968 0000100000                   "
				+ "SC0200468 08170902042166611           000RH0592968 0000100000                   "
				+ "Y  8888XJ5IN00001";
			var bill1_dec3 = dec3.Bills.AddNew();
			bill1_dec3.CU_BillNum = "Bill1Dec3";
			bill1_dec3.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var disposition6 = bill1_dec3.DispositionCodes.AddNew();
			disposition6.US_Code = "91";
			disposition6.US_DispositionDate = new ZDateTime(2014, 01, 30, 13, 10, 32);
			var disposition7 = bill1_dec3.DispositionCodes.AddNew();
			disposition7.US_Code = "1C";
			disposition7.US_DispositionDate = new ZDateTime(2009, 08, 17, 13, 54, 38);
			var disposition8 = bill1_dec3.DispositionCodes.AddNew();
			disposition8.US_Code = "91";
			disposition8.US_DispositionDate = new ZDateTime(2014, 01, 30, 13, 10, 32);
			Factory.Save();
			var dataProvider = ObjectFactory.Get<Integration.Customs.US.IUpdateBillDispositionSource>();
			dataProvider.Update(null, new ZGuid[] { bill1_dec1.PK, bill2_dec1.PK, bill1_dec2.PK, bill2_dec2.PK, bill1_dec3.PK });
			AssertEquals("Source updated", BillDispositionSourceList.Codes.SO, disposition1.US_Source);
			AssertEquals("Matching disposition is not found in message", ZString.Empty, disposition2.US_Source);
			AssertEquals("Source updated", BillDispositionSourceList.Codes.IS, disposition3.US_Source);
			AssertEquals("Source updated", BillDispositionSourceList.Codes.IS, disposition4.US_Source);
			AssertEquals("Source updated", BillDispositionSourceList.Codes.IS, disposition5.US_Source);
			AssertEquals("Source cannot be updated because disposition code '91' in SO and in IS has the same time", ZString.Empty, disposition6.US_Source);
			AssertEquals("Source updated", BillDispositionSourceList.Codes.IS, disposition7.US_Source);
			AssertEquals("Source cannot be updated because disposition code '91' in SO and in IS has the same time", ZString.Empty, disposition6.US_Source);
		}

		public void TestDoNotCreateTasksFromTemplate()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec1.US_EnableCRL = true;
			dec1.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec1.US_EntryFilerCode = "SV9";
			var invoiceHeader = dec1.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			dec1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec1.ImportEntryNumber = "71002057";
			CreateStatusMessage("B003901SV9SO                                                                    "
				+ "SO103901SV9  71002057 0123-456789012CO                      3021 0909131        "
				+ "SO20CR B00160703                                                                "
				+ "SO40R    00591325206                                       00000150BL   00000000"
				+ "SO50013014103293BILL ON FILE                                                    "
				+ "SO60093013012597ADMISSIBLE                                                      "
				+ "Y  3901SV9SO00000                                                               ", dec1.ActiveEntryHeaders.SimplifiedEntry.Messages);
			var bill1_dec1 = dec1.Bills.AddNew();
			bill1_dec1.CU_BillNum = "Bill1";
			bill1_dec1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var disposition1 = bill1_dec1.DispositionCodes.AddNew();
			disposition1.US_Code = "93";
			disposition1.US_DispositionDate = new ZDateTime(2014, 01, 30, 10, 32, 0);
			var disposition2 = bill1_dec1.DispositionCodes.AddNew();
			disposition2.US_Code = "91";
			disposition2.US_DispositionDate = new ZDateTime(2014, 01, 30, 10, 32, 0);
			var bill2_dec1 = bill1_dec1.ChildBills.AddNew();
			bill2_dec1.CU_BillNum = "Bill2House";
			bill2_dec1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			Factory.Save();
			var templateIMP = Factory.NewWithValidTestData<MasterFiles.Business.ProcessTaskTemplate>();
			templateIMP.P0_ProcessType = MasterFiles.Business.JobInvoicingConsumerTypes.Brokerage.Code;
			templateIMP.P0_SubType2 = MasterFiles.Business.ImportExportCodeList.Codes.Import;
			var workflowIMP = templateIMP.WorkflowItems.AddNew();
			workflowIMP.P9_Description = "IMPWORK";
			Factory.Save();
			var dataProvider = ObjectFactory.Get<Integration.Customs.US.IUpdateBillDispositionSource>();
			dataProvider.Update(null, new ZGuid[] { bill1_dec1.PK, bill2_dec1.PK });
			AssertEquals("Source updated", BillDispositionSourceList.Codes.SO, disposition1.US_Source);
			AssertEquals("Matching disposition is not found in message", ZString.Empty, disposition2.US_Source);
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(dec1.PK);
			AssertEquals(0, loadedDeclaration.WorkflowItems.Tasks.Count);
		}

		MQEDIMessage CreateStatusMessage(ZString msgText, EDIMessageCollection messages)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_MessageText = msgText;
			messages.Add(message);
			return message;
		}
	}
}
