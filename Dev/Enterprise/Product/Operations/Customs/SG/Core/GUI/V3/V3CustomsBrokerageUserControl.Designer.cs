using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V3.GUI
{
	partial class V3CustomsBrokerageUserControl : ZUserControl
	{
		ZGroupBox declarationDetailsGroupBox;
		ZGroupBox messageHistoryGroupBox;
		public ZArchitecture.ZTextBox messageTextTextBox;
		CargoWise.Windows.UI.KSplitter splitter1;
		ZArchitecture.ZGrid messageHistoryGrid;

		#region Auto

		ZArchitecture.ZGrid declarationsGrid;

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			this.declarationsGrid = new ZArchitecture.ZGrid();
			this.declarationDetailsGroupBox = new ZGroupBox();
			this.messageHistoryGroupBox = new ZGroupBox();
			this.messageTextTextBox = new ZArchitecture.ZTextBox();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.messageHistoryGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.declarationsGrid)).BeginInit();
			this.declarationDetailsGroupBox.SuspendLayout();
			this.messageHistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.messageHistoryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// DeclarationsGrid
			// 
			this.declarationsGrid.AllowNavigation = false;
			this.declarationsGrid.AllowSorting = false;
			this.declarationsGrid.BindTo = "Declarations";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Business.V3Brokerage)(null)).Declarations)));
			this.declarationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B0F673DF-7E74-4575-A6C5-2C2105008FE3", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "JE_MessageType";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2C3AEC9E-5718-402A-8D74-B5EB6C272C51", "Sub");
			zTextBoxColumnStyleInfo2.ColumnName = "JE_MessageSubType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1AE8DF2C-8C44-4AC0-BD48-46423EA470A9", "Purpose");
			zTextBoxColumnStyleInfo3.ColumnName = "JE_OwnerRef";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("2AB178B9-3480-445A-9996-1F099F828738", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "JE_EntryStatus";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("7E9C6173-736E-4FBA-9F40-18E69B424593", "Permit No.");
			zTextBoxColumnStyleInfo5.ColumnName = "PermitNumber";
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Importers";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("FFC161C3-BD0F-468B-A952-71E84A7014A4", "Importer");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JE_OH_Importer";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Exporters";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A14CD883-4A60-4259-898A-DBCF63F797C2", "Exporter");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "JE_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("0627E147-5A8E-4C7B-BC03-B1C4180A79AB", "House Bill");
			zTextBoxColumnStyleInfo6.ColumnName = "JE_HouseBill";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("7FD3EB48-5CC6-4B7D-AF77-C6175CAFD1E6", "Master Bill");
			zTextBoxColumnStyleInfo7.ColumnName = "JE_MasterBill";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BF2FB41B-3C18-4212-90F0-13DA44078544", "Loading");
			zTextBoxColumnStyleInfo8.ColumnName = "JE_RL_NKPortOfLoading";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("AD406D1D-00A7-4D2B-83CF-E130A680B2B7", "Discharge");
			zTextBoxColumnStyleInfo9.ColumnName = "JE_RL_NKPortOfArrival";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8D81AF43-01B3-4753-BB2C-27BB8A8B0F3A", "Arrival Date");
			zDateEditColumnStyleInfo1.ColumnName = "JE_DateOfArrival";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("C3F1CE9B-E204-4C92-B321-A184543EF6C2", "Export Date");
			zDateEditColumnStyleInfo2.ColumnName = "JE_ExportDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.declarationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.declarationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.declarationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.declarationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.declarationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.declarationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.declarationsGrid.LayoutKey = "DeclarationsGrid";
			this.declarationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.declarationsGrid.Name = "DeclarationsGrid";
			this.declarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 81, true);
			this.declarationsGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_MessageType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_MessageSubTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_MessageSubType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_OwnerRefInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_OwnerRef)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_EntryStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_EntryStatus)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).PermitNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).PermitNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_OH_Importer)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_OH_ImporterInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Importers)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_OH_Supplier)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_OH_SupplierInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Exporters)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_HouseBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_HouseBill)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_MasterBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_MasterBill)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_RL_NKPortOfLoadingInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_RL_NKPortOfLoading)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_RL_NKPortOfArrivalInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_RL_NKPortOfArrival)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_DateOfArrival)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_DateOfArrivalInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_ExportDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).JE_ExportDateInfo)));
			// 
			// DeclarationDetailsGroupBox
			// 
			this.declarationDetailsGroupBox.Controls.Add(this.declarationsGrid);
			this.declarationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.declarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.declarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.declarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 100, true);
			this.declarationDetailsGroupBox.TabIndex = 6;
			this.declarationDetailsGroupBox.TabStop = false;
			this.declarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("3CBD7818-89C4-4927-A1B2-85556AAE15C3", "TradeNet V3 Declaration Details");
			// 
			// MessageHistoryGroupBox
			// 
			this.messageHistoryGroupBox.Controls.Add(this.messageTextTextBox);
			this.messageHistoryGroupBox.Controls.Add(this.splitter1);
			this.messageHistoryGroupBox.Controls.Add(this.messageHistoryGrid);
			this.messageHistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageHistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.messageHistoryGroupBox.Name = "MessageHistoryGroupBox";
			this.messageHistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 565, true);
			this.messageHistoryGroupBox.TabIndex = 7;
			this.messageHistoryGroupBox.TabStop = false;
			this.messageHistoryGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("31A71A30-179D-453B-BE7D-6746E7FB10FD", "Message History");
			// 
			// MessageTextTextBox
			// 
			this.messageTextTextBox.BindTo = "Declarations.Messages.FormattedMessage";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).FormattedMessageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).FormattedMessage)));
			this.messageTextTextBox.CaptionResourceString = null;
			this.messageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 16, true);
			this.messageTextTextBox.Multiline = true;
			this.messageTextTextBox.Name = "MessageTextTextBox";
			this.messageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 546, true);
			this.messageTextTextBox.TabIndex = 3;
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 16, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 546, true);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// MessageHistoryGrid
			// 
			this.messageHistoryGrid.AllowNavigation = false;
			this.messageHistoryGrid.BindTo = "Declarations.Messages";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)));
			this.messageHistoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("D5E8AB3A-3F3A-4336-B2AD-C3135888FC8D", "Unique Reference No. (URN)");
			zTextBoxColumnStyleInfo10.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("DBE5A227-FAED-4A14-BA96-F1FC2B73A33C", "Status");
			zTextBoxColumnStyleInfo11.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo11.IsMandatory = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9D8AF37D-43A2-47B9-92F9-099321B3A52B", "Message Type");
			zTextBoxColumnStyleInfo12.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo12.IsMandatory = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CFB0D8C7-A993-4AA4-8AE3-C26B7F880185", "Create Time");
			zDateEditColumnStyleInfo3.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.messageHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.messageHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.messageHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.messageHistoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.messageHistoryGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.messageHistoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messageHistoryGrid.LayoutKey = "MessageHistoryGrid";
			this.messageHistoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messageHistoryGrid.Name = "MessageHistoryGrid";
			this.messageHistoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 546, true);
			this.messageHistoryGrid.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_ApplicationReferenceInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_ApplicationReference)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_StatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_Status)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_MessageType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_SystemCreateTimeUtc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Business.V3Message)(((object)(((Business.V3JobDeclaration)(((object)(((Business.V3Brokerage)(null)).Declarations)))).Messages)))).EM_SystemCreateTimeUtcInfo)));
			// 
			// V3CustomsBrokerageUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.messageHistoryGroupBox);
			this.Controls.Add(this.declarationDetailsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.SG.V4.Business";
			this.DataSourceTypeName = "Enterprise.Customs.SG.V3.Business.V3Brokerage";
			this.Name = "V3CustomsBrokerageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 665, true);
			((System.ComponentModel.ISupportInitialize)(this.declarationsGrid)).EndInit();
			this.declarationDetailsGroupBox.ResumeLayout(false);
			this.messageHistoryGroupBox.ResumeLayout(false);
			this.messageHistoryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.messageHistoryGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
