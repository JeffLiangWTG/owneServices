namespace Enterprise.Customs.GUI
{
	partial class CusRefTariffVersionForm
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
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TariffVersionCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffVersionDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffVersionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TariffVersionEffectiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryCodeFindBox.SuspendLayout();
			this.TariffVersionDetailsGroupBox.SuspendLayout();
			this.TariffVersionEffectiveDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 221, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.TariffVersionDetailsGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 194, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 194, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 194, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 221, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefTariffVersion);
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "CRT_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTariffVersion)(null)).CRT_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 19, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.PreBoundMaxLength = 2;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.CountryCodeFindBox.TabIndex = 0;
			// 
			// TariffVersionCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffVersionCodeTextBox, "CRT_Version");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTariffVersion)(null)).CRT_Version)));
			this.TariffVersionCodeTextBox.CaptionResourceString = null;
			this.TariffVersionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 43, true);
			this.TariffVersionCodeTextBox.Name = "TariffVersionCodeTextBox";
			this.TariffVersionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.TariffVersionCodeTextBox.TabIndex = 1;
			// 
			// TariffVersionDescriptionTextBox
			// 
			this.TariffVersionDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TariffVersionDescriptionTextBox, "CRT_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTariffVersion)(null)).CRT_Description)));
			this.TariffVersionDescriptionTextBox.CaptionResourceString = null;
			this.TariffVersionDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 67, true);
			this.TariffVersionDescriptionTextBox.Multiline = true;
			this.TariffVersionDescriptionTextBox.Name = "TariffVersionDescriptionTextBox";
			this.TariffVersionDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TariffVersionDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 95, true);
			this.TariffVersionDescriptionTextBox.TabIndex = 2;
			// 
			// TariffVersionDetailsGroupBox
			// 
			this.TariffVersionDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("173E490C-3D59-4572-98B9-36673B966AD0", "Tariff Version Details");
			this.TariffVersionDetailsGroupBox.Controls.Add(this.TariffVersionCodeTextBox);
			this.TariffVersionDetailsGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.TariffVersionDetailsGroupBox.Controls.Add(this.TariffVersionDescriptionTextBox);
			this.TariffVersionDetailsGroupBox.Controls.Add(this.TariffVersionEffectiveDateEdit);
			this.TariffVersionDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TariffVersionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TariffVersionDetailsGroupBox.Name = "TariffVersionDetailsGroupBox";
			this.TariffVersionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 194, true);
			this.TariffVersionDetailsGroupBox.TabIndex = 0;
			this.TariffVersionDetailsGroupBox.TabStop = false;
			// 
			// TariffVersionEffectiveDateEdit
			// 
			this.TariffVersionEffectiveDateEdit.AllowDrop = true;
			this.TariffVersionEffectiveDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TariffVersionEffectiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.TariffVersionEffectiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TariffVersionEffectiveDateEdit, "CRT_EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.CusRefTariffVersion)(null)).CRT_EffectiveDate)));
			this.TariffVersionEffectiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 166, true);
			this.TariffVersionEffectiveDateEdit.Name = "TariffVersionEffectiveDateEdit";
			this.TariffVersionEffectiveDateEdit.TabIndex = 3;
			// 
			// CusRefTariffVersionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 277, true);
			this.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefTariffVersion);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 316, true);
			this.Name = "CusRefTariffVersionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CusRefTariffVersionForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.TariffVersionDetailsGroupBox.ResumeLayout(false);
			this.TariffVersionDetailsGroupBox.PerformLayout();
			this.TariffVersionEffectiveDateEdit.ResumeLayout(true);
			this.TariffVersionEffectiveDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZGroupBox TariffVersionDetailsGroupBox;
		protected ZArchitecture.ZTextBox TariffVersionCodeTextBox;
		protected ZArchitecture.ZTextBox TariffVersionDescriptionTextBox;
		protected ZArchitecture.GUI.ZDateEdit TariffVersionEffectiveDateEdit;
		ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;

		#endregion
	}
}
