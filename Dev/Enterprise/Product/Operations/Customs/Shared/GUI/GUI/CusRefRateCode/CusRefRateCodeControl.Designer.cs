using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class CusRefRateCodeControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BasicInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RateCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RateTypeGropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BasicInformationGroupBox.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.RateTypeGropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefRateCode);
			// 
			// BasicInformationGroupBox
			// 
			this.BasicInformationGroupBox.BackColor = System.Drawing.SystemColors.Window;
			this.BasicInformationGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("763650ad-60ac-4196-9f42-29c9d53ee1bb", "Basic Information");
			this.BasicInformationGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.BasicInformationGroupBox.Controls.Add(this.RateCodeTextBox);
			this.BasicInformationGroupBox.Controls.Add(this.DescriptionTextBox);
			this.BasicInformationGroupBox.Controls.Add(this.RateTypeGropEdit);
			this.BasicInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BasicInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicInformationGroupBox.Name = "BasicInformationGroupBox";
			this.BasicInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 335, true);
			this.BasicInformationGroupBox.TabIndex = 1;
			this.BasicInformationGroupBox.TabStop = false;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "CR7_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefRateCode)(null)).CR7_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 33, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.CountryCodeFindBox.TabIndex = 0;
			// 
			// RateCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RateCodeTextBox, "CR7_RateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefRateCode)(null)).CR7_RateCode)));
			this.RateCodeTextBox.CaptionResourceString = null;
			this.RateCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 59, true);
			this.RateCodeTextBox.Name = "RateCodeTextBox";
			this.RateCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.RateCodeTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CR7_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefRateCode)(null)).CR7_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 85, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 147, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// RateTypeGropEdit
			// 
			this.RateTypeGropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RateTypeGropEdit, "CR7_RateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.CusRefRateCode)(null)).CR7_RateType)));
			this.RateTypeGropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 238, true);
			this.RateTypeGropEdit.Name = "RateTypeGropEdit";
			this.RateTypeGropEdit.ShouldResizeByMaxLength = true;
			this.RateTypeGropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.RateTypeGropEdit.TabIndex = 3;
			// 
			// CusRefRateCodeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BasicInformationGroupBox);
			this.Name = "CusRefRateCodeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 335, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BasicInformationGroupBox.ResumeLayout(false);
			this.BasicInformationGroupBox.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.RateTypeGropEdit.ResumeLayout(true);
			this.RateTypeGropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZGroupBox BasicInformationGroupBox;
		ZCodeFindBox CountryCodeFindBox;
		ZTextBox RateCodeTextBox;
		ZTextBox DescriptionTextBox;
		ZDropEdit RateTypeGropEdit;

		#endregion
	}
}
