namespace Enterprise.MasterFiles.GUI
{
	partial class DefineGLSectionsForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProfitAndLossGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AppropriationEndGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AppropriationStartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.OverheadsEndGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OverheadsStartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.TradingStatementEndGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TradingStatementStartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TradingStatementHeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BalanceSheetGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LiabilitiesEndGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LiabilitiesStartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.AssetsEndGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AssetsStartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.OwnersEquityEndGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OwnersEquityStartGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProfitAndLossGroupBox.SuspendLayout();
			this.BalanceSheetGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 517, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 76, true);
			this.InstructionsLabel.TabIndex = 3;
			this.InstructionsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ProfitAndLossGroupBox
			// 
			this.ProfitAndLossGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|4fbf5de6-5938-4243-afd5-39f8907c8080", "Profit And Loss");
			this.ProfitAndLossGroupBox.Controls.Add(this.AppropriationEndGuidFindBox);
			this.ProfitAndLossGroupBox.Controls.Add(this.AppropriationStartGuidFindBox);
			this.ProfitAndLossGroupBox.Controls.Add(this.zLabel6);
			this.ProfitAndLossGroupBox.Controls.Add(this.OverheadsEndGuidFindBox);
			this.ProfitAndLossGroupBox.Controls.Add(this.OverheadsStartGuidFindBox);
			this.ProfitAndLossGroupBox.Controls.Add(this.zLabel3);
			this.ProfitAndLossGroupBox.Controls.Add(this.TradingStatementEndGuidFindBox);
			this.ProfitAndLossGroupBox.Controls.Add(this.TradingStatementStartGuidFindBox);
			this.ProfitAndLossGroupBox.Controls.Add(this.TradingStatementHeadingLabel);
			this.ProfitAndLossGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 88, true);
			this.ProfitAndLossGroupBox.Name = "ProfitAndLossGroupBox";
			this.ProfitAndLossGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 192, true);
			this.ProfitAndLossGroupBox.TabIndex = 4;
			this.ProfitAndLossGroupBox.TabStop = false;
			// 
			// AppropriationEndGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AppropriationEndGuidFindBox, "AG_ProfitAndLossAppropriationEndAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_ProfitAndLossAppropriationEndAccount)));
			this.AppropriationEndGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|e02cd8ff-afa3-4955-88f8-34c7d013b1b2", "End");
			this.AppropriationEndGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 156, true);
			this.AppropriationEndGuidFindBox.Name = "AppropriationEndGuidFindBox";
			this.AppropriationEndGuidFindBox.PopupCaption = null;
			this.AppropriationEndGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AppropriationEndGuidFindBox.TabIndex = 16;
			// 
			// AppropriationStartGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AppropriationStartGuidFindBox, "AG_ProfitAndLossAppropriationStartAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_ProfitAndLossAppropriationStartAccount)));
			this.AppropriationStartGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|33cc0e04-740b-48f1-a1f7-2e980a3c1e52", "Start");
			this.AppropriationStartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 157, true);
			this.AppropriationStartGuidFindBox.Name = "AppropriationStartGuidFindBox";
			this.AppropriationStartGuidFindBox.PopupCaption = null;
			this.AppropriationStartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AppropriationStartGuidFindBox.TabIndex = 15;
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|abb271ac-2c07-4ec8-ad48-b04f726db195", "Profit and Loss Appropriation");
			this.zLabel6.IsFontBold = true;
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 137, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 13, true);
			this.zLabel6.TabIndex = 12;
			// 
			// OverheadsEndGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OverheadsEndGuidFindBox, "AG_OverheadsEndAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_OverheadsEndAccount)));
			this.OverheadsEndGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|4da71ff2-3e7f-4191-9cb1-4a69f62c15a9", "End");
			this.OverheadsEndGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 100, true);
			this.OverheadsEndGuidFindBox.Name = "OverheadsEndGuidFindBox";
			this.OverheadsEndGuidFindBox.PopupCaption = null;
			this.OverheadsEndGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OverheadsEndGuidFindBox.TabIndex = 11;
			// 
			// OverheadsStartGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OverheadsStartGuidFindBox, "AG_OverheadsStartAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_OverheadsStartAccount)));
			this.OverheadsStartGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|402ebf9d-7e65-40f8-83bf-b5ac5f8bbea7", "Start");
			this.OverheadsStartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 100, true);
			this.OverheadsStartGuidFindBox.Name = "OverheadsStartGuidFindBox";
			this.OverheadsStartGuidFindBox.PopupCaption = null;
			this.OverheadsStartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OverheadsStartGuidFindBox.TabIndex = 10;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|15b9a6c7-95ef-4681-ab4f-a27b4a8ed077", "Overheads");
			this.zLabel3.IsFontBold = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 80, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.zLabel3.TabIndex = 7;
			// 
			// TradingStatementEndGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.TradingStatementEndGuidFindBox, "AG_TradingStatementEndAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_TradingStatementEndAccount)));
			this.TradingStatementEndGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|f41086a4-23e6-438f-86e7-cb47db5a82bc", "End");
			this.TradingStatementEndGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 44, true);
			this.TradingStatementEndGuidFindBox.Name = "TradingStatementEndGuidFindBox";
			this.TradingStatementEndGuidFindBox.PopupCaption = null;
			this.TradingStatementEndGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TradingStatementEndGuidFindBox.TabIndex = 6;
			// 
			// TradingStatementStartGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.TradingStatementStartGuidFindBox, "AG_TradingStatementStartAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_TradingStatementStartAccount)));
			this.TradingStatementStartGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|745a1df4-5ae0-456f-8211-82cb0185bf21", "Start");
			this.TradingStatementStartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 44, true);
			this.TradingStatementStartGuidFindBox.Name = "TradingStatementStartGuidFindBox";
			this.TradingStatementStartGuidFindBox.PopupCaption = null;
			this.TradingStatementStartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TradingStatementStartGuidFindBox.TabIndex = 5;
			// 
			// TradingStatementHeadingLabel
			// 
			this.TradingStatementHeadingLabel.AutoSize = true;
			this.TradingStatementHeadingLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|1fb24fef-5206-408a-9ab3-1608dfc2b6a5", "Trading Statement");
			this.TradingStatementHeadingLabel.IsFontBold = true;
			this.TradingStatementHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 24, true);
			this.TradingStatementHeadingLabel.Name = "TradingStatementHeadingLabel";
			this.TradingStatementHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 13, true);
			this.TradingStatementHeadingLabel.TabIndex = 2;
			// 
			// BalanceSheetGroupBox
			// 
			this.BalanceSheetGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|c939b066-e21c-4054-b486-3380d30f8d6c", "Balance Sheet");
			this.BalanceSheetGroupBox.Controls.Add(this.LiabilitiesEndGuidFindBox);
			this.BalanceSheetGroupBox.Controls.Add(this.LiabilitiesStartGuidFindBox);
			this.BalanceSheetGroupBox.Controls.Add(this.zLabel9);
			this.BalanceSheetGroupBox.Controls.Add(this.AssetsEndGuidFindBox);
			this.BalanceSheetGroupBox.Controls.Add(this.AssetsStartGuidFindBox);
			this.BalanceSheetGroupBox.Controls.Add(this.zLabel12);
			this.BalanceSheetGroupBox.Controls.Add(this.OwnersEquityEndGuidFindBox);
			this.BalanceSheetGroupBox.Controls.Add(this.OwnersEquityStartGuidFindBox);
			this.BalanceSheetGroupBox.Controls.Add(this.zLabel15);
			this.BalanceSheetGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 286, true);
			this.BalanceSheetGroupBox.Name = "BalanceSheetGroupBox";
			this.BalanceSheetGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 192, true);
			this.BalanceSheetGroupBox.TabIndex = 5;
			this.BalanceSheetGroupBox.TabStop = false;
			// 
			// LiabilitiesEndGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.LiabilitiesEndGuidFindBox, "AG_LiabilitiesEndAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_LiabilitiesEndAccount)));
			this.LiabilitiesEndGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|6415ecae-59da-4407-a0a3-3cf2dcd36cd0", "End");
			this.LiabilitiesEndGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 156, true);
			this.LiabilitiesEndGuidFindBox.Name = "LiabilitiesEndGuidFindBox";
			this.LiabilitiesEndGuidFindBox.PopupCaption = null;
			this.LiabilitiesEndGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.LiabilitiesEndGuidFindBox.TabIndex = 16;
			// 
			// LiabilitiesStartGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.LiabilitiesStartGuidFindBox, "AG_LiabilitiesStartAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_LiabilitiesStartAccount)));
			this.LiabilitiesStartGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|2ad1ea03-ece5-4322-805d-5ec532b470d7", "Start");
			this.LiabilitiesStartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 157, true);
			this.LiabilitiesStartGuidFindBox.Name = "LiabilitiesStartGuidFindBox";
			this.LiabilitiesStartGuidFindBox.PopupCaption = null;
			this.LiabilitiesStartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.LiabilitiesStartGuidFindBox.TabIndex = 15;
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.zLabel9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|9b25637b-ee72-4f16-9b35-eb05ecea76cb", "Liabilities");
			this.zLabel9.IsFontBold = true;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 137, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.zLabel9.TabIndex = 12;
			// 
			// AssetsEndGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AssetsEndGuidFindBox, "AG_AssetsEndAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_AssetsEndAccount)));
			this.AssetsEndGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|1452f88f-c1d6-4775-beb9-2887a1f75610", "End");
			this.AssetsEndGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 100, true);
			this.AssetsEndGuidFindBox.Name = "AssetsEndGuidFindBox";
			this.AssetsEndGuidFindBox.PopupCaption = null;
			this.AssetsEndGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AssetsEndGuidFindBox.TabIndex = 11;
			// 
			// AssetsStartGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AssetsStartGuidFindBox, "AG_AssetsStartAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_AssetsStartAccount)));
			this.AssetsStartGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|7a37af93-c7bc-42e3-962e-8196efc60edb", "Start");
			this.AssetsStartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 100, true);
			this.AssetsStartGuidFindBox.Name = "AssetsStartGuidFindBox";
			this.AssetsStartGuidFindBox.PopupCaption = null;
			this.AssetsStartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AssetsStartGuidFindBox.TabIndex = 10;
			// 
			// zLabel12
			// 
			this.zLabel12.AutoSize = true;
			this.zLabel12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|cfd43ca6-cfda-4d81-941c-e456202a3ead", "Assets");
			this.zLabel12.IsFontBold = true;
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 80, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.zLabel12.TabIndex = 7;
			// 
			// OwnersEquityEndGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OwnersEquityEndGuidFindBox, "AG_OwnersEquityEndAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_OwnersEquityEndAccount)));
			this.OwnersEquityEndGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|7549c53c-10c0-4a47-911d-bea70f85164c", "End");
			this.OwnersEquityEndGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 44, true);
			this.OwnersEquityEndGuidFindBox.Name = "OwnersEquityEndGuidFindBox";
			this.OwnersEquityEndGuidFindBox.PopupCaption = null;
			this.OwnersEquityEndGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OwnersEquityEndGuidFindBox.TabIndex = 6;
			// 
			// OwnersEquityStartGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OwnersEquityStartGuidFindBox, "AG_OwnersEquityStartAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater)(null)).AG_OwnersEquityStartAccount)));
			this.OwnersEquityStartGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|2ddf2e69-76cf-48c5-bcd0-5eb5ab08ff59", "Start");
			this.OwnersEquityStartGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 44, true);
			this.OwnersEquityStartGuidFindBox.Name = "OwnersEquityStartGuidFindBox";
			this.OwnersEquityStartGuidFindBox.PopupCaption = null;
			this.OwnersEquityStartGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OwnersEquityStartGuidFindBox.TabIndex = 5;
			// 
			// zLabel15
			// 
			this.zLabel15.AutoSize = true;
			this.zLabel15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|6043d9ba-fc6d-4e6c-a78e-8191578d089d", "Owners Equity");
			this.zLabel15.IsFontBold = true;
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 24, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 13, true);
			this.zLabel15.TabIndex = 2;
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|fa40667d-2d62-4eb3-a0e9-aae5501a60ea", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 484, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.SaveButton.TabIndex = 6;
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|f0dcd88a-02c9-49d2-bfe2-85d23a88c708", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 484, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// DefineGLSectionsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 541, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DefineGLSectionsForm|bfcfad14-8e74-4ba2-9c83-317a7fa69f34", "Define General Ledger Report Sections");
			this.Controls.Add(this.InstructionsLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ProfitAndLossGroupBox);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.BalanceSheetGroupBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLHeaderBulkUpdater);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimizeBox = false;
			this.Name = "DefineGLSectionsForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.BalanceSheetGroupBox, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.ProfitAndLossGroupBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProfitAndLossGroupBox.ResumeLayout(false);
			this.ProfitAndLossGroupBox.PerformLayout();
			this.BalanceSheetGroupBox.ResumeLayout(false);
			this.BalanceSheetGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.ZLabel InstructionsLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ProfitAndLossGroupBox;
		private Enterprise.ZArchitecture.ZLabel TradingStatementHeadingLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TradingStatementEndGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TradingStatementStartGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OverheadsEndGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OverheadsStartGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AppropriationEndGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AppropriationStartGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BalanceSheetGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox LiabilitiesEndGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox LiabilitiesStartGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AssetsEndGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AssetsStartGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel12;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OwnersEquityEndGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OwnersEquityStartGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel15;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;

		#endregion
	}
}
