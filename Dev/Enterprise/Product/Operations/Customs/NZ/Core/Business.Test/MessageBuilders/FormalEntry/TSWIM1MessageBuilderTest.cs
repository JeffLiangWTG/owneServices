using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.Testing
{
	sealed class TSWIM1MessageBuilderTest : XmlMessageBuilderTest
	{
		public override void TestGenerateTestMessage()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			Declaration.JE_DeclarationReference = "B000570090";
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			DecCreator.SetupTestConsignmentDetails();
			DecCreator.SetupTestForAir();
			DecCreator.SetupTestForImportFromAU();
			DecCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			DecCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			DecCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			DecCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			DecCreator.MergeDeclaration();
			DecCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.CusEntryHeader;

			var expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>IM</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\">\r\n  <ID />\r\n  <TypeCode>I10</TypeCode>\r\n  <FunctionalReferenceID>||SNDREFPHLDR||</FunctionalReferenceID>\r\n  <FunctionCode>1</FunctionCode>\r\n  <Submitter>\r\n    <ID>00009917B</ID>\r\n  </Submitter>\r\n  <Declarant>\r\n    <ID />\r\n  </Declarant>\r\n</Declaration>\n</DocumentMetadata>";
			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.CancelEntry);
			messageBuilder.GenerateMessage();
			var messageGenerated = messageBuilder.GetMessageText();
			AssertEquals("IM1 message generated", expectedMessage, messageGenerated);

			var message = Declaration.CusEntryHeader.Messages[0];
			AssertEquals("EDIMessage generated MUST have TSW message type from MappedTSWMessageSubType", MessageTypeList.Codes.I10, message.EM_MessageType);
		}

		[TestDate(2024, 3, 1)]
		public void TestGenerateQueuedMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var queuedDate = new ZDateTime(2024, 3, 21);
				Declaration.JE_EDITransmitDate = queuedDate;
				messageBuilder = new TSWIM1MessageBuilder((CusEntryHeader)Declaration.CusEntryHeader, null, MessageBuilder.MessageTypes.Original);
				messageBuilder.GenerateMessage();
				var message = messageBuilder.MessageBusinessObject;
				AssertEquals("Message is queued in UTC date", queuedDate.ToUniversalBranchTime(Factory).AddMinutes(15), message.EM_HeldUntilDate);
			}
		}

		[TestDate(2017, 10, 16)]
		public void TestSetParentMessagingStatusAfterMessagePosting()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			Declaration.JE_DeclarationReference = "B000570090";
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);

			CusEntryHeader entryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.CancelEntry);
			messageBuilder.GenerateMessage();
			var messageGenerated = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			Assert(Declaration.JE_EntrySubmittedDate.IsEmpty);

			Declaration.JE_EDITransmitDate = ZDateTime.Today;
			entryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageBuilder.MessageTypes.CancelEntry);
			messageBuilder.GenerateMessage();
			messageGenerated = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			Assert(!Declaration.JE_EntrySubmittedDate.IsEmpty);
		}

		public void TestCombinedMessageStatusAfterReplacement()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			Declaration.JE_DeclarationReference = "B00003981";
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EDITransmitDate = ZDateTime.Today;

			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("2845", "FOB", "NZD", 1250m);
			decCreator.SetupImportInvoiceLine("0504000051L", "BEEF (Packaged)", "AU", "AU", "N", 1250m);
			decCreator.AddHouseBillWithPackingDetails("G52882", 100, "PK");
			decCreator.MergeDeclaration();

			var entryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			AssertEquals(TSWEntryStatusList.Codes.STC, Declaration.JE_TSWCombinedStatus);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;
			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Replacement);
			messageBuilder.GenerateMessage();
			AssertEquals(TSWEntryStatusList.Codes.PCC, Declaration.JE_TSWCombinedStatus);
		}

		public void TestReplacementSendsPDO()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			Declaration.JE_DeclarationReference = "B000570090";
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EDITransmitDate = ZDateTime.Today;
			Declaration.OtherInfos.RemoveAndDeleteAll();
			Assert("Pre-condition: 'PDO' Other info should not be included on this declaration", !Declaration.JE_PDOOtherInfoValue);

			var entryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original);
			Assert("'PDO' Other info should not be included on this entry", !Declaration.JE_PDOOtherInfoValue);

			messageBuilder = new TSWIM1MessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Replacement);
			Assert("'PDO' Other info should have been included for this replacement entry", Declaration.JE_PDOOtherInfoValue);
		}

		public override void TestGenerateLiveMessage()
		{
			Assert(true);
		}

		#region DecCreator
		TestFormalEntryCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = new TestFormalEntryCreator(Declaration);
				}
				return fDecCreator;
			}
		}
		TestFormalEntryCreator fDecCreator;
		#endregion

		JobDeclaration Declaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;
	}
}
