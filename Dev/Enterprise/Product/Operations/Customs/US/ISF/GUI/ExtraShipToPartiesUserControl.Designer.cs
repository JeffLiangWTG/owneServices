using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	partial class ExtraShipToPartiesUserControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.extraShipToPartiesGrid = new ZArchitecture.ZGrid();
			this.ExtraShipToPartyDocAddressControl = new ISFDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.extraShipToPartiesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CusISFHeaderExtraShipToPartyAddresses);
			// 
			// ExtraShipToPartiesGrid
			// 
			this.extraShipToPartiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.extraShipToPartiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ISFDocAddress)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.ISFDocAddress)(null)).E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ISFDocAddress)(null)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ISFDocAddress)(null)).Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ISFDocAddress)(null)).E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ISFDocAddress)(null)).Organisation.Addresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_GovRegNumType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_GovRegNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).ContactDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ISFDocAddress)(null)).Organisation.Contacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Mobile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ISFDocAddress)(null)).E2_Email)));
			this.extraShipToPartiesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ExtraShipToPartiesUserControl|854df756-3797-4a72-a26a-d8d7580a8db4", "Organization");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			zGuidDropEditColumnStyleInfo1.BindToList = "Organisation+Addresses";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "E2_CompanyName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "E2_Address1";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "E2_Address2";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "E2_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "E2_State";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "E2_City";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(41);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "E2_Postcode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "E2_GovRegNumType";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.ISF.GUI.Res.GetData("ExtraShipToPartiesUserControl|e0ee8a3e-7760-40c3-add5-08be1046a86f", "Gov. Reg. Num.");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "E2_GovRegNum";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.US.ISF.GUI.Res.GetData("ExtraShipToPartiesUserControl|e0ee8a3e-7760-40c3-add5-08be1046a86f", "Gov. Reg. Num.");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zMultiControlColumnStyleInfo1.BindToList = "Organisation+Contacts";
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "E2_Contact";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ContactDataFieldType";
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "E2_Mobile";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(54);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "E2_Phone";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "E2_Fax";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "E2_Email";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(49);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.extraShipToPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.extraShipToPartiesGrid.GridId = "3fb0f6bf-ef0c-444c-a7d9-13435720919e";
			this.extraShipToPartiesGrid.CopySelectedRowsAllowed = true;
			this.extraShipToPartiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extraShipToPartiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.extraShipToPartiesGrid.LayoutKey = "ExtraShipToPartiesGrid";
			this.extraShipToPartiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.extraShipToPartiesGrid.Name = "ExtraShipToPartiesGrid";
			this.extraShipToPartiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 420, true);
			this.extraShipToPartiesGrid.TabIndex = 3;
			// 
			// ExtraShipToPartyDocAddressControl
			// 
			this.ExtraShipToPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExtraShipToPartyDocAddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((Business.ISFDocAddress)(null)))));
			this.ExtraShipToPartyDocAddressControl.BindToContacts = "Organisation+Contacts";
			this.ExtraShipToPartyDocAddressControl.BindToOrganisations = "Lookups+OrgHeader_List";
			this.ExtraShipToPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ExtraShipToPartiesUserControl|4392DEAD-BE3E-47C2-A0D7-EA833C5F7A3F", "Extra Ship To Party");
			this.ExtraShipToPartyDocAddressControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExtraShipToPartyDocAddressControl, false);
			this.ExtraShipToPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 420, true);
			this.ExtraShipToPartyDocAddressControl.Name = "ExtraShipToPartyDocAddressControl";
			this.ExtraShipToPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ExtraShipToPartyDocAddressControl.TabIndex = 4;
			// 
			// ExtraShipToPartiesUserControl
			// 
			this.Controls.Add(this.extraShipToPartiesGrid);
			this.Controls.Add(this.ExtraShipToPartyDocAddressControl);
			this.Name = "ExtraShipToPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 602, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.extraShipToPartiesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		ZArchitecture.ZGrid extraShipToPartiesGrid;
		public ISFDocAddressControl ExtraShipToPartyDocAddressControl;
	}
}
