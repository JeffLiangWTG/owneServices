using System.ComponentModel;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	public partial class CusClassificationUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new Container();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.nZCClassificationFindBox = new NZCClassFindBox();
			this.infoTabControl = new ZArchitecture.GUI.ZTemplateTabControl();
			this.permitTabPage = new ZArchitecture.GUI.ZTabPage();
			this.permitGrid = new ZArchitecture.ZGrid();
			this.prohibitTabPage = new ZArchitecture.GUI.ZTabPage();
			this.prohibitsGrid = new ZArchitecture.ZGrid();
			this.otherInfoTabPage = new ZArchitecture.GUI.ZTabPage();
			this.otherInfoGrid = new ZArchitecture.ZGrid();
			this.cC_PartsOfClassificationNZCClassFindBox = new NZCClassFindBox();
			this.cC_ConcessionCodeCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.tariffNumFindBox = new Universal.GUI.TariffFindBox();
			this.partsOfClassificationFindBox = new Universal.GUI.TariffFindBox();
			this.concessionDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.BaseClassificationGroupBox.SuspendLayout();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.infoTabControl.SuspendLayout();
			this.permitTabPage.SuspendLayout();
			((ISupportInitialize)(this.permitGrid)).BeginInit();
			this.prohibitTabPage.SuspendLayout();
			((ISupportInitialize)(this.prohibitsGrid)).BeginInit();
			this.otherInfoTabPage.SuspendLayout();
			((ISupportInitialize)(this.otherInfoGrid)).BeginInit();
			this.tariffNumFindBox.SuspendLayout();
			this.partsOfClassificationFindBox.SuspendLayout();
			this.concessionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.tariffNumFindBox);
			this.BaseClassificationGroupBox.Controls.Add(this.nZCClassificationFindBox);
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 175, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.nZCClassificationFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffNumFindBox, 0);
			// 
			// LookupCodeTextBox
			// 
			this.LookupCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.LookupCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 24, true);
			this.LookupCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 148, true);
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 80, true);
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 32, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.MasterFiles.CusClassification);
			// 
			// NZCClassificationFindBox
			// 
			this.nZCClassificationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nZCClassificationFindBox, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).CC_TariffNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).ClassificationList);
			this.nZCClassificationFindBox.BindToList = "ClassificationList";
			this.nZCClassificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 52, true);
			this.nZCClassificationFindBox.MaxTariffLength = 14;
			this.nZCClassificationFindBox.Name = "NZCClassificationFindBox";
			this.nZCClassificationFindBox.PreBoundMaxLength = 15;
			this.nZCClassificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.nZCClassificationFindBox.TabIndex = 1;
			// 
			// tariffNumFindBox
			// 
			this.tariffNumFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tariffNumFindBox, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).CC_TariffNum);
			this.tariffNumFindBox.ErrorForUnsupportedCountry = null;
			this.tariffNumFindBox.GetEffectiveDate = null;
			this.tariffNumFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 52, true);
			this.tariffNumFindBox.Name = "tariffNumFindBox";
			this.tariffNumFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.tariffNumFindBox.ParentType = null;
			this.tariffNumFindBox.PreBoundMaxLength = 15;
			this.tariffNumFindBox.SelectNomenclatureModes = null;
			this.tariffNumFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.tariffNumFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.tariffNumFindBox.TabIndex = 6;
			this.tariffNumFindBox.TariffType = null;
			// 
			// InfoTabControl
			// 
			this.infoTabControl.Controls.Add(this.permitTabPage);
			this.infoTabControl.Controls.Add(this.prohibitTabPage);
			this.infoTabControl.Controls.Add(this.otherInfoTabPage);
			this.infoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 206, true);
			this.infoTabControl.Name = "InfoTabControl";
			this.infoTabControl.SelectedIndex = 0;
			this.infoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 146, true);
			this.infoTabControl.TabIndex = 3;
			// 
			// PermitTabPage
			// 
			this.permitTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|23a7d582-dd25-4ed2-a189-3f951fc433c2", "Permit Codes");
			this.permitTabPage.Controls.Add(this.permitGrid);
			this.permitTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.permitTabPage.Name = "PermitTabPage";
			this.permitTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 119, true);
			this.permitTabPage.TabIndex = 0;
			// 
			// PermitGrid
			// 
			this.permitGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.permitGrid, "PermitCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).PermitCodes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PermitCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).PermitCodes)).SyncRoot)).ZO_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PermitCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).PermitCodes)).SyncRoot)).ZO_CodeList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PermitCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).PermitCodes)).SyncRoot)).ZO_Data);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PermitCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).PermitCodes)).SyncRoot)).ZO_Description);
			this.permitGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|7091d096-2d1a-47a3-9bdb-8727591f7215", "Code");
			zDropEditColumnStyleInfo4.ColumnName = "ZO_Code";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|ddaa22c4-04dd-4663-8f1d-6b884a5a488d", "Permit No");
			zTextBoxColumnStyleInfo6.ColumnName = "ZO_Data";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|bba09a2f-9695-477d-9322-5339f4ad0777", "Description");
			zTextBoxColumnStyleInfo7.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.permitGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.permitGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.permitGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.permitGrid.GridId = "8e76d401-d71f-4383-8515-83e402c55f58";
			this.permitGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.permitGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.permitGrid.LayoutKey = "PermitGrid";
			this.permitGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.permitGrid.Name = "PermitGrid";
			this.permitGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 119, true);
			this.permitGrid.TabIndex = 0;
			// 
			// ProhibitTabPage
			// 
			this.prohibitTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|c48a6dfe-0521-492e-a571-7794b1e08682", "Prohibited Codes");
			this.prohibitTabPage.Controls.Add(this.prohibitsGrid);
			this.prohibitTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.prohibitTabPage.Name = "ProhibitTabPage";
			this.prohibitTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 119, true);
			this.prohibitTabPage.TabIndex = 1;
			// 
			// ProhibitsGrid
			// 
			this.prohibitsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.prohibitsGrid, "ProhibitedCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).ProhibitedCodes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ProhibitedCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).ProhibitedCodes)).SyncRoot)).ZO_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ProhibitedCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).ProhibitedCodes)).SyncRoot)).ZO_CodeList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ProhibitedCode)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).ProhibitedCodes)).SyncRoot)).ZO_Description);
			this.prohibitsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|3734c528-91be-4e33-93be-860ce689bcdc", "Code");
			zDropEditColumnStyleInfo5.ColumnName = "ZO_Code";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|41f55928-a970-4e01-9a80-13636ff60fe1", "Description");
			zTextBoxColumnStyleInfo8.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.prohibitsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.prohibitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.prohibitsGrid.GridId = "7e0e11b3-4fea-44ec-81e3-12d0ebb44de7";
			this.prohibitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.prohibitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.prohibitsGrid.LayoutKey = "zGrid1";
			this.prohibitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.prohibitsGrid.Name = "ProhibitsGrid";
			this.prohibitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 119, true);
			this.prohibitsGrid.TabIndex = 0;
			// 
			// OtherInfoTabPage
			// 
			this.otherInfoTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|90aadc4b-f8f2-4c24-9e07-4e9f80c81819", "Other Infos");
			this.otherInfoTabPage.Controls.Add(this.otherInfoGrid);
			this.otherInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.otherInfoTabPage.Name = "OtherInfoTabPage";
			this.otherInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 119, true);
			this.otherInfoTabPage.TabIndex = 2;
			// 
			// OtherInfoGrid
			// 
			this.otherInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.otherInfoGrid, "OtherInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).OtherInfos);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LineOtherInfo)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).OtherInfos)).SyncRoot)).ZO_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LineOtherInfo)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).OtherInfos)).SyncRoot)).ZO_CodeList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LineOtherInfo)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).OtherInfos)).SyncRoot)).ZO_Data);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LineOtherInfo)(((System.Collections.IList)(((Business.MasterFiles.CusClassification)(null)).OtherInfos)).SyncRoot)).ZO_Description);
			this.otherInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|8ff8b8b7-af2b-406a-894a-25dd9af81030", "Code");
			zDropEditColumnStyleInfo6.ColumnName = "ZO_Code";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|502ca781-eb36-487c-81cd-683333155afe", "Data");
			zTextBoxColumnStyleInfo9.ColumnName = "ZO_Data";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CusClassificationUserControl|0fa9b604-e34f-49a3-a273-deb16f65246d", "Description");
			zTextBoxColumnStyleInfo10.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.otherInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.otherInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.otherInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.otherInfoGrid.GridId = "0e6df1d7-040a-42cf-8003-6d1e093c05dc";
			this.otherInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.otherInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.otherInfoGrid.LayoutKey = "OtherInfoGrid";
			this.otherInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.otherInfoGrid.Name = "OtherInfoGrid";
			this.otherInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 119, true);
			this.otherInfoGrid.TabIndex = 0;
			// 
			// CC_PartsOfClassificationNZCClassFindBox
			// 
			this.cC_PartsOfClassificationNZCClassFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.cC_PartsOfClassificationNZCClassFindBox, "CC_PartsOfClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).CC_PartsOfClassification);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).ClassificationList);
			this.cC_PartsOfClassificationNZCClassFindBox.BindToList = "ClassificationList";
			this.cC_PartsOfClassificationNZCClassFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 360, true);
			this.cC_PartsOfClassificationNZCClassFindBox.MaxTariffLength = 14;
			this.cC_PartsOfClassificationNZCClassFindBox.Name = "CC_PartsOfClassificationNZCClassFindBox";
			this.cC_PartsOfClassificationNZCClassFindBox.PreBoundMaxLength = 15;
			this.cC_PartsOfClassificationNZCClassFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 21, true);
			this.cC_PartsOfClassificationNZCClassFindBox.TabIndex = 5;
			// 
			// CC_ConcessionCodeCodeFindBox
			// 
			this.cC_ConcessionCodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.cC_ConcessionCodeCodeFindBox, "CC_ConcessionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).CC_ConcessionCode);
			this.cC_ConcessionCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 181, true);
			this.cC_ConcessionCodeCodeFindBox.Name = "CC_ConcessionCodeCodeFindBox";
			this.cC_ConcessionCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 21, true);
			this.cC_ConcessionCodeCodeFindBox.TabIndex = 2;
			// 
			// partsOfClassificationFindBox
			// 
			this.partsOfClassificationFindBox.AllowDrop = true;
			this.partsOfClassificationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.partsOfClassificationFindBox, "CC_PartsOfClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).CC_PartsOfClassification);
			this.partsOfClassificationFindBox.ErrorForUnsupportedCountry = null;
			this.partsOfClassificationFindBox.GetEffectiveDate = null;
			this.partsOfClassificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 360, true);
			this.partsOfClassificationFindBox.Name = "partsOfClassificationFindBox";
			this.partsOfClassificationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.partsOfClassificationFindBox.ParentType = null;
			this.partsOfClassificationFindBox.PreBoundMaxLength = 15;
			this.partsOfClassificationFindBox.SelectNomenclatureModes = null;
			this.partsOfClassificationFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.partsOfClassificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 21, true);
			this.partsOfClassificationFindBox.TabIndex = 7;
			this.partsOfClassificationFindBox.TariffType = null;
			// 
			// concessionDropEdit
			// 
			this.concessionDropEdit.AllowDrop = true;
			this.concessionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.concessionDropEdit, "CC_ConcessionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.MasterFiles.CusClassification)(null)).CC_ConcessionCode);
			this.concessionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 181, true);
			this.concessionDropEdit.Name = "concessionDropEdit";
			this.concessionDropEdit.ShowDescriptionBox = false;
			this.concessionDropEdit.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.concessionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.concessionDropEdit.TabIndex = 8;
			// 
			// CusClassificationUserControl
			// 
			this.Controls.Add(this.concessionDropEdit);
			this.Controls.Add(this.partsOfClassificationFindBox);
			this.Controls.Add(this.cC_PartsOfClassificationNZCClassFindBox);
			this.Controls.Add(this.cC_ConcessionCodeCodeFindBox);
			this.Controls.Add(this.infoTabControl);
			this.Name = "CusClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 392, true);
			this.Controls.SetChildIndex(this.BaseClassificationGroupBox, 0);
			this.Controls.SetChildIndex(this.infoTabControl, 0);
			this.Controls.SetChildIndex(this.cC_ConcessionCodeCodeFindBox, 0);
			this.Controls.SetChildIndex(this.cC_PartsOfClassificationNZCClassFindBox, 0);
			this.Controls.SetChildIndex(this.partsOfClassificationFindBox, 0);
			this.Controls.SetChildIndex(this.concessionDropEdit, 0);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.infoTabControl.ResumeLayout(false);
			this.permitTabPage.ResumeLayout(false);
			((ISupportInitialize)(this.permitGrid)).EndInit();
			this.prohibitTabPage.ResumeLayout(false);
			((ISupportInitialize)(this.prohibitsGrid)).EndInit();
			this.otherInfoTabPage.ResumeLayout(false);
			((ISupportInitialize)(this.otherInfoGrid)).EndInit();
			this.tariffNumFindBox.ResumeLayout(true);
			this.tariffNumFindBox.PerformLayout();
			this.partsOfClassificationFindBox.ResumeLayout(true);
			this.partsOfClassificationFindBox.PerformLayout();
			this.concessionDropEdit.ResumeLayout(true);
			this.concessionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
