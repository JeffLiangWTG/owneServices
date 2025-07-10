using System;
using System.Web.UI.WebControls;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingInventoryTemplateTest : ZItemTemplateTest
	{
		[HttpContextEnabledTest]
		public void TestGetControl()
		{
			var grid = GetGridForTesting();
			AssertNotNull(grid);
		}

		[HttpContextEnabledTest]
		public void TestGridInventoryDetailsPageSettings()
		{
			using (WebDataRegistry.Instance.WarehouseInventoryDetailsPageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 95m))
			{
				var grid = GetGridForTesting();
				AssertEquals(true, grid.AllowPaging);
				AssertEquals(95, grid.PageSize);
			}

			using (WebDataRegistry.Instance.WarehouseInventoryDetailsPageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m))
			{
				var grid = GetGridForTesting();
				AssertEquals(false, grid.AllowPaging);
			}
		}

		ZDataGrid GetGridForTesting()
		{
			var cell = new TableCell();
			TestItemTemplate.InstantiateIn(cell);
			Assert("Added Control", cell.Controls.Count > 0);
			var control = (ISelfBindingWebControl)cell.Controls[0];
			AssertNotNull("Control", control);

			return (ZDataGrid)control;
		}

		#region Implementation

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZNewRowColumn); }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			return new ZNewRowColumn(bindTo);
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(TrackingInventoryTemplate); }
		}

		#endregion
	}
}
