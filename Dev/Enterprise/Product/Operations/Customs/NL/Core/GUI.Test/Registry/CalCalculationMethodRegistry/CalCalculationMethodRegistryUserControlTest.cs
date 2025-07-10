using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.NL.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(CalCalculationMethodRegistryUserControl))]
sealed class CalCalculationMethodRegistryUserControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new CalCalculationMethodRegistryCollection();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;

	public void TestCalCalculationMethodGrid()
	{
		using (var userControl = new CalCalculationMethodRegistryUserControl())
		{
			var calCalculationMethodGrid = (ZGrid)userControl.Controls.Find("CalCalculationMethodGrid", true).Single();

			CombineAssertions(() =>
			{
				AssertNotNull("The grid should exist.", calCalculationMethodGrid);
				Assert("The grid should be visible.", calCalculationMethodGrid.Visible);

				AssertColumn(calCalculationMethodGrid, "CalculationMethodName", 75);
				AssertColumn(calCalculationMethodGrid, "CalculationMethodDescription", 150);
				AssertColumn(calCalculationMethodGrid, "CalculationMethodValue", 75, true);
				AssertColumn(calCalculationMethodGrid, "CalculationMethodDefault", 50);
			});
		}
	}

	void AssertColumn(ZGrid grid, string columnName, int width, bool showEmptyStringForEmptyValue = false)
	{
		var column = FindColumnByName(grid, columnName);
		AssertNotNull($"{columnName} not null", column);
		Assert($"{columnName} visible", column.IsVisible);
		AssertEquals($"{columnName} width", width, column.Width);
		if (column is ZCalcEditColumnStyleInfo calcEditColumn)
		{
			AssertEquals($"{columnName} ShowEmptyStringForEmptyValue", calcEditColumn.ShowEmptyStringForEmptyValue, showEmptyStringForEmptyValue);
		}
	}

	ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
}
