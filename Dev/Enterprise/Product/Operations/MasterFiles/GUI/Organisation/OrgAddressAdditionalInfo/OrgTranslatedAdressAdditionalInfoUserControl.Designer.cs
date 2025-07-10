namespace Enterprise.MasterFiles.GUI
{
	partial class OrgTranslatedAdressAdditionalInfoUserControl
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
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.TranslatedAdditionalInfoGrid = new Enterprise.ZArchitecture.ZGrid();
            this.InfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TranslatedAdditionalInfoGrid)).BeginInit();
            this.TranslatedAdditionalInfoGrid.SuspendLayout();
            this.InfoGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
            // 
            // TranslatedAdditionalInfoGrid
            // 
            this.TranslatedAdditionalInfoGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.TranslatedAdditionalInfoGrid, "ActiveOrAllAddresses.CurrentTranslatedAddress.AdditionalInfoWrapperCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).CurrentTranslatedAddress.AdditionalInfoWrapperCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgTranslatedAddressAdditionalInfoWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).CurrentTranslatedAddress.AdditionalInfoWrapperCollection)).SyncRoot)).IsPrimary)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTranslatedAddressAdditionalInfoWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).CurrentTranslatedAddress.AdditionalInfoWrapperCollection)).SyncRoot)).AdditionalInfo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTranslatedAddressAdditionalInfoWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).CurrentTranslatedAddress.AdditionalInfoWrapperCollection)).SyncRoot)).TranslatedAdditionalInfo)));
            this.TranslatedAdditionalInfoGrid.CaptionVisible = false;
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1e888b6a-d29a-4ec7-b3c6-fa5b7e7b1d19", "Main");
            zCheckBoxColumnStyleInfo1.ColumnName = "IsPrimary";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f5843d08-4ff8-4a09-912f-7233c8833017", "Additional Information");
            zTextBoxColumnStyleInfo1.ColumnName = "AdditionalInfo";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9b3b3ce8-ac05-4f29-9f92-dd8f3cf7ff76", "Translation");
            zTextBoxColumnStyleInfo2.ColumnName = "TranslatedAdditionalInfo";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
            this.TranslatedAdditionalInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.TranslatedAdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.TranslatedAdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.TranslatedAdditionalInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TranslatedAdditionalInfoGrid.GridId = "03f78b6b-6b91-4b0a-9612-09dec41c1883";
            this.TranslatedAdditionalInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TranslatedAdditionalInfoGrid.LayoutKey = "TranslatedAdditionalInfoGrid";
            this.TranslatedAdditionalInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 21, true);
            this.TranslatedAdditionalInfoGrid.Name = "TranslatedAdditionalInfoGrid";
            this.TranslatedAdditionalInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 241, true);
            this.TranslatedAdditionalInfoGrid.TabIndex = 0;
            // 
            // InfoGroupBox
            // 
            this.InfoGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("348bc6d0-733f-49e6-af47-651c94c0f880", "Translated Address Additional Information");
            this.InfoGroupBox.Controls.Add(this.TranslatedAdditionalInfoGrid);
            this.InfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.InfoGroupBox.Name = "InfoGroupBox";
            this.InfoGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
            this.InfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 270, true);
            this.InfoGroupBox.TabIndex = 2;
            this.InfoGroupBox.TabStop = false;
            // 
            // OrgTranslatedAdressAdditionalInfoUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.InfoGroupBox);
            this.Name = "OrgTranslatedAdressAdditionalInfoUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 270, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TranslatedAdditionalInfoGrid)).EndInit();
            this.TranslatedAdditionalInfoGrid.ResumeLayout(false);
            this.TranslatedAdditionalInfoGrid.PerformLayout();
            this.InfoGroupBox.ResumeLayout(false);
            this.InfoGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid TranslatedAdditionalInfoGrid;
		private ZArchitecture.GUI.ZGroupBox InfoGroupBox;
	}
}
