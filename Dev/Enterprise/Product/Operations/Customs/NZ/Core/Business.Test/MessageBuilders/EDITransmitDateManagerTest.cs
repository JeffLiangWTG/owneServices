using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class EDITransmitDateManagerTest : TestCaseWithFactory
	{
		public void TestErrorImportTooLate()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			declaration.JE_DateOfArrival = declaration.CachedTodaysDate;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(21);
			AssertNoErrors("We will not prevent users from submitting an import job even it is too late.");
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(20);
			AssertNoErrors();
		}

		public void TestErrorExportTooEarly()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(31);
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			AssertHasError(EDITransmitDateManager.ErrorExportTooEarly, expectRecommendedDate: true, "We prevent users from submitting an export job if is ts too early.");
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(1);
			AssertNoErrors();
		}

		public void TestErrorExportTooLate()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = declaration.CachedTodaysDate;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(31);
			AssertNoErrors("We will not prevent users from submitting an export job even it is too late.");
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(30);
			AssertNoErrors();
		}

		public void TestErrorEDITransmitDateInThePast()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(-1);
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(-1);
			AssertHasError(EDITransmitDateManager.ErrorEDITransmitDateInThePast, expectRecommendedDate: true);
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			AssertNoErrors();
		}

		public void TestWarningNotEarliestDatePossible()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(10);
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(2);
			AssertHasWarning(EDITransmitDateManager.WarningNotEarliestDatePossible, expectRecommendedDate: true);
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			AssertNoWarnings();
			declaration.JE_ExportDate = ZDateTime.Empty;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(10);
			AssertNoWarnings();
		}

		public void TestRecommendedDateExportAir()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(40);
			AssertEquals(declaration.CachedTodaysDate.AddDays(10), dateManager.RecommendedDate);
			declaration.JE_ExportDate = declaration.CachedTodaysDate;
			AssertEquals(declaration.CachedTodaysDate, dateManager.RecommendedDate);
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(-5);
			AssertEquals(declaration.CachedTodaysDate, dateManager.RecommendedDate);
		}

		public void TestRecommendedDateExportSea()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(40);
			AssertEquals(declaration.CachedTodaysDate.AddDays(10), dateManager.RecommendedDate);
			declaration.JE_ExportDate = declaration.CachedTodaysDate;
			AssertEquals(declaration.CachedTodaysDate, dateManager.RecommendedDate);
			declaration.JE_ExportDate = declaration.CachedTodaysDate.AddDays(-5);
			AssertEquals(declaration.CachedTodaysDate, dateManager.RecommendedDate);
		}

		#region Implementation
		Declaration.JobDeclaration declaration;
		EDITransmitDateManager dateManager;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Declaration.JobDeclaration>();
			dateManager = new EDITransmitDateManager(declaration);
		}

		void AssertHasError(string expectedMessage, bool expectRecommendedDate, string because = "")
		{
			EDITransmitDateManager.ValidationResult result = declaration.TransmitDateManager.CheckEDITransmitDate();
			ZString messageText = result != null ? result.MessageText : ZString.Empty;
			expectedMessage += expectRecommendedDate ? EDITransmitDateManager.NotificationRecommendedDatePrefix + dateManager.RecommendedDate.ToString() + ")" : "";
			AssertContains("result.MessageText", expectedMessage, messageText);
			AssertEquals("result.IsError " + because, true, result.IsError);
		}

		void AssertNoErrors(string because = "")
		{
			EDITransmitDateManager.ValidationResult result = declaration.TransmitDateManager.CheckEDITransmitDate();
			AssertEquals("No Errors " + because, true, result == null || !result.IsError);
		}

		void AssertHasWarning(string expectedMessage, bool expectRecommendedDate)
		{
			EDITransmitDateManager.ValidationResult result = declaration.TransmitDateManager.CheckEDITransmitDate();
			ZString messageText = result != null ? result.MessageText : ZString.Empty;
			expectedMessage += expectRecommendedDate ? EDITransmitDateManager.NotificationRecommendedDatePrefix + dateManager.RecommendedDate.ToString() + ")" : "";
			AssertContains("result.MessageText", expectedMessage, messageText);
			AssertEquals("result.IsError", false, result.IsError);
		}

		void AssertNoWarnings()
		{
			EDITransmitDateManager.ValidationResult result = declaration.TransmitDateManager.CheckEDITransmitDate();
			AssertEquals("No Errors", true, result == null || result.IsError);
		}
		#endregion
	}
}
