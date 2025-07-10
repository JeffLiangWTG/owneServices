using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class WarehouseDocketLineGridAddOnTest : ZDataGridAddOnTest
	{
		public virtual void TestProperties()
		{
			AssertNotNull(Control);
			AssertEquals("Product", Control.ProductBindTo);
			AssertEquals("Description", Control.DescriptionBindTo);
			AssertEquals("Packs", Control.PacksBindTo);
			AssertEquals("PacksUQ", Control.PacksUQBindTo);
			AssertEquals("Quantity", Control.QuantityBindTo);
			AssertEquals("QuantityUQ", Control.ProductUQBindTo);
			AssertEquals("Attribute1", Control.Attribute1BindTo);
			AssertEquals("Attribute2", Control.Attribute2BindTo);
			AssertEquals("Attribute3", Control.Attribute3BindTo);
			AssertEquals("SerialNumber", Control.SerialNumberBindTo);
		}

		public void TestOnGridChange()
		{
			var grid = new ZGrid();
			Assert(!grid.IncludeItemDataRefKey);

			Control.Grid = grid;
			AssertEquals(grid, Control.Grid);
			Assert(grid.IncludeItemDataRefKey);
		}

		protected static WebControl GetControl(IEnumerable<ISelfBindingWebControl> controls, string bindTo) => controls.Where(c => c.BindTo == bindTo).Cast<WebControl>().FirstOrDefault();

		public void TestWebServiceMethods()
		{
			AssertNotNull(Control.WebServiceMethods);
			AssertEquals(1, Control.WebServiceMethods.Count);
			AssertEquals(GetExpectedDocketLineUpdateMethodType(), Control.WebServiceMethods[0].GetType());
		}

		protected abstract Type GetExpectedDocketLineUpdateMethodType();

		protected new WarehouseDocketLineGridAddOn Control => (WarehouseDocketLineGridAddOn)base.Control;
	}
}
