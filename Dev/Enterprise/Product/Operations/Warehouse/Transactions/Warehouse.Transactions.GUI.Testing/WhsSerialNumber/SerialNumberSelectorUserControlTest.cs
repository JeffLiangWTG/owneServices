using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class SerialNumberSelectorUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestGridSerialNumberColumns()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();

				var columns = form.UserControl.SerialNumberGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var colSelected = columns.Single(c => c.ColumnName == WhsSerialNumberPivotSelector.Schema.Selected);
				AssertEquals(false, colSelected.IsUnavailable);

				var colSerialNumberValue = columns.Single(c => c.ColumnName == WhsSerialNumberPivotSelector.Schema.SerialNumberValue);
				AssertEquals(false, colSerialNumberValue.IsUnavailable);
			}
		}

		#region TestForm

		class TestForm : ZForm
		{
			public TestForm(WhsPick pick)
				: base(pick)
			{
			}
			public SerialNumberSelectorUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = new SerialNumberSelectorUserControl();
				this.UserControl.BindTo = "OrderedInventories.AvailableInventories.SerialNumbers";
				this.UserControl.DataSourceAssemblyName = "";
				this.UserControl.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsSerialNumberPivotSelector";

				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsPick";
			}
		}

		#endregion
	}
}
