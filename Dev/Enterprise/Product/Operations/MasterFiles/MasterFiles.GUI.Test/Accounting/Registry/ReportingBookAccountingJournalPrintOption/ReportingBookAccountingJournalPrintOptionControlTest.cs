using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ReportingBookAccountingJournalPrintOptionControl))]
	sealed class ReportingBookAccountingJournalPrintOptionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ReportingBookAccountingJournalPrintOptionCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ReportingBookAccountingJournalPrintOptionControl)control).ReportingBookAccountingJournalPrintOptionGrid.ReadOnly;
		}

		public void TestReportingBookAccountingJournalPrintOptionGrid()
		{
			using (var form = GetFormToBash())
			{
				var grid = form.GetControl<ZGrid>("ReportingBookAccountingJournalPrintOptionGrid");

				var expectedListOfColumns = new[]
				{
					$"{ReportingBookAccountingJournalPrintOption.Schema.ReportingBook} (ZGuidFindBoxColumnStyleInfo) IsVisible:True",
					$"ReportingBookDescription (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{ReportingBookAccountingJournalPrintOption.Schema.DisplayParentAccount} (ZCheckBoxColumnStyleInfo) IsVisible:True",
					$"{ReportingBookAccountingJournalPrintOption.Schema.DisplayAttribute} (ZCheckBoxColumnStyleInfo) IsVisible:True",
					$"{ReportingBookAccountingJournalPrintOption.Schema.Default} (ZCheckBoxColumnStyleInfo) IsVisible:True",
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();

				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}
	}
}
