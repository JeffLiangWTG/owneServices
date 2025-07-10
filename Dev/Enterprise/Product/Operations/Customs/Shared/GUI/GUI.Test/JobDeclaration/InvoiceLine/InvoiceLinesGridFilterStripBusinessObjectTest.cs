using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(InvoiceLinesGridFilterStripBusinessObject))]
	sealed class InvoiceLinesGridFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestGetJI_FormattedTariffModuleFilter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			using (var control = new ZUserControl())
			{
				using (var grid = new ZGrid())
				{
					control.Controls.Add(grid);
					grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo { ColumnName = "JI_FormattedTariff" });
					grid.SetDataBinding(invoice.InvoiceLines, "");
					var filters = new InvoiceLinesGridFilterStripBusinessObject(grid).ModuleFilters;
					AssertEquals(1, filters.Count());
					AssertNotNull(filters["JI_FormattedTariff"]);
					AssertType<ModuleNumberFilter>(filters["JI_FormattedTariff"]);
				}
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new InvoiceLinesGridFilterStripBusinessObject(TestGrid);

		ZGrid TestGrid
		{
			get
			{
				if (testGrid == null)
				{
					testGrid = new ZGrid();
					testGrid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo { ColumnName = "JI_FormattedTariff" });
				}

				return testGrid;
			}
		}
		ZGrid testGrid;

		protected override void TearDown()
		{
			if (testGrid != null)
			{
				testGrid.Dispose();
				testGrid = null;
			}

			base.TearDown();
		}
	}
}
