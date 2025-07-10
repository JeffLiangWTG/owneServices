namespace Enterprise.Customs.US.GUI
{
	partial class SealNumberUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SealNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SealNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SealNumberSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SealNumberSplitContainer)).BeginInit();
			this.SealNumberSplitContainer.Panel1.SuspendLayout();
			this.SealNumberSplitContainer.Panel2.SuspendLayout();
			this.SealNumberSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// SealNumberButton
			// 
			this.SealNumberButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealNumberButton.Name = "SealNumberButton";
			this.SealNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 21, true);
			this.SealNumberButton.TabIndex = 0;
			this.SealNumberButton.Text = "...";
			this.SealNumberButton.UseVisualStyleBackColor = true;
			this.SealNumberButton.Click += new System.EventHandler(this.SealNumberButton_Click);
			// 
			// SealNumberTextBox
			// 
			this.SealNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("223d4ee5-9efe-45dc-b017-c170e93257ec", "Seal Number String");
			this.SealNumberTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealNumberTextBox.Name = "SealNumberTextBox";
			this.SealNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.SealNumberTextBox.TabIndex = 1;
			// 
			// SealNumberSplitContainer
			// 
			this.SealNumberSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealNumberSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SealNumberSplitContainer.IsSplitterFixed = true;
			this.SealNumberSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealNumberSplitContainer.Name = "SealNumberSplitContainer";
			// 
			// SealNumberSplitContainer.Panel1
			// 
			this.SealNumberSplitContainer.Panel1.Controls.Add(this.SealNumberTextBox);
			// 
			// SealNumberSplitContainer.Panel2
			// 
			this.SealNumberSplitContainer.Panel2.Controls.Add(this.SealNumberButton);
			this.SealNumberSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 21, true);
			this.SealNumberSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(77);
			this.SealNumberSplitContainer.TabIndex = 2;
			// 
			// SealNumberUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SealNumberSplitContainer);
			this.Name = "SealNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealNumberSplitContainer.Panel1.ResumeLayout(false);
			this.SealNumberSplitContainer.Panel1.PerformLayout();
			this.SealNumberSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SealNumberSplitContainer)).EndInit();
			this.SealNumberSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal protected ZArchitecture.GUI.ZButton SealNumberButton;
		internal protected ZArchitecture.ZTextBox SealNumberTextBox;
		private CargoWise.Windows.UI.KSplitContainer SealNumberSplitContainer;
	}
}
