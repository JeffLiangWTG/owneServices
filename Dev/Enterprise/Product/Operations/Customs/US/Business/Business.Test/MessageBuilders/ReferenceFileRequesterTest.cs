using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReferenceFileRequesterTest : TestCaseWithFactory
	{
		[TestDate(2016, 06, 02)]
		public void TestReferenceFileRequestWhenSavingForReconJob()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.BrettsBirthday;
			JobComInvoiceLine invoiceLine = originalEntry.Invoice.InvoiceLines.AddNew();

			Factory.Save();

			IMessageAttachee reconMsgAttachee = reconDeclaration;
			AssertEquals("No message should have been generated", 0, reconMsgAttachee.Messages.Count);

			invoiceLine.JI_Tariff = "0000";
			AssertEquals("PreCondition", false, invoiceLine.TariffMarkedForReferenceFileRequest);

			invoiceLine.JI_Tariff = "0001200000";
			AssertEquals("PreCondition", true, invoiceLine.TariffMarkedForReferenceFileRequest);

			Factory.Save();
			AssertEquals("One message should have been generated", 1, reconMsgAttachee.Messages.Count);
			AssertEquals("Message type", ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, reconMsgAttachee.Messages[0].EM_MessageType);
			AssertEquals("Message sub type", EM_MessageSubTypeList.Codes.RequestTariffUpdate, reconMsgAttachee.Messages[0].EM_MessageSubType);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			ReconDeclaration reconDecLoaded = new ReconDeclaration(factory.Load<JobDeclaration>(declaration.PK));
			AssertEquals("one message should be there", 1, reconDecLoaded.Messages.Count);

			invoiceLine.US_R_OrigTariff = "1111";
			AssertEquals("PreCondition", true, invoiceLine.ReconOrigTariffIsCandidateForUpdateRequest);
			AssertEquals("PreCondition", false, invoiceLine.ReconOrigTariffMarkedForReferenceFileRequest);

			invoiceLine.US_R_OrigTariff = "1111234562";
			AssertEquals("PreCondition", true, invoiceLine.ReconOrigTariffIsCandidateForUpdateRequest);
			AssertEquals("PreCondition", true, invoiceLine.ReconOrigTariffMarkedForReferenceFileRequest);

			Factory.Save();

			AssertEquals("One more message should have been generated", 2, reconMsgAttachee.Messages.Count);
			AssertEquals("Message type", ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, reconMsgAttachee.Messages[1].EM_MessageType);
			AssertEquals("Message sub type", EM_MessageSubTypeList.Codes.RequestTariffUpdate, reconMsgAttachee.Messages[1].EM_MessageSubType);

			MQEDIMessage requestMsg = (MQEDIMessage)reconMsgAttachee.Messages[1];

			int countOfRequest = 0;
			foreach (MessageBlock block in requestMsg.MessageBlock.MessageBlocks)
			{
				HTSW htsw = block as HTSW;
				if (htsw != null)
				{
					countOfRequest++;

					AssertEquals("AsOfDate in request should be the recon original duty date", ZDateTime.BrettsBirthday, htsw.AsOfDate);
				}
			}

			AssertEquals(1, countOfRequest);
		}

		public void TestRequestADDCVDCasesForACS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line5 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line6 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line7 = declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line1.US_ADDCaseNo = "A1";

			line2.JI_Tariff = line1.JI_Tariff;
			line2.US_ADDCaseNo = line1.US_ADDCaseNo;

			line3.JI_Tariff = line1.JI_Tariff;
			line3.US_ADDCaseNo = "A2";

			line4.JI_Tariff = line1.JI_Tariff;
			line4.US_CVDCaseNo = "C1";

			line5.JI_Tariff = line1.JI_Tariff;

			line6.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;

			new ReferenceFileRequester().RequestADDCVDs(declaration);

			AssertEquals("No Message generated", 0, declaration.Messages.Count);
		}

		[TestDate(2016, 03, 08)]
		public void TesttRequestADDCVDCasesForACE()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertRequestADDCVDCases(declaration, ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQuery);
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestADDCVDsByTariffForACS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			AsssertRequestADDCVDsByTariff(declaration, ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery);
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestADDCVDsByTariffForACE()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			AsssertRequestADDCVDsByTariff(declaration, ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQuery);
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestADDCVDsByCaseNumber()
		{
			int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			AssertEquals("Pre-condition - ediMessage count", 0, ediMessageCount);

			new ReferenceFileRequester().RequestADDCVDsByCaseNumber("A570929006");
			EDIMessage messageGenerated = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery, messageGenerated.EM_MessageType);
			AssertEquals(true, messageGenerated.EM_MessageText.Contains("A570929006"));
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestTariffs_ByUser()
		{
			List<TariffDate> tariffs = new List<TariffDate>();
			TariffDate tariffDate = new TariffDate("00012000", "", new ZDate(2009, 02, 19));
			tariffs.Add(tariffDate);

			int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			AssertEquals("Pre-condition - ediMessage count", 0, ediMessageCount);

			new ReferenceFileRequester().RequestTariffs(tariffs, null);

			AssertEquals("There should be a message generated", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

			EDIMessage messageGenerated = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, messageGenerated.EM_MessageType);
			AssertEquals(true, messageGenerated.EM_MessageText.Contains("00012000"));
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestTariffs_NotAllLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line2.JI_Tariff = line1.JI_Tariff;
			line3.JI_Tariff = USCTariff.DOTMayBeApplicable;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0001200000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.BrettsBirthday.AddYears(1);

			line4.JI_Tariff = "0001200000";

			new ReferenceFileRequester().RequestTariffs(declaration, false);

			AssertEquals("There should be a message generated", 1, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, declaration.Messages[0].EM_MessageType);
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("0001200000"));
			AssertEquals(false, declaration.Messages[0].EM_MessageText.Contains(USCTariff.CottonFeeApplicable));
			AssertEquals(false, declaration.Messages[0].EM_MessageText.Contains(USCTariff.DOTMayBeApplicable));
		}

		[TestDate(2000, 1, 1)]
		public void TestRequestForSupTariff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_SupTariff = "99100402";
			line1.JI_Tariff = USCTariff.CottonFeeApplicable;

			AssertNull("PreCondition", line1.ImportSupTariff);
			AssertNotNull("PreCondition", line1.ImportTariff);

			new ReferenceFileRequester().RequestTariffs(declaration, false);
			AssertEquals("There should be a message generated", 1, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, declaration.Messages[0].EM_MessageType);
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("99100402"));
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestTariffs_AllLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line2.JI_Tariff = line1.JI_Tariff;
			line3.JI_Tariff = USCTariff.DOTMayBeApplicable;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00012000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.BrettsBirthday.AddYears(1);

			line4.JI_Tariff = "00012000";

			new ReferenceFileRequester().RequestTariffs(declaration, true);

			AssertEquals("There should be a message generated", 1, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, declaration.Messages[0].EM_MessageType);
			AssertEquals("Message sub type", EM_MessageSubTypeList.Codes.RequestTariffUpdate, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("00012000"));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains(USCTariff.CottonFeeApplicable));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains(USCTariff.DOTMayBeApplicable));
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestTariffs_AllLines_NASupTariff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line2.JI_Tariff = line1.JI_Tariff;
			line3.JI_Tariff = USCTariff.DOTMayBeApplicable;

			line1.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			line2.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;
			line3.US_SupTariff = TariffViewAsCodeDescription.NotApplicableCode;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00012000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.BrettsBirthday.AddYears(1);

			line4.JI_Tariff = "00012000";

			new ReferenceFileRequester().RequestTariffs(declaration, true);

			AssertEquals("There should be a message generated", 1, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, declaration.Messages[0].EM_MessageType);
			AssertEquals("Message sub type", EM_MessageSubTypeList.Codes.RequestTariffUpdate, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("00012000"));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains(USCTariff.CottonFeeApplicable));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains(USCTariff.DOTMayBeApplicable));
		}

		[TestDate(2000, 1, 1)]
		public void TestRequestForSupAdditionalTariffs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line1.SupFormattedAdditionalTariff1 = "9910.04.01";
			line1.SupFormattedAdditionalTariff2 = "9910.04.02";
			line1.SupFormattedAdditionalTariff3 = "9910.04.03";
			line1.SupFormattedAdditionalTariff4 = "9910.04.04";
			line1.SupFormattedAdditionalTariff5 = "9910.04.05";

			AssertNotNull("PreCondition ImportTariff", line1.ImportTariff);
			AssertNull("PreCondition ImportSupAdditionalTariff1", line1.ImportSupAdditionalTariff1);
			AssertNull("PreCondition ImportSupAdditionalTariff2", line1.ImportSupAdditionalTariff2);
			AssertNull("PreCondition ImportSupAdditionalTariff3", line1.ImportSupAdditionalTariff3);
			AssertNull("PreCondition ImportSupAdditionalTariff4", line1.ImportSupAdditionalTariff4);
			AssertNull("PreCondition ImportSupAdditionalTariff5", line1.ImportSupAdditionalTariff5);

			new ReferenceFileRequester().RequestTariffs(declaration, false);
			AssertEquals("There should be a message generated", 1, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, declaration.Messages[0].EM_MessageType);
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("99100401"));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("99100402"));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("99100403"));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("99100404"));
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("99100405"));
		}

		[TestDate(2006, 9, 18)]
		public void TestRequestTariffByUpdate()
		{
			MQEDIMessage message = requester.RequestTariffByUpdate(1357);
			AssertEquals(ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles, message.EM_MessageType);
			Assert(message.EM_MessageText.Contains("1357"));
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestImportSpecialistTeamAssignmentFile()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "F112");
			int cnt = Factory.GetDatabaseCount(typeof(EDIMessage), query);
			requester.RequestImportSpecialistTeamAssignmentFile();
			AssertEquals(cnt + 1, Factory.GetDatabaseCount(typeof(EDIMessage), query));
		}

		[TestDate(2006, 9, 18)]
		public void TestRequestExchangeRates()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "F108");
			int cnt = Factory.GetDatabaseCount(typeof(EDIMessage), query);
			requester.RequestExchangeRates();
			AssertEquals(cnt + 1, Factory.GetDatabaseCount(typeof(EDIMessage), query));
		}

		[TestDate(2006, 9, 18)]
		public void TestRequestCountry()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "F102");
			int cnt = Factory.GetDatabaseCount(typeof(EDIMessage), query);
			requester.RequestCountry();
			AssertEquals(cnt + 1, Factory.GetDatabaseCount(typeof(EDIMessage), query));
		}

		[TestDate(2017, 12, 31)]
		public void TestACERequestTariffs()
		{
			ZGuid messagePK;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ACEHTS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, false))
			{
				var tariffs = new List<TariffDate>();
				var tariffDate = new TariffDate("00012000", "", new ZDate(2009, 02, 19));
				tariffs.Add(tariffDate);

				int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals("Pre-condition - ediMessage count", 0, ediMessageCount);

				new ReferenceFileRequester().RequestTariffs(tariffs, null);
				AssertEquals("There should be a message generated", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				EDIMessage messageGenerated = Factory.LoadTop1<EDIMessage>(new ZQuery());
				messagePK = messageGenerated.PK;

				AssertEquals(true, messageGenerated.EM_MessageText.Contains("00012000"));
				AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, messageGenerated.EM_MessageType);
				Assert(messageGenerated.EM_MessageText.StartsWith("B01"));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ACEHTS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				var tariffs = new List<TariffDate>();
				var tariffDate = new TariffDate("00012000", "", new ZDate(2009, 02, 19));
				tariffs.Add(tariffDate);

				int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
				AssertEquals("Pre-condition - ediMessage count", 1, ediMessageCount);

				new ReferenceFileRequester().RequestTariffs(tariffs, null);
				AssertEquals("There should be a message generated", ++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

				EDIMessage messageGenerated = Factory.Load<EDIMessage>(new ZQuery()).FirstOrDefault(x => x.PK != messagePK);

				AssertEquals(true, messageGenerated.EM_MessageText.Contains("00012000"));
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQuery, messageGenerated.EM_MessageType);
				Assert(messageGenerated.EM_MessageText.StartsWith("B  "));//ACE B block has space at position 2 and 3
			}
		}

		[TestDate(2017, 12, 31)]
		public void TestACERequestTariffByUpdate()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ACEHTS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, false))
			{
				var message = requester.RequestTariffByUpdate(1357);

				Assert(message.EM_MessageText.Contains("1357"));
				AssertEquals(ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles, message.EM_MessageType);
				Assert(message.EM_MessageText.StartsWith("B01"));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ACEHTS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				var message = requester.RequestTariffByUpdate(1357);

				Assert(message.EM_MessageText.Contains("1357"));
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQuery, message.EM_MessageType);
				Assert(message.EM_MessageText.StartsWith("B  "));//ACE B block has space at position 2 and 3
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			requester = new ReferenceFileRequester();
		}
		ReferenceFileRequester requester;

		void AssertRequestADDCVDCases(JobDeclaration declaration, string messageType)
		{
			declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line5 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line6 = declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line1.US_ADDCaseNo = "A1";

			line2.JI_Tariff = line1.JI_Tariff;
			line2.US_ADDCaseNo = line1.US_ADDCaseNo;

			line3.JI_Tariff = line1.JI_Tariff;
			line3.US_ADDCaseNo = "A2";

			line4.JI_Tariff = line1.JI_Tariff;
			line4.US_CVDCaseNo = "C1";

			line5.JI_Tariff = line1.JI_Tariff;

			line6.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;

			new ReferenceFileRequester().RequestADDCVDs(declaration);

			Assert("Message(s) generated", declaration.Messages.Count > 0);
			AssertEquals(messageType, declaration.Messages[0].EM_MessageType);
			Assert("Country of Origin should not be part of the query: CSMS #10-000098", !declaration.Messages[0].EM_MessageText.Contains("AU"));
			Assert(declaration.Messages[0].EM_MessageSubType == EM_MessageSubTypeList.Codes.ADDCVDDutyQuery);
		}

		void AsssertRequestADDCVDsByTariff(JobDeclaration declaration, string messageType)
		{
			JobComInvoiceLine firstLine = declaration.InvoiceLines.AddNew();
			firstLine.JI_Tariff = USCTariff.CAFTABenefitsApplicable;

			JobComInvoiceLine line = declaration.InvoiceLines.AddNew();
			line.JI_Tariff = USCTariff.AGOABenefitsApplicable;
			line.US_UC_NKCountryOfOrigin = "IT";

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = USCTariff.DOTIsApplicable;
			line2.US_UC_NKCountryOfOrigin = "IT";

			VisaQuotaADDCVDQuerySendingActionCollection actions = new VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD.ADDCVD, declaration);
			AssertEquals("2 lines", 2, actions.Count);
			actions[0].US_SendMessage = true;
			actions[1].US_SendMessage = true;

			new ReferenceFileRequester().RequestADDCVDsByTariff(actions);
			Assert("message(s) generated", declaration.Messages.Count > 0);
			AssertEquals("ADD/CVD Query message has been generated", messageType, declaration.Messages[0].EM_MessageType);

			ZString messageText = "";

			foreach (MQEDIMessage message in declaration.Messages)
			{
				messageText += "\r\n" + message.EM_MessageText;
			}

			Assert(messageText.Contains(USCTariff.DOTIsApplicable));
			Assert(messageText.Contains(USCTariff.AGOABenefitsApplicable));
		}
	}
}
