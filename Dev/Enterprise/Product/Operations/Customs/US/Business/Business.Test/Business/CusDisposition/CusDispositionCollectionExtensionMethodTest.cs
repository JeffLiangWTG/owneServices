using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusDispositionCollection))]
	sealed class CusDispositionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddOrUpdateCusDisposition()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDARAD012216122801DATA UNDER PGA REVIEW         01  001001                  " +
					"SO70NHTOFF012216122801DATA UNDER PGA REVIEW         01  001001                  " +

					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var message2 = CreateStatusMessage(
				"B003901SV9SO                                                                    " +
				"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
				"SO20CR B00160703                                                                " +
				"SO40R    00591325206                                       00000150BL   00000000" +
				"SO70FDARAD012216122701DATA UNDER PGA REVIEW         01  001001                  " +
				"SO70NHTOFF012216122707MAY PROCEED                   07  001001                  " +
				"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 2);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "NHT" && x.CDI_Status == "01"));
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "01"));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusDispositionCollection(Factory.New<JobDeclaration>());

		JobDeclaration GetDeclaration(ZString entryNum)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			var dec = Factory.New<JobDeclaration>();
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
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
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
