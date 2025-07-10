namespace Enterprise.Freight.Agency.GUI
{
	partial class CustomsDetailsControl
	{
		void InitializeComponent()
		{
			this.DetailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			// 
			// DetailsButton
			// 
			this.DetailsButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CustomsDetailsControl|5e3459bf-d8f5-44fa-8ffa-f0b8bf236cc0", "Details");
			this.DetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 6, true);
			this.DetailsButton.Name = "DetailsButton";
			this.DetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.DetailsButton.TabIndex = 7;
			this.DetailsButton.UseVisualStyleBackColor = true;
			this.DetailsButton.Click += new System.EventHandler(this.DetailsButton_Click);
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).MessageStatus)));
			this.MessageStatusTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|38f9bd33-7f7d-4c09-928b-17546dbcd9ca", "Message Status");
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 34, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 20, true);
			this.MessageStatusTextBox.TabIndex = 6;
			// 
			// CustomsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsStatusTextBox, "CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).CustomsStatus)));
			this.CustomsStatusTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|14b3be36-5bee-4870-8c81-8d69b6f89bf2", "Customs Status");
			this.CustomsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 8, true);
			this.CustomsStatusTextBox.Name = "CustomsStatusTextBox";
			this.CustomsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 20, true);
			this.CustomsStatusTextBox.TabIndex = 5;
			// 
			// CustomsDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsButton);
			this.Controls.Add(this.MessageStatusTextBox);
			this.Controls.Add(this.CustomsStatusTextBox);
			this.Name = "CustomsDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZButton DetailsButton;
		private ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZArchitecture.ZTextBox CustomsStatusTextBox;
	}
}
