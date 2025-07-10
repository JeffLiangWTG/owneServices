using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InputBlockControlGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageReferenceNumber()
		{
			AssertEquals(EDIMessage.MessageNumberPlaceHolder, new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch).B.UserData);
		}

		public void TestBIRDInputBlockControlGenerator()
		{
			AssertEquals(typeof(BRDAA), new BIRDInputBlockControlGenerator(GlbBranch.CurrentBranch, "", "").B.GetType());
			AssertEquals(typeof(BRDZZ), new BIRDInputBlockControlGenerator(GlbBranch.CurrentBranch, "", "").Y.GetType());
		}

		[TestDate(2009, 12, 23)]
		public void TestAESInputBlockControlGenerator()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			OrgHeader supplier = helper.CreateOrganisation("SUPPLIER", "USLAX");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "126597687", Core.Constants.CountryCodes.UnitedStates);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.JI_LinePrice = 15000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AESInputBlockControlGenerator generator = new AESInputBlockControlGenerator(entry);
			AssertEquals(typeof(AESCommShipBXP), generator.B.GetType());
			AssertEquals(typeof(AESCommShipYXP), generator.Y.GetType());
			MQEDIMessage message = generator.CreateMessage<MQEDIMessage>(Factory);
			AssertEquals(MQEDIMessage.ApplicationCodes.USCustomsExport, message.EM_ApplicationCode);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, message.EM_MessageType);
			AssertEquals(MQEDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(MQEDIMessage.Status.Queued, message.EM_Status);
			AssertMultilineASCIIEquals("MessageData", "B  12659768700E          SUPPLIER                                               Y  12659768700E          SUPPLIER", message.EM_MessageText);
		}

		public void TestPSCWithExternalBrokerEntryFilerCode()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ6";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "123";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 1, 2);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var bBlock = (IABIControlMessageBlockB)message.MessageBlock.B;
			AssertEquals("XJ6", bBlock.EntryFilerCode);

			var aens10 = (AENS10)message.MessageBlock.MessageBlocks.Cast<MessageBlock>().FirstOrDefault(x => x is AENS10);
			AssertEquals("123", aens10.EntryFilerCode);
		}
	}
}
