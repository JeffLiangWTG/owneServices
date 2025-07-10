using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ReportTesting.SystemTest
{
	[TemplateName("Fax Log")]
	public class FaxLogReportTemplateTest : TemplateTestCase
	{
		#region override

		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}

		protected override void FillReportWithDefaultValues()
		{
			var filterFields = Report.FilterCollection
				.ToArray()
				.OfType<FilterFieldWithUTSupport>()
				.Where(field => ((IFilterFieldForUT)field).IsRequired);

			foreach (var field in filterFields)
			{
				if (field.DisplayName == "Start Date UTC")
				{
					((DateField)field).Value = ZDateTime.Today.AddDays(-1);
				}
				else if (field.DisplayName == "End Date UTC")
				{
					((DateField)field).Value = ZDateTime.Today.AddDays(1);
				}
				field.Factory.Save();
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestFaxReportWithData()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var log = Factory.New<StmALog>();
			var parentBizO = staff;

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = parentBizO.PK;
				log.SL_Table = parentBizO.TableName;
				log.SL_Reference = "+64 (9) 256-0071 - EDN - AKL - TAX INVOICE EDN00011139 RHCHUSAKL (01-Sep-10)";
				log.SL_EventTime = ZDateTime.Now;
				log.SL_SE_NKEvent = Events.DocumentSentCode;
				log.SL_GS_NKUser = staff.GS_Code;
			}

			Factory.Save();
			base.TestReportRunsWithNoException();
		}
	}

	public class FaxLogReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new SystemReports(); }
		}

		public override string Hint
		{
			get
			{
				return "The Fax Log Report shows a listing of faxes sent from the CargoWise system.";
			}
		}

		public override string MenuName
		{
			get
			{
				return "Fax Log";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new FaxLogReportTemplateTest();
		}
	}
}
