namespace Enterprise.MasterFiles.GUI
{
	partial class GlbCompanySignatureCredentialUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SignatureCredentialsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SignatureCredentialsGrid.BindingContextChanged += SignatureCredentialsGrid_BindingContextChanged;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SignatureCredentialsGrid)).BeginInit();
			this.SignatureCredentialsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SignatureCredentialsGrid
			// 
			this.SignatureCredentialsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SignatureCredentialsGrid, "SignatureCredentials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).SignatureCredentials)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanySignatureCredential)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).SignatureCredentials)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanySignatureCredential)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).SignatureCredentials)).SyncRoot)).CurrentDecryptedPassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanySignatureCredential)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbCompany)(null)).SignatureCredentials)).SyncRoot)).GP_PasswordStatus)));
			this.SignatureCredentialsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zTextBoxColumnStyleInfo2.ColumnName = "CurrentDecryptedPassword";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zTextBoxColumnStyleInfo3.ColumnName = "GP_PasswordStatus";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SignatureCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SignatureCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SignatureCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SignatureCredentialsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SignatureCredentialsGrid.GridId = "d35b96df-79f1-483f-8770-3aa78d5599f0";
			this.SignatureCredentialsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SignatureCredentialsGrid.LayoutKey = "SignatureCredentialsGrid";
			this.SignatureCredentialsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SignatureCredentialsGrid.MaximumRows = 1;
			this.SignatureCredentialsGrid.Name = "SignatureCredentialsGrid";
			this.SignatureCredentialsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 219, true);
			this.SignatureCredentialsGrid.TabIndex = 3;
			// 
			// GlbCompany_SignatureCredentials
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SignatureCredentialsGrid);
			this.Name = "GlbCompany_SignatureCredentials";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 219, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SignatureCredentialsGrid)).EndInit();
			this.SignatureCredentialsGrid.ResumeLayout(false);
			this.SignatureCredentialsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.ZGrid SignatureCredentialsGrid;
	}
}
