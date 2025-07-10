using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class ProductEntryUserControl
	{
		private ZGroupBox PickFaceGroupBox;
		private ZGroupBox ClientWhsDetailsGroupBox;
		private ZDropEdit StocktakeCycleDropEdit;
		private ZGrid zGrid2;
		private MasterFiles.GUI.ZOrganisationFindBox zOrganisationFindBox1;
		private ZGuidFindBox zGuidFindBox1;
		private ZCalcEdit zCalcEdit2;
		private ZCalcEdit zCalcEdit1;
		private ZCalcEdit zCalcEdit3;
		private ZCalcEdit zCalcEdit4;
		private ZTextBox zTextBox1;
		private CargoWise.Windows.UI.KPanel panel2;
		private CargoWise.Windows.UI.KSplitter oSplitter2;
		private ZPanel panel1;
		private ZGroupBox ApparelGroupBox;
		private ZGuidFindBox ProductStyleGuidFindBox;
		private ZGrid zGrid1;
		private ZGuidDropEdit ProductStyleSizeGuidDropEdit;
		private ZTextBox StyleOwnerTextBox;
		private ZLinkLabel ProductionRulesEngineLinkLabel;
		private ZGuidDropEdit ProductStyleColourGuidDropEdit;
		private ZGuidDropEdit ProductStyleClassificationGuidDropEdit;

		private void InitializeComponent()
		{
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new ZGuidDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			this.ClientWhsDetailsGroupBox = new ZGroupBox();
			this.zGrid2 = new ZGrid();
			this.panel2 = new CargoWise.Windows.UI.KPanel();
			this.ProductionRulesEngineLinkLabel = new ZLinkLabel();
			this.zTextBox1 = new ZTextBox();
			this.zCalcEdit3 = new ZCalcEdit();
			this.zOrganisationFindBox1 = new MasterFiles.GUI.ZOrganisationFindBox();
			this.zCalcEdit2 = new ZCalcEdit();
			this.zCalcEdit4 = new ZCalcEdit();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.StocktakeCycleDropEdit = new ZDropEdit();
			this.zCalcEdit1 = new ZCalcEdit();
			this.PickFaceGroupBox = new ZGroupBox();
			this.zGrid1 = new ZGrid();
			this.oSplitter2 = new CargoWise.Windows.UI.KSplitter();
			this.panel1 = new ZPanel();
			this.ApparelGroupBox = new ZGroupBox();
			this.StyleOwnerTextBox = new ZTextBox();
			this.ProductStyleSizeGuidDropEdit = new ZGuidDropEdit();
			this.ProductStyleGuidFindBox = new ZGuidFindBox();
			this.ProductStyleColourGuidDropEdit = new ZGuidDropEdit();
			this.ProductStyleClassificationGuidDropEdit = new ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClientWhsDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.zGrid2.SuspendLayout();
			this.panel2.SuspendLayout();
			this.zOrganisationFindBox1.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.StocktakeCycleDropEdit.SuspendLayout();
			this.PickFaceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.ApparelGroupBox.SuspendLayout();
			this.ProductStyleSizeGuidDropEdit.SuspendLayout();
			this.ProductStyleGuidFindBox.SuspendLayout();
			this.ProductStyleColourGuidDropEdit.SuspendLayout();
			this.ProductStyleClassificationGuidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsProduct);
			// 
			// ClientWhsDetailsGroupBox
			// 
			this.ClientWhsDetailsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ProductEntryUserControl|6575d7da-524d-4e32-98f7-537a9cd3ac89", "Client / Warehouse Details");
			this.ClientWhsDetailsGroupBox.Controls.Add(this.zGrid2);
			this.ClientWhsDetailsGroupBox.Controls.Add(this.panel2);
			this.ClientWhsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientWhsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientWhsDetailsGroupBox.Name = "ClientWhsDetailsGroupBox";
			this.ClientWhsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1060, 299, true);
			this.ClientWhsDetailsGroupBox.TabIndex = 0;
			this.ClientWhsDetailsGroupBox.TabStop = false;
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid2, "ParamsByWhsAndClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_WW)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_WL_StagingLocationBOM)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_WL_InwardsProcessingStagingLocationBOM)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_WA_DynamicPickFaceArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_F3_NKReceivedPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_F3_NKReleasedPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_StockTakeCycle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ExpiryNotificationPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_MaximumShelfLife)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ReplenishmentMinimum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_EconomicQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ReplenishmentMultiple)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).UQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ABCAnalysisCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ABCAnalysisPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).PickGroupForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_WPG_PutawayGroup)));
			this.zGrid2.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "W3_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "W3_WW";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "W3_WL_StagingLocationBOM";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "W3_WL_InwardsProcessingStagingLocationBOM";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "W3_WA_DynamicPickFaceArea";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "W3_F3_NKReceivedPackType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "W3_F3_NKReleasedPackType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "W3_StockTakeCycle";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "W3_ExpiryNotificationPeriod";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "W3_MaximumShelfLife";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.ColumnName = "W3_ReplenishmentMinimum";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo4.ColumnName = "W3_EconomicQuantity";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo5.ColumnName = "W3_ReplenishmentMultiple";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ProductEntryUserControl|915fe906-eab9-43ab-be7e-637324cbfb1a", "UQ");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "UQ";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("f6a05e1b-230c-4e58-bceb-db16470bec2d", "ABC Category");
			zTextBoxColumnStyleInfo2.ColumnName = "W3_ABCAnalysisCategory";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("6cb82687-d110-4041-9848-25a69ed3f51d", "ABC Period");
			zTextBoxColumnStyleInfo3.ColumnName = "W3_ABCAnalysisPeriod";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("8e7bc035-0468-4916-bede-cb5221137b38", "Pick Group");
			zDropEditColumnStyleInfo4.ColumnName = "PickGroupForBinding";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "W3_WPG_PutawayGroup";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid2.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.zGrid2.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.zGrid2.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.zGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid2.GridId = "32a40fd9-efde-4a4c-b9e8-a76c742a78a3";
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid2";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 154, true);
			this.zGrid2.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.ProductionRulesEngineLinkLabel);
			this.panel2.Controls.Add(this.zTextBox1);
			this.panel2.Controls.Add(this.zCalcEdit3);
			this.panel2.Controls.Add(this.zOrganisationFindBox1);
			this.panel2.Controls.Add(this.zCalcEdit2);
			this.panel2.Controls.Add(this.zCalcEdit4);
			this.panel2.Controls.Add(this.zGuidFindBox1);
			this.panel2.Controls.Add(this.StocktakeCycleDropEdit);
			this.panel2.Controls.Add(this.zCalcEdit1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 188, true);
			this.panel2.Name = "panel2";
			this.panel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 108, true);
			this.panel2.TabIndex = 3;
			// 
			// ProductionRulesEngineLinkLabel
			// 
			this.ProductionRulesEngineLinkLabel.AutoSize = true;
			this.ProductionRulesEngineLinkLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("dce85c34-4def-4a68-8f42-e02d728960f7", "Setup Putaway Rules");
			this.ProductionRulesEngineLinkLabel.IsFontBold = false;
			this.ProductionRulesEngineLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(721, 82, true);
			this.ProductionRulesEngineLinkLabel.Name = "ProductionRulesEngineLinkLabel";
			this.ProductionRulesEngineLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 13, true);
			this.ProductionRulesEngineLinkLabel.TabIndex = 16;
			this.ProductionRulesEngineLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ProductionRulesEngineLinkLabel_LinkClicked);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "ParamsByWhsAndClient.UQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).UQ)));
			this.zTextBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ProductEntryUserControl|7aabe9d4-fcc9-43e1-9b9a-7462a6cd8fb8", "Unit of Quantity");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 79, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 38, true);
			this.zTextBox1.TabIndex = 15;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "ParamsByWhsAndClient.W3_ExpiryNotificationPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ExpiryNotificationPeriod)));
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 10, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 38, true);
			this.zCalcEdit3.TabIndex = 9;
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zOrganisationFindBox1
			// 
			this.zOrganisationFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zOrganisationFindBox1, "ParamsByWhsAndClient.W3_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_OH)));
			this.zOrganisationFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 10, true);
			this.zOrganisationFindBox1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zOrganisationFindBox1.Name = "zOrganisationFindBox1";
			this.zOrganisationFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 38, true);
			this.zOrganisationFindBox1.TabIndex = 1;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "ParamsByWhsAndClient.W3_ReplenishmentMinimum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ReplenishmentMinimum)));
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 33, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 38, true);
			this.zCalcEdit2.TabIndex = 11;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "ParamsByWhsAndClient.W3_ReplenishmentMultiple");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_ReplenishmentMultiple)));
			this.zCalcEdit4.DecimalPlaces = 2;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(832, 10, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 38, true);
			this.zCalcEdit4.TabIndex = 14;
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "ParamsByWhsAndClient.W3_WW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_WW)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 33, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zGuidFindBox1.ParentType = null;
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 38, true);
			this.zGuidFindBox1.TabIndex = 3;
			// 
			// StocktakeCycleDropEdit
			// 
			this.StocktakeCycleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StocktakeCycleDropEdit, "ParamsByWhsAndClient.W3_StockTakeCycle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_StockTakeCycle)));
			this.StocktakeCycleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 56, true);
			this.StocktakeCycleDropEdit.Name = "StocktakeCycleDropEdit";
			this.StocktakeCycleDropEdit.PreBoundMaxLength = 4;
			this.StocktakeCycleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 38, true);
			this.StocktakeCycleDropEdit.TabIndex = 7;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "ParamsByWhsAndClient.W3_EconomicQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsProductParamsByWhsAndClient)(((System.Collections.IList)(((WhsProduct)(null)).ParamsByWhsAndClient)).SyncRoot)).W3_EconomicQuantity)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 56, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 38, true);
			this.zCalcEdit1.TabIndex = 13;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickFaceGroupBox
			// 
			this.PickFaceGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ProductEntryUserControl|5a4eb128-407e-4ff6-a47a-3f455f828e10", "Pick Faces");
			this.PickFaceGroupBox.Controls.Add(this.zGrid1);
			this.PickFaceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickFaceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PickFaceGroupBox.Name = "PickFaceGroupBox";
			this.PickFaceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 144, true);
			this.PickFaceGroupBox.TabIndex = 1;
			this.PickFaceGroupBox.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "PickFaces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).WF_OH_Client)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).LocationWhsGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).LocationString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).WF_ReplenishMinimum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).WF_ReplenishMaximum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).SupplierPart.OP_CountDecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Environment.Business.WhsPickFace)(((System.Collections.IList)(((WhsProduct)(null)).PickFaces)).SyncRoot)).WF_ReplenishmentMultiple)));
			this.zGrid1.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "WF_OH_Client";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ProductEntryUserControl|58416c1e-7de8-4ad8-8ff9-5674f6ef6f4e", "Warehouse");
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "LocationWhsGuid";
			zGuidDropEditColumnStyleInfo2.IsMandatory = true;
			zGuidDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.AutoCompleteDisabled = true;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ProductEntryUserControl|80e4d423-1e25-4dab-aa7f-6ad8922e8fa8", "Location");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "LocationString";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Enter the Pickface location here";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "WF_ReplenishMinimum";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.IsMandatory = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "WF_ReplenishMaximum";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.IsMandatory = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("31ad596f-370d-4b97-bb27-dfef3cec7054", "Replenish Multiple");
			zCalcEditColumnStyleInfo8.ColumnName = "WF_ReplenishmentMultiple";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "d4731e46-910b-4dd9-9fae-66c17271ecc2";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 85, true);
			this.zGrid1.TabIndex = 0;
			// 
			// oSplitter2
			// 
			this.oSplitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.oSplitter2.DoNotSaveSplitterLayout = false;
			this.oSplitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 447, true);
			this.oSplitter2.Name = "oSplitter2";
			this.oSplitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1060, 3, true);
			this.oSplitter2.TabIndex = 2;
			this.oSplitter2.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.PickFaceGroupBox);
			this.panel1.Controls.Add(this.ApparelGroupBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 299, true);
			this.panel1.Name = "panel1";
			this.panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1060, 148, true);
			this.panel1.TabIndex = 3;
			// 
			// ApparelGroupBox
			// 
			this.ApparelGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("4b90bde1-7f3a-4643-9e53-364390d96fd3", "Product Style");
			this.ApparelGroupBox.Controls.Add(this.StyleOwnerTextBox);
			this.ApparelGroupBox.Controls.Add(this.ProductStyleSizeGuidDropEdit);
			this.ApparelGroupBox.Controls.Add(this.ProductStyleGuidFindBox);
			this.ApparelGroupBox.Controls.Add(this.ProductStyleColourGuidDropEdit);
			this.ApparelGroupBox.Controls.Add(this.ProductStyleClassificationGuidDropEdit);
			this.ApparelGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ApparelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 2, true);
			this.ApparelGroupBox.Name = "ApparelGroupBox";
			this.ApparelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 144, true);
			this.ApparelGroupBox.TabIndex = 2;
			this.ApparelGroupBox.TabStop = false;
			// 
			// StyleOwnerTextBox
			// 
			this.BindingSource.SetBindingMember(this.StyleOwnerTextBox, "ProductStyleOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsProduct)(null)).ProductStyleOwner)));
			this.StyleOwnerTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("f1d8f1a6-995f-405b-9609-371142d7fc9f", "Owner");
			this.StyleOwnerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 112, true);
			this.StyleOwnerTextBox.Name = "StyleOwnerTextBox";
			this.StyleOwnerTextBox.ReadOnly = true;
			this.StyleOwnerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 38, true);
			this.StyleOwnerTextBox.TabIndex = 12;
			// 
			// ProductStyleSizeGuidDropEdit
			// 
			this.ProductStyleSizeGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductStyleSizeGuidDropEdit, "ProductStyleSizePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsProduct)(null)).ProductStyleSizePK)));
			this.ProductStyleSizeGuidDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("2175c8e7-a85f-400f-bfbf-5d11a35f2975", "Size");
			this.ProductStyleSizeGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 67, true);
			this.ProductStyleSizeGuidDropEdit.Name = "ProductStyleSizeGuidDropEdit";
			this.ProductStyleSizeGuidDropEdit.PreBoundMaxLength = 4;
			this.ProductStyleSizeGuidDropEdit.ShowDescriptionBox = false;
			this.ProductStyleSizeGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ProductStyleSizeGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 38, true);
			this.ProductStyleSizeGuidDropEdit.TabIndex = 10;
			// 
			// ProductStyleGuidFindBox
			// 
			this.ProductStyleGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductStyleGuidFindBox, "ProductStylePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsProduct)(null)).ProductStylePK)));
			this.ProductStyleGuidFindBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("6e824fe9-07b6-4e45-923d-5725eda87a26", "Style");
			this.ProductStyleGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 22, true);
			this.ProductStyleGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigProductStyle;
			this.ProductStyleGuidFindBox.Name = "ProductStyleGuidFindBox";
			this.ProductStyleGuidFindBox.PreBoundMaxLength = 20;
			this.ProductStyleGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 38, true);
			this.ProductStyleGuidFindBox.TabIndex = 8;
			// 
			// ProductStyleColourGuidDropEdit
			// 
			this.ProductStyleColourGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductStyleColourGuidDropEdit, "ProductStyleColourPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsProduct)(null)).ProductStyleColourPK)));
			this.ProductStyleColourGuidDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("214eff4c-27b4-405e-b58b-39ca8f02682c", "Color");
			this.ProductStyleColourGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 45, true);
			this.ProductStyleColourGuidDropEdit.Name = "ProductStyleColourGuidDropEdit";
			this.ProductStyleColourGuidDropEdit.PreBoundMaxLength = 3;
			this.ProductStyleColourGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 38, true);
			this.ProductStyleColourGuidDropEdit.TabIndex = 9;
			// 
			// ProductStyleClassificationGuidDropEdit
			// 
			this.ProductStyleClassificationGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductStyleClassificationGuidDropEdit, "ProductStyleClassificationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsProduct)(null)).ProductStyleClassificationPK)));
			this.ProductStyleClassificationGuidDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("80723522-5a67-4e27-b269-291cf9eecec6", "Classification");
			this.ProductStyleClassificationGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 89, true);
			this.ProductStyleClassificationGuidDropEdit.Name = "ProductStyleClassificationGuidDropEdit";
			this.ProductStyleClassificationGuidDropEdit.PreBoundMaxLength = 3;
			this.ProductStyleClassificationGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 38, true);
			this.ProductStyleClassificationGuidDropEdit.TabIndex = 11;
			// 
			// ProductEntryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClientWhsDetailsGroupBox);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.oSplitter2);
			this.Name = "ProductEntryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1060, 450, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClientWhsDetailsGroupBox.ResumeLayout(false);
			this.ClientWhsDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.zGrid2.ResumeLayout(false);
			this.zGrid2.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.zOrganisationFindBox1.ResumeLayout(true);
			this.zOrganisationFindBox1.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.StocktakeCycleDropEdit.ResumeLayout(true);
			this.StocktakeCycleDropEdit.PerformLayout();
			this.PickFaceGroupBox.ResumeLayout(false);
			this.PickFaceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ApparelGroupBox.ResumeLayout(false);
			this.ApparelGroupBox.PerformLayout();
			this.ProductStyleSizeGuidDropEdit.ResumeLayout(true);
			this.ProductStyleSizeGuidDropEdit.PerformLayout();
			this.ProductStyleGuidFindBox.ResumeLayout(true);
			this.ProductStyleGuidFindBox.PerformLayout();
			this.ProductStyleColourGuidDropEdit.ResumeLayout(true);
			this.ProductStyleColourGuidDropEdit.PerformLayout();
			this.ProductStyleClassificationGuidDropEdit.ResumeLayout(true);
			this.ProductStyleClassificationGuidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
