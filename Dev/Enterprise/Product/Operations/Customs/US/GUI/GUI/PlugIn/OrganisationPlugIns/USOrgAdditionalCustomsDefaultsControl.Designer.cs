using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USOrgAdditionalCustomsDefaultsControl
	{
		internal ZDropEdit ReconIndicatorDropEdit;
		internal ZDropEdit FirstSaleDropEdit;
		internal ZCheckBox NAFTAReconIndicatorCheckBox;
		ZDropEdit aESUltimateConsigneeTypeDropEdit;
		ZDropEdit entryTypeDropEdit;

		void InitializeComponent()
		{
			this.ReconIndicatorDropEdit = new ZDropEdit();
			this.FirstSaleDropEdit = new ZDropEdit();
			this.aESUltimateConsigneeTypeDropEdit = new ZDropEdit();
			this.entryTypeDropEdit = new ZDropEdit();
			this.NAFTAReconIndicatorCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReconIndicatorDropEdit.SuspendLayout();
			this.FirstSaleDropEdit.SuspendLayout();
			this.aESUltimateConsigneeTypeDropEdit.SuspendLayout();
			this.entryTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USOrgSupplierBuyerLinkAddInfo);
			// 
			// ReconIndicatorDropEdit
			// 
			this.ReconIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReconIndicatorDropEdit, "ZO_OtherReconIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.USOrgSupplierBuyerLinkAddInfo)(null)).ZO_OtherReconIndicator)));
			this.ReconIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgAdditionalCustomsDefaultsControl|ZO_OtherReconIndicator", "Recon. Issue");
			this.ReconIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 67, true);
			this.ReconIndicatorDropEdit.Name = "ReconIndicatorDropEdit";
			this.ReconIndicatorDropEdit.PreBoundMaxLength = 2;
			this.ReconIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.ReconIndicatorDropEdit.TabIndex = 2;
			// 
			// FirstSaleDropEdit
			// 
			this.FirstSaleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstSaleDropEdit, "ZO_FirstSale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.USOrgSupplierBuyerLinkAddInfo)(null)).ZO_FirstSale)));
			this.FirstSaleDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgAdditionalCustomsDefaultsControl|ZO_FirstSale", "First Sale");
			this.FirstSaleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 40, true);
			this.FirstSaleDropEdit.Name = "FirstSaleDropEdit";
			this.FirstSaleDropEdit.PreBoundMaxLength = 1;
			this.FirstSaleDropEdit.ShowDescriptionBox = false;
			this.FirstSaleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.FirstSaleDropEdit.TabIndex = 1;
			// 
			// AESUltimateConsigneeTypeDropEdit
			// 
			this.aESUltimateConsigneeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aESUltimateConsigneeTypeDropEdit, "ZO_AESUltConsigneeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.USOrgSupplierBuyerLinkAddInfo)(null)).ZO_AESUltConsigneeType)));
			this.aESUltimateConsigneeTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fac75fb1-b070-4838-8ffe-99935df0ebc0", "AES Ult. Consign Type");
			this.aESUltimateConsigneeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 11, true);
			this.aESUltimateConsigneeTypeDropEdit.Name = "AESUltimateConsigneeTypeDropEdit";
			this.aESUltimateConsigneeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.aESUltimateConsigneeTypeDropEdit.TabIndex = 0;
			// 
			// EntryTypeDropEdit
			// 
			this.entryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.entryTypeDropEdit, "ZO_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.USOrgSupplierBuyerLinkAddInfo)(null)).ZO_EntryType)));
			this.entryTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("46DF2906-8536-4B20-AB98-88286FEEAE7D", "Entry Type");
			this.entryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 114, true);
			this.entryTypeDropEdit.Name = "EntryTypeDropEdit";
			this.entryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.entryTypeDropEdit.TabIndex = 4;
			// 
			// NAFTAReconIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NAFTAReconIndicatorCheckBox, "ZO_NAFTAReconIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.USOrgSupplierBuyerLinkAddInfo)(null)).ZO_NAFTAReconIndicator)));
			this.NAFTAReconIndicatorCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgAdditionalCustomsDefaultsControl|ZO_NAFTAReconIndicator", "FTA Recon.");
			this.NAFTAReconIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NAFTAReconIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NAFTAReconIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 93, true);
			this.NAFTAReconIndicatorCheckBox.Name = "NAFTAReconIndicatorCheckBox";
			this.NAFTAReconIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.NAFTAReconIndicatorCheckBox.TabIndex = 3;
			this.NAFTAReconIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// USOrgAdditionalCustomsDefaultsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.aESUltimateConsigneeTypeDropEdit);
			this.Controls.Add(this.FirstSaleDropEdit);
			this.Controls.Add(this.ReconIndicatorDropEdit);
			this.Controls.Add(this.NAFTAReconIndicatorCheckBox);
			this.Controls.Add(this.entryTypeDropEdit);
			this.Name = "USOrgAdditionalCustomsDefaultsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReconIndicatorDropEdit.ResumeLayout(true);
			this.ReconIndicatorDropEdit.PerformLayout();
			this.FirstSaleDropEdit.ResumeLayout(true);
			this.FirstSaleDropEdit.PerformLayout();
			this.aESUltimateConsigneeTypeDropEdit.ResumeLayout(true);
			this.aESUltimateConsigneeTypeDropEdit.PerformLayout();
			this.entryTypeDropEdit.ResumeLayout(true);
			this.entryTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
