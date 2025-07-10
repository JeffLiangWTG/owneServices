using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class SerialNumberUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestSerialNumberColumnInitialised()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new TestForm(receive))
			{
				form.Show();

				var column = form.UserControl.SerialNumberGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single();
				AssertEquals(false, column.IsUnavailable);
				AssertEquals("SerialNumberValue", column.ColumnName);
			}
		}
	}

	#region TestForm

	class TestForm : ZForm
	{
		public TestForm(WhsDocket docket)
			: base(docket)
		{
		}
		public SerialNumberUserControl UserControl;

		protected override void InitializeComponent()
		{
			this.UserControl = new SerialNumberUserControl();
			this.UserControl.BindTo = "Lines.SerialNumbers";
			this.UserControl.DataSourceAssemblyName = "";
			this.UserControl.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsSerialNumberPivotCollection";

			this.Controls.Add(this.UserControl);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocket";
		}
	}

	#endregion
}
