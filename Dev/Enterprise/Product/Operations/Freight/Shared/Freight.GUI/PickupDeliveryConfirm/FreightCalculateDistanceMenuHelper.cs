using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public static class FreightCalculateDistanceMenuHelper
	{
		public static void SetupCalculateDistanceContextMenu(ZGrid grid)
		{
			MenuItem calculateDistanceMenuItem = new ZMenuItem(ResString.GetMultilingualString("9800990C-CB18-434b-A6C3-76C1447A9B2B", "Calculate distance"));
			calculateDistanceMenuItem.Click += delegate
			{
				IDistanceCalculationConsumer consumer = null;

				if (grid.ListManager != null)
				{
					consumer = grid.ListManager.GetCurrent() as IDistanceCalculationConsumer;
				}

				if (consumer != null)
				{
					NotificationBuffer notifications = new NotificationBuffer();
					new FreightDistanceCalculator(consumer, notifications).SetCalculatedDistance();

					if (!string.IsNullOrEmpty(notifications.AsString))
					{
						Globals.Message.Show(notifications.AsString);
					}
				}
			};

			grid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			grid.ContextMenu.MenuItems.Add(calculateDistanceMenuItem);
		}
	}
}
