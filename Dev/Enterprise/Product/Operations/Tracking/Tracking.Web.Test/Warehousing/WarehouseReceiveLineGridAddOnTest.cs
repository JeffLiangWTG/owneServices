using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseReceiveLineGridAddOnTest : WarehouseDocketLineGridAddOnTest
	{
		public override void TestProperties()
		{
			AssertNotNull(Control);
			AssertEquals("ExpectedQuantity", Control.ExpectedQuantityBindTo);
			AssertEquals("ExpiryDate", Control.ExpiryDateBindTo);
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
			var addOn = (WarehouseReceiveLineGridAddOnForTesting)Control;
			page.Controls.Add(grid);
			page.Controls.Add(addOn);
			addOn.Grid = grid;

			var pageData = Factory.NewWithValidTestData<DummyBusinessObject>();
			pageData.Collection.AddNew();
			pageData.Collection.AddNew();
			grid.AllowPaging = true;
			grid.BindTo = "Collection";
			grid.Columns.Add(new ZTextEditColumn("Code", DummyChildBusinessObject.Schema.Z0_Code));
			grid.Columns.Add(new ZFindBoxColumn("Product", "Product"));
			grid.Columns.Add(new ZTextEditColumn("Description", "Description"));
			grid.Columns.Add(new ZTextEditColumn("Packs", "Packs"));
			grid.Columns.Add(new ZTextEditColumn("PacksUQ", "PacksUQ"));
			grid.Columns.Add(new ZTextEditColumn("Quantity", "Quantity"));
			grid.Columns.Add(new ZTextEditColumn("QuantityUQ", "QuantityUQ"));
			grid.Columns.Add(new ZTextEditColumn("ExpectedQuantity", "ExpectedQuantity"));
			grid.Columns.Add(new ZTextEditColumn("Attribute1", "Attribute1"));
			grid.Columns.Add(new ZTextEditColumn("Attribute2", "Attribute2"));
			grid.Columns.Add(new ZTextEditColumn("Attribute3", "Attribute3"));
			grid.Columns.Add(new ZTextEditColumn("SerialNumber", "SerialNumber"));
			grid.Columns.Add(new ZTextEditColumn("ExpiryDate", "ExpiryDate"));

			page.TestDataSource = pageData;
			grid.Bind(pageData);
			addOn.OnPreRenderForTesting();

			AssertEquals(2, grid.Items.Count);
			foreach (DataGridItem item in grid.Items)
			{
				var cellControls = item.Controls
							.OfType<TableCell>()
							.SelectMany(c => c.Controls
								.OfType<WebControl>()
								.OfType<ISelfBindingWebControl>());

				var productControl = GetControl(cellControls, "Product")?.Controls.OfType<ZTextBox>().FirstOrDefault();
				var descriptionControl = GetControl(cellControls, "Description");
				var packsControl = GetControl(cellControls, "Packs");
				var packsUQControl = GetControl(cellControls, "PacksUQ");
				var quantityControl = GetControl(cellControls, "Quantity");
				var productUQControl = GetControl(cellControls, "QuantityUQ");
				var expectedQuantityControl = GetControl(cellControls, "ExpectedQuantity");
				var attribute1Control = GetControl(cellControls, "Attribute1");
				var attribute2Control = GetControl(cellControls, "Attribute2");
				var attribute3Control = GetControl(cellControls, "Attribute3");
				var serialNumberControl = GetControl(cellControls, "SerialNumber");
				var expiryDateControl = GetControl(cellControls, "ExpiryDate");

				AssertNotNull(productControl);
				AssertNotNull(descriptionControl);
				AssertNotNull(packsControl);
				AssertNotNull(packsUQControl);
				AssertNotNull(quantityControl);
				AssertNotNull(productUQControl);
				AssertNotNull(expectedQuantityControl);
				AssertNotNull(attribute1Control);
				AssertNotNull(attribute2Control);
				AssertNotNull(attribute3Control);
				AssertNotNull(serialNumberControl);
				AssertNotNull(expiryDateControl);

				var expectedOnChangeScript = $"WarehouseReceiveLineUpdate(this, '{page.DataSourceIndexer}', '{item.Attributes["ref"]}', '{productControl.ClientID}', '{descriptionControl.ClientID}', '{packsControl.ClientID}', '{packsUQControl.ClientID}', '{quantityControl.ClientID}', '{productUQControl.ClientID}', '{expectedQuantityControl.ClientID}', '{attribute1Control.ClientID}', '{attribute2Control.ClientID}', '{attribute3Control.ClientID}', '{serialNumberControl.ClientID}', '{expiryDateControl.ClientID}');";
				AssertEquals(expectedOnChangeScript, productControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, packsControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, packsUQControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, quantityControl.Attributes["onchange"]);
				AssertEquals(expectedOnChangeScript, expectedQuantityControl.Attributes["onchange"]);
			}
		}

		protected override Type GetExpectedDocketLineUpdateMethodType() => typeof(WarehouseReceiveLineUpdateWSMethod);

		new WarehouseReceiveLineGridAddOn Control => (WarehouseReceiveLineGridAddOn)base.Control;

		protected override Control GetNewControl()
		{
			return new WarehouseReceiveLineGridAddOnForTesting()
			{
				ProductBindTo = "Product",
				DescriptionBindTo = "Description",
				PacksBindTo = "Packs",
				PacksUQBindTo = "PacksUQ",
				QuantityBindTo = "Quantity",
				ProductUQBindTo = "QuantityUQ",
				ExpectedQuantityBindTo = "ExpectedQuantity",
				Attribute1BindTo = "Attribute1",
				Attribute2BindTo = "Attribute2",
				Attribute3BindTo = "Attribute3",
				SerialNumberBindTo = "SerialNumber",
				ExpiryDateBindTo = "ExpiryDate"
			};
		}
	}
}
