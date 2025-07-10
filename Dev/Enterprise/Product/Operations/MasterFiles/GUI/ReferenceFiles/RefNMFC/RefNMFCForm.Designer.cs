namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Summary description for RefNMFCForm.
	/// </summary>
	public partial class RefNMFCForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/// 

		private new void InitializeComponent()
		{
			this.FN_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ArticleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FN_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FN_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FN_ClassTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FN_ItemNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArticleGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 324, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ArticleGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 297, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefNMFC);
			// 
			// FN_IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FN_IsActiveCheckBox, "FN_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefNMFC)(null)).FN_IsActive)));
			this.FN_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FN_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 13, true);
			this.FN_IsActiveCheckBox.Name = "FN_IsActiveCheckBox";
			this.FN_IsActiveCheckBox.ReadOnly = true;
			this.FN_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 22, true);
			this.FN_IsActiveCheckBox.TabIndex = 8;
			// 
			// ArticleGroupBox
			// 
			this.ArticleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefNMFCForm|a38fecfe-c57b-4611-87df-e008fd794e4c", "Article");
			this.ArticleGroupBox.Controls.Add(this.FN_CodeTextBox);
			this.ArticleGroupBox.Controls.Add(this.FN_IsActiveCheckBox);
			this.ArticleGroupBox.Controls.Add(this.FN_DescriptionTextBox);
			this.ArticleGroupBox.Controls.Add(this.FN_ClassTextBox);
			this.ArticleGroupBox.Controls.Add(this.FN_ItemNoTextBox);
			this.ArticleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.ArticleGroupBox.Name = "ArticleGroupBox";
			this.ArticleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 269, true);
			this.ArticleGroupBox.TabIndex = 9;
			this.ArticleGroupBox.TabStop = false;
			// 
			// FN_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FN_CodeTextBox, "FN_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefNMFC)(null)).FN_Code)));
			this.FN_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 14, true);
			this.FN_CodeTextBox.Name = "FN_CodeTextBox";
			this.FN_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.FN_CodeTextBox.TabIndex = 1;
			// 
			// FN_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.FN_DescriptionTextBox, "FN_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefNMFC)(null)).FN_Description)));
			this.FN_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 91, true);
			this.FN_DescriptionTextBox.Multiline = true;
			this.FN_DescriptionTextBox.Name = "FN_DescriptionTextBox";
			this.FN_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 172, true);
			this.FN_DescriptionTextBox.TabIndex = 7;
			// 
			// FN_ClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.FN_ClassTextBox, "FN_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefNMFC)(null)).FN_Class)));
			this.FN_ClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 65, true);
			this.FN_ClassTextBox.Name = "FN_ClassTextBox";
			this.FN_ClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.FN_ClassTextBox.TabIndex = 5;
			// 
			// FN_ItemNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.FN_ItemNoTextBox, "FN_ItemNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefNMFC)(null)).FN_ItemNo)));
			this.FN_ItemNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 39, true);
			this.FN_ItemNoTextBox.Name = "FN_ItemNoTextBox";
			this.FN_ItemNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.FN_ItemNoTextBox.TabIndex = 3;
			// 
			// RefNMFCForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 380, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefNMFCForm|7a583e3e-e8a2-4e47-b899-a06e041e4516", "NMFC");
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefNMFC);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 416, true);
			this.Name = "RefNMFCForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArticleGroupBox.ResumeLayout(false);
			this.ArticleGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ArticleGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox FN_IsActiveCheckBox;
		private Enterprise.ZArchitecture.ZTextBox FN_DescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox FN_ClassTextBox;
		private Enterprise.ZArchitecture.ZTextBox FN_ItemNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox FN_CodeTextBox;
	}
}
