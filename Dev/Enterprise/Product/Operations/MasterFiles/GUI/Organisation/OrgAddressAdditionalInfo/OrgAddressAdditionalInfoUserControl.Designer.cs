namespace Enterprise.MasterFiles.GUI
{
	partial class OrgAddressAdditionalInfoUserControl
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
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.AdditionalInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AdditionalInfoGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AdditionalInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).BeginInit();
            this.AdditionalInfoGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
            // 
            // AdditionalInfoGroupBox
            // 
            this.AdditionalInfoGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aef192eb-fb37-40f0-862f-3b560c87ff42", "Address Additional Information");
            this.AdditionalInfoGroupBox.Controls.Add(this.AdditionalInfoGrid);
            this.AdditionalInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AdditionalInfoGroupBox.Name = "AdditionalInfoGroupBox";
            this.AdditionalInfoGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
            this.AdditionalInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 227, true);
            this.AdditionalInfoGroupBox.TabIndex = 0;
            this.AdditionalInfoGroupBox.TabStop = false;
            // 
            // AdditionalInfoGrid
            // 
            this.AdditionalInfoGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AdditionalInfoGrid, "ActiveOrAllAddresses.AdditionalInfos");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AdditionalInfos)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgAddressAdditionalInfo)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AdditionalInfos)).SyncRoot)).OAI_IsPrimary)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddressAdditionalInfo)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgAddress)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ActiveOrAllAddresses)).SyncRoot)).AdditionalInfos)).SyncRoot)).OAI_AdditionalInfo)));
            this.AdditionalInfoGrid.CaptionVisible = false;
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bd24e15f-d525-425d-ba7b-0e91cba46526", "Main");
            zCheckBoxColumnStyleInfo1.ColumnName = "OAI_IsPrimary";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("86e6e0d5-0a1d-4167-b54f-963896e6b312", "Additional Information");
            zTextBoxColumnStyleInfo1.ColumnName = "OAI_AdditionalInfo";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
            this.AdditionalInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AdditionalInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalInfoGrid.GridId = "06b37f25-36c6-463e-b843-4ce28628be5c";
            this.AdditionalInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AdditionalInfoGrid.LayoutKey = "AdditionalInfoGrid";
            this.AdditionalInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 21, true);
            this.AdditionalInfoGrid.Name = "AdditionalInfoGrid";
            this.AdditionalInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 198, true);
            this.AdditionalInfoGrid.TabIndex = 0;
            // 
            // OrgAddressAdditionalInfoUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.AdditionalInfoGroupBox);
            this.Name = "OrgAddressAdditionalInfoUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 227, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AdditionalInfoGroupBox.ResumeLayout(false);
            this.AdditionalInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).EndInit();
            this.AdditionalInfoGrid.ResumeLayout(false);
            this.AdditionalInfoGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AdditionalInfoGroupBox;
		private ZArchitecture.ZGrid AdditionalInfoGrid;
	}
}
