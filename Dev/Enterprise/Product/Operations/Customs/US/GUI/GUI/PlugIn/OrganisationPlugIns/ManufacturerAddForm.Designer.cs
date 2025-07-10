
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI
{
	partial class ManufacturerAddForm
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
		private new void InitializeComponent()
		{
			this.FirmNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StreetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZipTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendAddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_OA_AddressDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Label = new Enterprise.ZArchitecture.ZLabel();
			this.StreetLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ZipLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_OA_AddressDetailsGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.SendQueryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 24, true);
			this.MainStatusBar.TabIndex = 18;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ManufacturerAddMessageData);
			// 
			// FirmNameTextBox
			// 
			this.FirmNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FirmNameTextBox, "US_FirmName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_FirmName)));
			this.FirmNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 31, true);
			this.FirmNameTextBox.Name = "FirmNameTextBox";
			this.FirmNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.FirmNameTextBox.TabIndex = 3;
			// 
			// StreetTextBox
			// 
			this.StreetTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StreetTextBox, "US_Street");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_Street)));
			this.StreetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 57, true);
			this.StreetTextBox.Name = "StreetTextBox";
			this.StreetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.StreetTextBox.TabIndex = 5;
			// 
			// CityTextBox
			// 
			this.CityTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CityTextBox, "US_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_City)));
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 83, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.CityTextBox.TabIndex = 7;
			// 
			// ZipTextBox
			// 
			this.ZipTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ZipTextBox, "US_Zip");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_Zip)));
			this.ZipTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 109, true);
			this.ZipTextBox.Name = "ZipTextBox";
			this.ZipTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.ZipTextBox.TabIndex = 9;
			// 
			// SendAddButton
			// 
			this.SendAddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 161, true);
			this.SendAddButton.Name = "SendAddButton";
			this.SendAddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.SendAddButton.TabIndex = 14;
			this.SendAddButton.Text = "Send MID &Add";
			this.SendAddButton.UseVisualStyleBackColor = true;
			this.SendAddButton.Click += new System.EventHandler(this.SendAddButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 161, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelButton2.TabIndex = 17;
			this.CancelButton2.Text = "&Cancel";
			this.CancelButton2.UseVisualStyleBackColor = true;
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton2_Click);
			// 
			// MIDTextBox
			// 
			this.MIDTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MIDTextBox, "US_MID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_MID)));
			this.MIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 135, true);
			this.MIDTextBox.Name = "MIDTextBox";
			this.MIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.MIDTextBox.TabIndex = 13;
			// 
			// US_OA_AddressDetailsLabel
			// 
			this.US_OA_AddressDetailsLabel.AutoSize = true;
			this.US_OA_AddressDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.US_OA_AddressDetailsLabel.Name = "US_OA_AddressDetailsLabel";
			this.US_OA_AddressDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.US_OA_AddressDetailsLabel.TabIndex = 0;
			this.US_OA_AddressDetailsLabel.Text = "Selected Address:";
			// 
			// Label
			// 
			this.Label.AutoSize = true;
			this.Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 34, true);
			this.Label.Name = "Label";
			this.Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.Label.TabIndex = 2;
			this.Label.Text = "Firm Name:";
			// 
			// StreetLabel
			// 
			this.StreetLabel.AutoSize = true;
			this.StreetLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 60, true);
			this.StreetLabel.Name = "StreetLabel";
			this.StreetLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.StreetLabel.TabIndex = 4;
			this.StreetLabel.Text = "Street:";
			// 
			// CityLabel
			// 
			this.CityLabel.AutoSize = true;
			this.CityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 86, true);
			this.CityLabel.Name = "CityLabel";
			this.CityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.CityLabel.TabIndex = 6;
			this.CityLabel.Text = "City:";
			// 
			// ZipLabel
			// 
			this.ZipLabel.AutoSize = true;
			this.ZipLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 112, true);
			this.ZipLabel.Name = "ZipLabel";
			this.ZipLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 13, true);
			this.ZipLabel.TabIndex = 8;
			this.ZipLabel.Text = "Zip:";
			// 
			// CountryLabel
			// 
			this.CountryLabel.AutoSize = true;
			this.CountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 112, true);
			this.CountryLabel.Name = "CountryLabel";
			this.CountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.CountryLabel.TabIndex = 10;
			this.CountryLabel.Text = "Country:";
			// 
			// MIDLabel
			// 
			this.MIDLabel.AutoSize = true;
			this.MIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 138, true);
			this.MIDLabel.Name = "MIDLabel";
			this.MIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.MIDLabel.TabIndex = 12;
			this.MIDLabel.Text = "MID:";
			// 
			// CountryTextBox
			// 
			this.CountryTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CountryTextBox, "US_Country");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_Country)));
			this.CountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 109, true);
			this.CountryTextBox.Name = "CountryTextBox";
			this.CountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.CountryTextBox.TabIndex = 11;
			// 
			// US_OA_AddressDetailsGuidDropEdit
			// 
			this.US_OA_AddressDetailsGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_AddressDetailsGuidDropEdit, "US_OA_AddressDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ManufacturerAddMessageData)(null)).US_OA_AddressDetails)));
			this.US_OA_AddressDetailsGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 6, true);
			this.US_OA_AddressDetailsGuidDropEdit.Name = "US_OA_AddressDetailsGuidDropEdit";
			this.US_OA_AddressDetailsGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.US_OA_AddressDetailsGuidDropEdit.TabIndex = 1;
			// 
			// SendQueryButton
			// 
			this.SendQueryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 161, true);
			this.SendQueryButton.Name = "SendQueryButton";
			this.SendQueryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.SendQueryButton.TabIndex = 16;
			this.SendQueryButton.Text = "Send MID &Query";
			this.SendQueryButton.UseVisualStyleBackColor = true;
			this.SendQueryButton.Click += new System.EventHandler(this.SendQueryButton_Click);
			// 
			// SendUpdateButton
			//
			this.SendUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 161, true);
			this.SendUpdateButton.Name = "SendUpdateButton";
			this.SendUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.SendUpdateButton.TabIndex = 15;
			this.SendUpdateButton.Text = "Send Postal Code &Update";
			this.SendUpdateButton.UseVisualStyleBackColor = true;
			this.SendUpdateButton.Click += new System.EventHandler(this.SendUpdateButton_Click);
			// 
			// ManufacturerAddForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 214, true);
			this.Controls.Add(this.SendQueryButton);
			this.Controls.Add(this.SendUpdateButton);
			this.Controls.Add(this.US_OA_AddressDetailsGuidDropEdit);
			this.Controls.Add(this.CountryTextBox);
			this.Controls.Add(this.MIDLabel);
			this.Controls.Add(this.CountryLabel);
			this.Controls.Add(this.ZipLabel);
			this.Controls.Add(this.CityLabel);
			this.Controls.Add(this.StreetLabel);
			this.Controls.Add(this.Label);
			this.Controls.Add(this.US_OA_AddressDetailsLabel);
			this.Controls.Add(this.MIDTextBox);
			this.Controls.Add(this.CancelButton2);
			this.Controls.Add(this.SendAddButton);
			this.Controls.Add(this.ZipTextBox);
			this.Controls.Add(this.CityTextBox);
			this.Controls.Add(this.FirmNameTextBox);
			this.Controls.Add(this.StreetTextBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.ManufacturerAddMessageData);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ManufacturerAddForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ManufacturerAddForm";
			this.Controls.SetChildIndex(this.StreetTextBox, 0);
			this.Controls.SetChildIndex(this.FirmNameTextBox, 0);
			this.Controls.SetChildIndex(this.CityTextBox, 0);
			this.Controls.SetChildIndex(this.ZipTextBox, 0);
			this.Controls.SetChildIndex(this.SendAddButton, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.MIDTextBox, 0);
			this.Controls.SetChildIndex(this.US_OA_AddressDetailsLabel, 0);
			this.Controls.SetChildIndex(this.Label, 0);
			this.Controls.SetChildIndex(this.StreetLabel, 0);
			this.Controls.SetChildIndex(this.CityLabel, 0);
			this.Controls.SetChildIndex(this.ZipLabel, 0);
			this.Controls.SetChildIndex(this.CountryLabel, 0);
			this.Controls.SetChildIndex(this.MIDLabel, 0);
			this.Controls.SetChildIndex(this.CountryTextBox, 0);
			this.Controls.SetChildIndex(this.US_OA_AddressDetailsGuidDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendQueryButton, 0);
			this.Controls.SetChildIndex(this.SendUpdateButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox FirmNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox StreetTextBox;
		private Enterprise.ZArchitecture.ZTextBox CityTextBox;
		private Enterprise.ZArchitecture.ZTextBox ZipTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton SendAddButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
		private Enterprise.ZArchitecture.ZTextBox MIDTextBox;
		private Enterprise.ZArchitecture.ZLabel US_OA_AddressDetailsLabel;
		private Enterprise.ZArchitecture.ZLabel Label;
		private Enterprise.ZArchitecture.ZLabel StreetLabel;
		private Enterprise.ZArchitecture.ZLabel CityLabel;
		private Enterprise.ZArchitecture.ZLabel ZipLabel;
		private Enterprise.ZArchitecture.ZLabel CountryLabel;
		private Enterprise.ZArchitecture.ZLabel MIDLabel;
		private Enterprise.ZArchitecture.ZTextBox CountryTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit US_OA_AddressDetailsGuidDropEdit;
		internal ZArchitecture.GUI.ZButton SendQueryButton;
		internal ZArchitecture.GUI.ZButton SendUpdateButton;
	}
}
