using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutomaticSTUConditionCheckerTest : TestCaseWithFactory
	{
		public void TestShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "1";
			Factory.Save();

			var psd = ZDateTime.Today.AddDays(1).Date;
			var checker = new AutomaticSTUConditionChecker();

			AssertEquals(false, checker.ShouldSend(declaration, psd));
			AssertHasSuppressSendingMessageEventWithText(declaration, AutomaticSTUConditionChecker.NotificationSuppressSTUNoActiveHeaders);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			((IRegistryItemInternals)USCustomsDataRegistry.Instance.AutoSendSDCR).DeleteValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty);
			AssertEquals(false, checker.ShouldSend(declaration, psd));
			var logReference = string.Format(AutomaticSTUConditionChecker.NotificationSuppressSTUAutoSendRegistryDisabled, declaration.Branch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code);
			AssertHasSuppressSendingMessageEventWithText(declaration, logReference);

			using (DataRegistry.Business.USCustomsDataRegistry.Instance.SuppressStatementUpdate.SetTemporaryValue(declaration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var request = new AutoSendStatementDateChangeRequest();

				request.OverrideAllOrByOrganisation = "ORG";
				declaration.IOROrgPK = Factory.New<OrgHeader>().PK;
				declaration.IORWrapper.ZO_DoNotAutoGenerateSDCR = true;
				declaration.IOR.OH_Code = "ABC";
				USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, request);
				AssertEquals(false, checker.ShouldSend(declaration, psd));
				logReference = string.Format(AutomaticSTUConditionChecker.NotificationSuppressSTUAutoSendDisabledForOrg, "ABC");
				AssertHasSuppressSendingMessageEventWithText(declaration, logReference);
				declaration.IOROrgPK = Guid.Empty;
				declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;

				request.OverrideAllOrByOrganisation = "ALL";
				USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, request);
				AssertEquals(true, checker.ShouldSend(declaration, psd));
			}

			using (DataRegistry.Business.USCustomsDataRegistry.Instance.SuppressStatementUpdate.SetTemporaryValue(declaration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, checker.ShouldSend(declaration, psd));

				var statement = Factory.New<CusStatementHeader>();
				var statementLine = statement.StatementLines.AddNew();
				statementLine.B3_EntryFilerCode = "XJ5";
				statementLine.B3_EntryNum = "1";
				statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
				declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 04, 26);
				AssertEquals(false, checker.ShouldSend(declaration, psd));
			}

			declaration.US_PSC = true;
			AssertEquals(false, checker.ShouldSend(declaration, psd));
			AssertHasSuppressSendingMessageEventWithText(declaration, AutomaticSTUConditionChecker.NotificationSuppressSTUPSCFlagTicked);

			declaration.US_PSC = false;
			AssertEquals(true, checker.ShouldSend(declaration, psd));
		}

		public void TestShouldSendWhenEntryTypeIsQuota()
		{
			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, request);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "1";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var notifications = declaration.ENSStatusNotifications;
			declaration.ImportEntryNumber = "36194291";

			var workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
			var nextWorkingDay = ((ZDateTime)workingDays.GetAnotherStandardWorkingDay(ZDateTime.Now.ToDateTime(), 1)).Date;
			var notification1 = notifications.AddNew();
			notification1.DispositionCode = ENSStatusDispositionCodeList._1;
			notification1.ActionIDNumber = "1";
			notification1.StatusDate = nextWorkingDay;

			var checker = new AutomaticSTUConditionChecker();
			Factory.Save();

			var psd = ((ZDateTime)workingDays.GetAnotherStandardWorkingDay(ZDateTime.Now.ToDateTime(), 12)).Date;
			var result = false;
			AssertNoExceptionThrown(() =>
			{
				result = checker.ShouldSend(declaration, psd);
			});
			AssertEquals(true, result);

			var notification2 = notifications.AddNew();
			notification2.DispositionCode = ENSStatusDispositionCodeList._Q;
			notification2.ActionIDNumber = "1";
			notification2.StatusDate = ((ZDateTime)workingDays.GetAnotherStandardWorkingDay(ZDateTime.Now.ToDateTime(), 2)).Date;
			result = checker.ShouldSend(declaration, psd);
			AssertEquals(true, result);

			notification2.StatusDate = nextWorkingDay;
			result = checker.ShouldSend(declaration, psd);
			AssertEquals(false, result);
		}

		public void TestAutoGenerateSDCRAcceptsCompanyLevelFallBack()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "1";
			Factory.Save();

			var psd = ZDateTime.Today.AddDays(1).Date;
			var checker = new AutomaticSTUConditionChecker();

			AssertEquals(false, checker.ShouldSend(declaration, psd));
			AssertHasSuppressSendingMessageEventWithText(declaration, AutomaticSTUConditionChecker.NotificationSuppressSTUNoActiveHeaders);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			((IRegistryItemInternals)USCustomsDataRegistry.Instance.AutoSendSDCR).DeleteValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			AssertEquals(false, checker.ShouldSend(declaration, psd));

			var logReference = string.Format(AutomaticSTUConditionChecker.NotificationSuppressSTUAutoSendRegistryDisabled, declaration.Branch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code);
			AssertHasSuppressSendingMessageEventWithText(declaration, logReference);

			using (DataRegistry.Business.USCustomsDataRegistry.Instance.SuppressStatementUpdate.SetTemporaryValue(declaration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var request = new AutoSendStatementDateChangeRequest();

				request.OverrideAllOrByOrganisation = "ORG";
				declaration.IOROrgPK = Factory.New<OrgHeader>().PK;
				declaration.IORWrapper.ZO_DoNotAutoGenerateSDCR = true;
				declaration.IOR.OH_Code = "ABC";
				USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, request);
				AssertEquals(false, checker.ShouldSend(declaration, psd));

				logReference = string.Format(AutomaticSTUConditionChecker.NotificationSuppressSTUAutoSendDisabledForOrg, "ABC");
				AssertHasSuppressSendingMessageEventWithText(declaration, logReference);

				declaration.IOROrgPK = Guid.Empty;
				declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
				request.OverrideAllOrByOrganisation = "ALL";
				USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, request);
				AssertEquals(true, checker.ShouldSend(declaration, psd));
			}
		}

		void AssertHasSuppressSendingMessageEventWithText(JobDeclaration declaration, ZString text)
		{
			Assert($"Should have SSM event with text: {text}",
				declaration.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.SuppressSendingMessageCode && x.ReferenceFreeText == text));
		}
	}
}
