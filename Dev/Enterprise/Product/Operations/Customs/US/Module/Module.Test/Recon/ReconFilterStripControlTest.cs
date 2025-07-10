using System.Linq;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Module.Testing
{
	class ReconFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFilterGridColorContextKey()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new ReconModule())
			using (var filterControl = new ReconFilterStripControl(declarations, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals("", filterControl.FilteredGrid.ColorContextKey);
			}
		}

		public void TestAdditionalFieldsContainLiquidationDate()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using var module = new ReconModule();
			using var filterControl = new ReconFilterStripControl(declarations, module.FilterBusinessObject);
			filterControl.OnLoad_Exposed();
			var liquidationDateEdit = filterControl.FilteredGrid.ColumnStyles.OfType<ZDateEditColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == ReconDeclaration.Schema.US_LiquidationDate);
			AssertEquals("Liquidation Date", liquidationDateEdit.Caption);
			AssertEquals(ZArchitecture.Core.ZDateTimePickerFormat.Short, liquidationDateEdit.DateTimeFormat);
		}
	}
}
