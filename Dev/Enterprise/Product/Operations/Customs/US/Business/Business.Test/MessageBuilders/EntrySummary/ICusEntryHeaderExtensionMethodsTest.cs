using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ICusEntryHeaderExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetPGAEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var pga01 = declaration.EntryPGACusDispositions.AddNew();
			var pga02 = declaration.EntryPGACusDispositions.AddNew();
			var pga03 = declaration.EntryPGACusDispositions.AddNew();

			pga01.CDI_StatusKey = "PGA01";
			pga02.CDI_StatusKey = "PGA02";
			pga03.CDI_StatusKey = "PGA03";

			pga01.CDI_Status = "01";
			pga02.CDI_Status = "02";
			pga03.CDI_Status = "04";

			var bo = declaration as IEntryHeaderParentBusinessObject;
			var dic = bo.GetPGAEntryStatus();

			AssertEquals(3, dic.Count);
			ZString status;
			Assert(dic.TryGetValue(pga01.CDI_StatusKey, out status));
			AssertEquals("01", status);
			Assert(dic.TryGetValue(pga02.CDI_StatusKey, out status));
			AssertEquals("02", status);
			Assert(dic.TryGetValue(pga03.CDI_StatusKey, out status));
			AssertEquals("04", status);

			AssertEquals("01", declaration.GetSinglePGAEntryStatus(pga01.CDI_StatusKey));
			AssertEquals("02", declaration.GetSinglePGAEntryStatus(pga02.CDI_StatusKey));
			AssertEquals("04", declaration.GetSinglePGAEntryStatus(pga03.CDI_StatusKey));
			AssertEquals(ZString.Empty, declaration.GetSinglePGAEntryStatus(""));
		}

		public void TestIsBillDetailRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Assert(entry.IsBillDetailRequired());

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Assert(entry.IsBillDetailRequired());

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			Assert(!entry.IsBillDetailRequired());

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Assert(entry.IsBillDetailRequired());

			declaration.US_EntryType = EntryTypeList.Codes.TradeFair;
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			Assert(entry.IsBillDetailRequired());
		}

		public void TestShouldForceFilingInACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			Assert(declaration.ShouldForceFilingInACECargoRelease());

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(declaration.ShouldForceFilingInACECargoRelease());

			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Assert(!declaration.ShouldForceFilingInACECargoRelease());
		}

		public void TestReleaseDateRemovedWhenSuspended()
		{
			var declaration = GetDeclaration("71002057");
			CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO60030413172098RELEASED                                02271301                " +
					"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(CRLReleaseStatusList.Codes.REL, declarationLoaded.ReleaseStatus);
			AssertEquals(new ZDateTime(2013, 02, 27), declarationLoaded.JE_EntryAuthorisationDate);

			CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO60030413172099RELEASE SUSPENDED                                               " +
				"Y  3901SV9SO00000                                                               ");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertEquals(CRLReleaseStatusList.Codes.NRL, declaration.ReleaseStatus);
			AssertEquals(ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
		}

		JobDeclaration GetDeclaration(ZString entryNum)
		{
			var dec = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec.US_EntryFilerCode = "SV9";

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec.ImportEntryNumber = entryNum;
			Factory.Save();
			return dec;
		}

		MQEDIMessage CreateStatusMessage(ZString msgText)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = msgText;
			return message;
		}
	}
}
