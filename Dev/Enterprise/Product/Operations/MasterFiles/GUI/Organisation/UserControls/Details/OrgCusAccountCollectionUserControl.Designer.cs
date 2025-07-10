using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	partial class OrgCusAccountCollectionUserControl
	{
		private System.ComponentModel.Container components = null;

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Component Designer generated code

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrgCusAccountCollectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgCusAccountCollectionsGrid)).BeginInit();
			this.OrgCusAccountCollectionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCusAccountCollection);
			// 
			// OrgCusAccountCollectionsGrid
			// 
			this.OrgCusAccountCollectionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgCusAccountCollectionsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).CZ_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).CZ_Account)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).CZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).CZ_Issuer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).DecryptedPassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).CZ_ReportingPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).Provider.CZ_IssuerFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(null)).CZ_RepresentativeID)));
			this.OrgCusAccountCollectionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f664a69-7c41-4068-88ee-d8538262cb52", "Code");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CZ_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0f790b83-b652-48e4-b167-50eb934327b1", "Account");
			zTextBoxColumnStyleInfo1.ColumnName = "CZ_Account";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F9B67965-A45C-46DD-856F-25D300B6A65A", "Account Type");
			zDropEditColumnStyleInfo2.ColumnName = "CZ_Type";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("47c2f577-6d41-4e07-959d-ae4ff22e16ab", "Issuer");
			zMultiControlColumnStyleInfo1.ColumnName = "CZ_Issuer";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "Provider+CZ_IssuerFieldType";
			zMultiControlColumnStyleInfo1.ModuleID = ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dfb47c92-821c-4924-bb9f-6f860dceb980", "Decrypted Password");
			zTextBoxColumnStyleInfo2.ColumnName = "DecryptedPassword";
			zTextBoxColumnStyleInfo2.PasswordChar = '*';
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("{E24F8536-CBA1-40AA-AED2-4F0CCD6B21B3}", "Reporting Period");
			zDropEditColumnStyleInfo4.ColumnName = "CZ_ReportingPeriod";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5578719d-dac8-48fd-b254-349293ed2e3b", "Declarant/Representative ID");
			zTextBoxColumnStyleInfo3.ColumnName = "CZ_RepresentativeID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrgCusAccountCollectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrgCusAccountCollectionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgCusAccountCollectionsGrid.GridId = "fa99abc5-17e6-45bd-a634-25513e7de4db";
			this.OrgCusAccountCollectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgCusAccountCollectionsGrid.LayoutKey = "OrgCusAccountCollectionsGrid";
			this.OrgCusAccountCollectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgCusAccountCollectionsGrid.Name = "OrgCusAccountCollectionsGrid";
			this.OrgCusAccountCollectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 416, true);
			this.OrgCusAccountCollectionsGrid.TabIndex = 0;
			// 
			// OrgCusAccountCollectionUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgCusAccountCollectionsGrid);
			this.Name = "OrgCusAccountCollectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgCusAccountCollectionsGrid)).EndInit();
			this.OrgCusAccountCollectionsGrid.ResumeLayout(false);
			this.OrgCusAccountCollectionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.ZGrid OrgCusAccountCollectionsGrid;
	}
}
