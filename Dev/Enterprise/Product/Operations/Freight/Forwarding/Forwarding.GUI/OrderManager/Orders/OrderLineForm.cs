using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderLineForm : ZForm
	{
		protected OrderLineForm()
		{
			InitializeComponent();
		}

		public OrderLineForm(OrderLine bO) : base(bO)
		{
			InitializeComponent();

			ShippingToleranceTabPage.TabVisible = OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.Value;

			WorkflowTabPage.Initialize(OrderLine);

			fDeliveriesContainersGroupBoxDiff = ContainersGroupBox.Top - DeliveriesGroupBox.Bottom;

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			OrderLine.JO_ContainersVisibleInfo.ValueChanged += new EventHandler(JO_ContainersVisible_Changed);

			AutoAddPreviousNextButtons = false;

			PlugIns.Add(ControllerIDs.DocAddresses);

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		public OrderLine OrderLine
		{
			get { return (OrderLine)BusinessEntity; }
		}

		#region Form Caption

		public override string FormCaption => OrderLine.FormCaption;

		#endregion

		#region Custom Labels

		protected override void OnLoad(EventArgs e)
		{
			if (!this.IsDesignMode())
			{
				JO_ContainersVisible_Changed(this, EventArgs.Empty);
			}

			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				SetupCustomLabelsConfiguredByBuyer();

				if (OrderLine.Order.Buyer != null)
				{
					PartAttrib1TextBox.GetExtension<ILabelCaptionRenderer>().Caption = OrderLine.Order.Buyer.PartAttributeManager.PartAttributeName1;
					PartAttrib2TextBox.GetExtension<ILabelCaptionRenderer>().Caption = OrderLine.Order.Buyer.PartAttributeManager.PartAttributeName2;
					PartAttrib3TextBox.GetExtension<ILabelCaptionRenderer>().Caption = OrderLine.Order.Buyer.PartAttributeManager.PartAttributeName3;
				}

				SetupHSCodeEffectiveDate();
			}
		}

		void SetupHSCodeEffectiveDate()
		{
			if (this.OrderLine?.Order != null)
			{
				TariffFindHelper.AddDefaultPropertyToTariffControlWithEffectiveDate(HarmonisedCodeFindBox, this.OrderLine.Order.JD_SystemCreateTimeUtc);
			}
		}

		void SetupCustomLabelsConfiguredByBuyer()
		{
			// grids
			fDeliveriesGridLayoutPersister = new CustomLabelsGridLayoutPersister(DeliveriesBoundGrid, OrderLineDelivery.NewCustomLabelsProvider(OrderLine.Order));
			fContainersGridLayoutPersister = new CustomLabelsGridLayoutPersister(DeliveryContainersBoundGrid, new OrderLineDeliverContainer.CustomLabelsProvider(OrderLine.Order));
			fRelatedContainersGridLayoutPersister = new CustomLabelsGridLayoutPersister(RelatedDeliveryContainersBoundGrid, new OrderLineDeliverContainer.CustomLabelsProvider(OrderLine.Order));
		}

		readonly List<CustomLabelControlRenamer> fControlRenamers = new List<CustomLabelControlRenamer>();
		CustomLabelsGridLayoutPersister fDeliveriesGridLayoutPersister;
		CustomLabelsGridLayoutPersister fContainersGridLayoutPersister;
		CustomLabelsGridLayoutPersister fRelatedContainersGridLayoutPersister;

		#endregion

		#region Show/Hide Containers

		public bool ContainersVisible
		{
			get { return fContainersVisible; }
			set
			{
				fContainersVisible = value;
				int heightOfBothContainerGroupBoxes = RelatedContainersGroupBox.Bottom - ContainersGroupBox.Top;
				if (fContainersVisible)
				{
					ControlDpiScalingHelper.SetHeight(this, this.Height + heightOfBothContainerGroupBoxes + fDeliveriesContainersGroupBoxDiff, false);
					ControlDpiScalingHelper.SetHeight(ref DeliveriesGroupBox, ContainersGroupBox.Top - DeliveriesGroupBox.Top - fDeliveriesContainersGroupBoxDiff, false);
					ShowHideContainers.Text = Res.GetString("Forwarding|OrderLineForm|HideContainers", "Hide Containers");
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(ref DeliveriesGroupBox, RelatedContainersGroupBox.Bottom - DeliveriesGroupBox.Top, false);
					DeliveriesGroupBox.BringToFront();
					ShowHideContainers.Text = Res.GetString("Forwarding|OrderLineForm|ShowContainers", "Show Containers");
					ControlDpiScalingHelper.SetHeight(this, this.Height - (heightOfBothContainerGroupBoxes + fDeliveriesContainersGroupBoxDiff), false);
				}

				ContainersGroupBox.Visible = fContainersVisible;
				RelatedContainersGroupBox.Visible = fContainersVisible;
				AttachDeliveryToContainerGroupBox.Visible = fContainersVisible;
			}
		}

		bool fContainersVisible = true;
		readonly int fDeliveriesContainersGroupBoxDiff;

		void ShowHideContainers_Click(object sender, EventArgs e)
		{
			OrderLine.JO_ContainersVisible = !OrderLine.JO_ContainersVisible;
		}

		void JO_ContainersVisible_Changed(object sender, EventArgs e)
		{
			ContainersVisible = OrderLine.JO_ContainersVisible;
		}

		#endregion

		#region Deliveries

		void OnAttachToDelivery_Click(object sender, EventArgs e)
		{
			if (DeliveryContainersBoundGrid.ListManager.Count > 0 &&
				DeliveryContainersBoundGrid.ListManager.Position != -1)
			{
				OrderLineDeliverContainer containerToAttach = (OrderLineDeliverContainer)DeliveryContainersBoundGrid.ListManager.GetCurrent();
				if (containerToAttach != null)
				{
					if (!containerToAttach.TryAttachToDelivery(out var errorMessage))
					{
						Globals.Message.ShowError(errorMessage);
					}
				}
			}
		}

		#endregion

		#region OrderLineFormControl

		void OrderLineFormControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedTab = OrderLineFormControl.SelectedTab;

			if (selectedTab.Name == "DetailsTabPage")
			{
				this.ShowHideContainers.Visible = true;
			}
			else
			{
				this.ShowHideContainers.Visible = false;
			}
		}

		#endregion

		#region Dispose

		IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				foreach (CustomLabelControlRenamer renamer in fControlRenamers)
				{
					renamer.Dispose();
				}

				if (OrderLine != null)
				{
					OrderLine.JO_ContainersVisibleInfo.ValueChanged -= new EventHandler(JO_ContainersVisible_Changed);
				}

				if (fDeliveriesGridLayoutPersister != null)
				{
					fDeliveriesGridLayoutPersister.Dispose();
				}

				if (fContainersGridLayoutPersister != null)
				{
					fContainersGridLayoutPersister.Dispose();
				}

				if (fRelatedContainersGridLayoutPersister != null)
				{
					fRelatedContainersGridLayoutPersister.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
