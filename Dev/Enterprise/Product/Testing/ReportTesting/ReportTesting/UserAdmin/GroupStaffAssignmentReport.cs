using System;
using System.IO;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.UserAdmin
{
	[TemplateName("Group Staff Assignment Report")]
	public class GroupStaffAssignmentTemplateTest : TemplateTestCase
	{
		public void TestReport()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "A\\<";
			staff.GS_FullName = "Test staff";
			StmALog logAttachToGroup = Factory.New<StmALog>();
			StmALog logAttachToStaff = Factory.New<StmALog>();
			using (logAttachToGroup.LockForUpdatingKeyFieldsForTesting())
			{
				logAttachToGroup.SL_Table = "GlbGroup";
				logAttachToGroup.SL_Parent = group.PK;
				logAttachToGroup.SL_IsEstimate = false;
				logAttachToGroup.SL_Reference = "Attached - (" + staff.GS_Code + ") " + staff.GS_FullName;
				logAttachToGroup.SL_GS_NKUser = staff.GS_SystemLastEditUser;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				logAttachToGroup.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}
			using (logAttachToStaff.LockForUpdatingKeyFieldsForTesting())
			{
				logAttachToStaff.SL_Table = "GlbStaff";
				logAttachToStaff.SL_Parent = staff.PK;
				logAttachToStaff.SL_IsEstimate = false;
				logAttachToStaff.SL_Reference = "Attached - (" + group.GG_Code + ") testStaffName";
				logAttachToStaff.SL_GS_NKUser = group.GG_SystemLastEditUser;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				logAttachToStaff.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}
			Factory.Save();

			LoadReportAndAssertContents(excelInterface =>
			{
				AssertEquals("Modified By", excelInterface.WorkSheets[0][7, 9].ToString().Trim());
				AssertEquals("Staff Code", excelInterface.WorkSheets[0][7, 6].ToString().Trim());
				AssertEquals("A\\<", excelInterface.WorkSheets[0][8, 6].ToString().Trim());
			});
		}

		public void TestReport_WhenFilteredByGroupCategory()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("WER", "Things that were"),
				new CodeDescriptionPair("ARE", "Things that are"),
				new CodeDescriptionPair("PAS", "And some things that have not yet come to pass"),
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			PrepareReportForRender();
			FillReportWithDefaultValues();

			var staff1 = MasterFilesTestHelper.CreateStaff(Factory, "S1", "Joey");
			var staff2 = MasterFilesTestHelper.CreateStaff(Factory, "S2", "Joe");
			var staff3 = MasterFilesTestHelper.CreateStaff(Factory, "S3", "Joe");
			var staff4 = MasterFilesTestHelper.CreateStaff(Factory, "S4", "Junior");
			var staff5 = MasterFilesTestHelper.CreateStaff(Factory, "S5", "Shabadoo");

			MasterFilesTestHelper.CreateGroup(Factory, "GRP1", "Group The First", category: "WER", staff1);
			MasterFilesTestHelper.CreateGroup(Factory, "GRP2", "Group The Second", category: "ARE", staff2, staff3);
			MasterFilesTestHelper.CreateGroup(Factory, "GRP3", "Group The Third", category: "PAS", staff4, staff5);

			Factory.Save();

			var filter = (CodeListMultipleChoice)Report.FilterCollection["Group Category"];
			AssertSequencesEqual(validCategories.Cast<ICodeDescription>(), filter.List.Cast<ICodeDescription>());

			AssertCategoryResults(string.Empty,
				("GRP1", "Group The First", "WER", "S1", "Joey"),
				("GRP2", "Group The Second", "ARE", "S2", "Joe"),
				("GRP2", "Group The Second", "ARE", "S3", "Joe"),
				("GRP3", "Group The Third", "PAS", "S4", "Junior"),
				("GRP3", "Group The Third", "PAS", "S5", "Shabadoo"));

			AssertCategoryResults("WER",
				("GRP1", "Group The First", "WER", "S1", "Joey"));

			AssertCategoryResults("ARE",
				("GRP2", "Group The Second", "ARE", "S2", "Joe"),
				("GRP2", "Group The Second", "ARE", "S3", "Joe"));

			void AssertCategoryResults(string category, params (string GroupCode, string GroupDescription, string GroupCategory, string StaffCode, string StaffName)[] reportDetails)
			{
				filter.Value = category;

				LoadReportAndAssertContents(excelInterface =>
				{
					var sheet = excelInterface.WorkSheets[0];
					const int headingRow = 7;

					CombineAssertions("Heading cells", () =>
					{
						AssertEquals("Group Code", sheet[headingRow, 3]);
						AssertEquals("Group Name", sheet[headingRow, 4]);
						AssertEquals("Group Category", sheet[headingRow, 5]);
						AssertEquals("Staff Code", sheet[headingRow, 6]);
						AssertEquals("User Name", sheet[headingRow, 7]);
					});

					for (var i = 1; i <= reportDetails.Length; i++)
					{
						var row = headingRow + i;
						var expectedDetail = reportDetails[i - 1];
						CombineAssertions(FormattableString.Invariant($"Row {row} values"), () =>
						{
							AssertEquals("Group Code", expectedDetail.GroupCode, sheet[row, 3]);
							AssertEquals("Group Name", expectedDetail.GroupDescription, sheet[row, 4]);
							AssertEquals("Group Category", expectedDetail.GroupCategory, sheet[row, 5]);
							AssertEquals("Staff Code", expectedDetail.StaffCode, sheet[row, 6]);
							AssertEquals("User Name", expectedDetail.StaffName, sheet[row, 7]);
						});
					}
					AssertEquals("Beyond the expected details, we should find no report contents.", string.Empty, sheet[headingRow + reportDetails.Length + 1, 3]);
				});
				Report.ResetCachedExcelFileForTesting();
			}
		}

		void LoadReportAndAssertContents(Action<ExcelInterface> assertion)
		{
			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			using (Report.SuspendFilterValidationCheckingForTesting())
			{
				Report.Save(stream);

				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				assertion.Invoke(excelInterface);
			}
		}

		protected override void FillReportWithDefaultValues()
		{
			((DateRangeField)Report.FilterCollection["Event Date"]).ValueLow = ZDateTime.Today.AddYears(-1);
			((DateRangeField)Report.FilterCollection["Event Date"]).ValueHigh = ZDateTime.Today.AddYears(1);
		}
	}

	class GroupStaffAssignmentReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "The Group Staff Assignment Report lists users attached and/or detached to security groups in a period of time. The data is based on logs created under Groups & Users at the time a user is attached to or detached from a security group."; }
		}

		public override string MenuName
		{
			get { return "Group Staff Assignment Report"; }
		}

		public override ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new GroupStaffAssignmentTemplateTest();
		}
	}
}
