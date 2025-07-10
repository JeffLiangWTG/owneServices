using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class OrdersModuleButtonGridTest : TestCaseWithFactory
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestButtonClicksThrowNoExceptionOnEmptyDataSource()
		{
			using (var ordersModuleButtonGrid = new OrdersModuleButtonGridForTest())
			{
				ordersModuleButtonGrid.NewButton.PerformClick();
				ordersModuleButtonGrid.EditButton.PerformClick();
				ordersModuleButtonGrid.AttachButton.PerformClick();
				ordersModuleButtonGrid.DetachButton.PerformClick();
			}
		}

		class OrdersModuleButtonGridForTest : OrdersModuleButtonGrid
		{
			public ZToolStripButton NewButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();
				}
			}

			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}

			public ZToolStripButton AttachButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				}
			}

			public ZToolStripButton DetachButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				}
			}
		}
	}
}
