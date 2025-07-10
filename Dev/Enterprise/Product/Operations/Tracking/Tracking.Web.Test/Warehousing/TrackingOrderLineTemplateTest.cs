using System;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingOrderLineTemplateTest : ZItemTemplateTest
	{
		[HttpContextEnabledTest]
		public void TestGetControl()
		{
			var cell = new TableCell();
			TestItemTemplate.InstantiateIn(cell);
			Assert("Added Control", cell.Controls.Count > 0);

			var control = cell.Controls[0] as ISelfBindingWebControl;
			AssertNotNull("Control", control);

			var grid = control as ZDataGrid;
			AssertNotNull(grid);
		}

		#region Implementation

		protected override Type ExpectedColumnType => typeof(ZNewRowColumn);

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo) => new ZNewRowColumn(bindTo);

		protected override Type ExpectedItemTemplateType => typeof(TrackingOrderLineTemplate);

		#endregion
	}
}
