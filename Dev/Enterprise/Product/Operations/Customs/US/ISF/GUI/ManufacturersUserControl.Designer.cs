using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	partial class ManufacturersUserControl
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
			this.manufacturersGrid = new ZArchitecture.ZGrid();
			this.ManufacturerDocAddressControl = new ISFDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.manufacturersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CusISFHeaderManufacturerAddresses);
			// 
			// ManufacturersGrid
			// 
			this.manufacturersGrid.AllowNavigation = false;
			this.manufacturersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.manufacturersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.JobDocAddress)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((MasterFiles.Business.JobDocAddress)(null)).E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((MasterFiles.Business.JobDocAddress)(null)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.JobDocAddress)(null)).Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((MasterFiles.Business.JobDocAddress)(null)).E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.JobDocAddress)(null)).Organisation.Addresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_GovRegNumType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_GovRegNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).ContactDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.JobDocAddress)(null)).Organisation.Contacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Mobile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.JobDocAddress)(null)).E2_Email)));
			this.manufacturersGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ManufacturersUserControl|19c46a31-f7e1-472b-af79-cdd2ca4ad7ed", "Manufacturer");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
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
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.ISF.GUI.Res.GetData("ManufacturersUserControl|99da4147-9910-4fa1-a0eb-7c4d62d99c9b", "Gov. Reg. Num.");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "E2_GovRegNum";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.US.ISF.GUI.Res.GetData("ManufacturersUserControl|99da4147-9910-4fa1-a0eb-7c4d62d99c9b", "Gov. Reg. Num.");
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
			this.manufacturersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.manufacturersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.manufacturersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.manufacturersGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.manufacturersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.manufacturersGrid.GridId = "b60fdae6-538d-42bd-b2a0-d757e31daa50";
			this.manufacturersGrid.CopySelectedRowsAllowed = true;
			this.manufacturersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.manufacturersGrid.LayoutKey = "ManufacturersGrid";
			this.manufacturersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.manufacturersGrid.Name = "ManufacturersGrid";
			this.manufacturersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 182, true);
			this.manufacturersGrid.TabIndex = 3;
			// 
			// ManufacturerDocAddressControl
			// 
			this.ManufacturerDocAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ManufacturerDocAddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((MasterFiles.Business.JobDocAddress)(null)))));
			this.ManufacturerDocAddressControl.BindToContacts = "Organisation+Contacts";
			this.ManufacturerDocAddressControl.BindToOrganisations = "Lookups+OrgHeader_List";
			this.ManufacturerDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ManufacturersUserControl|330bd527-c1da-457e-92cb-69570707e054", "Manufacturer");
			this.ManufacturerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 3, true);
			this.ManufacturerDocAddressControl.Name = "ManufacturerDocAddressControl";
			this.ManufacturerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ManufacturerDocAddressControl.TabIndex = 4;
			// 
			// ManufacturersUserControl
			// 
			this.Controls.Add(this.manufacturersGrid);
			this.Controls.Add(this.ManufacturerDocAddressControl);
			this.Name = "ManufacturersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 191, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.manufacturersGrid)).EndInit();
			this.ResumeLayout(false);
		}

		ZArchitecture.ZGrid manufacturersGrid;
		public ISFDocAddressControl ManufacturerDocAddressControl;
	}
}
