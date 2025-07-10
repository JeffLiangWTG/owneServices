using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class BookingPackLineGridAddOnTest : ZDataGridAddOnTest
	{
		public void TestProperties()
		{
			AssertNotNull(Control);
			AssertEquals("Quantity", Control.QuantityBindTo);
			AssertEquals("Length", Control.LengthBindTo);
			AssertEquals("Width", Control.WidthBindTo);
			AssertEquals("Height", Control.HeightBindTo);
			AssertEquals("PackUD", Control.PackUDBindTo);
			AssertEquals("Volume", Control.VolumeBindTo);
			AssertEquals("VolumeUQ", Control.VolumeUQBindTo);
		}

		public void TestOnGridChange()
		{
			var grid = new ZGrid();
			Assert(!grid.IncludeItemDataRefKey);

			Control.Grid = grid;
			AssertEquals(grid, Control.Grid);
			Assert(grid.IncludeItemDataRefKey);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		public void TestScripts()
		{
			var grid = new ZDataGrid()
			{
				ReadOnly = false,
				AllowEdit = true
			};

			var page = new ZTestPage();
			page.Controls.Add(grid);
			page.Controls.Add(Control);
			Control.Grid = grid;

			var pageData = Factory.NewWithValidTestData<DummyBusinessObject>();
			pageData.Collection.AddNew();
			pageData.Collection.AddNew();
			grid.AllowPaging = true;
			grid.BindTo = "Collection";
			grid.Columns.Add(new ZTextEditColumn("Code", DummyChildBusinessObject.Schema.Z0_Code));
			grid.Columns.Add(new ZTextEditColumn("Quantity", "Quantity"));
			grid.Columns.Add(new ZTextEditColumn("Length", "Length"));
			grid.Columns.Add(new ZTextEditColumn("Width", "Width"));
			grid.Columns.Add(new ZTextEditColumn("Height", "Height"));
			grid.Columns.Add(new ZTextEditColumn("PackUD", "PackUD"));
			grid.Columns.Add(new ZTextEditColumn("Volume", "Volume"));
			grid.Columns.Add(new ZTextEditColumn("VolumeUQ", "VolumeUQ"));

			page.TestDataSource = pageData;
			grid.Bind(pageData);
			Control.OnPreRenderForTesting();

			AssertEquals(2, grid.Items.Count);
			foreach (DataGridItem item in grid.Items)
			{
				var cellControls = item.Controls
							.OfType<TableCell>()
							.SelectMany(c => c.Controls
								.OfType<WebControl>()
								.OfType<ISelfBindingWebControl>());

				var quantityControl = GetControl(cellControls, "Quantity");
				var lengthControl = GetControl(cellControls, "Length");
				var widthControl = GetControl(cellControls, "Width");
				var heightControl = GetControl(cellControls, "Height");
				var packUDControl = GetControl(cellControls, "PackUD");
				var volumeControl = GetControl(cellControls, "Volume");
				var volumeUQControl = GetControl(cellControls, "VolumeUQ");

				AssertNotNull(quantityControl);
				AssertNotNull(lengthControl);
				AssertNotNull(widthControl);
				AssertNotNull(heightControl);
				AssertNotNull(packUDControl);
				AssertNotNull(volumeControl);
				AssertNotNull(volumeUQControl);

				var expectedOnChangeScript = $"PackLineUpdate(this, '{page.DataSourceIndexer}', '{item.Attributes["ref"]}', '{quantityControl.ClientID}', '{lengthControl.ClientID}', '{widthControl.ClientID}', '{heightControl.ClientID}', '{packUDControl.ClientID}', '{volumeControl.ClientID}', '{volumeUQControl.ClientID}');";
				AssertEquals(expectedOnChangeScript, quantityControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, lengthControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, widthControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, heightControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, packUDControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, volumeUQControl.Attributes["onchange"]);
			}
		}
		static WebControl GetControl(IEnumerable<ISelfBindingWebControl> controls, string bindTo) => controls.Where(c => c.BindTo == bindTo).Cast<WebControl>().FirstOrDefault();

		public void TestWebServiceMethods()
		{
			AssertNotNull(Control.WebServiceMethods);
			AssertEquals(1, Control.WebServiceMethods.Count);
			AssertEquals(typeof(PackLineUpdateWSMethod), Control.WebServiceMethods[0].GetType());
		}

		protected override Control GetNewControl()
		{
			return new BookingPackLineGridAddOnForTesting()
			{
				QuantityBindTo = "Quantity",
				LengthBindTo = "Length",
				WidthBindTo = "Width",
				HeightBindTo = "Height",
				PackUDBindTo = "PackUD",
				VolumeBindTo = "Volume",
				VolumeUQBindTo = "VolumeUQ"
			};
		}

		new BookingPackLineGridAddOnForTesting Control => (BookingPackLineGridAddOnForTesting)base.Control;
	}
}
