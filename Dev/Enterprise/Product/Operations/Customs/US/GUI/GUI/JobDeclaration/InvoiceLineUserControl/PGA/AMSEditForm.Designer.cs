namespace Enterprise.Customs.US.GUI
{
	partial class AMSEditForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OR1GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OR1Grid = new Enterprise.ZArchitecture.ZGrid();
			this.US_CerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AMSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_CommercialDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_ProgramDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_NetWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_OA_Recipient_AddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.US_OA_CertifyingBodyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();

			this.US_EquivalentOrganicCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();

			this.US_USDAOrganicCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.OR1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OR1Grid)).BeginInit();
			this.OR1Grid.SuspendLayout();
			this.AMSGroupBox.SuspendLayout();
			this.US_IntendedUseCodeDropEdit.SuspendLayout();
			this.US_ProgramDropEdit.SuspendLayout();
			this.US_NetWeightDropEdit.SuspendLayout();
			this.US_OA_Recipient_AddressControl.SuspendLayout();
			this.US_OA_CertifyingBodyAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMS);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.OKButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 511, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 30, true);
			this.ButtonPanel.TabIndex = 9;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.IsCaptionOverridden = true;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(738, 4, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "&OK";
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// OR1GroupBox
			// 
			this.OR1GroupBox.Controls.Add(this.OR1Grid);
			this.OR1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 340, true);
			this.OR1GroupBox.Name = "OR1GroupBox";
			this.OR1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 130, true);
			this.OR1GroupBox.TabIndex = 8;
			this.OR1GroupBox.TabStop = false;
			this.OR1GroupBox.Text = "Certificate Details";
			// 
			// OR1Grid
			// 
			this.OR1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OR1Grid, "AMSLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_ProductLabel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_LotNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_LotEntity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).FinalHandlerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_OA_FinalHandler)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).CerFinalHandlerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_OA_CerFinalHandler)));
			this.OR1Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_ProductLabel";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_LotNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_LotEntity";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "FinalHandlerOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_FinalHandler";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CerFinalHandlerOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zAddressDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_CerFinalHandler";
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OR1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OR1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.OR1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.OR1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OR1Grid.GridId = "ee4496a2-0476-4d40-aaf7-7d92f21aa829";
			this.OR1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OR1Grid.LayoutKey = "MO1Grid";
			this.OR1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.OR1Grid.Name = "OR1Grid";
			this.OR1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 90, true);
			this.OR1Grid.TabIndex = 1;
			// 
			// US_CerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_CerNumberTextBox, "US_CerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMS)(null)).US_CerNumber)));
			this.US_CerNumberTextBox.CaptionResourceString = null;
			this.US_CerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 100, true);
			this.US_CerNumberTextBox.Name = "US_CerNumberTextBox";
			this.US_CerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_CerNumberTextBox.TabIndex = 5;
			// 
			// AMSGroupBox
			// 
			this.AMSGroupBox.Controls.Add(this.US_CommercialDescriptionTextBox);
			this.AMSGroupBox.Controls.Add(this.US_IntendedUseCodeDropEdit);
			this.AMSGroupBox.Controls.Add(this.US_ProgramDropEdit);
			this.AMSGroupBox.Controls.Add(this.US_RemarksTextBox);
			this.AMSGroupBox.Controls.Add(this.US_NetWeightDropEdit);
			this.AMSGroupBox.Controls.Add(this.US_OA_Recipient_AddressControl);
			this.AMSGroupBox.Controls.Add(this.US_OA_CertifyingBodyAddressControl);
			this.AMSGroupBox.Controls.Add(this.US_EquivalentOrganicCheckBox);
			this.AMSGroupBox.Controls.Add(this.US_CerNumberTextBox);
			this.AMSGroupBox.Controls.Add(this.US_USDAOrganicCheckBox);
			this.AMSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.AMSGroupBox.Name = "AMSGroupBox";
			this.AMSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 317, true);
			this.AMSGroupBox.TabIndex = 0;
			this.AMSGroupBox.TabStop = false;
			this.AMSGroupBox.Text = "AMS Summary";
			// 
			// US_CommercialDescriptionTextBox
			// 
			this.US_CommercialDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.US_CommercialDescriptionTextBox, "US_CommercialDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMS)(null)).US_CommercialDescription)));
			this.US_CommercialDescriptionTextBox.CaptionResourceString = null;
			this.US_CommercialDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 244, true);
			this.US_CommercialDescriptionTextBox.Name = "US_CommercialDescriptionTextBox";
			this.US_CommercialDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.US_CommercialDescriptionTextBox.TabIndex = 13;
			// 
			// US_IntendedUseCodeDropEdit
			// 
			this.US_IntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_IntendedUseCodeDropEdit, "US_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.AMS)(null)).US_IntendedUseCode)));
			this.US_IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 244, true);
			this.US_IntendedUseCodeDropEdit.Name = "US_IntendedUseCodeDropEdit";
			this.US_IntendedUseCodeDropEdit.PreBoundMaxLength = 7;
			this.US_IntendedUseCodeDropEdit.ShouldResizeByMaxLength = true;
			this.US_IntendedUseCodeDropEdit.ShowDescriptionBox = false;
			this.US_IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.US_IntendedUseCodeDropEdit.TabIndex = 11;
			// 
			// US_ProgramDropEdit
			// 
			this.US_ProgramDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_ProgramDropEdit, "US_Program");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.AMS)(null)).US_Program)));
			this.US_ProgramDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eae06f1c-76e6-40c9-a80e-47f1f47c204e", "Agency Program");
			this.US_ProgramDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 34, true);
			this.US_ProgramDropEdit.Name = "US_ProgramDropEdit";
			this.US_ProgramDropEdit.PreBoundMaxLength = 1;
			this.US_ProgramDropEdit.ShouldResizeByMaxLength = true;
			this.US_ProgramDropEdit.ShowDescriptionBox = false;
			this.US_ProgramDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.US_ProgramDropEdit.TabIndex = 0;
			// 
			// US_RemarksTextBox
			// 
			this.US_RemarksTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.US_RemarksTextBox, "US_Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMS)(null)).US_Remarks)));
			this.US_RemarksTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("05806cca-88ef-4ee7-87da-8bcc6c4beb60", "Remarks and Attestations");
			this.US_RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 281, true);
			this.US_RemarksTextBox.Name = "US_RemarksTextBox";
			this.US_RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 20, true);
			this.US_RemarksTextBox.TabIndex = 12;
			// 
			// US_NetWeightDropEdit
			// 
			this.US_NetWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_NetWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMS)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMS)(null)).US_NetWeightUQ)));
			this.US_NetWeightDropEdit.BindToAmount = "US_NetWeight";
			this.US_NetWeightDropEdit.BindToUnit = "US_NetWeightUQ";
			this.US_NetWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 210, true);
			this.US_NetWeightDropEdit.Name = "US_NetWeightDropEdit";
			this.US_NetWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_NetWeightDropEdit.TabIndex = 10;
			this.US_NetWeightDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// US_OA_Recipient_AddressControl
			// 
			this.US_OA_Recipient_AddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_Recipient_AddressControl, "US_OA_Recipient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMS)(null)).US_OA_Recipient)));
			this.US_OA_Recipient_AddressControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.US_OA_Recipient_AddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cbe94aa2-51ab-45e5-87fb-cf8e7b641963", "Recipient");
			this.US_OA_Recipient_AddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 176, true);
			this.US_OA_Recipient_AddressControl.Name = "US_OA_Recipient_AddressControl";
			this.US_OA_Recipient_AddressControl.PopupCaption = "Recipient";
			this.US_OA_Recipient_AddressControl.ReadOnly = false;
			this.US_OA_Recipient_AddressControl.ShowAddress = false;
			this.US_OA_Recipient_AddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.US_OA_Recipient_AddressControl.TabIndex = 9;
			// 
			// US_OA_CertifyingBodyAddressControl
			// 
			this.US_OA_CertifyingBodyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_CertifyingBodyAddressControl, "US_OA_CertifyingBody");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMS)(null)).US_OA_CertifyingBody)));
			this.US_OA_CertifyingBodyAddressControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.US_OA_CertifyingBodyAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("110c7ee2-c01e-4a2f-b35a-521b7f7dea8f", "Certifying Body");
			this.US_OA_CertifyingBodyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 136, true);
			this.US_OA_CertifyingBodyAddressControl.Name = "US_OA_CertifyingBodyAddressControl";
			this.US_OA_CertifyingBodyAddressControl.PopupCaption = "Certifying Body";
			this.US_OA_CertifyingBodyAddressControl.ReadOnly = false;
			this.US_OA_CertifyingBodyAddressControl.ShowAddress = false;
			this.US_OA_CertifyingBodyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.US_OA_CertifyingBodyAddressControl.TabIndex = 8;
			// 
			// US_EquivalentOrganicCheckBox
			// 
			this.US_EquivalentOrganicCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.US_EquivalentOrganicCheckBox, "US_EquivalentOrganicStandard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.AMS)(null)).US_EquivalentOrganicStandard)));
			this.US_EquivalentOrganicCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_EquivalentOrganicCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_EquivalentOrganicCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 69, true);
			this.US_EquivalentOrganicCheckBox.Name = "US_EquivalentOrganicCheckBox";
			this.US_EquivalentOrganicCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 17, true);
			this.US_EquivalentOrganicCheckBox.TabIndex = 2;
			this.US_EquivalentOrganicCheckBox.Text = "Equivalent Organic Standard?";
			// 
			// US_USDAOrganicCheckBox
			// 
			this.US_USDAOrganicCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.US_USDAOrganicCheckBox, "US_USDAOrganicStandard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.AMS)(null)).US_USDAOrganicStandard)));
			this.US_USDAOrganicCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_USDAOrganicCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_USDAOrganicCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 69, true);
			this.US_USDAOrganicCheckBox.Name = "US_USDAOrganicCheckBox";
			this.US_USDAOrganicCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
			this.US_USDAOrganicCheckBox.TabIndex = 1;
			this.US_USDAOrganicCheckBox.Text = "USDA Organic Standard?";
			// 
			// AMSEditForm
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 541, true);
			this.Controls.Add(this.AMSGroupBox);
			this.Controls.Add(this.OR1GroupBox);
			this.Controls.Add(this.ButtonPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.AMS);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AMSEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.OR1GroupBox, 0);
			this.Controls.SetChildIndex(this.AMSGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.OR1GroupBox.ResumeLayout(false);
			this.OR1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OR1Grid)).EndInit();
			this.OR1Grid.ResumeLayout(false);
			this.OR1Grid.PerformLayout();
			this.AMSGroupBox.ResumeLayout(false);
			this.AMSGroupBox.PerformLayout();
			this.US_IntendedUseCodeDropEdit.ResumeLayout(true);
			this.US_IntendedUseCodeDropEdit.PerformLayout();
			this.US_ProgramDropEdit.ResumeLayout(true);
			this.US_ProgramDropEdit.PerformLayout();
			this.US_NetWeightDropEdit.ResumeLayout(true);
			this.US_NetWeightDropEdit.PerformLayout();
			this.US_OA_Recipient_AddressControl.ResumeLayout(true);
			this.US_OA_Recipient_AddressControl.PerformLayout();
			this.US_OA_CertifyingBodyAddressControl.ResumeLayout(true);
			this.US_OA_CertifyingBodyAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ButtonPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZGroupBox OR1GroupBox;
		private ZArchitecture.ZTextBox US_CerNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox AMSGroupBox;
		private ZArchitecture.GUI.ZCheckBox US_USDAOrganicCheckBox;
		internal ZArchitecture.GUI.ZAddressControl US_OA_CertifyingBodyAddressControl;
		internal ZArchitecture.GUI.ZAddressControl US_OA_Recipient_AddressControl;
		private ZArchitecture.GUI.ZCheckBox US_EquivalentOrganicCheckBox;

		private ZArchitecture.GUI.ZCalcDropEdit US_NetWeightDropEdit;
		private ZArchitecture.ZTextBox US_RemarksTextBox;
		public ZArchitecture.ZGrid OR1Grid;
		internal ZArchitecture.GUI.ZDropEdit US_ProgramDropEdit;
		private ZArchitecture.GUI.ZDropEdit US_IntendedUseCodeDropEdit;
		private ZArchitecture.ZTextBox US_CommercialDescriptionTextBox;
	}
}
