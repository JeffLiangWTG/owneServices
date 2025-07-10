using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module.Testing
{
	sealed class OrgSupplierPartFilterStripControlTest : TestCaseWithFactory
	{
		public void TestShowTariffsOnGrid()
		{
			var products = new OrgSupplierPartCollection(Factory);
			var aProduct = products.AddNew();
			using (var form = new ZForm())
			using (var module = new OrgSupplierPartModule())
			using (var stripControl = new OrgSupplierPartFilterStripControlForTest(products, new OrgSupplierPartFilterStripBusinessObject()))
			{
				form.Controls.Add(stripControl);
				form.Show();
				Assert(!stripControl.FilteredGrid.Columns.Any(l => l.ColumnName == "Tariffs"));
			}
		}

		sealed class OrgSupplierPartFilterStripControlForTest : OrgSupplierPartFilterStripControl
		{
			public OrgSupplierPartFilterStripControlForTest(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
				: base(gridCollection, filterBusinessObject)
			{
			}

			protected override bool ShowTariffsOnGrid => false;
		}
	}
}
