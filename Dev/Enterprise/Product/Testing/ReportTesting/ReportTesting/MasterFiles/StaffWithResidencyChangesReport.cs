using System;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff With Residency Changes Report")]
	public class StaffWithResidencyChangesReportTemplateTest : TemplateTestCase
	{
		public void TestOnlyGetStaffWithAddressChanges()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_UserAddress1 = "123 Ocean Dr";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_UserAddress2 = "Sydney City";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Address2", new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["Address Changes"]).AddOption("Address Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 7, 1), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("123 Ocean Dr", sheetContent[codeRow, codeColumn + 7].ToString());

				GetIndex("STV", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("STV", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 7, 1), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("Sydney City", sheetContent[codeRow, codeColumn + 8].ToString());
			}
		}

		public void TestOnlyGetStaffWithPostcodeChange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_Postcode = "2000";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_UserAddress1 = "1000 Pitt St";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Postcode", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["Postcode Changes"]).AddOption("Postcode Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 6, 30), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("2000", sheetContent[codeRow, codeColumn + 9].ToString());

				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestOnlyGetStaffWithCityChange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_City = "Sydney";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_UserAddress1 = "1000 Pitt St";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Postcode", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "City", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["City Changes"]).AddOption("City Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 6, 30), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("Sydney", sheetContent[codeRow, codeColumn + 10].ToString());

				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestOnlyGetStaffWithStateChange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_State = "NSW";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_UserAddress1 = "1000 Pitt St";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Postcode", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "City", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "State", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["State Changes"]).AddOption("State Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 6, 30), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("NSW", sheetContent[codeRow, codeColumn + 11].ToString());

				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestOnlyGetStaffWithCountryChange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_RN_NKCountryCode = "AU";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_UserAddress1 = "1000 Pitt St";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Postcode", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "City", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "State", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Country/Region", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["Country/Region Changes"]).AddOption("Country Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 6, 30), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("Australia", sheetContent[codeRow, codeColumn + 12].ToString());

				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestIgnoreNonResidencyChangesWithinRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_UserAddress1 = "123 Ocean Dr";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_EmailAddress = "steve@gmail.com";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Email|OLD=|NEW=steve@gmail.com", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["Address Changes"]).AddOption("Address Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertEquals(new ZDateTime(2023, 6, 30), DateTime.FromOADate((double)sheetContent[codeRow, codeColumn + 6]) + offset);
				AssertContains("Australia", sheetContent[codeRow, codeColumn + 12].ToString());

				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestGetNoStaffWhenResidencyChangesNotSpecified()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_UserAddress1 = "123 Ocean Dr";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			steve.GS_EmailAddress = "steve@gmail.com";

			var greg = Factory.NewWithValidTestData<GlbStaff>();
			greg.GS_Code = "GRG";
			greg.GS_State = "NSW";

			var jeff = Factory.NewWithValidTestData<GlbStaff>();
			jeff.GS_Code = "JEF";
			jeff.GS_Postcode = "2000";

			var chris = Factory.NewWithValidTestData<GlbStaff>();
			chris.GS_Code = "CHR";
			chris.GS_RN_NKCountryCode = "AU";

			var john = Factory.NewWithValidTestData<GlbStaff>();
			john.GS_Code = "JON";
			john.GS_City = "Sydney";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Email|OLD=|NEW=steve@gmail.com", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			greg.Logs.AddNew(Events.EditedARecord, "State", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			jeff.Logs.AddNew(Events.EditedARecord, "Postcode", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			chris.Logs.AddNew(Events.EditedARecord, "Country/Region", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			john.Logs.AddNew(Events.EditedARecord, "City", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				var listOfCodes = new[] { "BOB", "STV", "GRG", "JEF", "CHR", "JON" };
				foreach (var code in listOfCodes)
				{
					AssertNotContains(code, sheetContent.ToString());
				}
			}
		}

		public void TestIgnoreStaffOutsideOfRange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_UserAddress1 = "123 Ocean Dr";

			var steve = Factory.NewWithValidTestData<GlbStaff>();
			steve.GS_Code = "STV";
			bob.GS_UserAddress1 = "123 Ocean Dr";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			steve.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 8, 1, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["Address Changes"]).AddOption("Address Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertNotContains("STV", sheetContent.ToString());
			}
		}

		public void TestGetOnlyOneStaffRowWithResidencyChange()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_Code = "BOB";
			bob.GS_UserAddress1 = "123 Ocean Dr";
			bob.GS_City = "Sydney";

			var offset = new TimeSpan(10, 0, 0);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Address1", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			bob.Logs.AddNew(Events.EditedARecord, "Postcode", new ZDateTimeOffset(2023, 6, 30, 0, 0, 0, offset));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			((DateTimeOffsetField)Report.FilterCollection["Report Start Date"]).Value = new ZDateTimeOffset(2023, 6, 1, 0, 0, 0, offset);
			((DateTimeOffsetField)Report.FilterCollection["Report End Date"]).Value = new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, offset);

			((OptionGroup)Report.FilterCollection["Address Changes"]).AddOption("Address Changes", "Y", true);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];
				int codeRow = -1, codeColumn = -1;
				GetIndex("BOB", sheetContent, ref codeRow, ref codeColumn);
				AssertContains("BOB", sheetContent[codeRow, codeColumn].ToString());
				AssertNotContains("BOB", sheetContent[codeRow + 1, codeColumn].ToString());
			}
		}

		void GetIndex(string code, ExcelWorkSheet report, ref int codeRow, ref int codeColumn)
		{
			for (var row = 0; row < report.RowCount; row++)
			{
				for (var col = 0; col < report.ColumnCount; col++)
				{
					if (report[row, col].ToString().Contains(code))
					{
						codeRow = row;
						codeColumn = col;
						return;
					}
				}
			}
			Fail("Couldn't find " + code);
		}
	}
}
