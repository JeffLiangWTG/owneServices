using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BIRDEntrySummaryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestFieldsInBRDAA()
		{
			BillOfLadingNumberCustomisation referenceNumberCustomisation = new BillOfLadingNumberCustomisation();
			referenceNumberCustomisation.RemoveFountainPrefix = true;
			referenceNumberCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Include = true;
			referenceNumberCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Order = 1;
			referenceNumberCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail = "ABCDE";
			CustomsDataRegistry.Instance.DeclarationNumberCustomisation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, referenceNumberCustomisation);

			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.SetExternalBrokerForTesting();
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();//JE_DeclarationReference is assigned

			MQEDIMessage message = new BIRDEntrySummaryMessageBuilder(entry).PopulateMessage();

			BRDAA aa = (BRDAA)message.MessageBlock.B;
			AssertEquals("Broker Reference Number", "ABCDE00001000", aa.OriginatingBrokerRef);
			AssertEquals("Application ID", BIRDApplicationCodeList.Codes.EntrySummary, aa.ApplicationCode);
			AssertNotEquals(ZDate.Empty, aa.CreationDate);

			AssertEquals(EDIMessage.Status.Pending, message.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.BIRDEntrySummary, message.EM_MessageSubType);
		}

		public void TestBuildForUltimateConsignee()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "POLO RALP LAUREN  CORP";
			consignee.MainAddress.OA_Address1 = "9 POLITO AVE";
			consignee.MainAddress.OA_City = "LYNDHURST";
			consignee.MainAddress.OA_PostCode = "07071";
			consignee.OH_RL_NKClosestPort = "USLDT";
			consignee.MainAddress.OA_State = "NJ";

			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = new BIRDEntrySummaryMessageBuilder(entry).PopulateMessage();
			var za = message.MessageBlock.MessageBlocks.OfType<BRDZA>().FirstOrDefault();
			var zb = message.MessageBlock.MessageBlocks.OfType<BRDZB>().FirstOrDefault();

			AssertEquals("POLO RALP LAUREN  CORP", za.ConsigneeName);
			AssertEquals("9 POLITO AVE", za.ConsigneeAddress1);

			AssertEquals("LYNDHURST", zb.ConsigneeCity);
			AssertEquals("07071", zb.ConsigneePostalCode);
			AssertEquals("NJ", zb.ConsigneeState);
		}

		public void TestBuildForInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			invoice.JZ_InvoiceCurrExRate = 0.8974m;
			invoice.JZ_InvoiceCurrExRateType = JobComInvoiceHeader.FixedExchangeRateTypeString;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice2.JZ_InvoiceCurrExRate = 1.0974m;
			invoice2.JZ_InvoiceCurrExRateType = JobComInvoiceHeader.FixedExchangeRateTypeString;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JobComInvoiceLines.AddNew().JI_LinePrice = 20000m;

			using (declaration.GetInvoiceNumberRenumberingSuspender())
			{
				invoice.JZ_InvoiceDisplaySequence = 2;
				invoice2.JZ_InvoiceDisplaySequence = 1;
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = new BIRDEntrySummaryMessageBuilder(entry).PopulateMessage();
			List<MessageBlock> zis = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(BRDZI));
			AssertEquals(MQEDIMessage.Status.Acknowledged, message.EM_Status);

			AssertEquals(2, zis.Count);

			BRDZI zi = (BRDZI)zis[0];
			//should be sorted on invoice sequence number
			AssertEquals((short)1, zi.InvoiceSequence);
			AssertEquals(Core.Constants.CurrencyCodes.Japan, zi.CurrencyCode);
			AssertEquals(1.0974m, zi.ExchangeRates);
			AssertEquals(20000m, zi.InvoiceValue);

			zi = (BRDZI)zis[1];
			AssertEquals((short)2, zi.InvoiceSequence);
			AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, zi.CurrencyCode);
			AssertEquals(0.8974m, zi.ExchangeRates);
			AssertEquals(10000m, zi.InvoiceValue);
		}

		public void TestBuildForContainers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.SetExternalBrokerForTesting();

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.CusContainers.FindOrCreate("CRUX897432");
			declaration.CusContainers.FindOrCreate("CRUX897433");

			AssertEquals("PreCondition", 2, declaration.CusContainers.Count);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = new BIRDEntrySummaryMessageBuilder(entry).PopulateMessage();
			List<MessageBlock> zcs = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(BRDZC));

			AssertEquals(1, zcs.Count);
			AssertEquals(MQEDIMessage.Status.Pending, message.EM_Status);

			BRDZC zc = (BRDZC)zcs[0];
			AssertEquals("CRUX897432", zc.ContainerNumber);
			AssertEquals("CRUX897433", zc.ContainerNumber2);
		}
	}
}
