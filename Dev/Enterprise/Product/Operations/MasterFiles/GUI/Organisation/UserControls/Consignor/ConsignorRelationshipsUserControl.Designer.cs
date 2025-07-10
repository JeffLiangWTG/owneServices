using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsignorRelationshipsUserControl
	{
		#region Component Designer generated code

		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZPanel zPanel3;
		internal RelationshipDetailsUserControl RelationshipDetailsUserControl;
		private ZPanel zPanel1;
		private ZPanel zPanel2;
		internal ZGrid OrgBuyerLinkBoundGrid;
		private ZPanel zPanel4;
		private ZDropEdit OL_SendImportDocsToDropEdit;
		private ZGuidFindBox OL_OH_ControllingCustomerGuidFindBox;
		private ZGuidFindBox OL_OH_ImportBrokerGuidFindBox;
		private ZCodeFindBox zGuidFindBox7;
		private ZCodeFindBox zCodeFindBox2;
		private ZGuidFindBox zGuidFindBox6;
		private ZGroupBox ValuationBasisGroupBox;
		private ZNumericUpDown RoyaltyPercentageNumericUpDown;
		private ZDropEdit OL_AuthorityToLeaveDropEdit;
		private ZNumericUpDown DefaultInsurancePercentageNumericUpDown;
		protected ZDropEdit TransactionsRelatedDropEdit;
		private ZDropEdit ProductRelationshipDropEdit;
		private ZDropEdit OL_EFreightStatusDropEdit;
		protected ZDropEdit OL_ValuationBasisBoundDropEdit;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RelationshipDetailsUserControl = new Enterprise.MasterFiles.GUI.RelationshipDetailsUserControl();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrgBuyerLinkBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProductRelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OL_SendImportDocsToDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OL_OH_ControllingCustomerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OL_OH_ImportBrokerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGuidFindBox7 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zGuidFindBox6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OL_AuthorityToLeaveDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ValuationBasisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BuyingCommissionPercentageNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.RoyaltyPercentageNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.DefaultInsurancePercentageNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.TransactionsRelatedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OL_ValuationBasisBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OL_EFreightStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel3.SuspendLayout();
			this.RelationshipDetailsUserControl.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgBuyerLinkBoundGrid)).BeginInit();
			this.OrgBuyerLinkBoundGrid.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.ProductRelationshipDropEdit.SuspendLayout();
			this.OL_SendImportDocsToDropEdit.SuspendLayout();
			this.OL_OH_ControllingCustomerGuidFindBox.SuspendLayout();
			this.OL_OH_ImportBrokerGuidFindBox.SuspendLayout();
			this.zGuidFindBox7.SuspendLayout();
			this.zCodeFindBox2.SuspendLayout();
			this.zGuidFindBox6.SuspendLayout();
			this.OL_AuthorityToLeaveDropEdit.SuspendLayout();
			this.ValuationBasisGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BuyingCommissionPercentageNumericUpDown)).BeginInit();
			this.BuyingCommissionPercentageNumericUpDown.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RoyaltyPercentageNumericUpDown)).BeginInit();
			this.RoyaltyPercentageNumericUpDown.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DefaultInsurancePercentageNumericUpDown)).BeginInit();
			this.DefaultInsurancePercentageNumericUpDown.SuspendLayout();
			this.TransactionsRelatedDropEdit.SuspendLayout();
			this.OL_ValuationBasisBoundDropEdit.SuspendLayout();
			this.OL_EFreightStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgBuyerLinkCollection);
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 2, true);
			this.splitter1.TabIndex = 0;
			this.splitter1.TabStop = false;
			// 
			// zPanel3
			// 
			this.zPanel3.AutoScroll = true;
			this.zPanel3.Controls.Add(this.RelationshipDetailsUserControl);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 341, true);
			this.zPanel3.TabIndex = 3;
			// 
			// RelationshipDetailsUserControl
			// 
			this.RelationshipDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelationshipDetailsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)))));
			this.RelationshipDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelationshipDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelationshipDetailsUserControl.Name = "RelationshipDetailsUserControl";
			this.RelationshipDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 341, true);
			this.RelationshipDetailsUserControl.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zPanel2);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 188, true);
			this.zPanel1.TabIndex = 4;
			// 
			// zPanel2
			// 
			this.zPanel2.AutoScroll = true;
			this.zPanel2.Controls.Add(this.OrgBuyerLinkBoundGrid);
			this.zPanel2.Controls.Add(this.zPanel4);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 188, true);
			this.zPanel2.TabIndex = 3;
			// 
			// OrgBuyerLinkBoundGrid
			// 
			this.OrgBuyerLinkBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgBuyerLinkBoundGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Buyer.UNLOCO.RL_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RN_NKImporterCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RX_NKDefaultCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OC_NotifyPartyContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_InitialShipmentExpected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_ValuationBasisDeterminationNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_ControllingCustomer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_ImportBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_SendImportDocsTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_ValuationBasisMarkupPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RelatedParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_ValuationBasis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_InsuranceUplift)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RoyaltyPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_DefaultDutyRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Buyer.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_VendorID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_EFreightStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_AuthorityToLeave)));
			this.OrgBuyerLinkBoundGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorRelationshipsUserControl|cdfbbafe-6b8b-4ca0-a571-52b2461e67a7", "Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OL_OH_Buyer";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.PopupCaption = "Select Buyer";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.ColumnName = "Buyer+UNLOCO+RL_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OL_RN_NKImporterCountry";
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Importer Country";
			zCodeFindBoxColumnStyleInfo1.ToolTip = "The country in which system uses these default values.";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "OL_RX_NKDefaultCurrency";
			zCodeFindBoxColumnStyleInfo2.PopupCaption = "Select Currency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "OL_OC_NotifyPartyContact";
			zGuidDropEditColumnStyleInfo1.ToolTip = "Select Contact to Notify";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo1.ColumnName = "OL_InitialShipmentExpected";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "OL_ValuationBasisDeterminationNum";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OL_OH_ControllingCustomer";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.PopupCaption = "Select Controller";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "OL_OH_ImportBroker";
			zGuidFindBoxColumnStyleInfo3.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorRelationshipsUserControl|eee15d8e-779e-40ed-8109-f0e01186185b", "Import Broker");
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.PopupCaption = "Select Broker";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "OL_SendImportDocsTo";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorRelationshipsUserControl|eee15d8e-779e-40ed-8109-f0e01186185b", "Import Broker");
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OL_ValuationBasisMarkupPercent";
			zCalcEditColumnStyleInfo1.MaxValue = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "OL_RelatedParty";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "OL_ValuationBasis";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OL_InsuranceUplift";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.MaxValue = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "OL_RoyaltyPercentage";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.MaxValue = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "OL_DefaultDutyRate";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.MaxValue = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorRelationshipsUserControl|f57832c3-869c-4b79-a0c7-1c4df66516f3", "Name");
			zTextBoxColumnStyleInfo3.ColumnName = "Buyer+OH_FullName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d97ed681-b775-47e9-9229-3ba1e0b4e1f9", "Vendor ID");
			zTextBoxColumnStyleInfo4.ColumnName = "OL_VendorID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "OL_EFreightStatus";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo5.ColumnName = "OL_AuthorityToLeave";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrgBuyerLinkBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.OrgBuyerLinkBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgBuyerLinkBoundGrid.GridId = "78931c32-a11f-4e66-af85-caf61e375552";
			this.OrgBuyerLinkBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgBuyerLinkBoundGrid.LayoutKey = "OrgBuyerLinkBoundGrid";
			this.OrgBuyerLinkBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgBuyerLinkBoundGrid.Name = "OrgBuyerLinkBoundGrid";
			this.OrgBuyerLinkBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 112, true);
			this.OrgBuyerLinkBoundGrid.TabIndex = 0;
			// 
			// zPanel4
			// 
			this.zPanel4.Controls.Add(this.ProductRelationshipDropEdit);
			this.zPanel4.Controls.Add(this.OL_SendImportDocsToDropEdit);
			this.zPanel4.Controls.Add(this.OL_OH_ControllingCustomerGuidFindBox);
			this.zPanel4.Controls.Add(this.OL_OH_ImportBrokerGuidFindBox);
			this.zPanel4.Controls.Add(this.zGuidFindBox7);
			this.zPanel4.Controls.Add(this.zCodeFindBox2);
			this.zPanel4.Controls.Add(this.zGuidFindBox6);
			this.zPanel4.Controls.Add(this.OL_AuthorityToLeaveDropEdit);
			this.zPanel4.Controls.Add(this.ValuationBasisGroupBox);
			this.zPanel4.Controls.Add(this.OL_EFreightStatusDropEdit);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 76, true);
			this.zPanel4.TabIndex = 1;
			// 
			// ProductRelationshipDropEdit
			// 
			this.ProductRelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductRelationshipDropEdit, "OL_ProductRelation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_ProductRelation)));
			this.ProductRelationshipDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("90a855fc-ad2a-4fee-94cc-2a1d47b7e965", "Product Relationship");
			this.ProductRelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 52, true);
			this.ProductRelationshipDropEdit.Name = "ProductRelationshipDropEdit";
			this.ProductRelationshipDropEdit.PreBoundMaxLength = 3;
			this.ProductRelationshipDropEdit.ShouldResizeByMaxLength = true;
			this.ProductRelationshipDropEdit.ShowDescriptionBox = false;
			this.ProductRelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ProductRelationshipDropEdit.TabIndex = 6;
			// 
			// OL_SendImportDocsToDropEdit
			// 
			this.OL_SendImportDocsToDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OL_SendImportDocsToDropEdit, "OL_SendImportDocsTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_SendImportDocsTo)));
			this.OL_SendImportDocsToDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 4, true);
			this.OL_SendImportDocsToDropEdit.Name = "OL_SendImportDocsToDropEdit";
			this.OL_SendImportDocsToDropEdit.PreBoundMaxLength = 3;
			this.OL_SendImportDocsToDropEdit.ShouldResizeByMaxLength = true;
			this.OL_SendImportDocsToDropEdit.ShowDescriptionBox = false;
			this.OL_SendImportDocsToDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OL_SendImportDocsToDropEdit.TabIndex = 7;
			// 
			// OL_OH_ControllingCustomerGuidFindBox
			// 
			this.OL_OH_ControllingCustomerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OL_OH_ControllingCustomerGuidFindBox, "OL_OH_ControllingCustomer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_ControllingCustomer)));
			this.OL_OH_ControllingCustomerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 4, true);
			this.OL_OH_ControllingCustomerGuidFindBox.Name = "OL_OH_ControllingCustomerGuidFindBox";
			this.OL_OH_ControllingCustomerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OL_OH_ControllingCustomerGuidFindBox.ParentType = null;
			this.OL_OH_ControllingCustomerGuidFindBox.ShowDescriptionBox = false;
			this.OL_OH_ControllingCustomerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OL_OH_ControllingCustomerGuidFindBox.TabIndex = 4;
			// 
			// OL_OH_ImportBrokerGuidFindBox
			// 
			this.OL_OH_ImportBrokerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OL_OH_ImportBrokerGuidFindBox, "OL_OH_ImportBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_ImportBroker)));
			this.OL_OH_ImportBrokerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 28, true);
			this.OL_OH_ImportBrokerGuidFindBox.Name = "OL_OH_ImportBrokerGuidFindBox";
			this.OL_OH_ImportBrokerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OL_OH_ImportBrokerGuidFindBox.ParentType = null;
			this.OL_OH_ImportBrokerGuidFindBox.ShowDescriptionBox = false;
			this.OL_OH_ImportBrokerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OL_OH_ImportBrokerGuidFindBox.TabIndex = 5;
			// 
			// zGuidFindBox7
			// 
			this.zGuidFindBox7.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox7, "OL_RX_NKDefaultCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RX_NKDefaultCurrency)));
			this.zGuidFindBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 52, true);
			this.zGuidFindBox7.Name = "zGuidFindBox7";
			this.zGuidFindBox7.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zGuidFindBox7.ParentType = null;
			this.zGuidFindBox7.PreBoundMaxLength = 3;
			this.zGuidFindBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.zGuidFindBox7.TabIndex = 3;
			// 
			// zCodeFindBox2
			// 
			this.zCodeFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox2, "OL_RN_NKImporterCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RN_NKImporterCountry)));
			this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 28, true);
			this.zCodeFindBox2.Name = "zCodeFindBox2";
			this.zCodeFindBox2.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zCodeFindBox2.ParentType = null;
			this.zCodeFindBox2.PreBoundMaxLength = 2;
			this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.zCodeFindBox2.TabIndex = 2;
			// 
			// zGuidFindBox6
			// 
			this.zGuidFindBox6.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox6, "OL_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_Buyer)));
			this.zGuidFindBox6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorRelationshipsUserControl|a1a0d84a-f0c6-4d6d-bcf9-5090c02261fb", "Code");
			this.zGuidFindBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 4, true);
			this.zGuidFindBox6.Name = "zGuidFindBox6";
			this.zGuidFindBox6.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zGuidFindBox6.ParentType = null;
			this.zGuidFindBox6.ShowDescriptionBox = false;
			this.zGuidFindBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zGuidFindBox6.TabIndex = 1;
			// 
			// OL_AuthorityToLeaveDropEdit
			// 
			this.OL_AuthorityToLeaveDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OL_AuthorityToLeaveDropEdit, "OL_AuthorityToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_AuthorityToLeave)));
			this.OL_AuthorityToLeaveDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 28, true);
			this.OL_AuthorityToLeaveDropEdit.Name = "OL_AuthorityToLeaveDropEdit";
			this.OL_AuthorityToLeaveDropEdit.PreBoundMaxLength = 3;
			this.OL_AuthorityToLeaveDropEdit.ShouldResizeByMaxLength = true;
			this.OL_AuthorityToLeaveDropEdit.ShowDescriptionBox = false;
			this.OL_AuthorityToLeaveDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OL_AuthorityToLeaveDropEdit.TabIndex = 13;
			// 
			// ValuationBasisGroupBox
			// 
			this.ValuationBasisGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorRelationshipsUserControl|82b25efa-c385-4946-818c-3cd60aa7c658", "Customs Valuation Defaults");
			this.ValuationBasisGroupBox.Controls.Add(this.BuyingCommissionPercentageNumericUpDown);
			this.ValuationBasisGroupBox.Controls.Add(this.RoyaltyPercentageNumericUpDown);
			this.ValuationBasisGroupBox.Controls.Add(this.DefaultInsurancePercentageNumericUpDown);
			this.ValuationBasisGroupBox.Controls.Add(this.TransactionsRelatedDropEdit);
			this.ValuationBasisGroupBox.Controls.Add(this.OL_ValuationBasisBoundDropEdit);
			this.ValuationBasisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 0, true);
			this.ValuationBasisGroupBox.Name = "ValuationBasisGroupBox";
			this.ValuationBasisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 69, true);
			this.ValuationBasisGroupBox.TabIndex = 13;
			this.ValuationBasisGroupBox.TabStop = false;
			// 
			// BuyingCommissionPercentageNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.BuyingCommissionPercentageNumericUpDown, "OL_BuyingCommissionPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_BuyingCommissionPercentage)));
			this.BuyingCommissionPercentageNumericUpDown.BindTo = "OL_BuyingCommissionPercentage";
			this.BuyingCommissionPercentageNumericUpDown.DecimalPlaces = 3;
			this.BuyingCommissionPercentageNumericUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.BuyingCommissionPercentageNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 40, true);
			this.BuyingCommissionPercentageNumericUpDown.Name = "BuyingCommissionPercentageNumericUpDown";
			this.BuyingCommissionPercentageNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.BuyingCommissionPercentageNumericUpDown.TabIndex = 5;
			// 
			// RoyaltyPercentageNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.RoyaltyPercentageNumericUpDown, "OL_RoyaltyPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RoyaltyPercentage)));
			this.RoyaltyPercentageNumericUpDown.BindTo = "OL_RoyaltyPercentage";
			this.RoyaltyPercentageNumericUpDown.DecimalPlaces = 3;
			this.RoyaltyPercentageNumericUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.RoyaltyPercentageNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 40, true);
			this.RoyaltyPercentageNumericUpDown.Name = "RoyaltyPercentageNumericUpDown";
			this.RoyaltyPercentageNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.RoyaltyPercentageNumericUpDown.TabIndex = 4;
			// 
			// DefaultInsurancePercentageNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.DefaultInsurancePercentageNumericUpDown, "OL_InsuranceUplift");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_InsuranceUplift)));
			this.DefaultInsurancePercentageNumericUpDown.BindTo = "OL_InsuranceUplift";
			this.DefaultInsurancePercentageNumericUpDown.DecimalPlaces = 5;
			this.DefaultInsurancePercentageNumericUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.DefaultInsurancePercentageNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 18, true);
			this.DefaultInsurancePercentageNumericUpDown.Name = "DefaultInsurancePercentageNumericUpDown";
			this.DefaultInsurancePercentageNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.DefaultInsurancePercentageNumericUpDown.TabIndex = 3;
			// 
			// TransactionsRelatedDropEdit
			// 
			this.TransactionsRelatedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionsRelatedDropEdit, "OL_RelatedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RelatedParty)));
			this.TransactionsRelatedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 18, true);
			this.TransactionsRelatedDropEdit.Name = "TransactionsRelatedDropEdit";
			this.TransactionsRelatedDropEdit.PreBoundMaxLength = 3;
			this.TransactionsRelatedDropEdit.ShouldResizeByMaxLength = true;
			this.TransactionsRelatedDropEdit.ShowDescriptionBox = false;
			this.TransactionsRelatedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TransactionsRelatedDropEdit.TabIndex = 1;
			// 
			// OL_ValuationBasisBoundDropEdit
			// 
			this.OL_ValuationBasisBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OL_ValuationBasisBoundDropEdit, "OL_ValuationBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_ValuationBasis)));
			this.OL_ValuationBasisBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 40, true);
			this.OL_ValuationBasisBoundDropEdit.Name = "OL_ValuationBasisBoundDropEdit";
			this.OL_ValuationBasisBoundDropEdit.PreBoundMaxLength = 10;
			this.OL_ValuationBasisBoundDropEdit.ShouldResizeByMaxLength = true;
			this.OL_ValuationBasisBoundDropEdit.ShowDescriptionBox = false;
			this.OL_ValuationBasisBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.OL_ValuationBasisBoundDropEdit.TabIndex = 2;
			// 
			// OL_EFreightStatusDropEdit
			// 
			this.OL_EFreightStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OL_EFreightStatusDropEdit, "OL_EFreightStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_EFreightStatus)));
			this.OL_EFreightStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 52, true);
			this.OL_EFreightStatusDropEdit.Name = "OL_EFreightStatusDropEdit";
			this.OL_EFreightStatusDropEdit.PreBoundMaxLength = 10;
			this.OL_EFreightStatusDropEdit.ShouldResizeByMaxLength = true;
			this.OL_EFreightStatusDropEdit.ShowDescriptionBox = false;
			this.OL_EFreightStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.OL_EFreightStatusDropEdit.TabIndex = 14;
			// 
			// ConsignorRelationshipsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.zPanel3);
			this.Name = "ConsignorRelationshipsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 531, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.RelationshipDetailsUserControl.ResumeLayout(true);
			this.RelationshipDetailsUserControl.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgBuyerLinkBoundGrid)).EndInit();
			this.OrgBuyerLinkBoundGrid.ResumeLayout(false);
			this.OrgBuyerLinkBoundGrid.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.ProductRelationshipDropEdit.ResumeLayout(true);
			this.ProductRelationshipDropEdit.PerformLayout();
			this.OL_SendImportDocsToDropEdit.ResumeLayout(true);
			this.OL_SendImportDocsToDropEdit.PerformLayout();
			this.OL_OH_ControllingCustomerGuidFindBox.ResumeLayout(true);
			this.OL_OH_ControllingCustomerGuidFindBox.PerformLayout();
			this.OL_OH_ImportBrokerGuidFindBox.ResumeLayout(true);
			this.OL_OH_ImportBrokerGuidFindBox.PerformLayout();
			this.zGuidFindBox7.ResumeLayout(true);
			this.zGuidFindBox7.PerformLayout();
			this.zCodeFindBox2.ResumeLayout(true);
			this.zCodeFindBox2.PerformLayout();
			this.zGuidFindBox6.ResumeLayout(true);
			this.zGuidFindBox6.PerformLayout();
			this.OL_AuthorityToLeaveDropEdit.ResumeLayout(true);
			this.OL_AuthorityToLeaveDropEdit.PerformLayout();
			this.ValuationBasisGroupBox.ResumeLayout(false);
			this.ValuationBasisGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BuyingCommissionPercentageNumericUpDown)).EndInit();
			this.BuyingCommissionPercentageNumericUpDown.ResumeLayout(false);
			this.BuyingCommissionPercentageNumericUpDown.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RoyaltyPercentageNumericUpDown)).EndInit();
			this.RoyaltyPercentageNumericUpDown.ResumeLayout(false);
			this.RoyaltyPercentageNumericUpDown.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DefaultInsurancePercentageNumericUpDown)).EndInit();
			this.DefaultInsurancePercentageNumericUpDown.ResumeLayout(false);
			this.DefaultInsurancePercentageNumericUpDown.PerformLayout();
			this.TransactionsRelatedDropEdit.ResumeLayout(true);
			this.TransactionsRelatedDropEdit.PerformLayout();
			this.OL_ValuationBasisBoundDropEdit.ResumeLayout(true);
			this.OL_ValuationBasisBoundDropEdit.PerformLayout();
			this.OL_EFreightStatusDropEdit.ResumeLayout(true);
			this.OL_EFreightStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZNumericUpDown BuyingCommissionPercentageNumericUpDown;
	}
}
