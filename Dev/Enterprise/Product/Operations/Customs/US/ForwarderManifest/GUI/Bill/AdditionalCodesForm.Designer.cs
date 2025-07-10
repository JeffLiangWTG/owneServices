namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class AdditionalCodesForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CusEntryNumGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CusEntryNumSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GaveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CusEntryNumGrid)).BeginInit();
			this.CusEntryNumGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusEntryNumSplitContainer)).BeginInit();
			this.CusEntryNumSplitContainer.Panel1.SuspendLayout();
			this.CusEntryNumSplitContainer.Panel2.SuspendLayout();
			this.CusEntryNumSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 262, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.CusEntryNumCollection);
			// 
			// CusEntryNumGrid
			// 
			this.CusEntryNumGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CusEntryNumGrid, ".");
			this.CusEntryNumGrid.CaptionVisible = false;
			this.CusEntryNumGrid.CopySelectedRowsAllowed = true;
			this.CusEntryNumGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusEntryNumGrid.GridId = "6AD193BE-A853-4751-9C56-85976A55060E";
			this.CusEntryNumGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusEntryNumGrid.LayoutKey = "CusEntryNumGrid";
			this.CusEntryNumGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusEntryNumGrid.Name = "CusEntryNumGrid";
			this.CusEntryNumGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 231, true);
			this.CusEntryNumGrid.TabIndex = 0;
			// 
			// CusEntryNumSplitContainer
			// 
			this.CusEntryNumSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusEntryNumSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusEntryNumSplitContainer.Name = "CusEntryNumSplitContainer";
			this.CusEntryNumSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// CusEntryNumSplitContainer.Panel1
			// 
			this.CusEntryNumSplitContainer.Panel1.Controls.Add(this.CusEntryNumGrid);
			// 
			// SealNumbersSplitContainer.Panel2
			// 
			this.CusEntryNumSplitContainer.Panel2.Controls.Add(this.GaveUpButton);
			this.CusEntryNumSplitContainer.Panel2.Controls.Add(this.OKButton);
			this.CusEntryNumSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 262, true);
			this.CusEntryNumSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(231);
			this.CusEntryNumSplitContainer.TabIndex = 1;
			// 
			// GaveUpButton
			// 
			this.GaveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GaveUpButton.CaptionResourceString = Res.GetData("E7062071-B349-490D-9608-E5665AEC3F21", "Cancel");
			this.GaveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GaveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 3, true);
			this.GaveUpButton.Name = "GaveUpButton";
			this.GaveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GaveUpButton.TabIndex = 1;
			this.GaveUpButton.UseVisualStyleBackColor = true;
			this.GaveUpButton.Click += new System.EventHandler(this.GaveUpButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Res.GetData("B1F0BB9F-5E95-41BE-B588-68B0F06B4B77", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);

			// 
			// AdditionalCodesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.GaveUpButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 286, true);
			this.Controls.Add(this.CusEntryNumSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.CusEntryNumCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "AdditionalCodesForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CusEntryNumSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CusEntryNumGrid)).EndInit();
			this.CusEntryNumGrid.ResumeLayout(false);
			this.CusEntryNumGrid.PerformLayout();
			this.CusEntryNumSplitContainer.Panel1.ResumeLayout(false);
			this.CusEntryNumSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusEntryNumSplitContainer)).EndInit();
			this.CusEntryNumSplitContainer.ResumeLayout(false);
			this.CusEntryNumSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer CusEntryNumSplitContainer;
		private ZArchitecture.ZGrid CusEntryNumGrid;
		private ZArchitecture.GUI.ZButton GaveUpButton;
		internal ZArchitecture.GUI.ZButton OKButton;
	}
}
