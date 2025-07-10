using System;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class MultipleProductsTemplateTest : ZItemTemplateTest
	{
		[HttpContextEnabledTest]
		public void TestGetControl()
		{
			TableCell cell = new TableCell();
			TestItemTemplate.InstantiateIn(cell);
			Assert("Added Control", cell.Controls.Count > 0);
			ISelfBindingWebControl control = cell.Controls[0] as ISelfBindingWebControl;
			AssertNotNull("Control", control);

			ZDataGrid grid = control as ZDataGrid;
			AssertNotNull(grid);
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
			get { return typeof(MultipleProductsTemplate); }
		}

		#endregion
	}
}
