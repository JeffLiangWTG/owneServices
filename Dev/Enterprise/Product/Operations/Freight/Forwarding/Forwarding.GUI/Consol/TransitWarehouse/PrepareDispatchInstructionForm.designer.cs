using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PrepareDispatchInstructionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			this.selectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.selectNoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.deliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.shipmentSelectionGrid = new ShipmentsPrepareForDispatchInstructionGrid();

			((System.ComponentModel.ISupportInitialize)(this.shipmentSelectionGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.MessageStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.ErrorStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.BindingSource).BeginInit();

			this.shipmentSelectionGrid.SuspendLayout();
			this.buttonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonsPanel
			// 
			this.buttonsPanel.Controls.Add(this.selectNoneButton);
			this.buttonsPanel.Controls.Add(this.selectAllButton);
			this.buttonsPanel.Controls.Add(this.deliverButton);
			this.buttonsPanel.Controls.Add(this.cancelButton);
			this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.buttonsPanel.Name = "buttonsPanel";
			this.buttonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 23, true);
			this.buttonsPanel.TabIndex = 9;

			// selectAllButton
			this.selectAllButton.Dock = DockStyle.Left;
			this.selectAllButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ff11f971-156c-11a7-476d-edfe8d42ed81", "Select All");
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.selectAllButton.TabIndex = 0;
			this.selectAllButton.ToolTipCaption = null;
			this.selectAllButton.UseVisualStyleBackColor = true;
			this.selectAllButton.Click += SelectAllButton_OnClick;

			// selectNoneButton
			this.selectNoneButton.Dock = DockStyle.Left;
			this.selectNoneButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d993f105-fc0e-1f80-4d98-8bb09843e5c1", "Select None");
			this.selectNoneButton.Name = "selectNoneButton";
			this.selectNoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.selectNoneButton.TabIndex = 1;
			this.selectNoneButton.ToolTipCaption = null;
			this.selectNoneButton.UseVisualStyleBackColor = true;
			this.selectNoneButton.Click += SelectNoneButton_OnClick;

			// cancelButton
			this.cancelButton.Dock = DockStyle.Right;
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("61a92dfa-caa0-8db9-48c3-f7181756e08b", "Cancel");
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += CancelButton_OnClick;

			// deliverButton
			this.deliverButton.Dock = DockStyle.Right;
			this.deliverButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("49974f5f-20b1-ec87-47f4-021fe550df0b", "Deliver");
			this.deliverButton.Name = "deliverButton";
			this.deliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.deliverButton.TabIndex = 2;
			this.deliverButton.ToolTipCaption = null;
			this.deliverButton.UseVisualStyleBackColor = true;
			this.deliverButton.Click += Deliverbutton_OnClick;

			// shipmentSelectionGrid
			this.BindingSource.SetBindingMember(this.shipmentSelectionGrid, "ShipmentsForSelection");
			this.shipmentSelectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shipmentSelectionGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.shipmentSelectionGrid.GridId = "c1edf506-a411-5d99-4f30-e11569bd6cbe";
			this.shipmentSelectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.shipmentSelectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 335, true);

			this.AcceptButton = this.deliverButton;
			this.CancelButton = this.cancelButton;

			this.Name = "PrepareDispatchInstructionForm";
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5e9a7d9b-6e23-e994-45ba-c00c44d04c00", "Deliver Prepare Dispatch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 400, true);
			this.Controls.Add(this.shipmentSelectionGrid);
			this.Controls.Add(this.buttonsPanel);
			this.DataSourceType = typeof(ConsolPrepareForDispatchInstruction);
			this.Controls.SetChildIndex(this.shipmentSelectionGrid, 0);
			this.Controls.SetChildIndex(this.buttonsPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);

			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.shipmentSelectionGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

			this.buttonsPanel.ResumeLayout(false);
			this.buttonsPanel.PerformLayout();

			this.shipmentSelectionGrid.ResumeLayout(false);
			this.shipmentSelectionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZButton selectAllButton;
		private ZArchitecture.GUI.ZButton selectNoneButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton deliverButton;


		private ShipmentsPrepareForDispatchInstructionGrid shipmentSelectionGrid;
		private KTableLayoutPanel MainPanel;
		private ZPanel buttonsPanel;
	}
}
