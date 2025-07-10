using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TaxIdAndTaxMessageCombinationRulesRegistryControl))]
	public class TaxIdAndTaxMessageCombinationRulesRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestShowTaxGroupCodeRelatedColumn()
		{
			using (var control = new TaxIdAndTaxMessageCombinationRulesRegistryControl())
			{
				var grid = control.Controls.Find("TaxIdAndTaxMessageCombinationRulesGrid", true).Single() as ZGrid;

				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "TaxGroupCode"));
				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "TaxGroupDescription"));
				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "GovernmentCode"));
				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "TaxRateType"));
				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "TaxRateValue"));
				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "AuxiliaryType"));
				AssertNotNull(grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == "ExtraTaxRate"));
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new TaxIdAndTaxMessageCombinationRulesConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var taxIdAndTaxMessageCombinationRulesGrid = control.Controls.Find("TaxIdAndTaxMessageCombinationRulesGrid", true).Single() as ZGrid;
			var validationOptionDropEdit = control.Controls.Find("ValidationOptionDropEdit", true).Single() as ZDropEdit;

			return taxIdAndTaxMessageCombinationRulesGrid.ReadOnly && validationOptionDropEdit.ReadOnly;
		}
	}
}
