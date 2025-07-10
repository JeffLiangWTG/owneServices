using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public class RefHarbourRateFilterStripControlTest : ZFilterStripControlTest
	{
		RefHarbourRateFilterStripControl GetNewFilterControl()
		{
			var collection = new RefHarbourRateCollection(Factory);
			var filterBizO = new RefHarbourRateFilterStripBusinessObject();
			return new RefHarbourRateFilterStripControl(collection, filterBizO);
		}

		[RequiresSTA]
		public void TestColumns()
		{
			using var form = new ZForm();
			using var filterControl = GetNewFilterControl();
			form.Controls.Add(filterControl);
			form.Show();

			var typeColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_Type));
			var portColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_Port));
			var modeColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_Mode));
			var commodityColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_Commodity));
			var portTaxTypeColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_PortTaxType));
			var startDateColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_StartDate));
			var endDateColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_EndDate));
			var countryOrGroupingColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_ZZZ_NKDataGrouping));
			var rateFormulaColumn = filterControl.Grid.GetColumnStyle(nameof(RefHarbourRate.ZXF_RateFormula));

			CombineAssertions(() =>
			{
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Type", typeColumn, 80);
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Port", portColumn, 80);
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Mode", modeColumn, 80);
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Commodity", commodityColumn, 80);
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Port Tax Type", portTaxTypeColumn, 80);
				AssertColumnInfo<ZDateEditColumnStyleInfo>("Start Date", startDateColumn, 80);
				AssertColumnInfo<ZDateEditColumnStyleInfo>("End Date", endDateColumn, 80);
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Country Or Grouping", countryOrGroupingColumn, 120);
				AssertColumnInfo<ZTextBoxColumnStyleInfo>("Rate Formula", rateFormulaColumn, 160);
			});

			void AssertColumnInfo<T>(string columnName, ZGridColumnInfo column, int width)
			{
				AssertType<T>($"Column type for {columnName} is correct.", column);
				AssertEquals($"{columnName} is visible.", true, column.IsVisible);
				AssertEquals($"Width of {columnName}.", width, column.Width);
			}
		}
	}
}
