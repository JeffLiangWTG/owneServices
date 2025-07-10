using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderDetailsBulkUpdateForm : ZForm
	{
		public OrderDetailsBulkUpdateForm(OrderDetailsBulkUpdateBusinessObject bO) : base(bO)
		{
			UpdateButton.TextChanged += new EventHandler(OnUpdateButton_TextChanged);
			ZFormPostingButtonsStrategy.SetupPosting(this, UpdateButton, CloseButton);
			OnUpdateButton_TextChanged(null, null);

			ControlDpiScalingHelper.SetWidth(ref PropertyForEnsuringDataEnteredBoundTextBox, 0, true);
			bO.SaveSucceeded += new EventHandler(OnSaveSucceeded);
		}

		public new OrderDetailsBulkUpdateBusinessObject BusinessEntity
		{
			get { return (OrderDetailsBulkUpdateBusinessObject)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get
			{
				return ZString.Empty;
			}
		}

		#region Attach / Detach Buttons

		protected EmbeddedModulePopup LastShownAttachPopup;

		protected void OnAttachButton_Click(object sender, EventArgs e)
		{
			ZFilterGridModule orderModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Orders);
			orderModule.SetupAndGetGrid();

			SelectFromCollectionHelper helper = new SelectFromCollectionHelper(BusinessEntity.SelectedOrders, orderModule);
			helper.BusinessObjectSelected += new SelectFromCollectionHelper.BusinessObjectSelectedHandler(OnBusinessObjectSelected);
			helper.Select(this);
			this.LastShownAttachPopup = helper.LastShownPopup;
		}

		void OnBusinessObjectSelected(SelectFromCollectionHelper sender, BusinessObject[] selectedOrders)
		{
			foreach (Order order in selectedOrders)
			{
				BusinessEntity.SelectedOrders.AddOrderToBulkUpdate(order.PK);
			}
		}

		protected void OnDetachButton_Click(object sender, EventArgs e)
		{
			if (SelectedOrdersBoundGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("e345fe2e-0b65-4c9a-8255-abcaa0574f67", "Select an order in the grid to detach"));
			}
			foreach (OrderToBulkUpdate order in SelectedOrdersBoundGrid.SelectedElements)
			{
				BusinessEntity.SelectedOrders.RemoveAndDelete(order);
			}
		}

		#endregion

		#region Implementation

		void OnUpdateButton_TextChanged(object sender, EventArgs e)
		{
			this.UpdateButton.Text = Res.GetString("Forwarding|OrderDetailsBulkUpdateForm|UpdateButton", "Update");
		}

		void OnSaveSucceeded(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(Res.GetString("ce68827e-6a92-4c43-8369-3e2780da790e", "Orders updated successfully"), Res.GetString("ae9ffc37-acbc-4c67-aec9-abda72a7b782", "Success"));
		}

		#endregion
	}
}
