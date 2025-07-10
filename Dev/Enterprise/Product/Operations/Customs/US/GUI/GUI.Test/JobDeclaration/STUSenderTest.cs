using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.STU;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class STUSenderTest : TestCaseWithFactory
	{
		[TestDate(2011, 1, 1)]
		public void TestSend()
		{
			var declaration = GetLiveEntryDeclaration();
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 3);
			Assert("PreCondition", declaration.HasPSDChangedAndLiveEntry());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			new STUSender(declaration).Send();
			var dateFormat = "MM-dd-yyyy"; // This is a US format.
			var fromPSD = declaration.US_PSDAccepted.Date.ToString(dateFormat);
			var toPSD = declaration.US_PreliminaryStatementPrintDate.ToString(dateFormat);
			var expected = string.Format("The Preliminary Statement Date on file at Customs is '{0}'. Would you like to send a Statement Delete/Add Message to change the date to '{1}'?", fromPSD, toPSD);
			AssertContains(expected, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, declaration.Messages.Count);
		}

		[TestDate(2011, 1, 1)]
		public void TestNoSend()
		{
			var declaration = GetLiveEntryDeclaration();
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 3);
			Assert("PreCondition", declaration.HasPSDChangedAndLiveEntry());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
			new STUSender(declaration).Send();
			var dateFormat = "MM-dd-yyyy"; // This is a US format.
			var fromPSD = declaration.US_PSDAccepted.Date.ToString(dateFormat);
			var toPSD = declaration.US_PreliminaryStatementPrintDate.ToString(dateFormat);
			var expected = string.Format("The Preliminary Statement Date on file at Customs is '{0}'. Would you like to send a Statement Delete/Add Message to change the date to '{1}'?", fromPSD, toPSD);
			AssertContains(expected, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Not sent", 0, declaration.Messages.Count);
			var logs = declaration.Logs.Find(new CargoWise.EntityFramework.ZQuery(Enterprise.ZArchitecture.Schema.StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.Events.PeriodDateChanged.Code));
			AssertEquals("Logged", 1, logs.Length);
		}

		[TestDate(2011, 1, 1)]
		public void TestSTUSenderForLiveEntryHookedToForm()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = GetLiveEntryDeclaration();
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 3);
				Assert("PreCondition", declaration.HasPSDChangedAndLiveEntry());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				var continueWithSave = form.FireSaveButton();
				AssertEquals(CargoWise.EntityFramework.ContinueWithSave.Yes, continueWithSave);
				AssertEquals("Message is generated", 1, declaration.Messages.Count);
			}
		}

		[TestDate(2011, 1, 1)]
		public void TestSTUSenderForLiveEntryHookedToPlugIn()
		{
			var declaration = GetLiveEntryDeclaration();
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(shipment))
			{
				declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 3);
				Assert("PreCondition", declaration.HasPSDChangedAndLiveEntry());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				var continueWithSave = plugin.ShowPreSaveDialogs();
				AssertEquals(CargoWise.EntityFramework.ContinueWithSave.Yes, continueWithSave);
				AssertEquals("Message is generated", 1, declaration.Messages.Count);
			}
		}

		JobDeclaration GetLiveEntryDeclaration()
		{
			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, request);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 1);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 1, 2);
			return declaration;
		}
	}
}
