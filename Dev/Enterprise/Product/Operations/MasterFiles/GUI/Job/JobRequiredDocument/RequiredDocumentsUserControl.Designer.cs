using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RequiredDocumentsUserControl : ZUserControl
	{
		ZGroupBox RequiredDocumentsGroupBox;
		protected internal ZPanel DocumentsDetailsPanel;
		ZDropEdit DocumentPeriodDropEdit;
		ZDropEdit DocumentTypeDropEdit;
		ZDateEdit ReturnedToShipperDateEdit;
		ZDateEdit RcvdFromBrokerDateEdit;
		ZDateTimeOffsetEdit ReceivedDateEdit;
		ZDateEdit ValidToDateEdit;
		ZDateEdit SentToBrokerDateEdit;
		ZTextBox DocNumberTextBox;
		ZOrganisationFindBox DocumentOwnerFindBox;
		ZGrid DocumentsGrid;
		ZTextBox JL_OutturnCommentTextBox;
		ZDropEdit DocUsageDropEdit;
		ZCheckBox CreditControlDocCheckBox;
		ZGroupBox AttributesGroupBox;
		internal ZGrid AttributesGrid;
		internal ZButton DISDataButton;

		void InitializeComponent()
		{
			var zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			var zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			var zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			var zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			var zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.RequiredDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocumentsDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DISDataButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttributesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttributesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocUsageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JL_OutturnCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReturnedToShipperDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RcvdFromBrokerDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReceivedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.ValidToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SentToBrokerDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CreditControlDocCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DocNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentOwnerFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RequiredDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			this.DocumentsGrid.SuspendLayout();
			this.DocumentsDetailsPanel.SuspendLayout();
			this.AttributesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).BeginInit();
			this.AttributesGrid.SuspendLayout();
			this.DocUsageDropEdit.SuspendLayout();
			this.DocumentPeriodDropEdit.SuspendLayout();
			this.DocumentTypeDropEdit.SuspendLayout();
			this.ReturnedToShipperDateEdit.SuspendLayout();
			this.RcvdFromBrokerDateEdit.SuspendLayout();
			this.ReceivedDateEdit.SuspendLayout();
			this.ValidToDateEdit.SuspendLayout();
			this.SentToBrokerDateEdit.SuspendLayout();
			this.DocumentOwnerFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IHaveRequiredDocuments);
			// 
			// RequiredDocumentsGroupBox
			// 
			this.RequiredDocumentsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.RequiredDocumentsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RequiredDocumentsUserControl|ba750e73-7898-42d7-93f0-a155725c2c8a", "Document Tracking");
			this.RequiredDocumentsGroupBox.Controls.Add(this.DocumentsGrid);
			this.RequiredDocumentsGroupBox.Controls.Add(this.DocumentsDetailsPanel);
			this.RequiredDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequiredDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequiredDocumentsGroupBox.Name = "RequiredDocumentsGroupBox";
			this.RequiredDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 222, true);
			this.RequiredDocumentsGroupBox.TabIndex = 0;
			this.RequiredDocumentsGroupBox.TabStop = false;
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentsGrid, "RequiredDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocDescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DateReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_ValidToDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocUsage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_RN_NKRelatedCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_OriginalDocRequired)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_CreditControlDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_SntToCustomsBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_RcvFromCustomsBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_ReturnToShipper)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_OH_DocumentOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocumentNotes)));
			this.DocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EQ_DocCategory";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "EQ_DocType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "EQ_DocDescriptionMultilingual";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "EQ_DocPeriod";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F6E3C96F-493F-46A7-89AE-C55FC70666C3", "Date Received (Local)");
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "EQ_DateReceived";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("14BA0AB4-CFA9-4A78-94C4-E46CB99E6278", "Date Received (UTC)");
			zDateEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo6.ColumnName = "EQ_DateReceivedUtc";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "EQ_ValidToDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "EQ_DocNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "EQ_DocUsage";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "EQ_RN_NKRelatedCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.ColumnName = "EQ_OriginalDocRequired";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "EQ_CreditControlDoc";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "EQ_SntToCustomsBroker";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "EQ_RcvFromCustomsBroker";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.ColumnName = "EQ_ReturnToShipper";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "EQ_OH_DocumentOwner";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "EQ_DocumentNotes";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.DocumentsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.DocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.DocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.DocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.DocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.DocumentsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGrid.GridId = "8ddbccb3-98d4-4d78-97ca-ec7943743af7";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "DocumentsGrid";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 83, true);
			this.DocumentsGrid.TabIndex = 0;
			// 
			// DocumentsDetailsPanel
			// 
			this.DocumentsDetailsPanel.AutoScroll = true;
			this.DocumentsDetailsPanel.Controls.Add(this.DISDataButton);
			this.DocumentsDetailsPanel.Controls.Add(this.AttributesGroupBox);
			this.DocumentsDetailsPanel.Controls.Add(this.DocUsageDropEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.JL_OutturnCommentTextBox);
			this.DocumentsDetailsPanel.Controls.Add(this.DocumentPeriodDropEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.DocumentTypeDropEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.ReturnedToShipperDateEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.RcvdFromBrokerDateEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.ReceivedDateEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.ValidToDateEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.SentToBrokerDateEdit);
			this.DocumentsDetailsPanel.Controls.Add(this.CreditControlDocCheckBox);
			this.DocumentsDetailsPanel.Controls.Add(this.DocNumberTextBox);
			this.DocumentsDetailsPanel.Controls.Add(this.DocumentOwnerFindBox);
			this.DocumentsDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DocumentsDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 98, true);
			this.DocumentsDetailsPanel.Name = "DocumentsDetailsPanel";
			this.DocumentsDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 122, true);
			this.DocumentsDetailsPanel.TabIndex = 0;
			// 
			// DISDataButton
			// 
			this.DISDataButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 91, true);
			this.DISDataButton.Name = "DISDataButton";
			this.DISDataButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DISDataButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.DISDataButton.TabIndex = 14;
			this.DISDataButton.Click += new System.EventHandler(this.DISDataButton_Click);
			// 
			// AttributesGroupBox
			// 
			this.AttributesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AttributesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RequiredDocumentsUserControl|a3cde648-00b1-4f56-80ed-a6d453072779", "Attributes");
			this.AttributesGroupBox.Controls.Add(this.AttributesGrid);
			this.AttributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(968, -1, true);
			this.AttributesGroupBox.Name = "AttributesGroupBox";
			this.AttributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 118, true);
			this.AttributesGroupBox.TabIndex = 12;
			this.AttributesGroupBox.TabStop = false;
			// 
			// AttributesGrid
			// 
			this.AttributesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttributesGrid, "RequiredDocuments.Attributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).Attributes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocAttrib)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).Attributes)).SyncRoot)).D0_AttribName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocAttrib)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).Attributes)).SyncRoot)).D0_AttribDisplayValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocAttrib)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).Attributes)).SyncRoot)).AttribValueFieldType)));
			this.AttributesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "D0_AttribName";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("462785ec-d6c7-49b1-8c30-132bfd86f615", "Value", "", "");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zMultiControlColumnStyleInfo1.ColumnName = "D0_AttribDisplayValue";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "AttribValueFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.AttributesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.AttributesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.AttributesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGrid.GridId = "6301c8a3-69f5-41fe-aee8-90930dd10150";
			this.AttributesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttributesGrid.LayoutKey = "AttributesGrid";
			this.AttributesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AttributesGrid.Name = "AttributesGrid";
			this.AttributesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 101, true);
			this.AttributesGrid.TabIndex = 13;
			// 
			// DocUsageDropEdit
			// 
			this.DocUsageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocUsageDropEdit, "RequiredDocuments.EQ_DocUsage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocUsage)));
			this.DocUsageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 48, true);
			this.DocUsageDropEdit.Name = "DocUsageDropEdit";
			this.DocUsageDropEdit.PreBoundMaxLength = 3;
			this.DocUsageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 17, true);
			this.DocUsageDropEdit.TabIndex = 2;
			// 
			// JL_OutturnCommentTextBox
			// 
			this.JL_OutturnCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JL_OutturnCommentTextBox, "RequiredDocuments.EQ_DocumentNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocumentNotes)));
			this.JL_OutturnCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JL_OutturnCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 48, true);
			this.JL_OutturnCommentTextBox.Multiline = true;
			this.JL_OutturnCommentTextBox.Name = "JL_OutturnCommentTextBox";
			this.JL_OutturnCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 61, true);
			this.JL_OutturnCommentTextBox.TabIndex = 11;
			// 
			// DocumentPeriodDropEdit
			// 
			this.DocumentPeriodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentPeriodDropEdit, "RequiredDocuments.EQ_DocPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocPeriod)));
			this.DocumentPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 4, true);
			this.DocumentPeriodDropEdit.Name = "DocumentPeriodDropEdit";
			this.DocumentPeriodDropEdit.PreBoundMaxLength = 3;
			this.DocumentPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.DocumentPeriodDropEdit.TabIndex = 9;
			// 
			// DocumentTypeDropEdit
			// 
			this.DocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentTypeDropEdit, "RequiredDocuments.EQ_DocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocType)));
			this.DocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 4, true);
			this.DocumentTypeDropEdit.Name = "DocumentTypeDropEdit";
			this.DocumentTypeDropEdit.PreBoundMaxLength = 3;
			this.DocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.DocumentTypeDropEdit.TabIndex = 0;
			// 
			// ReturnedToShipperDateEdit
			// 
			this.ReturnedToShipperDateEdit.AllowDrop = true;
			this.ReturnedToShipperDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReturnedToShipperDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReturnedToShipperDateEdit, "RequiredDocuments.EQ_ReturnToShipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_ReturnToShipper)));
			this.ReturnedToShipperDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReturnedToShipperDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 70, true);
			this.ReturnedToShipperDateEdit.Name = "ReturnedToShipperDateEdit";
			this.ReturnedToShipperDateEdit.TabIndex = 7;
			// 
			// RcvdFromBrokerDateEdit
			// 
			this.RcvdFromBrokerDateEdit.AllowDrop = true;
			this.RcvdFromBrokerDateEdit.AutoCompleteMonthThreshold = 1;
			this.RcvdFromBrokerDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RcvdFromBrokerDateEdit, "RequiredDocuments.EQ_RcvFromCustomsBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_RcvFromCustomsBroker)));
			this.RcvdFromBrokerDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RcvdFromBrokerDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 48, true);
			this.RcvdFromBrokerDateEdit.Name = "RcvdFromBrokerDateEdit";
			this.RcvdFromBrokerDateEdit.TabIndex = 6;
			// 
			// ReceivedDateEdit
			// 
			this.ReceivedDateEdit.AllowDrop = true;
			this.ReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivedDateEdit, "RequiredDocuments.EQ_DateReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DateReceived)));
			this.ReceivedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 4, true);
			this.ReceivedDateEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.ReceivedDateEdit.Name = "ReceivedDateEdit";
			this.ReceivedDateEdit.TabIndex = 4;
			// 
			// ValidToDateEdit
			// 
			this.ValidToDateEdit.AllowDrop = true;
			this.ValidToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ValidToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ValidToDateEdit, "RequiredDocuments.EQ_ValidToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_ValidToDate)));
			this.ValidToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(792, 26, true);
			this.ValidToDateEdit.Name = "ValidToDateEdit";
			this.ValidToDateEdit.TabIndex = 10;
			// 
			// SentToBrokerDateEdit
			// 
			this.SentToBrokerDateEdit.AllowDrop = true;
			this.SentToBrokerDateEdit.AutoCompleteMonthThreshold = 1;
			this.SentToBrokerDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SentToBrokerDateEdit, "RequiredDocuments.EQ_SntToCustomsBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_SntToCustomsBroker)));
			this.SentToBrokerDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SentToBrokerDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 93, true);
			this.SentToBrokerDateEdit.Name = "SentToBrokerDateEdit";
			this.SentToBrokerDateEdit.TabIndex = 8;
			// 
			// CreditControlDocCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CreditControlDocCheckBox, "RequiredDocuments.EQ_CreditControlDoc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_CreditControlDoc)));
			this.CreditControlDocCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CreditControlDocCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CreditControlDocCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 70, true);
			this.CreditControlDocCheckBox.Name = "CreditControlDocCheckBox";
			this.CreditControlDocCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.CreditControlDocCheckBox.TabIndex = 3;
			// 
			// DocNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocNumberTextBox, "RequiredDocuments.EQ_DocNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_DocNumber)));
			this.DocNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 26, true);
			this.DocNumberTextBox.Name = "DocNumberTextBox";
			this.DocNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.DocNumberTextBox.TabIndex = 1;
			// 
			// DocumentOwnerFindBox
			// 
			this.DocumentOwnerFindBox.AllowDrop = true;
			this.DocumentOwnerFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocumentOwnerFindBox, "RequiredDocuments.EQ_OH_DocumentOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(null)).RequiredDocuments)).SyncRoot)).EQ_OH_DocumentOwner)));
			this.DocumentOwnerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 26, true);
			this.DocumentOwnerFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.DocumentOwnerFindBox.Name = "DocumentOwnerFindBox";
			this.DocumentOwnerFindBox.PreBoundMaxLength = 12;
			this.DocumentOwnerFindBox.ShowDescriptionBox = false;
			this.DocumentOwnerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.DocumentOwnerFindBox.TabIndex = 5;
			// 
			// RequiredDocumentsUserControl
			// 
			this.AutoScroll = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RequiredDocumentsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 150, true);
			this.Name = "RequiredDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 222, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RequiredDocumentsGroupBox.ResumeLayout(false);
			this.RequiredDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			this.DocumentsGrid.ResumeLayout(false);
			this.DocumentsGrid.PerformLayout();
			this.DocumentsDetailsPanel.ResumeLayout(false);
			this.DocumentsDetailsPanel.PerformLayout();
			this.AttributesGroupBox.ResumeLayout(false);
			this.AttributesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).EndInit();
			this.AttributesGrid.ResumeLayout(false);
			this.AttributesGrid.PerformLayout();
			this.DocUsageDropEdit.ResumeLayout(true);
			this.DocUsageDropEdit.PerformLayout();
			this.DocumentPeriodDropEdit.ResumeLayout(true);
			this.DocumentPeriodDropEdit.PerformLayout();
			this.DocumentTypeDropEdit.ResumeLayout(true);
			this.DocumentTypeDropEdit.PerformLayout();
			this.ReturnedToShipperDateEdit.ResumeLayout(true);
			this.ReturnedToShipperDateEdit.PerformLayout();
			this.RcvdFromBrokerDateEdit.ResumeLayout(true);
			this.RcvdFromBrokerDateEdit.PerformLayout();
			this.ReceivedDateEdit.ResumeLayout(true);
			this.ReceivedDateEdit.PerformLayout();
			this.ValidToDateEdit.ResumeLayout(true);
			this.ValidToDateEdit.PerformLayout();
			this.SentToBrokerDateEdit.ResumeLayout(true);
			this.SentToBrokerDateEdit.PerformLayout();
			this.DocumentOwnerFindBox.ResumeLayout(true);
			this.DocumentOwnerFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
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
	}
}
