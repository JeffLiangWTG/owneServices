using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class GenericOrdersModuleButtonGrid : ZModuleButtonGrid
	{
		void InitializeComponent()
		{
			this.NewPurchaseOrderMenuItem = new ZToolStripMenuItem();
			this.NewWarehouseOrderMenuItem = new ZToolStripMenuItem();
			this.AttachPurchaseOrderMenuItem = new ZToolStripMenuItem();
			this.AttachWarehouseOrderMenuItem = new ZToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DetachOrderButton
			// 
			this.DetachOrderButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Left;
			this.DetachOrderButton.AutoSize = false;
			this.DetachOrderButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ca5865a5-2c05-4652-a55a-de7b0d79f37d", "Detach");
			this.DetachOrderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.DetachOrderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.DetachOrderButton.Name = "DetachOrderButton";
			this.DetachOrderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 22, true);
			this.DetachOrderButton.Click += new EventHandler(this.DetachButton_Click);
			this.DetachOrderButton.Image = Enterprise.ZArchitecture.GUI.Icons.GetImage(Enterprise.ZArchitecture.Modules.IconTypes.BlackWhite_Remove);
			// 
			// AttachOrderSplitButton
			// 
			this.AttachOrderSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Left;
			this.AttachOrderSplitButton.AutoSize = false;
			this.AttachOrderSplitButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5002b358-eb8a-43b2-95af-dcedcd3e44bf", "Attach");
			this.AttachOrderSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.AttachOrderSplitButton.DropDownItems.AddRange(new ToolStripItem[] {
			this.AttachPurchaseOrderMenuItem,
			this.AttachWarehouseOrderMenuItem });
			this.AttachOrderSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AttachOrderSplitButton.Name = "AttachOrderSplitButton";
			this.AttachOrderSplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.AttachOrderSplitButton.ButtonClick += new EventHandler(this.AttachButton_Click);
			this.AttachOrderSplitButton.Image = Enterprise.ZArchitecture.GUI.Icons.GetImage(Enterprise.ZArchitecture.Modules.IconTypes.BlackWhite_Add);
			// 
			// AttachPurchaseOrderMenuItem
			// 
			this.AttachPurchaseOrderMenuItem.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2ba3d88d-560a-4e7a-a7e2-849d8593369c", "Order");
			this.AttachPurchaseOrderMenuItem.Name = "AttachPurchaseOrderMenuItem";
			this.AttachPurchaseOrderMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 22, true);
			this.AttachPurchaseOrderMenuItem.Click += new EventHandler(this.AttachButton_Click);
			// 
			// AttachWarehouseOrderMenuItem
			// 
			this.AttachWarehouseOrderMenuItem.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e991665c-70a3-46aa-b0bb-c661270249a5", "Warehouse Order");
			this.AttachWarehouseOrderMenuItem.Name = "AttachWarehouseOrderMenuItem";
			this.AttachWarehouseOrderMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 22, true);
			this.AttachWarehouseOrderMenuItem.Click += new EventHandler(this.AttachWarehouseOrderToolStripMenuItem_Click);
			// 
			// EditOrderButton
			// 
			this.EditOrderButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Left;
			this.EditOrderButton.AutoSize = false;
			this.EditOrderButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5367c693-f83f-43f5-97b5-f5e69e3163e2", "Edit");
			this.EditOrderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.EditOrderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.EditOrderButton.Name = "EditOrderButton";
			this.EditOrderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 22, true);
			this.EditOrderButton.Click += new EventHandler(this.EditButton_Click);
			this.EditOrderButton.Image = Enterprise.ZArchitecture.GUI.Icons.GetImage(Enterprise.ZArchitecture.Modules.IconTypes.EditButtonRest);
			// 
			// NewOrderSplitButton
			// 
			this.NewOrderSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Left;
			this.NewOrderSplitButton.AutoSize = false;
			this.NewOrderSplitButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("77fb9860-beae-42f7-b802-afbdb2e4532c", "New");
			this.NewOrderSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
			this.NewOrderSplitButton.DropDownItems.AddRange(new ToolStripItem[] {
			this.NewPurchaseOrderMenuItem,
			this.NewWarehouseOrderMenuItem });
			this.NewOrderSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.NewOrderSplitButton.Name = "NewOrderSplitButton";
			this.NewOrderSplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 22, true);
			this.NewOrderSplitButton.ButtonClick += new EventHandler(this.NewButton_Click);
			this.NewOrderSplitButton.Image = Enterprise.ZArchitecture.GUI.Icons.GetImage(Enterprise.ZArchitecture.Modules.IconTypes.NewButtonRest);
			// 
			// NewPurchaseOrderMenuItem
			// 
			this.NewPurchaseOrderMenuItem.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3e0ebae1-bd69-4a4d-bcbc-4d9476660577", "Order");
			this.NewPurchaseOrderMenuItem.Name = "NewPurchaseOrderMenuItem";
			this.NewPurchaseOrderMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 22, true);
			this.NewPurchaseOrderMenuItem.Click += new EventHandler(this.NewButton_Click);
			// 
			// NewWarehouseOrderMenuItem
			// 
			this.NewWarehouseOrderMenuItem.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e991665c-70a3-46aa-b0bb-c661270249a5", "Warehouse Order");
			this.NewWarehouseOrderMenuItem.Name = "NewWarehouseOrderMenuItem";
			this.NewWarehouseOrderMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 22, true);
			this.NewWarehouseOrderMenuItem.Click += new EventHandler(this.NewWarehouseOrderToolStripMenuItem_Click);
			// 
			// GenericOrdersModuleButtonGrid
			// 
			this.Name = "GenericOrdersModuleButtonGrid";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		ZToolStripSplitButton NewOrderSplitButton;
		ZToolStripMenuItem NewPurchaseOrderMenuItem;
		ZToolStripMenuItem NewWarehouseOrderMenuItem;
		ZToolStripButton EditOrderButton;
		ZToolStripSplitButton AttachOrderSplitButton;
		ZToolStripMenuItem AttachPurchaseOrderMenuItem;
		ZToolStripMenuItem AttachWarehouseOrderMenuItem;
		ZToolStripButton DetachOrderButton;
	}
}
