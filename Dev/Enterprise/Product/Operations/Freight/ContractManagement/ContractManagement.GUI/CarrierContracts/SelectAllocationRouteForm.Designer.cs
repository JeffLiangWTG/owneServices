using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	partial class SelectAllocationRouteForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.headerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.infoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.overrideLabel = new Enterprise.ZArchitecture.ZLabel();
			this.headerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.buttonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.selectionGrid = new AllocationRouteGrid();

			((System.ComponentModel.ISupportInitialize)(this.selectionGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.MessageStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.ErrorStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.BindingSource).BeginInit();

			this.headerLabel.SuspendLayout();
			this.infoLabel.SuspendLayout();
			this.overrideLabel.SuspendLayout();
			this.SuspendLayout();
			//
			// headerPanel
			//
			this.headerPanel.Controls.Add(headerLabel);
			this.headerPanel.Controls.Add(infoLabel);
			this.headerPanel.Controls.Add(overrideLabel);
			this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 80);
			// 
			// buttonsPanel
			// 
			this.buttonsPanel.Controls.Add(this.okButton);
			this.buttonsPanel.Controls.Add(this.cancelButton);
			this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.buttonsPanel.Name = "buttonsPanel";
			this.buttonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 23, true);
			//
			// headerLabel
			//
			this.headerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 14, true);
			this.headerLabel.Text = ContractManagement.GUI.Res.GetString("SelectAllocationRouteForm|cada1c02-c74a-4294-42ac-b0f379eec0ca", "Multiple Allocation Routes are found to be linked with the Sailing Schedule(s).");
			this.headerLabel.IsFontBold = true;
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// infoLabel
			//
			this.infoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 30, true);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 14, true);
			this.infoLabel.Text = ContractManagement.GUI.Res.GetString("SelectAllocationRouteForm|235fbf29-03f0-02aa-4095-f2b7432a5b00", "Please choose one for allocating {0} to.", this.routeAssignable.HumanReadableName);
			this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			//
			// overrideLabel
			//
			this.overrideLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 50, true);
			this.overrideLabel.Name = "overrideLabel";
			this.overrideLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 14, true);
			this.overrideLabel.Text = ContractManagement.GUI.Res.GetString("SelectAllocationRouteForm|70cb677c-925c-4ca0-bccf-88d6bdd542cd", "This will override any pre-existing Carrier Contracts and Allocation Routes assigned.");
			this.overrideLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// okButton 
			//
			this.okButton.CaptionResourceString = ContractManagement.GUI.Res.GetData("SelectAllocationRouteForm|8b183c13-57b0-1492-41ad-09424e22250b", "OK");
			this.okButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 24, true);
			this.okButton.TabIndex = 1;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += OkButton_Click;
			//
			// cancelButton
			//
			this.cancelButton.CaptionResourceString = ContractManagement.GUI.Res.GetData("SelectAllocationRouteForm|49ba6883-5d54-7aa0-4f31-75294ae52955", "Cancel");
			this.cancelButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 24, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += CancelButton_Click;
			//
			// selectionGrid
			//
			this.BindingSource.SetBindingMember(this.selectionGrid, "AllocationRoutesForSelection");
			this.selectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.selectionGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.selectionGrid.GridId = "3590b752-3687-c19a-4cd7-bf7b5036a43c";
			this.selectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 80, true);
			this.selectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 200, true);
			this.selectionGrid.Name = "selectionGrid";

			this.AcceptButton = okButton;
			this.CancelButton = cancelButton;

			this.Controls.Add(headerPanel);
			this.Controls.Add(selectionGrid);
			this.Controls.Add(buttonsPanel);
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 350, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.selectionGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Name = "SelectAllocationRouteForm";
			this.Text = "SelectAllocationRouteForm";

			this.headerLabel.ResumeLayout(false);
			this.headerLabel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel headerLabel;
		private Enterprise.ZArchitecture.ZLabel infoLabel;
		private Enterprise.ZArchitecture.ZLabel overrideLabel;
		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private AllocationRouteGrid selectionGrid;
		private ZPanel headerPanel;
		private ZPanel buttonsPanel;
	}
}
