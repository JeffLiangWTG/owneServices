using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class SelectOrdersForm
	{
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

		ZPanel TopPanel;
		ZPanel BottomPanel;
		Enterprise.ZArchitecture.ZGrid OrdersGrid;
		Enterprise.ZArchitecture.GUI.ZButton CancelFormButton;
		Enterprise.ZArchitecture.GUI.ZButton SelectButton;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code

		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.OrdersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersGrid)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.zLabel1);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 36, true);
			this.TopPanel.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|974cbef4-ceac-499b-af1f-59e196bb8b78", "Orders exist for this Consignee/Consignor. Select order(s) to attach to Shipment.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// OrdersGrid
			// 
			this.OrdersGrid.AllowNavigation = false;
			this.OrdersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrdersGrid, "StrictlyMatchedRelatedOrders");
			this.OrdersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|7b2f928c-e098-477d-ae42-a3013914647d", "Order Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JD_OrderNumberAndSplit";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|ef96bd4a-64e3-40ef-a4d3-f4b202a26882", "Order Number");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo1.ColumnName = "JD_OrderDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|5d6f586d-91cd-4ffe-aa7a-c26f2a8b087f", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "JD_OrderGoodsDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrdersGrid.GridId = "d7f8aa95-85e9-494e-8fd3-6ee1a62dd10d";
			this.OrdersGrid.CopySelectedRowsAllowed = true;
			this.OrdersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrdersGrid.IsWholeRowSelectedOnClick = true;
			this.OrdersGrid.LayoutKey = "OrdersGrid";
			this.OrdersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.OrdersGrid.Name = "OrdersGrid";
			this.OrdersGrid.ReadOnly = true;
			this.OrdersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 150, true);
			this.OrdersGrid.TabIndex = 1;
			this.OrdersGrid.DoubleClick += new System.EventHandler(this.OrdersGrid_DoubleClick);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CancelFormButton);
			this.BottomPanel.Controls.Add(this.SelectButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 36, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelFormButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|d6dd7dfd-736a-4549-a1c2-051c9fafff3b", "&Cancel");
			this.CancelFormButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 8, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 1;
			// 
			// SelectButton
			// 
			this.SelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|9833bfe8-f88a-4b9d-8c8e-a25d4e7a5260", "&Select");
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 8, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectButton.TabIndex = 0;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// SelectOrdersForm
			// 
			this.AcceptButton = this.SelectButton;
			this.CancelButton = this.CancelFormButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 252, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SelectOrdersForm|4fd37eb7-0951-4f0d-8af6-55667f64252b", "Select Orders");
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.OrdersGrid);
			this.Controls.Add(this.TopPanel);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ForwardingShipment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SelectOrdersForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Load += new System.EventHandler(this.SelectOrdersForm_Load);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.OrdersGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersGrid)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
