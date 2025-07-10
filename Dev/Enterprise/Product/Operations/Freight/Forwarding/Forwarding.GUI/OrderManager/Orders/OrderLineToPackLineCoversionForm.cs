using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderLineToPackLineConversionForm : ZChildForm
	{
		public OrderLineToPackLineConversionForm(OrderLineToPackLineConversionHelper helper)
			: base(helper)
		{
			Helper = helper;
			InitializeComponent();

			DummyPackLinesGrid.ReadOnly = true;
			DummyPackLinesGrid.AllowOverlap(ButtonsPanel);
			OrderLinesGrid.AllowOverlap(FinishButton);
			UndoButton.AllowOverlap(MergeButton);
		}

		readonly OrderLineToPackLineConversionHelper Helper;

		#region Button Clicks

		void MergeButton_Click(object sender, EventArgs e)
		{
			if (OrderLinesGrid.SelectedRowCount > 1)
			{
				Helper.CreateDummyPackLine(OrderLinesGrid.GetSelectedElements<OrderLine>());
			}
			else
			{
				string message = Res.GetString("2FC0E118-0C93-4498-94A5-8EACEB5AC7EF", "Please select order lines to merge or press 'Finish' to a create pack line for every non-merged order line.");
				string caption = Res.GetString("B49B251F-A229-4723-9B5E-445BD0810208", "Select order lines to merge");
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		void UndoButton_Click(object sender, EventArgs e)
		{
			if (DummyPackLinesGrid.SelectedRowCount > 0)
			{
				Helper.UndoDummyPackLines(DummyPackLinesGrid.GetSelectedElements<DummyPackLine>());
			}
			else
			{
				string message = Res.GetString("C8F85368-A674-4cb8-8B95-5FADC199630F", "Please select combined pack line(s) to undo.");
				string caption = Res.GetString("CC3EE4FA-F03F-4651-B8CD-EC24EEE80115", "Select pack line(s) to undo");
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		void FinishButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			ShowSplitOrdersOptionAndSave();

			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Helper.UnattachOrdersFromShipment();
			Helper.IsHelperBeingCancelled = true;

			Close();
		}

		#endregion

		#region Spliting Orders and Saving

		void ShowSplitOrdersOptionAndSave()
		{
			var newOrders = new List<Order>();
			var action = string.Empty;

			if (Helper.ShouldShowSplitOrders)
			{
				switch (ShowConfirmationForSplittingOrder())
				{
					case OrderSplitDialogResult.SplitOrder:
						action = Res.GetString("3199ab53-4ee2-49a2-9598-de59ddc56dfc", "split");
						newOrders = Helper.GetNewOrSplitOrdersFromOriginalOrdersList(CreateOrderType.Split);
						break;

					case OrderSplitDialogResult.CreateNewOrder:
						action = Res.GetString("75f685f1-2a8b-4d34-b7d5-2215255c5e5b", "created");
						newOrders = Helper.GetNewOrSplitOrdersFromOriginalOrdersList(CreateOrderType.New);
						break;

					case OrderSplitDialogResult.DoNothing:
						break;
				}
			}

			TrySaveAndNotifyUser(newOrders, action);
		}

		void TrySaveAndNotifyUser(IEnumerable<Order> newOrders, string action)
		{
			try
			{
				Helper.Factory.Save();

				if (!string.IsNullOrEmpty(action) && newOrders.Any())
				{
					var stringBuilder = new ZStringBuilder();
					foreach (Order order in newOrders)
					{
						stringBuilder.Append(order.JD_OrderNumberAndSplit);
					}

					NotifySaveSuccessful(stringBuilder.ToStringWithDelimiterBetweenAppends(", "), action);
				}
			}
			catch (ZSaveConcurrencyException)
			{
				NotifySaveFailed();
			}
		}

		#region User Notifications

		void NotifySaveSuccessful(string savedOrderNumbers, string action)
		{
			string caption = Res.GetString("9364e765-b201-4b6f-8ca8-9b1c9201281f", "Orders have been {0}", action);
			string message = Res.GetString("42c2af05-8b9e-4460-a1ae-e4bd6212b674", "The following orders have been {0} and saved successfully: {1}{2}", action, System.Environment.NewLine, savedOrderNumbers);

			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void NotifySaveFailed()
		{
			string caption = Res.GetString("23d66b9a-4dcc-4b7e-9ed7-db531afb1786", "Unable to complete action");
			string message = Res.GetString("24f6b32f-9420-4e94-8aae-aba0f3ab6bc0", "While you have been working with this form, another user has made changes which cannot be merged. Please cancel and reload this form.");

			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		#endregion

		OrderSplitDialogResult ShowConfirmationForSplittingOrder()
		{
			using (var messageBox = new OrderSplitMessageBox(Helper.OriginalOrders.Where(order => order.IsOrderPartiallyCompleteAndIsNotAlreadySplit).Select(order => order.JD_OrderNumber).ToArray()))
			{
				ZFormModaliser.ShowDialogWithoutDispose(messageBox);
				return messageBox.OrderSplitDialogResult;
			}
		}

		#endregion

		#region Implementation

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion
	}
}
