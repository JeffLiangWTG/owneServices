using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class AnotherShipmentWizardPage : WizardPage
	{
		private ZArchitecture.ZGrid zGrid1;
		private ZPanel zPanel1;
		private ZCheckBox zCheckBox1;
		private MasterFiles.GUI.ZOrganisationFindBox zOrganisationFindBox3;
		private ZArchitecture.ZLabel TopInfoLabel;
		private ZArchitecture.ZLabel BottomInfoLabel;
		private Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.zGrid1 = new ZArchitecture.ZGrid();
			this.zPanel1 = new ZPanel();
			this.TopInfoLabel = new ZArchitecture.ZLabel();
			this.zOrganisationFindBox3 = new MasterFiles.GUI.ZOrganisationFindBox();
			this.zCheckBox1 = new ZCheckBox();
			this.BottomInfoLabel = new ZArchitecture.ZLabel();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CoLoadWizardShipment);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "CoLoadShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CoLoadWizardShipment)(null)).CoLoadShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PackUnpackShipment)(((System.Collections.IList)(((Business.CoLoadWizardShipment)(null)).CoLoadShipments)).SyncRoot)).JS_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.PackUnpackShipment)(((System.Collections.IList)(((Business.CoLoadWizardShipment)(null)).CoLoadShipments)).SyncRoot)).ConsigneePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.PackUnpackShipment)(((System.Collections.IList)(((Business.CoLoadWizardShipment)(null)).CoLoadShipments)).SyncRoot)).TotalOuterPacks)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JS_HouseBill";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("AnotherShipmentWizardPage|a941df99-8d7e-4c12-9c91-6191d551345d", "Consignee");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ConsigneePK";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("AnotherShipmentWizardPage|0c3a6fc2-9c6f-4e3a-be3e-d6b3674e43e1", "Total Outer Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "TotalOuterPacks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = ControlDpiScalingHelper.NewScaledSize(424, 128, true);
			this.zGrid1.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.TopInfoLabel);
			this.zPanel1.Controls.Add(this.zOrganisationFindBox3);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = ControlDpiScalingHelper.NewScaledSize(424, 88, true);
			this.zPanel1.TabIndex = 1;
			// 
			// TopInfoLabel
			// 
			this.TopInfoLabel.CaptionResourceString = Res.GetData("AnotherShipmentWizardPage|b517b724-0ec6-4258-9c8e-7dafc01eaa2b", "The following shipments are ready to be added to the Co-Load Master:");
			this.TopInfoLabel.Location = ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.TopInfoLabel.Name = "TopInfoLabel";
			this.TopInfoLabel.Size = ControlDpiScalingHelper.NewScaledSize(400, 24, true);
			this.TopInfoLabel.TabIndex = 14;
			this.TopInfoLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zOrganisationFindBox3
			// 
			this.BindingSource.SetBindingMember(this.zOrganisationFindBox3, "CW_OH_CoLoadForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CoLoadWizardShipment)(null)).CW_OH_CoLoadForwarder)));
			this.zOrganisationFindBox3.CaptionResourceString = Res.GetData("AnotherShipmentWizardPage|c80e86d6-8a07-4ed5-adaf-1d98eee5015c", "Co-Loader");
			this.zOrganisationFindBox3.Location = ControlDpiScalingHelper.NewScaledPoint(152, 40, true);
			this.zOrganisationFindBox3.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zOrganisationFindBox3.Name = "zOrganisationFindBox3";
			this.zOrganisationFindBox3.Size = ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zOrganisationFindBox3.TabIndex = 13;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "CW_CreateAnotherShipment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.CoLoadWizardShipment)(null)).CW_CreateAnotherShipment)));
			this.zCheckBox1.CaptionResourceString = Res.GetData("AnotherShipmentWizardPage|4db26fe1-63cd-432f-b1c0-2a9f1db75d8e", "Add Another Shipment");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = ControlDpiScalingHelper.NewScaledPoint(248, 312, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zCheckBox1.TabIndex = 2;
			// 
			// BottomInfoLabel
			// 
			this.BottomInfoLabel.Location = ControlDpiScalingHelper.NewScaledPoint(8, 248, true);
			this.BottomInfoLabel.Name = "BottomInfoLabel";
			this.BottomInfoLabel.Size = ControlDpiScalingHelper.NewScaledSize(408, 40, true);
			this.BottomInfoLabel.TabIndex = 15;
			this.BottomInfoLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AnotherShipmentWizardPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomInfoLabel);
			this.Controls.Add(this.zCheckBox1);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.zPanel1);
			this.Name = "AnotherShipmentWizardPage";
			this.Size = ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.zGrid1)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
