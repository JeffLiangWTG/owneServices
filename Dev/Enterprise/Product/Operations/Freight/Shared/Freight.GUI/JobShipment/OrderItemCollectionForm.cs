using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class OrderItemCollectionForm : ZChildForm
	{
		public OrderItemCollectionForm(OrderItemCollection orderItems)
			: base(orderItems)
		{
			this.OrderItems = orderItems;
		}

		public readonly OrderItemCollection OrderItems;

		public static void ShowDialog(OrderItemCollection orderItems)
		{
			ZFormModaliser.ShowDialogAndDispose(new OrderItemCollectionForm(orderItems));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Loading

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fOldItems = OrderItems.AsString;
		}

		string fOldItems;

		#endregion

		#region Closing

		protected override void OnClosing(CancelEventArgs e)
		{
			CloseButton.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				OrderItems.AsString = fOldItems;
			}
			else
			{
				this.BusinessEntity.RunPreSaveValidation();
				foreach (OrderItem item in OrderItems)
				{
					if (item.NotificationsIncludingChildren.GetErrors().Count() > 0)
					{
						Globals.Message.ShowError(Res.GetString("c01e5eb1-c551-443f-8b78-53ce86874f84", "The form has errors. Please fix them before continuing."));
						e.Cancel = true;
						break;
					}
				}
			}

			base.OnClosing(e);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
