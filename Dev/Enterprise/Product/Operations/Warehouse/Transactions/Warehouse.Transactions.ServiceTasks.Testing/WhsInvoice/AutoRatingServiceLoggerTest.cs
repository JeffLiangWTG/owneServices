using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class AutoRatingServiceLoggerTest : TestCaseWithFactory
	{
		#region TestAutoRatingServiceLogger_Error

		[TestDate(2022, 5, 9, 10, 32, 0)]
		public void TestAutoRatingServiceLogger_Error()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			Factory.Save();

			var msg = "Test Message!";
			var logger = new Mock<ILogger>();
			var autoRatingServiceLogger = new BillingAutomationServiceLogger(logger.Object);
			autoRatingServiceLogger.Error(msg);

			logger.Verify(l => l.Log(LogType.Error, msg), Times.Once);
			logger.Verify(l => l.Log(LogType.Information, msg), Times.Never);
			logger.Verify(l => l.Log(LogType.Error, msg, It.IsAny<Exception>()), Times.Never);
			AssertExpectedNoteAddedToInvoice("Should not add to note if invoice process not started!", invoice, null);

			using (autoRatingServiceLogger.EnableAddingNoteWhileLogging(invoice.PK))
			{
				autoRatingServiceLogger.Error(msg);
				AssertExpectedNoteAddedToInvoice("Still nothing until finish process!", invoice, null);
			}
			AssertExpectedNoteAddedToInvoice("Should add to note to invoice!", invoice, "Error|" + msg);
		}

		#endregion

		#region TestAutoRatingServiceLogger_Information

		[TestDate(2022, 5, 9, 10, 32, 0)]
		public void TestAutoRatingServiceLogger_Information()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			Factory.Save();

			var msg = "Test Message!";
			var logger = new Mock<ILogger>();
			var autoRatingServiceLogger = new BillingAutomationServiceLogger(logger.Object);
			autoRatingServiceLogger.Information(msg);

			logger.Verify(l => l.Log(LogType.Error, msg), Times.Never);
			logger.Verify(l => l.Log(LogType.Information, msg), Times.Once);
			logger.Verify(l => l.Log(LogType.Error, msg, It.IsAny<Exception>()), Times.Never);
			AssertExpectedNoteAddedToInvoice("Should not add to note if invoice process not started!", invoice, null);

			using (autoRatingServiceLogger.EnableAddingNoteWhileLogging(invoice.PK))
			{
				autoRatingServiceLogger.Information(msg);
				AssertExpectedNoteAddedToInvoice("Still nothing until finish process!", invoice, null);
			}
			AssertExpectedNoteAddedToInvoice("Should add to note to invoice!", invoice, "Information|" + msg);
		}

		#endregion

		#region TestAutoRatingServiceLogger_EnableAddingNoteWhileLogging

		[TestDate(2022, 5, 9, 10, 32, 0)]
		public void TestAutoRatingServiceLogger_EnableAddingNoteWhileLogging()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			Factory.Save();

			var logger = new Mock<ILogger>();
			var autoRatingServiceLogger = new BillingAutomationServiceLogger(logger.Object);
			using (autoRatingServiceLogger.EnableAddingNoteWhileLogging(invoice.PK))
			{
				autoRatingServiceLogger.Information("Test Message <<1>>");
			}
			AssertExpectedNoteAddedToInvoice("Should add to note to invoice!", invoice, "Information|Test Message <<1>>");

			using (autoRatingServiceLogger.EnableAddingNoteWhileLogging(invoice.PK))
			{
				autoRatingServiceLogger.Error("Test Message <<2>>");
			}
			AssertExpectedNoteAddedToInvoice("Still one note and update with new text!", invoice, "Error|Test Message <<2>>");
		}

		#endregion

		#region TestAutoRatingServiceLogger_NoteRelatedCompany

		[TestDate(2022, 5, 9, 10, 32, 0)]
		public void TestAutoRatingServiceLogger_NoteRelatedCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			Factory.Save();

			var logger = new Mock<ILogger>();
			var autoRatingServiceLogger = new BillingAutomationServiceLogger(logger.Object);
			using (autoRatingServiceLogger.EnableAddingNoteWhileLogging(invoice.PK))
			{
				autoRatingServiceLogger.Information("Test Message <<1>>");
			}
			AssertExpectedNoteAddedToInvoice("Should add to note to invoice!", invoice, "Information|Test Message <<1>>");

			var company1PK = invoice.Warehouse.RelatedCompanyBranch.GB_GC;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Helper.CreateGlbBranch("Br2");
			otherBranch.GB_GC = company2.PK;
			invoice.Warehouse.WW_GB_RelatedCompanyBranch = otherBranch.PK;
			Factory.Save();

			using (autoRatingServiceLogger.EnableAddingNoteWhileLogging(invoice.PK))
			{
				autoRatingServiceLogger.Error("Test Message <<2>>");
			}
			var notes = NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description);
			var notes1 = notes.Where(n => n.ST_GC_RelatedCompany == company1PK).Single();
			AssertEquals("Should have two notes!", 2, notes.Length);
			AssertMultilineASCIIEquals("Should not delete first note!", notes1.ST_NoteText, @"Information|Test Message <<1>>
Service task log
Time: 09-May-22 10:32");

			var notes2 = notes.Where(n => n.ST_GC_RelatedCompany == company2.PK).Single();
			AssertMultilineASCIIEquals("Should add new note!", notes2.ST_NoteText, @"Error|Test Message <<2>>
Service task log
Time: 09-May-22 10:32");
		}

		#endregion

		#region AssertExpectedNoteAddedToInvoice

		void AssertExpectedNoteAddedToInvoice(string assertMsg, WhsInvoice invoice, string expectedNote)
		{
			var notes = NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description);
			if (expectedNote == null)
			{
				AssertEquals(false, notes.Length > 0);
			}
			else
			{
				var expectedNoteWithServiceTaskInfo = expectedNote + @"
Service task log
Time: 09-May-22 10:32";
				var note = notes.Single();
				AssertMultilineASCIIEquals(assertMsg, expectedNoteWithServiceTaskInfo, note.ST_NoteText);
				AssertEquals("Note should add for related company", invoice.Warehouse.RelatedCompanyBranch.GB_GC, note.ST_GC_RelatedCompany);
			}
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
