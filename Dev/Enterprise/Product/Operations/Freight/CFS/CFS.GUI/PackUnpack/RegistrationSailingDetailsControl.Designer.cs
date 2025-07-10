using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class RegistrationSailingDetailsControl
	{
		#region Component Designer generated code

		internal ZGroupBox SailingDetailsGroupBox;
		ZCodeFindBox PortOfLoadingCodeFindBox;
		ZGuidFindBox JC_OH_ShippingProviderFindBox;
		internal ZCodeFindBox VesselCodeFindBox;
		ZCodeFindBox PortOfDischargeCodeFindBox;
		ZDateEdit zDateEdit2;
		ZDateEdit zDateEdit1;
		internal ZButton SelectSailingButton;
		internal ZTextBox JX_VoyageTextBox;

		void InitializeComponent()
		{
			this.SailingDetailsGroupBox = new ZGroupBox();
			this.PortOfLoadingCodeFindBox = new ZCodeFindBox();
			this.JC_OH_ShippingProviderFindBox = new ZGuidFindBox();
			this.VesselCodeFindBox = new ZCodeFindBox();
			this.PortOfDischargeCodeFindBox = new ZCodeFindBox();
			this.zDateEdit2 = new ZDateEdit();
			this.zDateEdit1 = new ZDateEdit();
			this.SelectSailingButton = new ZButton();
			this.JX_VoyageTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CFSContainer);
			// 
			// SailingDetailsGroupBox
			// 
			this.SailingDetailsGroupBox.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.JC_OH_ShippingProviderFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.VesselCodeFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.SailingDetailsGroupBox.Controls.Add(this.zDateEdit2);
			this.SailingDetailsGroupBox.Controls.Add(this.zDateEdit1);
			this.SailingDetailsGroupBox.Controls.Add(this.SelectSailingButton);
			this.SailingDetailsGroupBox.Controls.Add(this.JX_VoyageTextBox);
			this.SailingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SailingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SailingDetailsGroupBox.Name = "SailingDetailsGroupBox";
			this.SailingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 190, true);
			this.SailingDetailsGroupBox.TabIndex = 5;
			this.SailingDetailsGroupBox.TabStop = false;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "JC_JA_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_JA_NKPortOfLoading)));
			this.PortOfLoadingCodeFindBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationSailingDetailsControl|6e85fefd-6b35-4f55-aa89-a54124611d0e", "Load", "NK Port Of Loading", "");
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 42, true);
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 2;
			// 
			// JC_OH_ShippingProviderFindBox
			// 
			this.BindingSource.SetBindingMember(this.JC_OH_ShippingProviderFindBox, "JC_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CFSContainer)(null)).JC_OH_ShippingLine)));
			this.JC_OH_ShippingProviderFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 162, true);
			this.JC_OH_ShippingProviderFindBox.Name = "JC_OH_ShippingProviderFindBox";
			this.JC_OH_ShippingProviderFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.JC_OH_ShippingProviderFindBox.TabIndex = 14;
			// 
			// VesselCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "JC_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_JV_NKVessel)));
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 90, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.ShowDescriptionBox = false;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselCodeFindBox.TabIndex = 6;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "JC_JB_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_JB_NKPortOfDischarge)));
			this.PortOfDischargeCodeFindBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationSailingDetailsControl|5038c4d4-70c3-44b0-960c-c672d08d646b", "Disch.", "Port Of Discharge", "");
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 66, true);
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 4;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "JC_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_JB_E_ARV)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationSailingDetailsControl|3f04dcba-ec08-40d2-b2ac-51abd0fd198b", "ETA", "Estimated Arrival Date", "");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 138, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 12;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "JC_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CFSContainer)(null)).JC_JA_E_DEP)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationSailingDetailsControl|f5972f47-d257-4ea7-872c-8db974456cc4", "ETD", "Estimated Departure Date", "");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 138, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 10;
			// 
			// SelectSailingButton
			// 
			this.SelectSailingButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("RegistrationSailingDetailsControl|7ef3e93d-24f1-4a97-8a81-91ebf8d8abaa", "<Select Sailing Schedule>");
			this.SelectSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 16, true);
			this.SelectSailingButton.Name = "SelectSailingButton";
			this.SelectSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 23, true);
			this.SelectSailingButton.TabIndex = 0;
			this.SelectSailingButton.Click += new EventHandler(this.SelectSailingButton_Click);
			// 
			// JX_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JX_VoyageTextBox, "JC_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CFSContainer)(null)).JC_JV_VoyageFlight)));
			this.JX_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 114, true);
			this.JX_VoyageTextBox.Name = "JX_VoyageTextBox";
			this.JX_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.JX_VoyageTextBox.TabIndex = 8;
			// 
			// RegistrationSailingDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingDetailsGroupBox);
			this.Name = "RegistrationSailingDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 190, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingDetailsGroupBox.ResumeLayout(false);
			this.SailingDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
