using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(EntryDutiesUserControl))]
	sealed class EntryDutiesUserControlTest : TestCaseWithFactory
	{
		public void TestDutiesGrid()
		{
			AssertType<ZGrid>(control.EntryDutiesGrid);
		}

		public void TestDutiesGrid_ColumnDetails()
		{
			var columnDetails = EntryDutiesGridColumnDetails.Select(column => (column.Name, column.Caption, column.Width)).ToArray();
			AssertColumnDetails(columnDetails);
		}

		public void TestDutiesGridNOCaptions()
		{
			using (var tempLanguage = Res.TemporarilySwitchLanguage(Core.Constants.Languages.Norwegian))
			{
				var columnDetails = EntryDutiesGridColumnDetails.Select(column => (column.Name, column.NOCaption, column.Width)).ToArray();
				AssertColumnDetails(columnDetails);
			}
		}

		void AssertColumnDetails((string Name, string Caption, int Width)[] columnDetails)
		{
			var grid = control.EntryDutiesGrid;
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.SetDataBinding(Factory.New<JobDeclaration>(), ".");
				form.Show();

				CombineAssertions(() =>
				{
					AssertSequencesEqual("Columns in order", columnDetails.Select(x => x.Name), grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					foreach (var (name, caption, width) in columnDetails)
					{
						AssertEquals($"Caption for {name}", caption, grid.GetColumnCaption(name));
						AssertEquals($"Width for {name}", width, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(grid.GetColumnStyle(name).Width));
					}
				});
			}
		}

		public void TestCustomsDuty() => CombineAssertions(() =>
			control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(EntryDutiesUserControl.CustomsDutyCalcEdit), x => x
				.WithCaption("Customs duty")
				.WithBindToAmount($"{nameof(JobDeclaration.CustomsEntryHeaders)}.{nameof(CusEntryHeader.CustomsDutyAmount)}")
				.WithBindToUnit($"{nameof(JobDeclaration.LocalCurrencyCode)}")
			));

		public void TestExciseDuties() => CombineAssertions(() =>
			control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(EntryDutiesUserControl.ExciseDutiesCalcEdit), x => x
				.WithCaption("Excise duties")
				.WithBindToAmount($"{nameof(JobDeclaration.CustomsEntryHeaders)}.{nameof(CusEntryHeader.ExciseDutyAmount)}")
				.WithBindToUnit($"{nameof(JobDeclaration.LocalCurrencyCode)}")
			));

		public void TestVAT() => CombineAssertions(() =>
			control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(EntryDutiesUserControl.VATCalcEdit), x => x
				.WithCaption("VAT")
				.WithBindToAmount($"{nameof(JobDeclaration.CustomsEntryHeaders)}.{nameof(CusEntryHeader.VatAmount)}")
				.WithBindToUnit($"{nameof(JobDeclaration.LocalCurrencyCode)}")
			));

		public void TestTotal() => CombineAssertions(() =>
			control.AssertContainsControl<ConvertToLocalCurrencyControl>(nameof(EntryDutiesUserControl.TotalCalcEdit), x => x
				.WithCaption("Total")
				.WithBindToAmount($"{nameof(JobDeclaration.CustomsEntryHeaders)}.{nameof(CusEntryHeader.TotalAmount)}")
				.WithBindToUnit($"{nameof(JobDeclaration.LocalCurrencyCode)}")
			));

		EntryDutiesUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryDutiesUserControl();
		}

		protected override void TearDown()
		{
			control?.Dispose();
			base.TearDown();
		}

		(string Name, string Caption, string NOCaption, int Width)[] EntryDutiesGridColumnDetails => new[]
		{
			("Duty", "Duty", "Avgift", 80),
			("Amount", "Amount", "Beløp", 80)
		};
	}
}
