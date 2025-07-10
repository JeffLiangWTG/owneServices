namespace Enterprise.Customs.US.GUI
{
	partial class PPQForm368NoticeOfArrivalDataForm
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
		protected override void InitializeComponent()
		{
			this.MarksBillOfLadingAndContainerNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MarksBillOfLadingAndContainerNoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QuantityAndNetWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QuantityAndNetWeightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommodityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DefaultButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MarksBillOfLadingAndContainerNoGroupBox.SuspendLayout();
			this.QuantityAndNetWeightGroupBox.SuspendLayout();
			this.CommodityGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 394, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.PPQForm368NoticeOfArrivalData);
			// 
			// MarksBillOfLadingAndContainerNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksBillOfLadingAndContainerNoTextBox, "US_PPQForm368Box13A");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.PPQForm368NoticeOfArrivalData)(null)).US_PPQForm368Box13A)));
			this.MarksBillOfLadingAndContainerNoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MarksBillOfLadingAndContainerNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MarksBillOfLadingAndContainerNoTextBox.Multiline = true;
			this.MarksBillOfLadingAndContainerNoTextBox.Name = "MarksBillOfLadingAndContainerNoTextBox";
			this.MarksBillOfLadingAndContainerNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 328, true);
			this.MarksBillOfLadingAndContainerNoTextBox.TabIndex = 0;
			// 
			// MarksBillOfLadingAndContainerNoGroupBox
			// 
			this.MarksBillOfLadingAndContainerNoGroupBox.Controls.Add(this.MarksBillOfLadingAndContainerNoTextBox);
			this.MarksBillOfLadingAndContainerNoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.MarksBillOfLadingAndContainerNoGroupBox.Name = "MarksBillOfLadingAndContainerNoGroupBox";
			this.MarksBillOfLadingAndContainerNoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 347, true);
			this.MarksBillOfLadingAndContainerNoGroupBox.TabIndex = 0;
			this.MarksBillOfLadingAndContainerNoGroupBox.TabStop = false;
			this.MarksBillOfLadingAndContainerNoGroupBox.Text = "Marks, Bill Of Lading, And/Or Container No.";
			// 
			// QuantityAndNetWeightGroupBox
			// 
			this.QuantityAndNetWeightGroupBox.Controls.Add(this.QuantityAndNetWeightTextBox);
			this.QuantityAndNetWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 12, true);
			this.QuantityAndNetWeightGroupBox.Name = "QuantityAndNetWeightGroupBox";
			this.QuantityAndNetWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 347, true);
			this.QuantityAndNetWeightGroupBox.TabIndex = 1;
			this.QuantityAndNetWeightGroupBox.TabStop = false;
			this.QuantityAndNetWeightGroupBox.Text = "Quantity And Net Weight";
			// 
			// QuantityAndNetWeightTextBox
			// 
			this.BindingSource.SetBindingMember(this.QuantityAndNetWeightTextBox, "US_PPQForm368Box13B");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.PPQForm368NoticeOfArrivalData)(null)).US_PPQForm368Box13B)));
			this.QuantityAndNetWeightTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuantityAndNetWeightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.QuantityAndNetWeightTextBox.Multiline = true;
			this.QuantityAndNetWeightTextBox.Name = "QuantityAndNetWeightTextBox";
			this.QuantityAndNetWeightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 328, true);
			this.QuantityAndNetWeightTextBox.TabIndex = 0;
			// 
			// CommodityGroupBox
			// 
			this.CommodityGroupBox.Controls.Add(this.CommodityTextBox);
			this.CommodityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 12, true);
			this.CommodityGroupBox.Name = "CommodityGroupBox";
			this.CommodityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 347, true);
			this.CommodityGroupBox.TabIndex = 2;
			this.CommodityGroupBox.TabStop = false;
			this.CommodityGroupBox.Text = "Commodity";
			// 
			// CommodityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommodityTextBox, "US_PPQForm368Box13C");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.US.Business.PPQForm368NoticeOfArrivalData)(null)).US_PPQForm368Box13C)));
			this.CommodityTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CommodityTextBox.Multiline = true;
			this.CommodityTextBox.Name = "CommodityTextBox";
			this.CommodityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 328, true);
			this.CommodityTextBox.TabIndex = 0;
			// 
			// DefaultButton
			// 
			this.DefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 365, true);
			this.DefaultButton.Name = "DefaultButton";
			this.DefaultButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DefaultButton.TabIndex = 3;
			this.DefaultButton.Text = "&Default";
			this.DefaultButton.UseVisualStyleBackColor = true;
			this.DefaultButton.Click += new System.EventHandler(this.DefaultButton_Click);
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(661, 365, true);
			this.CancelAndCloseButton.Name = "CancelAndCloseButton";
			this.CancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAndCloseButton.TabIndex = 5;
			this.CancelAndCloseButton.Text = "&Cancel";
			this.CancelAndCloseButton.UseVisualStyleBackColor = true;
			this.CancelAndCloseButton.Click += new System.EventHandler(this.CancelAndCloseButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 365, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// PPQForm368NoticeOfArrivalDataForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAndCloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 418, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelAndCloseButton);
			this.Controls.Add(this.DefaultButton);
			this.Controls.Add(this.MarksBillOfLadingAndContainerNoGroupBox);
			this.Controls.Add(this.CommodityGroupBox);
			this.Controls.Add(this.QuantityAndNetWeightGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.PPQForm368NoticeOfArrivalData);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimizeBox = false;
			this.Name = "PPQForm368NoticeOfArrivalDataForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PPQ Form 368 Notice Of Arrival Box 13 Data";
			this.Controls.SetChildIndex(this.QuantityAndNetWeightGroupBox, 0);
			this.Controls.SetChildIndex(this.CommodityGroupBox, 0);
			this.Controls.SetChildIndex(this.MarksBillOfLadingAndContainerNoGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DefaultButton, 0);
			this.Controls.SetChildIndex(this.CancelAndCloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MarksBillOfLadingAndContainerNoGroupBox.ResumeLayout(false);
			this.MarksBillOfLadingAndContainerNoGroupBox.PerformLayout();
			this.QuantityAndNetWeightGroupBox.ResumeLayout(false);
			this.QuantityAndNetWeightGroupBox.PerformLayout();
			this.CommodityGroupBox.ResumeLayout(false);
			this.CommodityGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox MarksBillOfLadingAndContainerNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MarksBillOfLadingAndContainerNoGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox QuantityAndNetWeightGroupBox;
		private Enterprise.ZArchitecture.ZTextBox QuantityAndNetWeightTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CommodityGroupBox;
		private Enterprise.ZArchitecture.ZTextBox CommodityTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton DefaultButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelAndCloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}
