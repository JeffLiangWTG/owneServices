namespace Enterprise.MasterFiles.GUI
{
	partial class RelatedCommunicationForm
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
			this.communicationGrid = new Enterprise.MasterFiles.GUI.RelatedCommunicationGrid();
			this.communicationGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CloseButtonUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.communicationGridPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 445, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCallCollection);
			// 
			// communicationGrid
			// 
			this.communicationGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.communicationGrid, ".");
			this.communicationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.communicationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 10, true);
			this.communicationGrid.Name = "communicationGrid";
			this.communicationGrid.ShowPopupContextMenu = false;
			this.communicationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 400, true);
			this.communicationGrid.TabIndex = 0;
			// 
			// communicationGridPanel
			// 
			this.communicationGridPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.communicationGridPanel.Controls.Add(this.communicationGrid);
			this.communicationGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.communicationGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.communicationGridPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.communicationGridPanel.Name = "communicationGridPanel";
			this.communicationGridPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 10, 5, 5, true);
			this.communicationGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 417, true);
			this.communicationGridPanel.TabIndex = 0;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.CloseButtonUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 417, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 28, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// CloseButtonUserControl
			// 
			this.CloseButtonUserControl.AllowDrop = true;
			this.CloseButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 2, true);
			this.CloseButtonUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.CloseButtonUserControl.Name = "CloseButtonUserControl";
			this.CloseButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.CloseButtonUserControl.TabIndex = 1;
			// 
			// RelatedCommunicationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ac6a0dce-5762-4636-af4d-609fe7d9bad5", "Related Communication");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 469, true);
			this.Controls.Add(this.communicationGridPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCallCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "RelatedCommunicationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.communicationGridPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.communicationGridPanel.ResumeLayout(false);
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private RelatedCommunicationGrid communicationGrid;
		private ZArchitecture.GUI.ZPanel communicationGridPanel;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private Core.Forms.ZPostingButtonsUserControl CloseButtonUserControl;
	}
}