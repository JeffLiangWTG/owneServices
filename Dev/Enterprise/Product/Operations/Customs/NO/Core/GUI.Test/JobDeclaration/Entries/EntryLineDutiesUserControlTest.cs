using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(EntryDutiesUserControl))]
	sealed class EntryLineDutiesUserControlTest : TestCaseWithFactory
	{
		public void TestDutiesTab()
		{
			var tabControl = userControl.EntryLineInfoTabControl;
			AssertNotNull(tabControl.FindSingleOrDefault<ZTabPage>("EntryLineDutiesTabPage", maxLevelsDeep: 1));
		}

		public void TestExtendedInfoTab()
		{
			using var form = new ZForm();
			form.Controls.Add(userControl);
			form.Show();
			var tabControl = userControl.EntryLineInfoTabControl;
			var tabPage = tabControl.FindSingleOrDefault<ZTabPage>("EntryLineExtendedInfoTabPage", maxLevelsDeep: 1);
			tabControl.SelectedTab = tabPage;
			CombineAssertions(() =>
			{
				_ = tabPage.AssertContainsControl<ZTextBox>("TariffCodeTextBox", x => x
					.WithCaption("Tariff Code:")
					.WithBindTo(
						$"{nameof(JobDeclaration.CustomsEntryHeaders)}." +
						$"{nameof(CusEntryHeader.AllEntryLines)}." +
						$"{nameof(CusEntryLine.CL_AdValoremTariff)}"
					)
				);
				_ = tabPage.AssertContainsControl<ZTextBox>("DescriptionTextBox", x => x
					.WithCaption("Description:")
					.WithBindTo(
						$"{nameof(JobDeclaration.CustomsEntryHeaders)}." +
						$"{nameof(CusEntryHeader.AllEntryLines)}." +
						$"{nameof(CusEntryLine.EffectiveDescription)}"
					)
				);
			});
		}

		public void TestEntryLineDutiesGrid_ColumnDetails()
		{
			var columnDetails = GridColumnDetails.Select(column => (column.Name, column.Caption, column.Width)).ToArray();
			AssertColumnDetails(columnDetails);
		}

		void AssertColumnDetails((string Name, string Caption, int Width)[] columnDetails)
		{
			var grid = userControl.EntryLineDutiesGrid;
			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(Factory.New<JobDeclaration>(), ".");
				form.Show();

				CombineAssertions(() =>
				{
					AssertSequencesEqual("Columns in order", columnDetails.Select(x => x.Name), grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					foreach (var (columnName, columnCaption, columnWidth) in columnDetails)
					{
						AssertEquals($"Caption for {columnName}", columnCaption, grid.GetColumnCaption(columnName));
						AssertEquals($"Width for {columnName}", columnWidth, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(grid.GetColumnStyle(columnName).Width));
					}
				});
			}
		}

		EntryLineDutiesUserControl userControl;

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new EntryLineDutiesUserControl();
		}

		protected override void TearDown()
		{
			userControl?.Dispose();
			base.TearDown();
		}

		(string Name, string Caption, string NOCaption, int Width)[] GridColumnDetails => new[]
		{
			("CF_DutyCode", "Duty Code", "Avg.type", 80),
			("CF_Sequence", "Sequence", "Sekvens(Gruppe)", 80),
			("CF_Rate", "Rate", "Sats", 80),
			("CF_RateType", "Rate type", "Enhet", 80),
			("CF_BaseValue", "Base value", "Grunnlag", 80),
			("CF_ChargeAmount", "Duty amt in NOK", "Kalkulert beløp", 80),
			("IsLandedCostOnlyAsText", "Payable to Customs/Tax authorities", "Payable to Customs/Tax authorities", 275),
		};
	}
}
