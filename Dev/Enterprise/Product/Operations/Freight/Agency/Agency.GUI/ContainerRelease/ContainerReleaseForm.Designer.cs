namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerReleaseForm
	{
		new void InitializeComponent()
		{
			this.releaseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			containerReleaseControl = new Enterprise.Freight.Agency.GUI.ContainerReleaseControl();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			selectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 309, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ReleaseHeader);
			// 
			// containerReleaseControl
			// 
			this.BindingSource.SetBindingMember(containerReleaseControl, ".");
			containerReleaseControl.Dock = System.Windows.Forms.DockStyle.Fill;
			containerReleaseControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			containerReleaseControl.Name = "containerReleaseControl";
			containerReleaseControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 280, true);
			containerReleaseControl.TabIndex = 0;
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(selectAllButton);
			bottomPanel.Controls.Add(cancelButton);
			bottomPanel.Controls.Add(this.releaseButton);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 280, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 29, true);
			bottomPanel.TabIndex = 1;
			// 
			// selectAllButton
			// 
			selectAllButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerReleaseForm|2a6f395d-b840-4b92-8bbd-317264a4f864", "Select All");
			selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			selectAllButton.Name = "selectAllButton";
			selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			selectAllButton.TabIndex = 0;
			selectAllButton.UseVisualStyleBackColor = true;
			selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// cancelButton
			// 
			cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			cancelButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerReleaseForm|ca79dcb5-a5a9-4314-b06a-00f10a39f04e", "Cancel");
			cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 0, true);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			cancelButton.TabIndex = 2;
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// releaseButton
			// 
			this.releaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.releaseButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerReleaseForm|9a28ccd3-6726-4bdf-bdb5-a42be8944216", "Release");
			this.releaseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 0, true);
			this.releaseButton.Name = "releaseButton";
			this.releaseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.releaseButton.TabIndex = 1;
			this.releaseButton.UseVisualStyleBackColor = true;
			this.releaseButton.Click += new System.EventHandler(this.releaseButton_Click);
			// 
			// ContainerReleaseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 333, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerReleaseForm|ed6407cf-53e6-490b-b742-ca14874ff9f1", "Container Release Wizard");
			this.Controls.Add(containerReleaseControl);
			this.Controls.Add(bottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Freight.Agency.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ReleaseHeader);
			this.DataSourceTypeName = "Enterprise.Freight.Agency.Business.ReleaseHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 360, true);
			this.Name = "ContainerReleaseForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(containerReleaseControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.Freight.Agency.GUI.ContainerReleaseControl containerReleaseControl;
		Enterprise.ZArchitecture.GUI.ZButton releaseButton;
		CargoWise.Windows.UI.KPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZButton selectAllButton;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}
