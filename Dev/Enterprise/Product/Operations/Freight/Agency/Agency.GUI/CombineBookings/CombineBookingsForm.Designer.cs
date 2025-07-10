namespace Enterprise.Freight.Agency.GUI
{
	partial class CombineBookingsForm
	{
		protected new void InitializeComponent()
		{
			this.combineBookingsControl = new Enterprise.Freight.Agency.GUI.CombineBookingsControl();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 449, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(cancelButton);
			bottomPanel.Controls.Add(okButton);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 414, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 35, true);
			bottomPanel.TabIndex = 1;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			cancelButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("2d53370a-40e4-4f46-b17d-2b2b9434b9e2", "Cancel");
			cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 6, true);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			cancelButton.TabIndex = 0;
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			okButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("40250cb9-485d-48f8-a296-dc48c616a142", "OK");
			okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 6, true);
			okButton.Name = "okButton";
			okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			okButton.TabIndex = 1;
			okButton.UseVisualStyleBackColor = true;
			okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// combineBookingsControl
			// 
			this.combineBookingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.combineBookingsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.combineBookingsControl.Name = "combineBookingsControl";
			this.combineBookingsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.combineBookingsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 414, true);
			this.combineBookingsControl.TabIndex = 0;
			// 
			// CombineBookingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 473, true);
			this.Controls.Add(this.combineBookingsControl);
			this.Controls.Add(bottomPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 500, true);
			this.Name = "CombineBookingsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(this.combineBookingsControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		CombineBookingsControl combineBookingsControl;
		CargoWise.Windows.UI.KPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		Enterprise.ZArchitecture.GUI.ZButton okButton;
	}
}
