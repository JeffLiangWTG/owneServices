using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class RMAForOrderLinesForm : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		public RMAForOrderLinesForm(WhsRMAOrder rmaOrder)
			: base(rmaOrder)
		{
			Argument.NotNull(rmaOrder, "rmaOrder");
			InitializeComponent();
		}

		WhsRMAOrder RMAOrder => CurrentDataItem as WhsRMAOrder;

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PartAttributeColumnManager.SetColumns(RMAOrder.Order.Client);
		}

		#endregion

		#region PartAttributeColumnManager

		PartAttributeColumnManager PartAttributeColumnManager
		{
			get
			{
				if (partAttributeColumnManager == null)
				{
					partAttributeColumnManager = new PartAttributeColumnManager(this.Grid,
						 "ExpiryDate",
						 "PackingDate",
						 "PartAttrib1",
						 "PartAttrib2",
						 "PartAttrib3",
						 "SerialNumber");
				}
				return partAttributeColumnManager;
			}
		}
		PartAttributeColumnManager partAttributeColumnManager;

		#endregion

		#region CommonOKButton_Click

		void CommonOKButton_Click(object sender, EventArgs e)
		{
			if (RMAOrder.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (!RMAOrder.Lines.Cast<WhsRMAOrderLine>().Any(l => l.QuantityToReturn > 0))
			{
				Globals.Message.ShowWarning(Res.GetString("1d096eef-1e80-4c03-81b0-5958e0a17ec1", "Please at least enter an amount more than 0 on one Line."));
			}
			else
			{
				var msg = RMAOrder.GenerateRMAReceivesMessage();
				Globals.Message.ShowInformation(msg);
				DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		#endregion

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region INotificationSubscriberQueryUser

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			var args = (QueryUserMsgBoxEventArgs)e;
			args.Response = Globals.Message.Show(args.Message, args.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class RMAForOrderLinesForm
	{
		public void CommonOKClickForTest()
		{
			CommonOKButton_Click(null, EventArgs.Empty);
		}

		public ZGrid GridForTest => Grid;
	}
}

#endif
#endregion
