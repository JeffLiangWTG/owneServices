using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation.Testing
{
	sealed class CriticalValidationHelpersTest : TestCaseWithFactory
	{
		public void TestAddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_ForMixedInDbState()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			AssertNotNull("Postcondition: WIP is created", charge.ARLine);
			var lineInDb = charge.ARLine;

			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb); //to allow info to be collected

			charge.JR_OH_SellAccount = ZGuid.NewZGuid();
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(lineInDb, ZGuid.Empty, charge);
			AssertInfoCollected(nameof(lineInDb), lineInDb, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb);

			using (lineInDb.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
			{
				lineInDb.AL_ReverseDate = ZDateTime.Now;
				charge.JR_AL_ARLine = ZGuid.Empty;
			}
			var lineNotInDb = Factory.New<AccTransactionLines>();
			lineNotInDb.AL_LineType = TransactionLineTypes.WIP;
			charge.JR_AL_ARLine = lineNotInDb.PK;
			AssertNoExceptionThrown(() => CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(lineNotInDb, ZGuid.Empty, charge));
			AssertInfoCollected(nameof(lineNotInDb), lineNotInDb, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);

			void AssertInfoCollected(string message, AccTransactionLines line, CriticalValidationInfoCollectorServiceKeyType key)
			{
				var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, key);
				AssertContains(message, "RelatedCharge PK:", collectedInfo);
			}
		}

		public void TestAddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways_WIP()
		{
			AssertNull(CriticalValidationInfoCollectorService.GetService(Factory));
			var expectedLineOrgPK = ZGuid.NewZGuid();
			var expectedChargeOrgPK = ZGuid.NewZGuid();
			var expectedOldValue = ZGuid.NewZGuid();

			var line = Factory.New<AccTransactionLines>();
			line.AL_OH = expectedLineOrgPK;
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = ZGuid.NewZGuid();
			charge.JR_OH_SellAccount = expectedChargeOrgPK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_AL_ARLine = line.PK;

			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue, charge);
			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			AssertContainsInOrder("line time is empty, charge is passed", collectedInfo, infoNotCollected);

			line.AL_LineType = TransactionLineTypes.WIP;
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line is not job related and charge is not passed", collectedInfo, infoNotCollected);

			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {expectedChargeOrgPK}, Line Organization: {expectedLineOrgPK}, Old Organization: {expectedOldValue}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue, charge);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line is not job related, but charge is passed", collectedInfo, expectedInfo);

			line.AL_JH = charge.JR_JH;
			var expectedOldValue2 = ZGuid.NewZGuid();
			var expectedInfo2 =
$@"
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {expectedChargeOrgPK}, Line Organization: {expectedLineOrgPK}, Old Organization: {expectedOldValue2}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line is job related and charge is not passed", collectedInfo, expectedInfo, expectedInfo2);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			line.AL_OH = charge.JR_OH_SellAccount;
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line and charge orgs are equal", collectedInfo, infoNotCollected);

			CriticalValidationInfoCollectorService.ClearKeysRequestedInThisSession_ForTestOnly();
			((INeedRow)line).Row.AcceptChanges();
			Assert("Precondition: line in db", line.IsInDatabase);
			line.AL_OH = expectedLineOrgPK;
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2, charge);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb);
			var infoInDbNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			AssertContainsInOrder("line is in db first time", collectedInfo, infoInDbNotCollected);

			var expectedInfo3 =
		$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb:
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {expectedChargeOrgPK}, Line Organization: {expectedLineOrgPK}, Old Organization: {expectedOldValue2}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2, charge);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb);
			AssertContainsInOrder("line is in db second time", collectedInfo, expectedInfo3);
		}

		public void TestAddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways_Accrual()
		{
			AssertNull(CriticalValidationInfoCollectorService.GetService(Factory));
			var expectedLineOrgPK = ZGuid.NewZGuid();
			var expectedChargeOrgPK = ZGuid.NewZGuid();
			var expectedOldValue = ZGuid.NewZGuid();

			var line = Factory.New<AccTransactionLines>();
			line.AL_OH = expectedLineOrgPK;
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = ZGuid.NewZGuid();
			charge.JR_OH_CostAccount = expectedChargeOrgPK;
			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_AL_APLine = line.PK;

			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue, charge);
			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			AssertContainsInOrder("line time is empty, charge is passed", collectedInfo, infoNotCollected);

			line.AL_LineType = TransactionLineTypes.Accrual;
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line is not job related and charge is not passed", collectedInfo, infoNotCollected);

			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {expectedChargeOrgPK}, Line Organization: {expectedLineOrgPK}, Old Organization: {expectedOldValue}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue, charge);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line is not job related, but charge is passed", collectedInfo, expectedInfo);

			line.AL_JH = charge.JR_JH;
			var expectedOldValue2 = ZGuid.NewZGuid();
			var expectedInfo2 =
