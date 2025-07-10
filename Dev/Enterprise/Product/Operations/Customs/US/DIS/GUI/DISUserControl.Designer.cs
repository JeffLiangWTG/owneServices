namespace Enterprise.Customs.US.DIS.GUI
{
	partial class DISUserControl
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

				if (previousDISDocument != null)
				{
					previousDISDocument.DocumentLabelInfo.ValueChanged -= DocumentLabelInfo_ValueChanged;
				}
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RequiredDocumentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DISDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsMainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OptionalDataTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShipmentNoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocReviewGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReviewCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocRejectReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentLabelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EDocsDocumentGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.AdditionalDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalDataGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PGAGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DISIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CBPRequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RequestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestIDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OptionalDataTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OptionalDataTabPagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoiceCommodityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommodityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommoditiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OptionalDataTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PermitGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitStatementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValidToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BondDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BondTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BondNameTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultBondCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BondAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SuretyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BondNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackingListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingListInvoiceNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PurchaseOrderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackingListNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateStatementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InspectionLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CertificateExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CertificateNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToxicSubstanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CASNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EPARegoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EPAProducerEstTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OptionalDataUnavailableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.MessagesGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InterpretedPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InterpretedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocReviewTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocReviewRejectReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainSplitter = new CargoWise.Windows.UI.KSplitter();
			this.GrossTonnageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetTonnageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RequiredDocumentsPanel.SuspendLayout();
			this.DISDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			this.DocumentsGrid.SuspendLayout();
			this.BottomDetailsPanel.SuspendLayout();
			this.DetailsMainPanel.SuspendLayout();
			this.OptionalDataTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ShipmentNoDropEdit.SuspendLayout();
			this.DocReviewGroupBox.SuspendLayout();
			this.DocumentLabelCodeFindBox.SuspendLayout();
			this.EDocsDocumentGuidDropEdit.SuspendLayout();
			this.AdditionalDataGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDataGrid)).BeginInit();
			this.AdditionalDataGrid.SuspendLayout();
			this.PGAGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PGAGrid)).BeginInit();
			this.PGAGrid.SuspendLayout();
			this.CBPRequestGroupBox.SuspendLayout();
			this.RequestTypeDropEdit.SuspendLayout();
			this.RequestIDDropEdit.SuspendLayout();
			this.RequestDateEdit.SuspendLayout();
			this.OptionalDataTabPage.SuspendLayout();
			this.OptionalDataTabPagePanel.SuspendLayout();
			this.InvoiceCommodityPanel.SuspendLayout();
			this.CommodityGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommoditiesGrid)).BeginInit();
			this.CommoditiesGrid.SuspendLayout();
			this.InvoiceGroupBox.SuspendLayout();
			this.InvoiceNumberDropEdit.SuspendLayout();
			this.InvoiceLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLinesGrid)).BeginInit();
			this.InvoiceLinesGrid.SuspendLayout();
			this.OptionalDataTopPanel.SuspendLayout();
			this.PermitGroupBox.SuspendLayout();
			this.ValidToDateEdit.SuspendLayout();
			this.PermitStartDateEdit.SuspendLayout();
			this.BondDataGroupBox.SuspendLayout();
			this.BondNameTypeDropEdit.SuspendLayout();
			this.DefaultBondCodeDropEdit.SuspendLayout();
			this.PackingListGroupBox.SuspendLayout();
			this.CertificateGroupBox.SuspendLayout();
			this.CertificateIssueDateEdit.SuspendLayout();
			this.CertificateExpiryDateEdit.SuspendLayout();
			this.ToxicSubstanceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CASNumbersGrid)).BeginInit();
			this.CASNumbersGrid.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.InterpretedPanel.SuspendLayout();
			this.MessageTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DIS.Business.DISHostWrapper);
			// 
			// RequiredDocumentsPanel
			// 
			this.RequiredDocumentsPanel.Controls.Add(this.DISDocumentsGroupBox);
			this.RequiredDocumentsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RequiredDocumentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequiredDocumentsPanel.Name = "RequiredDocumentsPanel";
			this.RequiredDocumentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 136, true);
			this.RequiredDocumentsPanel.TabIndex = 1;
			// 
			// DISDocumentsGroupBox
			// 
			this.DISDocumentsGroupBox.Controls.Add(this.DocumentsGrid);
			this.DISDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DISDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DISDocumentsGroupBox.Name = "DISDocumentsGroupBox";
			this.DISDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 136, true);
			this.DISDocumentsGroupBox.TabIndex = 2;
			this.DISDocumentsGroupBox.TabStop = false;
			this.DISDocumentsGroupBox.Text = "Document Image System (DIS) Documents";
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentsGrid, "DISDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).EDocsDocumentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).RequiredDocumentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).IDSuffix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentLabel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ShipmentNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ITN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).XTN)));
			this.DocumentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo3.Caption = "eDocs";
			zGuidDropEditColumnStyleInfo3.ColumnName = "EDocsDocumentPK";
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo4.Caption = "Tracking Document";
			zGuidDropEditColumnStyleInfo4.ColumnName = "RequiredDocumentPK";
			zGuidDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.Caption = "ID Suffix";
			zCalcEditColumnStyleInfo8.ColumnName = "IDSuffix";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo15.Caption = "Document ID";
			zTextBoxColumnStyleInfo15.ColumnName = "DocumentID";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCodeFindBoxColumnStyleInfo2.Caption = "Form Type";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "DocumentLabel";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.Caption = "Description";
			zTextBoxColumnStyleInfo16.ColumnName = "DocumentDescription";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zMultiLineTextBoxColumnInfo2.Caption = "Comment";
			zMultiLineTextBoxColumnInfo2.ColumnName = "Comment";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo17.Caption = "Status";
			zTextBoxColumnStyleInfo17.ColumnName = "Status";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.Caption = "Status Desc";
			zTextBoxColumnStyleInfo18.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("01f96bcb-d746-44e1-8c31-e917d26c321b", "Shipment No.");
			zDropEditColumnStyleInfo4.ColumnName = "ShipmentNo";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("dc418623-f3f2-424e-ae41-88f10d718e4c", "ITN");
			zTextBoxColumnStyleInfo19.ColumnName = "ITN";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("cd1d4fca-5c23-4d7a-b221-1f76129bedb7", "XTN");
			zTextBoxColumnStyleInfo20.ColumnName = "XTN";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139);
			this.DocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.DocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			this.DocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.DocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.DocumentsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.DocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.DocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGrid.GridId = "8ddbccb3-98d4-4d78-97ca-ec7943743af7";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "DocumentsGrid";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 117, true);
			this.DocumentsGrid.TabIndex = 2;
			// 
			// BottomDetailsPanel
			// 
			this.BottomDetailsPanel.Controls.Add(this.DetailsMainPanel);
			this.BottomDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.BottomDetailsPanel.Name = "BottomDetailsPanel";
			this.BottomDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 491, true);
			this.BottomDetailsPanel.TabIndex = 2;
			// 
			// DetailsMainPanel
			// 
			this.DetailsMainPanel.Controls.Add(this.OptionalDataTabControl);
			this.DetailsMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsMainPanel.Name = "DetailsMainPanel";
			this.DetailsMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 491, true);
			this.DetailsMainPanel.TabIndex = 2;
			// 
			// OptionalDataTabControl
			// 
			this.OptionalDataTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.OptionalDataTabControl.Controls.Add(this.DetailsTabPage);
			this.OptionalDataTabControl.Controls.Add(this.OptionalDataTabPage);
			this.OptionalDataTabControl.Controls.Add(this.MessagesTabPage);
			this.OptionalDataTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionalDataTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionalDataTabControl.Name = "OptionalDataTabControl";
			this.OptionalDataTabControl.SelectedIndex = 0;
			this.OptionalDataTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 491, true);
			this.OptionalDataTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.ShipmentNoDropEdit);
			this.DetailsTabPage.Controls.Add(this.DocReviewGroupBox);
			this.DetailsTabPage.Controls.Add(this.DocumentLabelCodeFindBox);
			this.DetailsTabPage.Controls.Add(this.EDocsDocumentGuidDropEdit);
			this.DetailsTabPage.Controls.Add(this.AdditionalDataGroupBox);
			this.DetailsTabPage.Controls.Add(this.CommentTextBox);
			this.DetailsTabPage.Controls.Add(this.DescriptionTextBox);
			this.DetailsTabPage.Controls.Add(this.PGAGroupBox);
			this.DetailsTabPage.Controls.Add(this.DISIDTextBox);
			this.DetailsTabPage.Controls.Add(this.CBPRequestGroupBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 464, true);
			this.DetailsTabPage.TabIndex = 5;
			this.DetailsTabPage.Text = "Details";
			// 
			// ShipmentNoDropEdit
			// 
			this.ShipmentNoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentNoDropEdit, "DISDocuments.ShipmentNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ShipmentNo)));
			this.ShipmentNoDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("56fbce72-8c14-4afc-ac3f-bd13aae1d081", "Shipment No.");
			this.ShipmentNoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 251, true);
			this.ShipmentNoDropEdit.Name = "ShipmentNoDropEdit";
			this.ShipmentNoDropEdit.PreBoundMaxLength = 35;
			this.ShipmentNoDropEdit.ShowDescriptionBox = false;
			this.ShipmentNoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.ShipmentNoDropEdit.TabIndex = 5;
			// 
			// DocReviewGroupBox
			// 
			this.DocReviewGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DocReviewGroupBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("fc782b23-084e-4e67-9e4b-cdcd6f99ab17", "Document Review");
			this.DocReviewGroupBox.Controls.Add(this.ReviewCommentTextBox);
			this.DocReviewGroupBox.Controls.Add(this.DocRejectReasonTextBox);
			this.DocReviewGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 374, true);
			this.DocReviewGroupBox.Name = "DocReviewGroupBox";
			this.DocReviewGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 89, true);
			this.DocReviewGroupBox.TabIndex = 9;
			this.DocReviewGroupBox.TabStop = false;
			this.DocReviewGroupBox.Text = "Document Review";
			// 
			// ReviewCommentTextBox
			// 
			this.ReviewCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReviewCommentTextBox, "DISDocuments.DocumentReviewComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentReviewComment)));
			this.ReviewCommentTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("82e16c5a-5c1b-4fe1-a768-93c6dd46ddd1", "Review Comment");
			this.ReviewCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 38, true);
			this.ReviewCommentTextBox.Multiline = true;
			this.ReviewCommentTextBox.Name = "ReviewCommentTextBox";
			this.ReviewCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 47, true);
			this.ReviewCommentTextBox.TabIndex = 1;
			// 
			// DocRejectReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.DocRejectReasonTextBox, "DISDocuments.DocumentRejectReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentRejectReason)));
			this.DocRejectReasonTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("f49bda4a-1eef-46fa-8eb1-e3896bc109d3", "Reject Reason");
			this.DocRejectReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
			this.DocRejectReasonTextBox.Name = "DocRejectReasonTextBox";
			this.DocRejectReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.DocRejectReasonTextBox.TabIndex = 0;
			// 
			// DocumentLabelCodeFindBox
			// 
			this.DocumentLabelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentLabelCodeFindBox, "DISDocuments.DocumentLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentLabel)));
			this.DocumentLabelCodeFindBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("e8d40b25-7809-4b43-8dea-ef53bb1f0427", "Form Type");
			this.DocumentLabelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 39, true);
			this.DocumentLabelCodeFindBox.Name = "DocumentLabelCodeFindBox";
			this.DocumentLabelCodeFindBox.PreBoundMaxLength = 5;
			this.DocumentLabelCodeFindBox.ShouldResize = true;
			this.DocumentLabelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 20, true);
			this.DocumentLabelCodeFindBox.TabIndex = 1;
			// 
			// EDocsDocumentGuidDropEdit
			// 
			this.EDocsDocumentGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EDocsDocumentGuidDropEdit, "DISDocuments.EDocsDocumentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).EDocsDocumentPK)));
			this.EDocsDocumentGuidDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("c63fa6be-76d0-43d3-81ed-edddf6e47fa5", "eDocs");
			this.EDocsDocumentGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 137, true);
			this.EDocsDocumentGuidDropEdit.Name = "EDocsDocumentGuidDropEdit";
			this.EDocsDocumentGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 20, true);
			this.EDocsDocumentGuidDropEdit.TabIndex = 3;
			// 
			// AdditionalDataGroupBox
			// 
			this.AdditionalDataGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalDataGroupBox.Controls.Add(this.AdditionalDataGrid);
			this.AdditionalDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 3, true);
			this.AdditionalDataGroupBox.Name = "AdditionalDataGroupBox";
			this.AdditionalDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 205, true);
			this.AdditionalDataGroupBox.TabIndex = 7;
			this.AdditionalDataGroupBox.TabStop = false;
			this.AdditionalDataGroupBox.Text = "Additional Data";
			// 
			// AdditionalDataGrid
			// 
			this.AdditionalDataGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDataGrid, "DISDocuments.AdditionalData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).AdditionalData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISAdditionalData)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).AdditionalData)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISAdditionalData)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).AdditionalData)).SyncRoot)).Data)));
			this.AdditionalDataGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Field Name";
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Value";
			zTextBoxColumnStyleInfo2.ColumnName = "Data";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.AdditionalDataGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalDataGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDataGrid.GridId = "5c236175-0aa0-46b2-82fb-e5ec80579016";
			this.AdditionalDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDataGrid.LayoutKey = "AdditionalDataGrid";
			this.AdditionalDataGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalDataGrid.Name = "AdditionalDataGrid";
			this.AdditionalDataGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 186, true);
			this.AdditionalDataGrid.TabIndex = 0;
			// 
			// CommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommentTextBox, "DISDocuments.Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Comment)));
			this.CommentTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("cb0d46f0-2c4b-4ce4-b734-7f6a465d499b", "Comment");
			this.CommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 163, true);
			this.CommentTextBox.Multiline = true;
			this.CommentTextBox.Name = "CommentTextBox";
			this.CommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 77, true);
			this.CommentTextBox.TabIndex = 4;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "DISDocuments.DocumentDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("162d4349-b35a-4f90-8f3a-30bad86ced21", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 65, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 66, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// PGAGroupBox
			// 
			this.PGAGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PGAGroupBox.Controls.Add(this.PGAGrid);
			this.PGAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 220, true);
			this.PGAGroupBox.Name = "PGAGroupBox";
			this.PGAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 150, true);
			this.PGAGroupBox.TabIndex = 8;
			this.PGAGroupBox.TabStop = false;
			this.PGAGroupBox.Text = "Agency Code";
			// 
			// PGAGrid
			// 
			this.PGAGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PGAGrid, "DISDocuments.PGAs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PGAs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISPGA)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PGAs)).SyncRoot)).Code)));
			this.PGAGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Code";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PGAGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PGAGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PGAGrid.GridId = "a5bef750-8ea0-41fc-ac32-571b6c47b323";
			this.PGAGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PGAGrid.LayoutKey = "PGAGrid";
			this.PGAGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PGAGrid.Name = "PGAGrid";
			this.PGAGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 131, true);
			this.PGAGrid.TabIndex = 0;
			// 
			// DISIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.DISIDTextBox, "DISDocuments.DocumentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).DocumentID)));
			this.DISIDTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("08f7227f-ee8f-42e0-afcf-c8d8bd17c575", "Document ID");
			this.DISIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 13, true);
			this.DISIDTextBox.Name = "DISIDTextBox";
			this.DISIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.DISIDTextBox.TabIndex = 0;
			// 
			// CBPRequestGroupBox
			// 
			this.CBPRequestGroupBox.Controls.Add(this.RequestTypeDropEdit);
			this.CBPRequestGroupBox.Controls.Add(this.RequestIDDropEdit);
			this.CBPRequestGroupBox.Controls.Add(this.RequestDateEdit);
			this.CBPRequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 273, true);
			this.CBPRequestGroupBox.Name = "CBPRequestGroupBox";
			this.CBPRequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 97, true);
			this.CBPRequestGroupBox.TabIndex = 6;
			this.CBPRequestGroupBox.TabStop = false;
			this.CBPRequestGroupBox.Text = "CBP Requests";
			// 
			// RequestTypeDropEdit
			// 
			this.RequestTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestTypeDropEdit, "DISDocuments.CBPRequest.Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CBPRequest.Type)));
			this.RequestTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("962b9d8b-06d3-4ab0-b9af-8bfc7c8c5887", "Type");
			this.RequestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 46, true);
			this.RequestTypeDropEdit.Name = "RequestTypeDropEdit";
			this.RequestTypeDropEdit.PreBoundMaxLength = 3;
			this.RequestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 20, true);
			this.RequestTypeDropEdit.TabIndex = 1;
			// 
			// RequestIDDropEdit
			// 
			this.RequestIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestIDDropEdit, "DISDocuments.CBPRequest.ID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CBPRequest.ID)));
			this.RequestIDDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("d95b3d38-f120-46bf-a068-085f9a1faec8", "ID");
			this.RequestIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.RequestIDDropEdit.Name = "RequestIDDropEdit";
			this.RequestIDDropEdit.PreBoundMaxLength = 12;
			this.RequestIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 20, true);
			this.RequestIDDropEdit.TabIndex = 0;
			// 
			// RequestDateEdit
			// 
			this.RequestDateEdit.AllowDrop = true;
			this.RequestDateEdit.AutoCompleteMonthThreshold = 1;
			this.RequestDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RequestDateEdit, "DISDocuments.CBPRequest.RequestDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CBPRequest.RequestDate)));
			this.RequestDateEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("4b581390-81b8-46ce-8dbc-3b04222fe239", "Date");
			this.RequestDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 72, true);
			this.RequestDateEdit.Name = "RequestDateEdit";
			this.RequestDateEdit.TabIndex = 2;
			// 
			// OptionalDataTabPage
			// 
			this.OptionalDataTabPage.Controls.Add(this.OptionalDataTabPagePanel);
			this.OptionalDataTabPage.Controls.Add(this.OptionalDataUnavailableLabel);
			this.OptionalDataTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OptionalDataTabPage.Name = "OptionalDataTabPage";
			this.OptionalDataTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OptionalDataTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 464, true);
			this.OptionalDataTabPage.TabIndex = 0;
			this.OptionalDataTabPage.Text = "Optional Data";
			// 
			// OptionalDataTabPagePanel
			// 
			this.OptionalDataTabPagePanel.Controls.Add(this.InvoiceCommodityPanel);
			this.OptionalDataTabPagePanel.Controls.Add(this.OptionalDataTopPanel);
			this.OptionalDataTabPagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionalDataTabPagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OptionalDataTabPagePanel.Name = "OptionalDataTabPagePanel";
			this.OptionalDataTabPagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 458, true);
			this.OptionalDataTabPagePanel.TabIndex = 11;
			// 
			// InvoiceCommodityPanel
			// 
			this.InvoiceCommodityPanel.Controls.Add(this.CommodityGroupBox);
			this.InvoiceCommodityPanel.Controls.Add(this.InvoiceGroupBox);
			this.InvoiceCommodityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceCommodityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 156, true);
			this.InvoiceCommodityPanel.Name = "InvoiceCommodityPanel";
			this.InvoiceCommodityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 302, true);
			this.InvoiceCommodityPanel.TabIndex = 8;
			// 
			// CommodityGroupBox
			// 
			this.CommodityGroupBox.Controls.Add(this.CommoditiesGrid);
			this.CommodityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityGroupBox.Name = "CommodityGroupBox";
			this.CommodityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 302, true);
			this.CommodityGroupBox.TabIndex = 5;
			this.CommodityGroupBox.TabStop = false;
			this.CommodityGroupBox.Text = "Commodities";
			// 
			// CommoditiesGrid
			// 
			this.CommoditiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommoditiesGrid, "DISDocuments.CommodityData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CommodityData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISCommodityLine)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CommodityData)).SyncRoot)).InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISCommodityLine)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CommodityData)).SyncRoot)).InvoiceLineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISCommodityLine)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CommodityData)).SyncRoot)).InvoiceLineTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISCommodityLine)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CommodityData)).SyncRoot)).VNELineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISCommodityLine)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CommodityData)).SyncRoot)).VNELineNumberTo)));
			this.CommoditiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.Caption = "Invoice Number";
			zDropEditColumnStyleInfo2.ColumnName = "InvoiceNumber";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Inv. Line #";
			zCalcEditColumnStyleInfo1.ColumnName = "InvoiceLineNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.Caption = "Inv. Line # To";
			zCalcEditColumnStyleInfo9.ColumnName = "InvoiceLineTo";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.Caption = "VNE Line # (if applicable)";
			zCalcEditColumnStyleInfo10.ColumnName = "VNELineNumber";
			zCalcEditColumnStyleInfo10.Decimals = 0;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.Caption = "VNE. Line # To (if applicable)";
			zCalcEditColumnStyleInfo11.ColumnName = "VNELineNumberTo";
			zCalcEditColumnStyleInfo11.Decimals = 0;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.CommoditiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.CommoditiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.CommoditiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommoditiesGrid.GridId = "b86b5e78-0394-4a60-b79e-f7960654d9b2";
			this.CommoditiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommoditiesGrid.LayoutKey = "InvoiceLinesGrid";
			this.CommoditiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CommoditiesGrid.Name = "CommoditiesGrid";
			this.CommoditiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 283, true);
			this.CommoditiesGrid.TabIndex = 2;
			// 
			// InvoiceGroupBox
			// 
			this.InvoiceGroupBox.Controls.Add(this.InvoiceNumberDropEdit);
			this.InvoiceGroupBox.Controls.Add(this.InvoiceLinesGroupBox);
			this.InvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceGroupBox.Name = "InvoiceGroupBox";
			this.InvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 302, true);
			this.InvoiceGroupBox.TabIndex = 6;
			this.InvoiceGroupBox.TabStop = false;
			this.InvoiceGroupBox.Text = "Invoice";
			// 
			// InvoiceNumberDropEdit
			// 
			this.InvoiceNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNumberDropEdit, "DISDocuments.Invoice.InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Invoice.InvoiceNumber)));
			this.InvoiceNumberDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("6ec4b7bb-9822-447a-a2ef-b7c87840ea6c", "Invoice Number");
			this.InvoiceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16, true);
			this.InvoiceNumberDropEdit.Name = "InvoiceNumberDropEdit";
			this.InvoiceNumberDropEdit.PreBoundMaxLength = 35;
			this.InvoiceNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 20, true);
			this.InvoiceNumberDropEdit.TabIndex = 5;
			// 
			// InvoiceLinesGroupBox
			// 
			this.InvoiceLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceLinesGroupBox.Controls.Add(this.InvoiceLinesGrid);
			this.InvoiceLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 40, true);
			this.InvoiceLinesGroupBox.Name = "InvoiceLinesGroupBox";
			this.InvoiceLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 250, true);
			this.InvoiceLinesGroupBox.TabIndex = 6;
			this.InvoiceLinesGroupBox.TabStop = false;
			this.InvoiceLinesGroupBox.Text = "Invoice Lines (For partial documents only)";
			// 
			// InvoiceLinesGrid
			// 
			this.InvoiceLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceLinesGrid, "DISDocuments.Invoice.InvoiceLineRanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Invoice.InvoiceLineRanges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISInvoiceLineRange)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Invoice.InvoiceLineRanges)).SyncRoot)).InvoiceLineFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISInvoiceLineRange)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Invoice.InvoiceLineRanges)).SyncRoot)).InvoiceLineTo)));
			this.InvoiceLinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "InvoiceLineFrom";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "InvoiceLineTo";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoiceLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLinesGrid.GridId = "b86b5e78-0394-4a60-b79e-f7960654d9b2";
			this.InvoiceLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceLinesGrid.LayoutKey = "InvoiceLinesGrid";
			this.InvoiceLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceLinesGrid.Name = "InvoiceLinesGrid";
			this.InvoiceLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 231, true);
			this.InvoiceLinesGrid.TabIndex = 0;
			// 
			// OptionalDataTopPanel
			// 
			this.OptionalDataTopPanel.Controls.Add(this.CertificateGroupBox);
			this.OptionalDataTopPanel.Controls.Add(this.PermitGroupBox);
			this.OptionalDataTopPanel.Controls.Add(this.BondDataGroupBox);
			this.OptionalDataTopPanel.Controls.Add(this.PackingListGroupBox);
			this.OptionalDataTopPanel.Controls.Add(this.ToxicSubstanceGroupBox);
			this.OptionalDataTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OptionalDataTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionalDataTopPanel.Name = "OptionalDataTopPanel";
			this.OptionalDataTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 156, true);
			this.OptionalDataTopPanel.TabIndex = 7;
			// 
			// PermitGroupBox
			// 
			this.PermitGroupBox.Controls.Add(this.PermitTypeTextBox);
			this.PermitGroupBox.Controls.Add(this.PermitStatementTextBox);
			this.PermitGroupBox.Controls.Add(this.ValidToDateEdit);
			this.PermitGroupBox.Controls.Add(this.ApprovalNumberTextBox);
			this.PermitGroupBox.Controls.Add(this.PermitStartDateEdit);
			this.PermitGroupBox.Controls.Add(this.PermitNumberTextBox);
			this.PermitGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitGroupBox.Name = "PermitGroupBox";
			this.PermitGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 156, true);
			this.PermitGroupBox.TabIndex = 4;
			this.PermitGroupBox.TabStop = false;
			this.PermitGroupBox.Text = "Permit";
			// 
			// PermitTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PermitTypeTextBox, "DISDocuments.PermitData.PermitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PermitData.PermitType)));
			this.PermitTypeTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("a86cac73-8fd1-4ad3-bf6b-c92df35276da", "Type");
			this.PermitTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 13, true);
			this.PermitTypeTextBox.Name = "PermitTypeTextBox";
			this.PermitTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.PermitTypeTextBox.TabIndex = 1;
			// 
			// PermitStatementTextBox
			// 
			this.BindingSource.SetBindingMember(this.PermitStatementTextBox, "DISDocuments.PermitData.Statement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PermitData.Statement)));
			this.PermitStatementTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("92facc52-8c23-4550-a602-4785665cd504", "Statement");
			this.PermitStatementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 66, true);
			this.PermitStatementTextBox.Multiline = true;
			this.PermitStatementTextBox.Name = "PermitStatementTextBox";
			this.PermitStatementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 70, true);
			this.PermitStatementTextBox.TabIndex = 5;
			// 
			// ValidToDateEdit
			// 
			this.ValidToDateEdit.AllowDrop = true;
			this.ValidToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ValidToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ValidToDateEdit, "DISDocuments.PermitData.EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PermitData.EndDate)));
			this.ValidToDateEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("6e3b65c3-f407-4978-9210-32d4e3e5e51e", "To");
			this.ValidToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 40, true);
			this.ValidToDateEdit.Name = "ValidToDateEdit";
			this.ValidToDateEdit.TabIndex = 3;
			// 
			// ApprovalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovalNumberTextBox, "DISDocuments.PermitData.ApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PermitData.ApprovalNumber)));
			this.ApprovalNumberTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("e1d33bfa-9fed-48e0-8346-090a45f5d327", "Approval No.");
			this.ApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 40, true);
			this.ApprovalNumberTextBox.Name = "ApprovalNumberTextBox";
			this.ApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ApprovalNumberTextBox.TabIndex = 4;
			// 
			// PermitStartDateEdit
			// 
			this.PermitStartDateEdit.AllowDrop = true;
			this.PermitStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.PermitStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PermitStartDateEdit, "DISDocuments.PermitData.StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PermitData.StartDate)));
			this.PermitStartDateEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("a0ddeb25-09a4-4f4a-bc7b-f0a7eca049fb", "From");
			this.PermitStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			this.PermitStartDateEdit.Name = "PermitStartDateEdit";
			this.PermitStartDateEdit.TabIndex = 2;
			// 
			// PermitNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PermitNumberTextBox, "DISDocuments.PermitData.PermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PermitData.PermitNumber)));
			this.PermitNumberTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("73474d1d-0dce-4d74-901d-af803083ac95", "Number");
			this.PermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 13, true);
			this.PermitNumberTextBox.Name = "PermitNumberTextBox";
			this.PermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.PermitNumberTextBox.TabIndex = 0;
			// 
			// BondDataGroupBox
			// 
			this.BondDataGroupBox.Controls.Add(this.BondTypeTextBox);
			this.BondDataGroupBox.Controls.Add(this.BondNameTypeDropEdit);
			this.BondDataGroupBox.Controls.Add(this.DefaultBondCodeDropEdit);
			this.BondDataGroupBox.Controls.Add(this.BondAmountCalcEdit);
			this.BondDataGroupBox.Controls.Add(this.SuretyCodeTextBox);
			this.BondDataGroupBox.Controls.Add(this.AgentIDTextBox);
			this.BondDataGroupBox.Controls.Add(this.BondNumberTextBox);
			this.BondDataGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BondDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BondDataGroupBox.Name = "BondDataGroupBox";
			this.BondDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 156, true);
			this.BondDataGroupBox.TabIndex = 9;
			this.BondDataGroupBox.TabStop = false;
			this.BondDataGroupBox.Text = "Bond Data";
			// 
			// BondTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondTypeTextBox, "DISDocuments.BondData.BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.BondType)));
			this.BondTypeTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("432d2f0b-4c38-40b5-b4d2-ff19bc74f8d6", "Bond Type");
			this.BondTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 43, true);
			this.BondTypeTextBox.Name = "BondTypeTextBox";
			this.BondTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.BondTypeTextBox.TabIndex = 2;
			// 
			// BondNameTypeDropEdit
			// 
			this.BondNameTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondNameTypeDropEdit, "DISDocuments.BondData.BondName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.BondName)));
			this.BondNameTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("4d750981-09e9-4bbd-9c37-8a776b934412", "Bond Name");
			this.BondNameTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 43, true);
			this.BondNameTypeDropEdit.Name = "BondNameTypeDropEdit";
			this.BondNameTypeDropEdit.PreBoundMaxLength = 3;
			this.BondNameTypeDropEdit.ShowDescriptionBox = false;
			this.BondNameTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.BondNameTypeDropEdit.TabIndex = 1;
			// 
			// DefaultBondCodeDropEdit
			// 
			this.DefaultBondCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultBondCodeDropEdit, "DISDocuments.BondData.DefaultBondCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.DefaultBondCode)));
			this.DefaultBondCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("a4be8012-c15a-495d-95cc-ceb797425447", "Default");
			this.DefaultBondCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 17, true);
			this.DefaultBondCodeDropEdit.Name = "DefaultBondCodeDropEdit";
			this.DefaultBondCodeDropEdit.PreBoundMaxLength = 3;
			this.DefaultBondCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 20, true);
			this.DefaultBondCodeDropEdit.TabIndex = 0;
			// 
			// BondAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BondAmountCalcEdit, "DISDocuments.BondData.BondAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.BondAmount)));
			this.BondAmountCalcEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("a872358c-3f64-4413-be41-dde66bfc449a", "Bond Amount");
			this.BondAmountCalcEdit.DecimalPlaces = 2;
			this.BondAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 98, true);
			this.BondAmountCalcEdit.Name = "BondAmountCalcEdit";
			this.BondAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.BondAmountCalcEdit.TabIndex = 6;
			this.BondAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SuretyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SuretyCodeTextBox, "DISDocuments.BondData.SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.SuretyCode)));
			this.SuretyCodeTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("a678c47c-f2d0-451c-a408-b68bcce784c7", "Surety Code");
			this.SuretyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 69, true);
			this.SuretyCodeTextBox.Name = "SuretyCodeTextBox";
			this.SuretyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.SuretyCodeTextBox.TabIndex = 3;
			// 
			// AgentIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentIDTextBox, "DISDocuments.BondData.AgentIDNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.AgentIDNumber)));
			this.AgentIDTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("13a8b392-cde3-48aa-abbc-83a01d4539a2", "Agent ID");
			this.AgentIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 98, true);
			this.AgentIDTextBox.Name = "AgentIDTextBox";
			this.AgentIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.AgentIDTextBox.TabIndex = 5;
			// 
			// BondNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondNumberTextBox, "DISDocuments.BondData.BondNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).BondData.BondNumber)));
			this.BondNumberTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("59653b5f-7f51-408c-9784-3f5c6d16c0ad", "Bond Number");
			this.BondNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 69, true);
			this.BondNumberTextBox.Name = "BondNumberTextBox";
			this.BondNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.BondNumberTextBox.TabIndex = 4;
			// 
			// PackingListGroupBox
			// 
			this.PackingListGroupBox.Controls.Add(this.PackingListInvoiceNoTextBox);
			this.PackingListGroupBox.Controls.Add(this.PurchaseOrderTextBox);
			this.PackingListGroupBox.Controls.Add(this.PackingListNoTextBox);
			this.PackingListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingListGroupBox.Name = "PackingListGroupBox";
			this.PackingListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 156, true);
			this.PackingListGroupBox.TabIndex = 8;
			this.PackingListGroupBox.TabStop = false;
			this.PackingListGroupBox.Text = "Packing List";
			// 
			// PackingListInvoiceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackingListInvoiceNoTextBox, "DISDocuments.PackingList.InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PackingList.InvoiceNumber)));
			this.PackingListInvoiceNoTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("49fbee94-fbdd-40c9-8234-423e4839f723", "Invoice Number");
			this.PackingListInvoiceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 48, true);
			this.PackingListInvoiceNoTextBox.Name = "PackingListInvoiceNoTextBox";
			this.PackingListInvoiceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.PackingListInvoiceNoTextBox.TabIndex = 1;
			// 
			// PurchaseOrderTextBox
			// 
			this.BindingSource.SetBindingMember(this.PurchaseOrderTextBox, "DISDocuments.PackingList.PurchaseOrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PackingList.PurchaseOrderNumber)));
			this.PurchaseOrderTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("9aaa695e-cfed-436c-8cee-3148ea56fc5e", "Purchase Order No.");
			this.PurchaseOrderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 74, true);
			this.PurchaseOrderTextBox.Name = "PurchaseOrderTextBox";
			this.PurchaseOrderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.PurchaseOrderTextBox.TabIndex = 2;
			// 
			// PackingListNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackingListNoTextBox, "DISDocuments.PackingList.PackingListNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).PackingList.PackingListNumber)));
			this.PackingListNoTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("694992aa-12d5-4aa6-a91c-5ef9d45dba37", "Packing List No.");
			this.PackingListNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 22, true);
			this.PackingListNoTextBox.Name = "PackingListNoTextBox";
			this.PackingListNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.PackingListNoTextBox.TabIndex = 0;
			// 
			// CertificateGroupBox
			// 
			this.CertificateGroupBox.Controls.Add(this.NetTonnageCalcEdit);
			this.CertificateGroupBox.Controls.Add(this.GrossTonnageCalcEdit);
			this.CertificateGroupBox.Controls.Add(this.CertificateTypeTextBox);
			this.CertificateGroupBox.Controls.Add(this.CertificateStatementTextBox);
			this.CertificateGroupBox.Controls.Add(this.InspectionLocationTextBox);
			this.CertificateGroupBox.Controls.Add(this.CertificateIssueDateEdit);
			this.CertificateGroupBox.Controls.Add(this.CertificateExpiryDateEdit);
			this.CertificateGroupBox.Controls.Add(this.CertificateNumberTextBox);
			this.CertificateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 156, true);
			this.CertificateGroupBox.TabIndex = 3;
			this.CertificateGroupBox.TabStop = false;
			this.CertificateGroupBox.Text = "Certificate";
			// 
			// CertificateTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateTypeTextBox, "DISDocuments.CertificateData.CertificateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.CertificateType)));
			this.CertificateTypeTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("60960d16-3f49-451d-b32c-134a73abac84", "Type");
			this.CertificateTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 16, true);
			this.CertificateTypeTextBox.Name = "CertificateTypeTextBox";
			this.CertificateTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.CertificateTypeTextBox.TabIndex = 1;
			// 
			// CertificateStatementTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateStatementTextBox, "DISDocuments.CertificateData.Statement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.Statement)));
			this.CertificateStatementTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("7a0d5f94-5b8a-4147-9b70-badc89751828", "Statement");
			this.CertificateStatementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 92, true);
			this.CertificateStatementTextBox.Multiline = true;
			this.CertificateStatementTextBox.Name = "CertificateStatementTextBox";
			this.CertificateStatementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 57, true);
			this.CertificateStatementTextBox.TabIndex = 7;
			// 
			// InspectionLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.InspectionLocationTextBox, "DISDocuments.CertificateData.InspectionLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.InspectionLocation)));
			this.InspectionLocationTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("69bc1184-6b11-4a97-9eb6-ebe120482fc6", "Inspection Location");
			this.InspectionLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 42, true);
			this.InspectionLocationTextBox.Name = "InspectionLocationTextBox";
			this.InspectionLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.InspectionLocationTextBox.TabIndex = 4;
			// 
			// CertificateIssueDateEdit
			// 
			this.CertificateIssueDateEdit.AllowDrop = true;
			this.CertificateIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.CertificateIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CertificateIssueDateEdit, "DISDocuments.CertificateData.IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.IssueDate)));
			this.CertificateIssueDateEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("7951BD44-26BA-4AF5-B150-8F7F79032BA4", "Issue Date");
			this.CertificateIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 67, true);
			this.CertificateIssueDateEdit.Name = "CertificateIssueDateEdit";
			this.CertificateIssueDateEdit.TabIndex = 6;
			// 
			// CertificateExpiryDateEdit
			// 
			this.CertificateExpiryDateEdit.AllowDrop = true;
			this.CertificateExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.CertificateExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CertificateExpiryDateEdit, "DISDocuments.CertificateData.ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.ExpiryDate)));
			this.CertificateExpiryDateEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("e78339dc-ba90-4e33-8560-75bf278df4db", "Expiry Date");
			this.CertificateExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 42, true);
			this.CertificateExpiryDateEdit.Name = "CertificateExpiryDateEdit";
			this.CertificateExpiryDateEdit.TabIndex = 3;
			// 
			// CertificateNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateNumberTextBox, "DISDocuments.CertificateData.CertificateNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.CertificateNumber)));
			this.CertificateNumberTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("c1bf322f-4def-4c14-8725-71bf4e0f1cf7", "Number");
			this.CertificateNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.CertificateNumberTextBox.Name = "CertificateNumberTextBox";
			this.CertificateNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.CertificateNumberTextBox.TabIndex = 0;
			// 
			// ToxicSubstanceGroupBox
			// 
			this.ToxicSubstanceGroupBox.Controls.Add(this.CASNumbersGrid);
			this.ToxicSubstanceGroupBox.Controls.Add(this.EPARegoTextBox);
			this.ToxicSubstanceGroupBox.Controls.Add(this.EPAProducerEstTextBox);
			this.ToxicSubstanceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToxicSubstanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToxicSubstanceGroupBox.Name = "ToxicSubstanceGroupBox";
			this.ToxicSubstanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 156, true);
			this.ToxicSubstanceGroupBox.TabIndex = 5;
			this.ToxicSubstanceGroupBox.TabStop = false;
			this.ToxicSubstanceGroupBox.Text = "Toxic Substance";
			// 
			// CASNumbersGrid
			// 
			this.CASNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CASNumbersGrid, "DISDocuments.ToxicSubstanceData.CASNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ToxicSubstanceData.CASNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISAdditionalNumber)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ToxicSubstanceData.CASNumbers)).SyncRoot)).Number)));
			this.CASNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("9824b087-523f-4fc9-9aee-d2e76e9ff209", "CAS Number");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CASNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CASNumbersGrid.GridId = "5ca86a34-ae5d-4118-83de-4744d7d2bb35";
			this.CASNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CASNumbersGrid.LayoutKey = "CASNumbersGrid";
			this.CASNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 12, true);
			this.CASNumbersGrid.Name = "CASNumbersGrid";
			this.CASNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 134, true);
			this.CASNumbersGrid.TabIndex = 3;
			// 
			// EPARegoTextBox
			// 
			this.BindingSource.SetBindingMember(this.EPARegoTextBox, "DISDocuments.ToxicSubstanceData.EPARegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ToxicSubstanceData.EPARegistrationNumber)));
			this.EPARegoTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("5705489d-a087-4efe-87bd-cebf048ea7fe", "EPA Registration No.");
			this.EPARegoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 16, true);
			this.EPARegoTextBox.Name = "EPARegoTextBox";
			this.EPARegoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.EPARegoTextBox.TabIndex = 1;
			// 
			// EPAProducerEstTextBox
			// 
			this.BindingSource.SetBindingMember(this.EPAProducerEstTextBox, "DISDocuments.ToxicSubstanceData.EPAProducerEstNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).ToxicSubstanceData.EPAProducerEstNumber)));
			this.EPAProducerEstTextBox.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("d9b72b39-e182-4ee1-934f-7c0caeb14cc2", "EPA Producer Est. No.");
			this.EPAProducerEstTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 42, true);
			this.EPAProducerEstTextBox.Name = "EPAProducerEstTextBox";
			this.EPAProducerEstTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.EPAProducerEstTextBox.TabIndex = 2;
			// 
			// OptionalDataUnavailableLabel
			// 
			this.BindingSource.SetBindingMember(this.OptionalDataUnavailableLabel, "DISDocuments.NoOptionalDataVisibleReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).NoOptionalDataVisibleReason)));
			this.OptionalDataUnavailableLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionalDataUnavailableLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OptionalDataUnavailableLabel.IsFontBold = true;
			this.OptionalDataUnavailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OptionalDataUnavailableLabel.Name = "OptionalDataUnavailableLabel";
			this.OptionalDataUnavailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 458, true);
			this.OptionalDataUnavailableLabel.TabIndex = 10;
			this.OptionalDataUnavailableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Controls.Add(this.splitter1);
			this.MessagesTabPage.Controls.Add(this.MessagesGridPanel);
			this.MessagesTabPage.Controls.Add(this.InterpretedPanel);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 464, true);
			this.MessagesTabPage.TabIndex = 1;
			this.MessagesTabPage.Text = "Messages";
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 3, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 463, true);
			this.splitter1.TabIndex = 10;
			this.splitter1.TabStop = false;
			// 
			// MessagesGridPanel
			// 
			this.MessagesGridPanel.Controls.Add(this.MessagesGrid);
			this.MessagesGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesGridPanel.Name = "MessagesGridPanel";
			this.MessagesGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 463, true);
			this.MessagesGridPanel.TabIndex = 9;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "DISDocuments.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_InterchangeStatus)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "Message Num";
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Message Time";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.Caption = "Sender";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "Interchange Num";
			zTextBoxColumnStyleInfo7.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo8.Caption = "Interchange Status";
			zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "b86b5e78-0394-4a60-b79e-f7960654d9b2";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "InvoiceLinesGrid";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 463, true);
			this.MessagesGrid.TabIndex = 4;
			// 
			// InterpretedPanel
			// 
			this.InterpretedPanel.Controls.Add(this.MessageTabControl);
			this.InterpretedPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.InterpretedPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 3, true);
			this.InterpretedPanel.Name = "InterpretedPanel";
			this.InterpretedPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 463, true);
			this.InterpretedPanel.TabIndex = 8;
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTabControl.Name = "MessageTabControl";
			this.MessageTabControl.SelectedIndex = 0;
			this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 463, true);
			this.MessageTabControl.TabIndex = 7;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Controls.Add(this.InterpretedTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 436, true);
			this.MessageTextTabPage.TabIndex = 1;
			this.MessageTextTabPage.Text = "Message Text";
			// 
			// InterpretedTextBox
			// 
			this.BindingSource.SetBindingMember(this.InterpretedTextBox, "DISDocuments.Messages.EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).EM_MessageText)));
			this.InterpretedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InterpretedTextBox.Multiline = true;
			this.InterpretedTextBox.Name = "InterpretedTextBox";
			this.InterpretedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.InterpretedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 430, true);
			this.InterpretedTextBox.TabIndex = 8;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("eba9b5ec-ff77-406a-8530-f308057b1c46", "Customs Review Details");
			this.MessageDetailsTabPage.Controls.Add(this.DocReviewTextBox);
			this.MessageDetailsTabPage.Controls.Add(this.DocReviewRejectReasonTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 436, true);
			this.MessageDetailsTabPage.TabIndex = 0;
			this.MessageDetailsTabPage.Text = "Customs Review Details";
			// 
			// DocReviewTextBox
			// 
			this.DocReviewTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocReviewTextBox, "DISDocuments.Messages.DocReviewComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).DocReviewComment)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DocReviewTextBox, false);
			this.DocReviewTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 36, true);
			this.DocReviewTextBox.Multiline = true;
			this.DocReviewTextBox.Name = "DocReviewTextBox";
			this.DocReviewTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 398, true);
			this.DocReviewTextBox.TabIndex = 2;
			// 
			// DocReviewRejectReasonTextBox
			// 
			this.DocReviewRejectReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocReviewRejectReasonTextBox, "DISDocuments.Messages.DocReviewRejectReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DIS.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).Messages)).SyncRoot)).DocReviewRejectReason)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DocReviewRejectReasonTextBox, false);
			this.DocReviewRejectReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 9, true);
			this.DocReviewRejectReasonTextBox.Name = "DocReviewRejectReasonTextBox";
			this.DocReviewRejectReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.DocReviewRejectReasonTextBox.TabIndex = 1;
			// 
			// MainSplitter
			// 
			this.MainSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.MainSplitter.Name = "MainSplitter";
			this.MainSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 3, true);
			this.MainSplitter.TabIndex = 10;
			this.MainSplitter.TabStop = false;
			// 
			// GrossTonnageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossTonnageCalcEdit, "DISDocuments.CertificateData.GrossTonnage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.GrossTonnage)));
			this.GrossTonnageCalcEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("b69ff1d0-065a-47c3-b7f7-c165aee794a9", "Gross Tonnage");
			this.GrossTonnageCalcEdit.DecimalPlaces = 2;
			this.GrossTonnageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 16, true);
			this.GrossTonnageCalcEdit.Name = "GrossTonnageCalcEdit";
			this.GrossTonnageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.GrossTonnageCalcEdit.TabIndex = 2;
			this.GrossTonnageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.NetTonnageCalcEdit, "DISDocuments.CertificateData.NetTonnage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DIS.Business.DISDocument)(((System.Collections.IList)(((Enterprise.Customs.US.DIS.Business.DISHostWrapper)(null)).DISDocuments)).SyncRoot)).CertificateData.NetTonnage)));
			this.NetTonnageCalcEdit.CaptionResourceString = Enterprise.Customs.US.DIS.GUI.Res.GetData("be3efaa8-3613-46c5-a579-92a63f8806bb", "Net Tonnage");
			this.NetTonnageCalcEdit.DecimalPlaces = 2;
			this.NetTonnageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 42, true);
			this.NetTonnageCalcEdit.Name = "NetTonnageCalcEdit";
			this.NetTonnageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.NetTonnageCalcEdit.TabIndex = 5;
			this.NetTonnageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DISUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitter);
			this.Controls.Add(this.BottomDetailsPanel);
			this.Controls.Add(this.RequiredDocumentsPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 627, true);
			this.Name = "DISUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 627, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RequiredDocumentsPanel.ResumeLayout(false);
			this.RequiredDocumentsPanel.PerformLayout();
			this.DISDocumentsGroupBox.ResumeLayout(false);
			this.DISDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			this.DocumentsGrid.ResumeLayout(false);
			this.DocumentsGrid.PerformLayout();
			this.BottomDetailsPanel.ResumeLayout(false);
			this.BottomDetailsPanel.PerformLayout();
			this.DetailsMainPanel.ResumeLayout(false);
			this.DetailsMainPanel.PerformLayout();
			this.OptionalDataTabControl.ResumeLayout(false);
			this.OptionalDataTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ShipmentNoDropEdit.ResumeLayout(true);
			this.ShipmentNoDropEdit.PerformLayout();
			this.DocReviewGroupBox.ResumeLayout(false);
			this.DocReviewGroupBox.PerformLayout();
			this.DocumentLabelCodeFindBox.ResumeLayout(true);
			this.DocumentLabelCodeFindBox.PerformLayout();
			this.EDocsDocumentGuidDropEdit.ResumeLayout(true);
			this.EDocsDocumentGuidDropEdit.PerformLayout();
			this.AdditionalDataGroupBox.ResumeLayout(false);
			this.AdditionalDataGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDataGrid)).EndInit();
			this.AdditionalDataGrid.ResumeLayout(false);
			this.AdditionalDataGrid.PerformLayout();
			this.PGAGroupBox.ResumeLayout(false);
			this.PGAGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PGAGrid)).EndInit();
			this.PGAGrid.ResumeLayout(false);
			this.PGAGrid.PerformLayout();
			this.CBPRequestGroupBox.ResumeLayout(false);
			this.CBPRequestGroupBox.PerformLayout();
			this.RequestTypeDropEdit.ResumeLayout(true);
			this.RequestTypeDropEdit.PerformLayout();
			this.RequestIDDropEdit.ResumeLayout(true);
			this.RequestIDDropEdit.PerformLayout();
			this.RequestDateEdit.ResumeLayout(true);
			this.RequestDateEdit.PerformLayout();
			this.OptionalDataTabPage.ResumeLayout(false);
			this.OptionalDataTabPage.PerformLayout();
			this.OptionalDataTabPagePanel.ResumeLayout(false);
			this.OptionalDataTabPagePanel.PerformLayout();
			this.InvoiceCommodityPanel.ResumeLayout(false);
			this.InvoiceCommodityPanel.PerformLayout();
			this.CommodityGroupBox.ResumeLayout(false);
			this.CommodityGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommoditiesGrid)).EndInit();
			this.CommoditiesGrid.ResumeLayout(false);
			this.CommoditiesGrid.PerformLayout();
			this.InvoiceGroupBox.ResumeLayout(false);
			this.InvoiceGroupBox.PerformLayout();
			this.InvoiceNumberDropEdit.ResumeLayout(true);
			this.InvoiceNumberDropEdit.PerformLayout();
			this.InvoiceLinesGroupBox.ResumeLayout(false);
			this.InvoiceLinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLinesGrid)).EndInit();
			this.InvoiceLinesGrid.ResumeLayout(false);
			this.InvoiceLinesGrid.PerformLayout();
			this.OptionalDataTopPanel.ResumeLayout(false);
			this.OptionalDataTopPanel.PerformLayout();
			this.PermitGroupBox.ResumeLayout(false);
			this.PermitGroupBox.PerformLayout();
			this.ValidToDateEdit.ResumeLayout(true);
			this.ValidToDateEdit.PerformLayout();
			this.PermitStartDateEdit.ResumeLayout(true);
			this.PermitStartDateEdit.PerformLayout();
			this.BondDataGroupBox.ResumeLayout(false);
			this.BondDataGroupBox.PerformLayout();
			this.BondNameTypeDropEdit.ResumeLayout(true);
			this.BondNameTypeDropEdit.PerformLayout();
			this.DefaultBondCodeDropEdit.ResumeLayout(true);
			this.DefaultBondCodeDropEdit.PerformLayout();
			this.PackingListGroupBox.ResumeLayout(false);
			this.PackingListGroupBox.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.CertificateIssueDateEdit.ResumeLayout(true);
			this.CertificateIssueDateEdit.PerformLayout();
			this.CertificateExpiryDateEdit.ResumeLayout(true);
			this.CertificateExpiryDateEdit.PerformLayout();
			this.ToxicSubstanceGroupBox.ResumeLayout(false);
			this.ToxicSubstanceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CASNumbersGrid)).EndInit();
			this.CASNumbersGrid.ResumeLayout(false);
			this.CASNumbersGrid.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesGridPanel.ResumeLayout(false);
			this.MessagesGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.InterpretedPanel.ResumeLayout(false);
			this.InterpretedPanel.PerformLayout();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageTabControl.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZPanel RequiredDocumentsPanel;
		protected ZArchitecture.GUI.ZPanel BottomDetailsPanel;
		private ZArchitecture.GUI.ZPanel DetailsMainPanel;
		private ZArchitecture.GUI.ZTabControl OptionalDataTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZArchitecture.ZTextBox CommentTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZGroupBox PGAGroupBox;
		private ZArchitecture.ZGrid PGAGrid;
		private ZArchitecture.ZTextBox DISIDTextBox;
		private ZArchitecture.GUI.ZTabPage OptionalDataTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private ZArchitecture.GUI.ZGroupBox AdditionalDataGroupBox;
		private ZArchitecture.ZGrid AdditionalDataGrid;
		private ZArchitecture.GUI.ZGuidDropEdit EDocsDocumentGuidDropEdit;
		private ZArchitecture.GUI.ZGroupBox CBPRequestGroupBox;
		private ZArchitecture.GUI.ZDropEdit RequestTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit RequestIDDropEdit;
		private ZArchitecture.GUI.ZDateEdit RequestDateEdit;
		private ZArchitecture.ZLabel OptionalDataUnavailableLabel;
		private ZArchitecture.GUI.ZPanel OptionalDataTabPagePanel;
		private ZArchitecture.GUI.ZPanel InvoiceCommodityPanel;
		private ZArchitecture.GUI.ZGroupBox InvoiceGroupBox;
		private ZArchitecture.GUI.ZDropEdit InvoiceNumberDropEdit;
		private ZArchitecture.GUI.ZGroupBox InvoiceLinesGroupBox;
		private ZArchitecture.ZGrid InvoiceLinesGrid;
		private ZArchitecture.GUI.ZGroupBox CommodityGroupBox;
		private ZArchitecture.ZGrid CommoditiesGrid;
		private ZArchitecture.GUI.ZPanel OptionalDataTopPanel;
		private ZArchitecture.GUI.ZGroupBox ToxicSubstanceGroupBox;
		private ZArchitecture.ZGrid CASNumbersGrid;
		private ZArchitecture.ZTextBox EPARegoTextBox;
		private ZArchitecture.ZTextBox EPAProducerEstTextBox;
		private ZArchitecture.GUI.ZGroupBox PermitGroupBox;
		private ZArchitecture.ZTextBox PermitTypeTextBox;
		private ZArchitecture.ZTextBox PermitStatementTextBox;
		private ZArchitecture.GUI.ZDateEdit ValidToDateEdit;
		private ZArchitecture.ZTextBox ApprovalNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit PermitStartDateEdit;
		private ZArchitecture.ZTextBox PermitNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox BondDataGroupBox;
		private ZArchitecture.ZTextBox BondTypeTextBox;
		private ZArchitecture.GUI.ZDropEdit BondNameTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit DefaultBondCodeDropEdit;
		private ZArchitecture.ZCalcEdit BondAmountCalcEdit;
		private ZArchitecture.ZTextBox SuretyCodeTextBox;
		private ZArchitecture.ZTextBox AgentIDTextBox;
		private ZArchitecture.ZTextBox BondNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox PackingListGroupBox;
		private ZArchitecture.ZTextBox PackingListInvoiceNoTextBox;
		private ZArchitecture.ZTextBox PurchaseOrderTextBox;
		private ZArchitecture.ZTextBox PackingListNoTextBox;
		private ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		private ZArchitecture.ZTextBox CertificateTypeTextBox;
		private ZArchitecture.ZTextBox CertificateStatementTextBox;
		private ZArchitecture.ZTextBox InspectionLocationTextBox;
		private ZArchitecture.GUI.ZDateEdit CertificateIssueDateEdit;
		private ZArchitecture.GUI.ZDateEdit CertificateExpiryDateEdit;
		private ZArchitecture.ZTextBox CertificateNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox DocumentLabelCodeFindBox;
		private ZArchitecture.GUI.ZPanel MessagesGridPanel;
		private ZArchitecture.ZGrid MessagesGrid;
		private ZArchitecture.GUI.ZPanel InterpretedPanel;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZArchitecture.GUI.ZGroupBox DISDocumentsGroupBox;
		protected internal ZArchitecture.ZGrid DocumentsGrid;
		private CargoWise.Windows.UI.KSplitter MainSplitter;
		private ZArchitecture.GUI.ZGroupBox DocReviewGroupBox;
		private ZArchitecture.ZTextBox DocRejectReasonTextBox;
		private ZArchitecture.ZTextBox ReviewCommentTextBox;
		protected ZArchitecture.GUI.ZTabControl MessageTabControl;
		protected ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		private ZArchitecture.ZTextBox InterpretedTextBox;
		protected ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		private ZArchitecture.ZTextBox DocReviewTextBox;
		private ZArchitecture.ZTextBox DocReviewRejectReasonTextBox;
		private ZArchitecture.GUI.ZDropEdit ShipmentNoDropEdit;
		private ZArchitecture.ZCalcEdit NetTonnageCalcEdit;
		private ZArchitecture.ZCalcEdit GrossTonnageCalcEdit;
	}
}
