using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateMRNFormat()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DBN");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_DateForDuty = new ZDateTime(2016, 01, 01);
			testInst.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			declaration.DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			CombineAssertions("Test MRN", () =>
			{
				var tester = new MessageSendingObject(entryHeader);
				tester.MovementReferenceNumber = "ABC";
				AssertHasMessageErrorContaining(tester.MovementReferenceNumberInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
				tester.MovementReferenceNumber = "ABC".PadRight(18, '0');
				AssertNoMessageErrorContaining(tester.MovementReferenceNumberInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
				AssertHasMessageErrorContaining(tester.MovementReferenceNumberInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				ZString sTomorrow = ZDateTime.Today.AddDays(1).ToString("yyyyMMdd");
				tester.MovementReferenceNumber = ZString.Format("{0}{1}", "DBN", sTomorrow).PadRight(18, '0');
				AssertNoMessageErrorContaining(tester.MovementReferenceNumberInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				AssertHasMessageErrorContaining(tester.MovementReferenceNumberInfo, ValidationConstants.Shared.InvalidMRNDate);
				tester.MovementReferenceNumber = "DBN20160101@@".PadRight(18, '0');
				AssertNoMessageErrorContaining(tester.MovementReferenceNumberInfo, ValidationConstants.Shared.InvalidMRNDate);
				AssertHasMessageErrorContaining(tester.MovementReferenceNumberInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
				tester.MovementReferenceNumber = "DBN20160101".PadRight(18, '0');
				AssertNoMessageErrors(tester.MovementReferenceNumberInfo);
			});
		}

		[TestDate(2016, 01, 01)]
		public void TestValidateLRNFormat()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_DateForDuty = new ZDateTime(2016, 01, 01);
			testInst.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			declaration.DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			CombineAssertions("Test LRN 1", () =>
			{
				var tester = new MessageSendingObject(entryHeader);
				tester.LocalReferenceNumber = "ABC";
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSize);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidAgentCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidOfficeCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatFutureDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSequenceNumber);
			});
			CombineAssertions("Test LRN 2", () =>
			{
				var tester = new MessageSendingObject(entryHeader);
				tester.LocalReferenceNumber = "123456123456123456123456123";
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSize);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidAgentCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidOfficeCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatFutureDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSequenceNumber);
			});
			CombineAssertions("Test LRN 3", () =>
			{
				var tester = new MessageSendingObject(entryHeader);
				tester.LocalReferenceNumber = "XXXXXXXXYYYZZZZZZZZTTTTTT";
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSize);
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidAgentCode);
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidOfficeCode);
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatFutureDate);
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSequenceNumber);
			});
			CombineAssertions("Test LRN 4", () =>
			{
				var tester = new MessageSendingObject(entryHeader);
				tester.LocalReferenceNumber = "12345678JSA20160201000000";
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSize);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidAgentCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidOfficeCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidDate);
				AssertHasMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatFutureDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSequenceNumber);
			});
			CombineAssertions("Test LRN 5", () =>
			{
				var tester = new MessageSendingObject(entryHeader);
				tester.LocalReferenceNumber = "12345678JSA20150201000000";
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSize);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidAgentCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidOfficeCode);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatFutureDate);
				AssertNoMessageErrorContaining(tester.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSequenceNumber);
				AssertNoNotifications(tester.LocalReferenceNumberInfo);
			});
		}

		public void TestGetGateInOutMessageSendingNotification()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.AirDepotGateIn;
			AssertEquals("For Gate In/Out Manifests of Type 'ADI' De-consolidation address is Mandatory.\r\nFor Gate In/Out Manifests of Type 'ADI' Terminal address is Mandatory.", header.GetGateInOutMessageSendingNotification());
			var orgHeader = Factory.New<OrgHeader>();
			header.AMA_OA_DeconsolidateAddress = orgHeader.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = orgHeader.MainAddress.PK;
			AssertEquals(string.Empty, header.GetGateInOutMessageSendingNotification());
		}

		public void TestGetJobVoyageMessageSendingNotification()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			var origin = jobVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "GBLON";
			var destination = jobVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "ZADUR";
			var totOrigin = jobVoyage.Origins.Count;
			AssertEquals("Prerequisite: Number of VoyageOrigin objects in collection.", 1, totOrigin);
			var totDest = jobVoyage.Destinations.Count;
			AssertEquals("Prerequisite: Number of VoyageDestination objects in collection.", 1, totDest);
			const string DepartureError = "For Load Port 'GBLON', one of ETD or ATD is Mandatory.";
			const string ArrivalError = "For Discharge Port 'ZADUR', one of ETA or ATA is Mandatory.";
			var date = new ZDateTime(2020, 07, 30);
			var testCases = GetJobVoyageMessageSendingTestCases();
			foreach (var testCase in testCases)
			{
				origin.JA_A_DEP = testCase.HasActualDepartureDate ? date : ZDateTime.Empty;
				origin.JA_E_DEP = testCase.HasEstimatedDepartureDate ? date : ZDateTime.Empty;
				destination.JB_A_ARV = testCase.HasActualArrivalDate ? date : ZDateTime.Empty;
				destination.JB_E_ARV = testCase.HasEstimatedArrivalDate ? date : ZDateTime.Empty;
				var validationText = jobVoyage.GetJobVoyageMessageSendingNotification();
				var assertMsg = testCase.ToString();
				AssertEquals(assertMsg, testCase.ShouldHaveDestinationError, validationText.Contains(DepartureError));
				AssertEquals(assertMsg, testCase.ShouldHaveArrivalError, validationText.Contains(ArrivalError));
			}

			origin.JA_A_DEP = ZDateTime.Empty;
			origin.JA_E_DEP = ZDateTime.Empty;
			destination.JB_A_ARV = ZDateTime.Empty;
			destination.JB_E_ARV = ZDateTime.Empty;
			var validationString = jobVoyage.GetJobVoyageMessageSendingNotification();
			AssertEquals("Check concatination of 2 error messages.", $"{DepartureError}\r\n{ArrivalError}", validationString);
		}

		public void TestGetJobNumberOfDuplicateUCR()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "JOB ONE";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.LoadOrCreateUCRNumber("UCR1123M");
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "JOB TWO";
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.LoadOrCreateUCRNumber("UCR1123S");
			Factory.Save();
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_DeclarationReference = "JOB THREE";
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			AssertEquals("No dupe", "", ValidationHelper.GetJobNumberOfDuplicateUCR(Factory, "", entry3.PK));
			AssertEquals("No dupe - ends with M", "", ValidationHelper.GetJobNumberOfDuplicateUCR(Factory, "UCR1123M", entry3.PK));
			AssertEquals("UCR is a duplicate on JOB TWO", "JOB TWO", ValidationHelper.GetJobNumberOfDuplicateUCR(Factory, "UCR1123S", entry3.PK));
		}

		public void TestValidateBlankLinesForEDIFACT()
		{
			const string message = "Too many Line Feed and Carriage Return characters in this text field are known to cause EDI Gateway errors";
			var dummy = Factory.New<DummyBusinessObject>();
			using (dummy.SuspendValidationTesting())
			{
				CombineAssertions(() =>
				{
					dummy.Z0_Description = "\rLine2\rLine 3";
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertHasMessageError("Blank line CR", dummy.Z0_DescriptionInfo, message);
					dummy.Z0_Description = ZString.Empty;
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertNoMessageError("Empty", dummy.Z0_DescriptionInfo, message);
					dummy.Z0_Description = "\nLine2\nLine 3";
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertHasMessageError("Blank line LF", dummy.Z0_DescriptionInfo, message);
					dummy.Z0_Description = "Line 1";
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertNoMessageError("Single line", dummy.Z0_DescriptionInfo, message);
					dummy.Z0_Description = "\r\nLine2\r\nLine 3";
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertHasMessageError("Blank line CRLF", dummy.Z0_DescriptionInfo, message);
					dummy.Z0_Description = "Line1\nLine 2";
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertNoMessageError("Multiple line", dummy.Z0_DescriptionInfo, message);
					dummy.Z0_Description = "Line1\r\n\r\nLine 3";
					ValidationHelper.ValidateBlankLinesForEDIFACT(dummy.Z0_DescriptionInfo, dummy.Z0_Description);
					AssertHasMessageError("Blank line 2 CRLF", dummy.Z0_DescriptionInfo, message);
				});
			}
		}

		sealed class JobVoyageMessageSendingTestCase
		{
			public JobVoyageMessageSendingTestCase(bool hasActualDepartureDate, bool hasEstimatedDepartureDate, bool hasActualArrivalDate, bool hasEstimatedArrivalDate)
			{
				HasActualDepartureDate = hasActualDepartureDate;
				HasEstimatedDepartureDate = hasEstimatedDepartureDate;
				HasActualArrivalDate = hasActualArrivalDate;
				HasEstimatedArrivalDate = hasEstimatedArrivalDate;
			}

			public readonly bool HasActualDepartureDate;
			public readonly bool HasEstimatedDepartureDate;
			public readonly bool HasActualArrivalDate;
			public readonly bool HasEstimatedArrivalDate;
			public bool ShouldHaveDestinationError => !HasActualDepartureDate && !HasEstimatedDepartureDate;
			public bool ShouldHaveArrivalError => !HasActualArrivalDate && !HasEstimatedArrivalDate;
			public override string ToString()
			{
				return "Test case: HasActualDepartureDate=" + HasActualDepartureDate +
					", HasEstimatedDepartureDate=" + HasEstimatedDepartureDate +
					", HasActualArrivalDate=" + HasActualArrivalDate +
					", HasEstimatedArrivalDate=" + HasEstimatedArrivalDate +
					", ShouldHaveDestinationError=" + ShouldHaveDestinationError +
					", ShouldHaveArrivalError=" + ShouldHaveArrivalError;
			}
		}

		List<JobVoyageMessageSendingTestCase> GetJobVoyageMessageSendingTestCases()
		{
			var list = new List<JobVoyageMessageSendingTestCase>();
			var boolValues = new List<bool>()
			{ false, true };
			foreach (bool boolValue1 in boolValues)
			{
				foreach (bool boolValue2 in boolValues)
				{
					foreach (bool boolValue3 in boolValues)
					{
						foreach (bool boolValue4 in boolValues)
						{
							list.Add(new JobVoyageMessageSendingTestCase(boolValue1, boolValue2, boolValue3, boolValue4));
						}
					}
				}
			}

			return list;
		}
	}
}
