using System.Drawing;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class ReceiveProductSummaryGridUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestGrid_ColourDeciding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m); // Sets ClientOrderedUnits and TransactionQuantity to 10m.

			var summaryLine = new WhsReceiveProductSummaryCollection(Factory, receive).Cast<WhsReceiveProductSummary>().First();
			using (var form = new ReceiveProductSummaryGridUserControlForm())
			{
				form.Show();
				var userControl = form.UserControl;
				AssertGridLineColorSynchronised(userControl, summaryLine, Color.Empty);
				summaryLine.ReceivedQuantity = 5m;
				AssertGridLineColorSynchronised(userControl, summaryLine, Color.LightSalmon);
				summaryLine.ReceivedQuantity = 20m;
				AssertGridLineColorSynchronised(userControl, summaryLine, Color.LightGreen);
				summaryLine.ReceivedQuantity = 10m;
				AssertGridLineColorSynchronised(userControl, summaryLine, Color.Empty);
			}
		}

		void AssertGridLineColorSynchronised(ReceiveProductSummaryGridUserControl userControl, WhsReceiveProductSummary summaryLine, Color color)
		{
			var args = new ColourDecidingEventArgs(summaryLine);
			userControl.OnColourDecidingForTest(args);
			AssertEquals(color, args.Colour);
		}

		#region Implementation

		class ReceiveProductSummaryGridUserControlForm : ZForm
		{
			public ReceiveProductSummaryGridUserControlForm()
			{
			}

			public ReceiveProductSummaryGridUserControl UserControl;

			protected override void InitializeComponent()
			{
				UserControl = new ReceiveProductSummaryGridUserControl();
				Controls.Add(UserControl);
			}
		}

		#endregion
	}
}
