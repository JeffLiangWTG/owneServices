using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.STU;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class STUExtensionMethods : TestCaseWithFactory
	{
		[TestDate(2011, 8, 8)]
		public void TestHasPSDChangedAndLiveEntry()
		{
			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, request);

			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 1;

			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, statementData);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 8, 8);
			Assert(!declaration.HasPSDChangedAndLiveEntry());

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.DeriveDeclarationStatus();
			Assert("Precondition", entry.HasBeenLodgedAtCustoms);
			Assert(declaration.US_PSDAccepted.IsValid);
			Assert("because PSD has not changed", !declaration.HasPSDChangedAndLiveEntry());

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 8, 10);
			Assert("For live entry, Presentation Date should have been entered", !declaration.HasPSDChangedAndLiveEntry());

			Factory.Save();

			declaration.US_PresentationDate = new ZDateTime(2011, 8, 7);
			Assert(declaration.HasPSDChangedAndLiveEntry());

			declaration.US_PresentationDate = new ZDateTime(2011, 8, 9);
			Assert(declaration.HasPSDChangedAndLiveEntry());

			declaration.Messages.Add(DeclarationTestHelper.CreateTransmittedSTUMsg(Factory, "30448", new ZDateTime(2013, 01, 29), "B011101SV9HP                                               " + MQEDIMessage.MessageNumberPlaceHolder + "H1101SV9 710004812012913                                                        Y  1101SV9HP00001"));
			Assert(!declaration.HasPSDChangedAndLiveEntry());

			var dateFormat = "MM-dd-yyyy";// This is a US format.
			var fromPSD = declaration.US_PSDAccepted.Date.ToString(dateFormat);
			var toPSD = declaration.US_PreliminaryStatementPrintDate.ToString(dateFormat);

			var expected = string.Format("The Preliminary Statement Date on file at Customs is '{0}'. Would you like to send a Statement Delete/Add Message to change the date to '{1}'?", fromPSD, toPSD);
			AssertEquals(expected, declaration.GetPSDChangedNotification());
		}

		public void TestLog()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.LogNoSTUSent();

			var logs = declaration.Logs.Find(new CargoWise.EntityFramework.ZQuery(ZArchitecture.Schema.StmALogSchema.SL_SE_NKEvent, Events.PeriodDateChanged.Code));
			AssertEquals("Logged", 1, logs.Length);
			AssertEquals(JobDeclarationSTUSendingExtensionMethods.LogReference, logs[0].SL_Reference);
		}
	}
}
