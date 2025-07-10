using System;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderContainersUserControl : ZUserControl, IBindTo
	{
		public OrderContainersUserControl()
		{
			InitializeComponent();
		}

		public string BindTo
		{
			get { return ContainersGridControl.BindTo; }
			set { ContainersGridControl.BindTo = value; }
		}

		#region GenerateDataFromContainers Button Event

		void GenerateLinesFromContainersButton_Click(object sender, EventArgs e)
		{
			GenerateDataFromContainers();
		}

		void GenerateDataFromContainers()
		{
			WhsOrder order = SelectedOrder;
			if (order != null)
			{
				EnsureParentFormImplementsINotifications();

				using (new DocketNotificationSubscriberWithWaitCursor(order, (INotifications)ParentForm))
				{
					order.GenerateDataFromContainers();
				}
			}
		}

		void EnsureParentFormImplementsINotifications()
		{
			if (ParentForm as INotifications == null)
			{
				throw new ArgumentException(GetType().Name + ".ParentForm does not implement " + nameof(INotifications) + ".");
			}
		}

		WhsOrder SelectedOrder
		{
			get
			{
				WhsOrder result = CurrentDataItem as WhsOrder;
				if (result == null)
				{
					WhsPick pick = CurrentDataItem as WhsPick;
					if (pick != null)
					{
						result = pick.CurrentOrder;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
