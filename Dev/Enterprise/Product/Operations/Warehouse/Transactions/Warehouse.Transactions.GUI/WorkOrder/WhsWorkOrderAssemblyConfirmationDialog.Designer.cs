namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class WhsWorkOrderAssemblyConfirmationDialog
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
            this.UserControl = new Enterprise.Warehouse.Transactions.GUI.WhsWorkOrderAssemblyConfirmationUserControl();
            this.AssemblyConfirmationLabel = new Enterprise.ZArchitecture.ZLabel();
            this.Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CancelAssemblyButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.UserControl.SuspendLayout();
            this.Panel.SuspendLayout();
            this.ButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 432, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 0, true);
            this.MainStatusBar.Visible = false;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsReceive);
            // 
            // UserControl
            // 
            this.UserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UserControl, ".");
            this.UserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
            this.UserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 200, true);
            this.UserControl.Name = "UserControl";
            this.UserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 385, true);
            this.UserControl.TabIndex = 4;
            // 
            // AssemblyConfirmationLabel
            // 
            this.AssemblyConfirmationLabel.AutoSize = true;
			this.AssemblyConfirmationLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("4e14f822-0828-4e13-b88b-b3e9b852225f", "The Grid below lists the Kits and left over Components resulting from this Work Order's Assembly.\r\nThe Linked Components Grid displays which Components were used to Assemble any kits shown.\r\nClick the Confirm Button to confirm the Kits are assembled as displayed. Click the Cancel button to prevent finalization.");
			this.AssemblyConfirmationLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.AssemblyConfirmationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.AssemblyConfirmationLabel.ForeColor = System.Drawing.Color.Blue;
            this.AssemblyConfirmationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AssemblyConfirmationLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(12, 4, 3, 0, true);
            this.AssemblyConfirmationLabel.Name = "AssemblyConfirmationLabel";
            this.AssemblyConfirmationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 12, true);
            this.AssemblyConfirmationLabel.TabIndex = 5;
            // 
            // Panel
            // 
            this.Panel.Controls.Add(this.UserControl);
            this.Panel.Controls.Add(this.AssemblyConfirmationLabel);
            this.Panel.Controls.Add(this.ButtonPanel);
            this.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.Panel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 200, true);
            this.Panel.Name = "Panel";
            this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 432, true);
            this.Panel.TabIndex = 0;
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this.ConfirmButton);
            this.ButtonPanel.Controls.Add(this.CancelAssemblyButton);
            this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 397, true);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 35, true);
            this.ButtonPanel.TabIndex = 6;
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfirmButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsWorkOrderAssemblyConfirmationDialog|ConfirmButton", "Confirm");
            this.ConfirmButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 5, true);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 24, true);
            this.ConfirmButton.TabIndex = 3;
            this.ConfirmButton.ToolTipCaption = null;
            this.ConfirmButton.UseVisualStyleBackColor = true;
            // 
            // CancelAssemblyButton
            // 
            this.CancelAssemblyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelAssemblyButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsWorkOrderAssemblyConfirmationDialog|CancelButton", "Cancel");
            this.CancelAssemblyButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelAssemblyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 5, true);
            this.CancelAssemblyButton.Name = "CancelAssemblyButton";
            this.CancelAssemblyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 24, true);
            this.CancelAssemblyButton.TabIndex = 3;
            this.CancelAssemblyButton.ToolTipCaption = null;
            this.CancelAssemblyButton.UseVisualStyleBackColor = true;
            // 
            // WhsWorkOrderAssemblyConfirmationDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.CancelAssemblyButton;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsWorkOrderAssemblyConfirmationDialog|FormCaption", "Assembly Confirmation");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 432, true);
            this.Controls.Add(this.Panel);
            this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsReceive);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 467, true);
            this.Name = "WhsWorkOrderAssemblyConfirmationDialog";
            this.ShouldSerializeTabPageMethods = false;
            this.ShowInTaskbar = false;
            this.Text = "WhsWorkOrderAssemblyConfirmationDialog";
            this.Controls.SetChildIndex(this.Panel, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.UserControl.ResumeLayout(true);
            this.UserControl.PerformLayout();
            this.Panel.ResumeLayout(false);
            this.Panel.PerformLayout();
            this.ButtonPanel.ResumeLayout(false);
            this.ButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		WhsWorkOrderAssemblyConfirmationUserControl UserControl;
        ZArchitecture.ZLabel AssemblyConfirmationLabel;
		ZArchitecture.GUI.ZPanel Panel;
		ZArchitecture.GUI.ZButton CancelAssemblyButton;
		ZArchitecture.GUI.ZButton ConfirmButton;
		private ZArchitecture.GUI.ZPanel ButtonPanel;
	}
}