$@"
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {expectedChargeOrgPK}, Line Organization: {expectedLineOrgPK}, Old Organization: {expectedOldValue2}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line is job related and charge is not passed", collectedInfo, expectedInfo, expectedInfo2);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			line.AL_OH = charge.JR_OH_CostAccount;
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("line and charge orgs are equal", collectedInfo, infoNotCollected);

			CriticalValidationInfoCollectorService.ClearKeysRequestedInThisSession_ForTestOnly();
			((INeedRow)line).Row.AcceptChanges();
			Assert("Precondition: line in db", line.IsInDatabase);
			line.AL_OH = expectedLineOrgPK;
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2, charge);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb);
			var infoInDbNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			AssertContainsInOrder("line is in db first time", collectedInfo, infoInDbNotCollected);

			var expectedInfo3 =
		$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb:
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {expectedChargeOrgPK}, Line Organization: {expectedLineOrgPK}, Old Organization: {expectedOldValue2}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";
			CriticalValidationHelpers.AddInfo_WIPACROrganizationNotEqualToRelatedChargeForNewLine_CollectAlways(line, expectedOldValue2, charge);
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb);
			AssertContainsInOrder("line is in db second time", collectedInfo, expectedInfo3);
		}

		public void TestReportInvalidOperation()
		{
			CriticalValidationHelpers.ReportInvalidOperation("TestReportInvalidOperation", "", true);
			AssertEquals("Silent exception must NOT be sent. LastKeyReported: ", "", ErrorReporter.LastKeyReported);
			AssertEquals("Silent exception must NOT be sent. LastMessageReported: ", "", ErrorReporter.LastMessageReported);
			AssertEquals("Silent exception must NOT be sent. LastExceptionReported: ", null, ErrorReporter.LastExceptionReported);

			CriticalValidationHelpers.ReportInvalidOperation("TestReportInvalidOperation", "All is not so bad.", false);
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "TestReportInvalidOperation", ErrorReporter.LastKeyReported);
			AssertEquals("Silent exception must be sent. LastMessageReported: ", "All is not so bad.", ErrorReporter.LastMessageReported);
			AssertEquals("Silent exception must be sent. LastExceptionReported: ", "All is not so bad.", ErrorReporter.LastExceptionReported.Message);
			AssertEquals("Inner exception must have call stack info", false, string.IsNullOrEmpty(ErrorReporter.LastExceptionReported.StackTrace));
			ErrorReporter.Clear();
			ExceptionReporterTestListener.Instance.Clear();

			try
			{
				CriticalValidationHelpers.ReportInvalidOperation("TestReportInvalidOperation", "All is very bad.", true);
				Assert("Exception must be raised.", false);
			}
			catch (InvalidOperationException ex)
			{
				AssertEquals("Silent exception must be sent. LastKeyReported: ", "TestReportInvalidOperation", ErrorReporter.LastKeyReported);
				AssertEquals("Silent exception must be sent. LastMessageReported: ", "All is very bad.", ErrorReporter.LastMessageReported);
				AssertEquals("Silent exception must be sent. LastExceptionReported: ", ex, ErrorReporter.LastExceptionReported);
				AssertEquals("Silent exception must be sent. LastExceptionReported.Message: ", "All is very bad.", ErrorReporter.LastExceptionReported.Message);
				AssertEquals("Inner exception must have call stack info", false, string.IsNullOrEmpty(ErrorReporter.LastExceptionReported.StackTrace));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestReportArrayChanges()
		{
			JobCharge charge1 = (JobCharge)Factory.New(typeof(JobCharge), new Guid("37b6f42b-0fb8-4fba-8473-88a64eb0f080"));
			charge1.JR_GC = GlbCompany.CurrentCompany.PK;
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge3 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge4 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge5 = (JobCharge)Factory.New(typeof(JobCharge), new Guid("664e85bb-5d93-41b0-8723-5550ecdab5b6"));
			JobCharge charge6 = (JobCharge)Factory.New(typeof(JobCharge), new Guid("d1ec9fec-25d2-4bbb-abcd-f89c446aed69"));
			charge5.JR_GC = GlbCompany.CurrentCompany.PK;
			charge6.JR_GC = GlbCompany.CurrentCompany.PK;

			CriticalValidationHelpers.ReportArrayChanges(
				new JobCharge[] { charge1, charge2, charge3, charge4 },
				new JobCharge[] { charge2, charge3, charge4, charge5, charge6 },
				"TestReportArrayChanges",
				"Charge collection was changed.",
				false,
				(charge) => charge.GetJobChargeInfo());

			string expectedMessage =
@"Charge collection was changed.
Elements added to the collection:
Charge: PK = 37b6f42b-0fb8-4fba-8473-88a64eb0f080, Type = Charge, Charge Type = , Job PK = 00000000-0000-0000-0000-000000000000, Job Number = , Charge Code = , Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = , Sell Currency = , Is In DB = No, Has Changes = Yes, Is Saved By Factory = No, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 0, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
Elements removed from the collection:
Charge: PK = 664e85bb-5d93-41b0-8723-5550ecdab5b6, Type = Charge, Charge Type = , Job PK = 00000000-0000-0000-0000-000000000000, Job Number = , Charge Code = , Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = , Sell Currency = , Is In DB = No, Has Changes = Yes, Is Saved By Factory = No, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 0, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
Charge: PK = d1ec9fec-25d2-4bbb-abcd-f89c446aed69, Type = Charge, Charge Type = , Job PK = 00000000-0000-0000-0000-000000000000, Job Number = , Charge Code = , Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = , Sell Currency = , Is In DB = No, Has Changes = Yes, Is Saved By Factory = No, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 0, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
";
			AssertEquals("Silent exception must be sent. LastKeyReported: ", "TestReportArrayChanges", ErrorReporter.LastKeyReported);
			AssertEquals("Silent exception must be sent. LastMessageReported: ", expectedMessage, ErrorReporter.LastMessageReported);
			AssertEquals("Silent exception must be sent. LastExceptionReported: ", expectedMessage, ErrorReporter.LastExceptionReported.Message);
			AssertEquals("Inner exception must have call stack info", false, string.IsNullOrEmpty(ErrorReporter.LastExceptionReported.StackTrace));
			ErrorReporter.Clear();
			ExceptionReporterTestListener.Instance.Clear();

			try
			{
				CriticalValidationHelpers.ReportArrayChanges(
					new JobCharge[] { charge4, charge6, charge3, charge2 },
					new JobCharge[] { charge2, charge4, charge1, charge3 },
					"TestReportArrayChanges",
					"Charge collection was changed.",
					true,
					(charge) => charge.GetJobChargeInfo());
				Assert("Exception must be raised.", false);
			}
			catch (InvalidOperationException ex)
			{
				expectedMessage =
@"Charge collection was changed.
Elements added to the collection:
Charge: PK = d1ec9fec-25d2-4bbb-abcd-f89c446aed69, Type = Charge, Charge Type = , Job PK = 00000000-0000-0000-0000-000000000000, Job Number = , Charge Code = , Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = , Sell Currency = , Is In DB = No, Has Changes = Yes, Is Saved By Factory = No, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 0, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
Elements removed from the collection:
Charge: PK = 37b6f42b-0fb8-4fba-8473-88a64eb0f080, Type = Charge, Charge Type = , Job PK = 00000000-0000-0000-0000-000000000000, Job Number = , Charge Code = , Charge Code Type = , Cost Account = , OS Cost Amount = 0, Local Cost Amount = 0, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = , Sell Currency = , Is In DB = No, Has Changes = Yes, Is Saved By Factory = No, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 0, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
";
				AssertEquals("Silent exception must be sent. LastKeyReported: ", "TestReportArrayChanges", ErrorReporter.LastKeyReported);
				AssertEquals("Silent exception must be sent. LastMessageReported: ", expectedMessage, ErrorReporter.LastMessageReported);
				AssertEquals("Silent exception must be sent. LastExceptionReported: ", ex, ErrorReporter.LastExceptionReported);
				AssertEquals("Silent exception must be sent. LastExceptionReported.Message: ", expectedMessage, ErrorReporter.LastExceptionReported.Message);
				AssertEquals("Inner exception must have call stack info", false, string.IsNullOrEmpty(ErrorReporter.LastExceptionReported.StackTrace));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestSetConflictWithCriticalFieldsBusinessContext()
		{
			var obj = Factory.New<DummyBusinessObject>();
			Assert("Precondition", !obj.HasContext(BusinessContext.ConflictWithCriticalFields));
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(obj);
			Assert("Should have Business Context ConflictWithCriticalFields", obj.HasContext(BusinessContext.ConflictWithCriticalFields));
		}
	}
}
