using System.ComponentModel;

namespace Enterprise.MasterFiles.GUI
{
	partial class RelatedOrganizationUserControl
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
			if (disposing)
			{
				UnsubscribeHandlers();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void UnsubscribeHandlers()
		{
			if (currentRelation != null)
			{
				currentRelation.OnOU_OHChanging -= currentRelation_OnOU_OHChanging;
			}
			if (GridPartRelations != null)
			{
				GridPartRelations.OnRemovingBizOFromList -= OnPartRelationRemoving;
				GridPartRelations.AfterBind -= zGridPartRelations_AfterBind;
			}
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            this.RelatedOrganizationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.GridPartRelations = new Enterprise.ZArchitecture.ZGrid();
            this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.RelationshipTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.RelationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.UnitPriceCurrencyBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.UntiPriceBox = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DefaultHoldCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.OU_HiCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.OU_TiCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ClientUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.LocalPartDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalPartNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            this.RelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.FormLayoutControllerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.PreventReceivingOversCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ReceiveOverageTolerancePercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AttributesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.AttributesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.IsSerialNumberReleaseCapturedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.UseSerialNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.barcodeParsingLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.JulianBatchNumberFormatDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.IsPartAttrib3ReleaseCapturedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IsPartAttrib2ReleaseCapturedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IsPartAttrib1ReleaseCapturedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.OU_RollUpAttributesOnDocumentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.OU_CompletePalletPickingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.OP_PickModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExpiryDateFormatStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PackingDateFormatStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RFConfirmDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UsePartAttrib1CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.UsePartAttrib2CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.UseExpiryDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.UsePartAttrib3CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.UsePackingDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.OtherDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.CartonizationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CartonGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.DefaultChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.RoyaltyFlatAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
            this.RoyaltyPercentageNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            this.LandedCostingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LandedCostMarginPercent3CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.LandedCostMarginPercent1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.LandedCostMarginPercent2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.CustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RelatedOrganizationPanel.SuspendLayout();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridPartRelations)).BeginInit();
            this.GridPartRelations.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.RelationshipTabPage.SuspendLayout();
            this.RelationGroupBox.SuspendLayout();
            this.UnitPriceCurrencyBox.SuspendLayout();
            this.DefaultHoldCodeFindBox.SuspendLayout();
            this.ClientUQDropEdit.SuspendLayout();
            this.OrganisationFindBox.SuspendLayout();
            this.RelationshipDropEdit.SuspendLayout();
            this.AttributesTabPage.SuspendLayout();
            this.AttributesGroupBox.SuspendLayout();
            this.JulianBatchNumberFormatDropEdit.SuspendLayout();
            this.OP_PickModeDropEdit.SuspendLayout();
            this.RFConfirmDropEdit.SuspendLayout();
            this.OtherDetailsTabPage.SuspendLayout();
            this.CartonizationGroupBox.SuspendLayout();
            this.CartonGroupGuidFindBox.SuspendLayout();
            this.DefaultChargesGroupBox.SuspendLayout();
            this.RoyaltyFlatAmountCalcFindBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RoyaltyPercentageNumericUpDown)).BeginInit();
            this.RoyaltyPercentageNumericUpDown.SuspendLayout();
            this.LandedCostingGroupBox.SuspendLayout();
            this.CustomFieldsTabPage.SuspendLayout();
            this.CustomFieldsControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierPart);
            // 
            // RelatedOrganizationPanel
            // 
            this.RelatedOrganizationPanel.Controls.Add(this.TopPanel);
            this.RelatedOrganizationPanel.Controls.Add(this.BottomPanel);
            this.RelatedOrganizationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RelatedOrganizationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RelatedOrganizationPanel.Name = "RelatedOrganizationPanel";
            this.RelatedOrganizationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 378, true);
            this.RelatedOrganizationPanel.TabIndex = 3;
            // 
            // TopPanel
            // 
            this.TopPanel.Controls.Add(this.GridPartRelations);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 138, true);
            this.TopPanel.TabIndex = 6;
            // 
            // GridPartRelations
            // 
            this.GridPartRelations.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.GridPartRelations, "RelatedOrganisations");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_OH)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Relationship)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_FormLayoutController)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Organisation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LandedCostMarginPercent1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LandedCostMarginPercent2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LandedCostMarginPercent3)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePartAttrib1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsPartAttrib1ReleaseCaptured)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePartAttrib2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsPartAttrib2ReleaseCaptured)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePartAttrib3)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsPartAttrib3ReleaseCaptured)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UseSerialNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsSerialNumberReleaseCaptured)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UseExpiryDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePackingDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_PreventReceivingOvers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_WCG_CartonGroup)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_ClientUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LocalPartNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LocalPartDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Hi)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Ti)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RoyaltyPercent)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RoyaltyFlatAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UnitPrice)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_ReceiveOverageTolerancePercent)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RX_NKRoyaltyCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_OPC_Category)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).Category.OPC_CategoryDescriptionMultilingual)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_WHC_DefaultInventoryHoldCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RX_NKUnitPriceCurrency)));
            this.GridPartRelations.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo5.ColumnName = "OU_OH";
            zGuidFindBoxColumnStyleInfo5.IsMandatory = true;
            zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDropEditColumnStyleInfo3.ColumnName = "OU_Relationship";
            zDropEditColumnStyleInfo3.IsMandatory = true;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zCheckBoxColumnStyleInfo13.ColumnName = "OU_FormLayoutController";
            zCheckBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|64d3fbe7-a5a7-4942-be83-87a06322ee88", "Company Name");
            zTextBoxColumnStyleInfo5.ColumnName = "OU_Organisation";
            zTextBoxColumnStyleInfo5.IsMandatory = true;
            zTextBoxColumnStyleInfo5.IsReadOnly = true;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo10.ColumnName = "OU_LandedCostMarginPercent1";
            zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo11.ColumnName = "OU_LandedCostMarginPercent2";
            zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo12.ColumnName = "OU_LandedCostMarginPercent3";
            zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo14.ColumnName = "OU_UsePartAttrib1";
            zCheckBoxColumnStyleInfo14.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b9f4014f-b469-488e-a970-d854a339e0a4", "Rel.", "Release", "Release Attribute", "");
            zCheckBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo15.ColumnName = "OU_IsPartAttrib1ReleaseCaptured";
            zCheckBoxColumnStyleInfo15.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCheckBoxColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo16.ColumnName = "OU_UsePartAttrib2";
            zCheckBoxColumnStyleInfo16.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo17.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b9f4014f-b469-488e-a970-d854a339e0a4", "Rel.", "Release", "Release Attribute", "");
            zCheckBoxColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo17.ColumnName = "OU_IsPartAttrib2ReleaseCaptured";
            zCheckBoxColumnStyleInfo17.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCheckBoxColumnStyleInfo18.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo18.ColumnName = "OU_UsePartAttrib3";
            zCheckBoxColumnStyleInfo18.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo19.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b9f4014f-b469-488e-a970-d854a339e0a4", "Rel.", "Release", "Release Attribute", "");
            zCheckBoxColumnStyleInfo19.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo19.ColumnName = "OU_IsPartAttrib3ReleaseCaptured";
            zCheckBoxColumnStyleInfo19.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCheckBoxColumnStyleInfo20.ColumnName = "OU_UseSerialNumber";
            zCheckBoxColumnStyleInfo20.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo21.ColumnName = "OU_IsSerialNumberReleaseCaptured";
            zCheckBoxColumnStyleInfo21.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo22.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo22.ColumnName = "OU_UseExpiryDate";
            zCheckBoxColumnStyleInfo22.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo23.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo23.ColumnName = "OU_UsePackingDate";
            zCheckBoxColumnStyleInfo23.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|fa18faf0-4df0-42d3-98d0-bdefe26731f1", "Attributes");
            zCheckBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo24.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCheckBoxColumnStyleInfo24.ColumnName = "OU_PreventReceivingOvers";
            zCheckBoxColumnStyleInfo24.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|bc33b839-e169-41e5-91dd-f590b7c899ca", "Prevent Receiving Overs");
            zCheckBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo6.ColumnName = "OU_WCG_CartonGroup";
            zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo4.ColumnName = "OU_ClientUQ";
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo6.ColumnName = "OU_LocalPartNumber";
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo7.ColumnName = "OU_LocalPartDescription";
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCalcEditColumnStyleInfo13.ColumnName = "OU_Hi";
            zCalcEditColumnStyleInfo13.Decimals = 0;
            zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCalcEditColumnStyleInfo14.ColumnName = "OU_Ti";
            zCalcEditColumnStyleInfo14.Decimals = 0;
            zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|20c4b86e-588f-4549-a0a6-e78664ebe732", "Royalty %", "Royalty Percentage.");
            zCalcEditColumnStyleInfo15.ColumnName = "OU_RoyaltyPercent";
            zCalcEditColumnStyleInfo15.Decimals = 3;
            zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo16.ColumnName = "OU_RoyaltyFlatAmount";
            zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo17.ColumnName = "OU_UnitPrice";
            zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo18.ColumnName = "OU_ReceiveOverageTolerancePercent";
            zCalcEditColumnStyleInfo18.Decimals = 0;
			zCalcEditColumnStyleInfo18.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|bc33b839-e169-41e5-91dd-f590b7c899ca", "Prevent Receiving Overs");
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            zCodeFindBoxColumnStyleInfo3.ColumnName = "OU_RX_NKRoyaltyCurrency";
            zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo7.ColumnName = "OU_OPC_Category";
            zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("875cd01c-130f-41f8-be93-9cef8cfba103", "Cat. Desc.", "Category Description", "");
            zTextBoxColumnStyleInfo8.ColumnName = "Category+OPC_CategoryDescriptionMultilingual";
            zTextBoxColumnStyleInfo8.IsReadOnly = true;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo8.ColumnName = "OU_WHC_DefaultInventoryHoldCode";
            zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo4.ColumnName = "OU_RX_NKUnitPriceCurrency";
            zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.GridPartRelations.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
            this.GridPartRelations.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo13);
            this.GridPartRelations.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo14);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo15);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo16);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo17);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo18);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo19);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo20);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo21);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo22);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo23);
            this.GridPartRelations.ColumnStyles.Add(zCheckBoxColumnStyleInfo24);
            this.GridPartRelations.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
            this.GridPartRelations.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.GridPartRelations.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.GridPartRelations.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
            this.GridPartRelations.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
            this.GridPartRelations.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
            this.GridPartRelations.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
            this.GridPartRelations.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.GridPartRelations.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
            this.GridPartRelations.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
            this.GridPartRelations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridPartRelations.GridId = "f736a073-316c-4deb-9480-3c7d5ae7b37e";
            this.GridPartRelations.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GridPartRelations.LayoutKey = "zGridPartRelations";
            this.GridPartRelations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GridPartRelations.Name = "GridPartRelations";
            this.GridPartRelations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 138, true);
            this.GridPartRelations.TabIndex = 2;
            // 
            // BottomPanel
            // 
            this.BottomPanel.Controls.Add(this.MainTabControl);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 240, true);
            this.BottomPanel.TabIndex = 5;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.MainTabControl.Controls.Add(this.RelationshipTabPage);
            this.MainTabControl.Controls.Add(this.AttributesTabPage);
            this.MainTabControl.Controls.Add(this.OtherDetailsTabPage);
            this.MainTabControl.Controls.Add(this.CustomFieldsTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 240, true);
            this.MainTabControl.TabIndex = 3;
            // 
            // RelationshipTabPage
            // 
            this.RelationshipTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|76cab0a9-bdbf-4143-8f6e-05aeb8cf87b1", "Relationship");
            this.RelationshipTabPage.Controls.Add(this.RelationGroupBox);
            this.RelationshipTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.RelationshipTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 213, true);
            this.RelationshipTabPage.TabIndex = 0;
            // 
            // RelationGroupBox
            // 
            this.RelationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|4bce90bd-53fd-4d35-b77c-de31883cac7d", "Relationship");
            this.RelationGroupBox.Controls.Add(this.UnitPriceCurrencyBox);
            this.RelationGroupBox.Controls.Add(this.UntiPriceBox);
            this.RelationGroupBox.Controls.Add(this.DefaultHoldCodeFindBox);
            this.RelationGroupBox.Controls.Add(this.OU_HiCalcEdit);
            this.RelationGroupBox.Controls.Add(this.OU_TiCalcEdit);
            this.RelationGroupBox.Controls.Add(this.ClientUQDropEdit);
            this.RelationGroupBox.Controls.Add(this.LocalPartDescriptionTextBox);
            this.RelationGroupBox.Controls.Add(this.LocalPartNumberTextBox);
            this.RelationGroupBox.Controls.Add(this.OrganisationFindBox);
            this.RelationGroupBox.Controls.Add(this.RelationshipDropEdit);
            this.RelationGroupBox.Controls.Add(this.FormLayoutControllerCheckBox);
            this.RelationGroupBox.Controls.Add(this.PreventReceivingOversCheckBox);
            this.RelationGroupBox.Controls.Add(this.ReceiveOverageTolerancePercentCalcEdit);
            this.RelationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
            this.RelationGroupBox.Name = "RelationGroupBox";
            this.RelationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 200, true);
            this.RelationGroupBox.TabIndex = 0;
            this.RelationGroupBox.TabStop = false;
            // 
            // UnitPriceCurrencyBox
            // 
            this.UnitPriceCurrencyBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UnitPriceCurrencyBox, "RelatedOrganisations.OU_RX_NKUnitPriceCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RX_NKUnitPriceCurrency)));
            this.UnitPriceCurrencyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 148, true);
            this.UnitPriceCurrencyBox.Name = "UnitPriceCurrencyBox";
            this.UnitPriceCurrencyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
            this.UnitPriceCurrencyBox.TabIndex = 14;
            // 
            // UntiPriceBox
            // 
            this.BindingSource.SetBindingMember(this.UntiPriceBox, "RelatedOrganisations.OU_UnitPrice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UnitPrice)));
            this.UntiPriceBox.CaptionResourceString = null;
            this.UntiPriceBox.DecimalPlaces = 2;
            this.UntiPriceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 148, true);
            this.UntiPriceBox.Name = "UntiPriceBox";
            this.UntiPriceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
            this.UntiPriceBox.TabIndex = 13;
            this.UntiPriceBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DefaultHoldCodeFindBox
            // 
            this.DefaultHoldCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DefaultHoldCodeFindBox, "RelatedOrganisations.OU_WHC_DefaultInventoryHoldCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_WHC_DefaultInventoryHoldCode)));
            this.DefaultHoldCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 122, true);
            this.DefaultHoldCodeFindBox.Name = "DefaultHoldCodeFindBox";
            this.DefaultHoldCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.DefaultHoldCodeFindBox.TabIndex = 10;
            // 
            // OU_HiCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.OU_HiCalcEdit, "RelatedOrganisations.OU_Hi");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Hi)));
            this.OU_HiCalcEdit.CaptionResourceString = null;
            this.OU_HiCalcEdit.DecimalPlaces = 2;
            this.OU_HiCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 69, true);
            this.OU_HiCalcEdit.Name = "OU_HiCalcEdit";
            this.OU_HiCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.OU_HiCalcEdit.TabIndex = 8;
            this.OU_HiCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // OU_TiCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.OU_TiCalcEdit, "RelatedOrganisations.OU_Ti");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Ti)));
            this.OU_TiCalcEdit.CaptionResourceString = null;
            this.OU_TiCalcEdit.DecimalPlaces = 2;
            this.OU_TiCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 95, true);
            this.OU_TiCalcEdit.Name = "OU_TiCalcEdit";
            this.OU_TiCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.OU_TiCalcEdit.TabIndex = 9;
            this.OU_TiCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ClientUQDropEdit
            // 
            this.ClientUQDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ClientUQDropEdit, "RelatedOrganisations.OU_ClientUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_ClientUQ)));
            this.ClientUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 43, true);
            this.ClientUQDropEdit.Name = "ClientUQDropEdit";
            this.ClientUQDropEdit.PreBoundMaxLength = 3;
			this.ClientUQDropEdit.ShouldResizeByMaxLength = true;
			this.ClientUQDropEdit.ShowDescriptionBox = false;
            this.ClientUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.ClientUQDropEdit.TabIndex = 2;
            // 
            // LocalPartDescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalPartDescriptionTextBox, "RelatedOrganisations.OU_LocalPartDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LocalPartDescription)));
            this.LocalPartDescriptionTextBox.CaptionResourceString = null;
            this.LocalPartDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 95, true);
            this.LocalPartDescriptionTextBox.Name = "LocalPartDescriptionTextBox";
            this.LocalPartDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
            this.LocalPartDescriptionTextBox.TabIndex = 5;
            // 
            // LocalPartNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalPartNumberTextBox, "RelatedOrganisations.OU_LocalPartNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LocalPartNumber)));
            this.LocalPartNumberTextBox.CaptionResourceString = null;
            this.LocalPartNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 69, true);
            this.LocalPartNumberTextBox.Name = "LocalPartNumberTextBox";
            this.LocalPartNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
            this.LocalPartNumberTextBox.TabIndex = 4;
            // 
            // OrganisationFindBox
            // 
            this.OrganisationFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OrganisationFindBox, "RelatedOrganisations.OU_OH");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_OH)));
            this.OrganisationFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|b006e20b-41a4-48e8-892d-769573df41a0", "Client");
            this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 17, true);
            this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.OrganisationFindBox.Name = "OrganisationFindBox";
            this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 20, true);
            this.OrganisationFindBox.TabIndex = 0;
            // 
            // RelationshipDropEdit
            // 
            this.RelationshipDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RelationshipDropEdit, "RelatedOrganisations.OU_Relationship");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_Relationship)));
            this.RelationshipDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|280f691b-db3e-41ed-ad6c-3e1c0e6c2162", "Relationship Type");
            this.RelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 43, true);
            this.RelationshipDropEdit.Name = "RelationshipDropEdit";
            this.RelationshipDropEdit.PreBoundMaxLength = 4;
			this.RelationshipDropEdit.ShouldResizeByMaxLength = true;
			this.RelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.RelationshipDropEdit.TabIndex = 1;
            // 
            // FormLayoutControllerCheckBox
            // 
            this.FormLayoutControllerCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.FormLayoutControllerCheckBox, "RelatedOrganisations.OU_FormLayoutController");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_FormLayoutController)));
            this.FormLayoutControllerCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|7f3b4590-85c8-43fe-858f-0faa2414d326", "Form Layout");
			this.FormLayoutControllerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FormLayoutControllerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 43, true);
            this.FormLayoutControllerCheckBox.Name = "FormLayoutControllerCheckBox";
            this.FormLayoutControllerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
            this.FormLayoutControllerCheckBox.TabIndex = 3;
            this.FormLayoutControllerCheckBox.UseVisualStyleBackColor = true;
            // 
            // PreventReceivingOversCheckBox
            // 
            this.BindingSource.SetBindingMember(this.PreventReceivingOversCheckBox, "RelatedOrganisations.OU_PreventReceivingOvers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_PreventReceivingOvers)));
            this.PreventReceivingOversCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 174, true);
            this.PreventReceivingOversCheckBox.Name = "PreventReceivingOversCheckBox";
            this.PreventReceivingOversCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
            this.PreventReceivingOversCheckBox.TabIndex = 15;
            this.PreventReceivingOversCheckBox.UseVisualStyleBackColor = true;
            // 
            // ReceiveOverageTolerancePercentCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ReceiveOverageTolerancePercentCalcEdit, "RelatedOrganisations.OU_ReceiveOverageTolerancePercent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_ReceiveOverageTolerancePercent)));
            this.ReceiveOverageTolerancePercentCalcEdit.CaptionResourceString = null;
            this.ReceiveOverageTolerancePercentCalcEdit.DecimalPlaces = 0;
            this.ReceiveOverageTolerancePercentCalcEdit.Decimals = 0;
            this.ReceiveOverageTolerancePercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 174, true);
            this.ReceiveOverageTolerancePercentCalcEdit.Name = "ReceiveOverageTolerancePercentCalcEdit";
            this.ReceiveOverageTolerancePercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.ReceiveOverageTolerancePercentCalcEdit.TabIndex = 16;
            this.ReceiveOverageTolerancePercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // AttributesTabPage
            // 
            this.AttributesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|a13bea5f-4a97-4c0a-a0c5-4c4ee0a5fc6a", "Attributes");
            this.AttributesTabPage.Controls.Add(this.AttributesGroupBox);
            this.AttributesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.AttributesTabPage.Name = "AttributesTabPage";
            this.AttributesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 213, true);
            this.AttributesTabPage.TabIndex = 1;
            // 
            // AttributesGroupBox
            // 
            this.AttributesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|5c0dacc7-a26e-42d2-85ba-0ef1ed1b2817", "Attributes");
            this.AttributesGroupBox.Controls.Add(this.IsSerialNumberReleaseCapturedCheckBox);
            this.AttributesGroupBox.Controls.Add(this.UseSerialNumberCheckBox);
            this.AttributesGroupBox.Controls.Add(this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit);
            this.AttributesGroupBox.Controls.Add(this.barcodeParsingLinkLabel);
            this.AttributesGroupBox.Controls.Add(this.JulianBatchNumberFormatDropEdit);
            this.AttributesGroupBox.Controls.Add(this.IsPartAttrib3ReleaseCapturedCheckBox);
            this.AttributesGroupBox.Controls.Add(this.IsPartAttrib2ReleaseCapturedCheckBox);
            this.AttributesGroupBox.Controls.Add(this.IsPartAttrib1ReleaseCapturedCheckBox);
            this.AttributesGroupBox.Controls.Add(this.OU_RollUpAttributesOnDocumentsCheckBox);
            this.AttributesGroupBox.Controls.Add(this.OU_CompletePalletPickingCheckBox);
            this.AttributesGroupBox.Controls.Add(this.OP_PickModeDropEdit);
            this.AttributesGroupBox.Controls.Add(this.ExpiryDateFormatStringTextBox);
            this.AttributesGroupBox.Controls.Add(this.PackingDateFormatStringTextBox);
            this.AttributesGroupBox.Controls.Add(this.RFConfirmDropEdit);
            this.AttributesGroupBox.Controls.Add(this.UsePartAttrib1CheckBox);
            this.AttributesGroupBox.Controls.Add(this.UsePartAttrib2CheckBox);
            this.AttributesGroupBox.Controls.Add(this.UseExpiryDateCheckBox);
            this.AttributesGroupBox.Controls.Add(this.UsePartAttrib3CheckBox);
            this.AttributesGroupBox.Controls.Add(this.UsePackingDateCheckBox);
            this.AttributesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AttributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.AttributesGroupBox.Name = "AttributesGroupBox";
            this.AttributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 207, true);
            this.AttributesGroupBox.TabIndex = 0;
            this.AttributesGroupBox.TabStop = false;
            // 
            // IsSerialNumberReleaseCapturedCheckBox
            // 
            this.IsSerialNumberReleaseCapturedCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsSerialNumberReleaseCapturedCheckBox, "RelatedOrganisations.OU_IsSerialNumberReleaseCaptured");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsSerialNumberReleaseCaptured)));
			this.IsSerialNumberReleaseCapturedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSerialNumberReleaseCapturedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 89, true);
            this.IsSerialNumberReleaseCapturedCheckBox.Name = "IsSerialNumberReleaseCapturedCheckBox";
			this.IsSerialNumberReleaseCapturedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsSerialNumberReleaseCapturedCheckBox.TabIndex = 7;
            this.IsSerialNumberReleaseCapturedCheckBox.UseVisualStyleBackColor = true;
            // 
            // UseSerialNumberCheckBox
            // 
            this.UseSerialNumberCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UseSerialNumberCheckBox, "RelatedOrganisations.OU_UseSerialNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UseSerialNumber)));
			this.UseSerialNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseSerialNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 89, true);
			this.UseSerialNumberCheckBox.Name = "UseSerialNumberCheckBox";
			this.UseSerialNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UseSerialNumberCheckBox.TabIndex = 6;
			this.UseSerialNumberCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsigneeMinShelfLifeAcceptedDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit, "RelatedOrganisations.OU_ConsigneeMinShelfLifeAccepted");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_ConsigneeMinShelfLifeAccepted)));
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.CaptionResourceString = null;
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.DecimalPlaces = 2;
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 132, true);
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.Name = "ConsigneeMinShelfLifeAcceptedDaysCalcEdit";
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.TabIndex = 12;
            this.ConsigneeMinShelfLifeAcceptedDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // barcodeParsingLinkLabel
            // 
            this.barcodeParsingLinkLabel.AutoSize = true;
            this.barcodeParsingLinkLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|1498400b-5ee0-4fce-9c4c-0bc9050ba98d", "Click here to open Barcode Parsing, where you can set up Application Identifiers.");
            this.barcodeParsingLinkLabel.IsFontBold = false;
            this.barcodeParsingLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 39, true);
            this.barcodeParsingLinkLabel.Name = "barcodeParsingLinkLabel";
            this.barcodeParsingLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
            this.barcodeParsingLinkLabel.TabIndex = 16;
            this.barcodeParsingLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.barcodeParsingLinkLabel_LinkClicked);
            // 
            // JulianBatchNumberFormatDropEdit
            // 
            this.JulianBatchNumberFormatDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.JulianBatchNumberFormatDropEdit, "RelatedOrganisations.OU_JulianBatchNoFormat");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_JulianBatchNoFormat)));
            this.JulianBatchNumberFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 132, true);
            this.JulianBatchNumberFormatDropEdit.Name = "JulianBatchNumberFormatDropEdit";
			this.JulianBatchNumberFormatDropEdit.ShouldResizeByMaxLength = true;
			this.JulianBatchNumberFormatDropEdit.ShowDescriptionBox = false;
            this.JulianBatchNumberFormatDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.JulianBatchNumberFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.JulianBatchNumberFormatDropEdit.TabIndex = 13;
            // 
            // IsPartAttrib3ReleaseCapturedCheckBox
            // 
            this.IsPartAttrib3ReleaseCapturedCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsPartAttrib3ReleaseCapturedCheckBox, "RelatedOrganisations.OU_IsPartAttrib3ReleaseCaptured");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsPartAttrib3ReleaseCaptured)));
			this.IsPartAttrib3ReleaseCapturedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPartAttrib3ReleaseCapturedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 66, true);
			this.IsPartAttrib3ReleaseCapturedCheckBox.Name = "IsPartAttrib3ReleaseCapturedCheckBox";
			this.IsPartAttrib3ReleaseCapturedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.IsPartAttrib3ReleaseCapturedCheckBox.TabIndex = 5;
			this.IsPartAttrib3ReleaseCapturedCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsPartAttrib2ReleaseCapturedCheckBox
			// 
			this.IsPartAttrib2ReleaseCapturedCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsPartAttrib2ReleaseCapturedCheckBox, "RelatedOrganisations.OU_IsPartAttrib2ReleaseCaptured");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsPartAttrib2ReleaseCaptured)));
			this.IsPartAttrib2ReleaseCapturedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPartAttrib2ReleaseCapturedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 43, true);
            this.IsPartAttrib2ReleaseCapturedCheckBox.Name = "IsPartAttrib2ReleaseCapturedCheckBox";
            this.IsPartAttrib2ReleaseCapturedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
            this.IsPartAttrib2ReleaseCapturedCheckBox.TabIndex = 3;
            this.IsPartAttrib2ReleaseCapturedCheckBox.UseVisualStyleBackColor = true;
            // 
            // IsPartAttrib1ReleaseCapturedCheckBox
            // 
            this.IsPartAttrib1ReleaseCapturedCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsPartAttrib1ReleaseCapturedCheckBox, "RelatedOrganisations.OU_IsPartAttrib1ReleaseCaptured");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_IsPartAttrib1ReleaseCaptured)));
			this.IsPartAttrib1ReleaseCapturedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPartAttrib1ReleaseCapturedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 20, true);
            this.IsPartAttrib1ReleaseCapturedCheckBox.Name = "IsPartAttrib1ReleaseCapturedCheckBox";
            this.IsPartAttrib1ReleaseCapturedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
            this.IsPartAttrib1ReleaseCapturedCheckBox.TabIndex = 1;
            this.IsPartAttrib1ReleaseCapturedCheckBox.UseVisualStyleBackColor = true;
            // 
            // OU_RollUpAttributesOnDocumentsCheckBox
            // 
            this.OU_RollUpAttributesOnDocumentsCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.OU_RollUpAttributesOnDocumentsCheckBox, "RelatedOrganisations.OU_RollUpAttributesOnDocuments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RollUpAttributesOnDocuments)));
			this.OU_RollUpAttributesOnDocumentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OU_RollUpAttributesOnDocumentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 181, true);
            this.OU_RollUpAttributesOnDocumentsCheckBox.Name = "OU_RollUpAttributesOnDocumentsCheckBox";
            this.OU_RollUpAttributesOnDocumentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
            this.OU_RollUpAttributesOnDocumentsCheckBox.TabIndex = 16;
            // 
            // OU_CompletePalletPickingCheckBox
            // 
            this.OU_CompletePalletPickingCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.OU_CompletePalletPickingCheckBox, "RelatedOrganisations.OU_CompletePalletPicking");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_CompletePalletPicking)));
			this.OU_CompletePalletPickingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OU_CompletePalletPickingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 158, true);
            this.OU_CompletePalletPickingCheckBox.Name = "OU_CompletePalletPickingCheckBox";
            this.OU_CompletePalletPickingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
            this.OU_CompletePalletPickingCheckBox.TabIndex = 14;
            this.OU_CompletePalletPickingCheckBox.UseVisualStyleBackColor = true;
            // 
            // OP_PickModeDropEdit
            // 
            this.OP_PickModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OP_PickModeDropEdit, "RelatedOrganisations.OU_PickMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_PickMode)));
            this.OP_PickModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 179, true);
            this.OP_PickModeDropEdit.Name = "OP_PickModeDropEdit";
            this.OP_PickModeDropEdit.PreBoundMaxLength = 4;
			this.OP_PickModeDropEdit.ShouldResizeByMaxLength = true;
			this.OP_PickModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
            this.OP_PickModeDropEdit.TabIndex = 17;
            // 
            // ExpiryDateFormatStringTextBox
            // 
            this.BindingSource.SetBindingMember(this.ExpiryDateFormatStringTextBox, "RelatedOrganisations.OU_ExpiryDateFormatString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_ExpiryDateFormatString)));
            this.ExpiryDateFormatStringTextBox.CaptionResourceString = null;
            this.ExpiryDateFormatStringTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ExpiryDateFormatStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 110, true);
            this.ExpiryDateFormatStringTextBox.Name = "ExpiryDateFormatStringTextBox";
            this.ExpiryDateFormatStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.ExpiryDateFormatStringTextBox.TabIndex = 10;
            // 
            // PackingDateFormatStringTextBox
            // 
            this.BindingSource.SetBindingMember(this.PackingDateFormatStringTextBox, "RelatedOrganisations.OU_PackingDateFormatString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_PackingDateFormatString)));
            this.PackingDateFormatStringTextBox.CaptionResourceString = null;
            this.PackingDateFormatStringTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PackingDateFormatStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 110, true);
            this.PackingDateFormatStringTextBox.Name = "PackingDateFormatStringTextBox";
            this.PackingDateFormatStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
            this.PackingDateFormatStringTextBox.TabIndex = 9;
            // 
            // RFConfirmDropEdit
            // 
            this.RFConfirmDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RFConfirmDropEdit, "RelatedOrganisations.OU_RFAttributeConfirm");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RFAttributeConfirm)));
            this.RFConfirmDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 156, true);
            this.RFConfirmDropEdit.Name = "RFConfirmDropEdit";
            this.RFConfirmDropEdit.PreBoundMaxLength = 4;
			this.RFConfirmDropEdit.ShouldResizeByMaxLength = true;
			this.RFConfirmDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
            this.RFConfirmDropEdit.TabIndex = 15;
            // 
            // UsePartAttrib1CheckBox
            // 
            this.UsePartAttrib1CheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UsePartAttrib1CheckBox, "RelatedOrganisations.OU_UsePartAttrib1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePartAttrib1)));
            this.UsePartAttrib1CheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|5e10e453-c9b1-4661-add1-65d545e9a7a5", "Use Part Attribute 1");
			this.UsePartAttrib1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UsePartAttrib1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 20, true);
            this.UsePartAttrib1CheckBox.Name = "UsePartAttrib1CheckBox";
            this.UsePartAttrib1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
            this.UsePartAttrib1CheckBox.TabIndex = 0;
            this.UsePartAttrib1CheckBox.UseVisualStyleBackColor = true;
            // 
            // UsePartAttrib2CheckBox
            // 
            this.UsePartAttrib2CheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UsePartAttrib2CheckBox, "RelatedOrganisations.OU_UsePartAttrib2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePartAttrib2)));
            this.UsePartAttrib2CheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|750ea062-aa91-4162-b302-0aa69d28f9de", "Use Part Attribute 2");
			this.UsePartAttrib2CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UsePartAttrib2CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 43, true);
            this.UsePartAttrib2CheckBox.Name = "UsePartAttrib2CheckBox";
            this.UsePartAttrib2CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
            this.UsePartAttrib2CheckBox.TabIndex = 2;
            this.UsePartAttrib2CheckBox.UseVisualStyleBackColor = true;
            // 
            // UseExpiryDateCheckBox
            // 
            this.UseExpiryDateCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UseExpiryDateCheckBox, "RelatedOrganisations.OU_UseExpiryDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UseExpiryDate)));
			this.UseExpiryDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseExpiryDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 135, true);
            this.UseExpiryDateCheckBox.Name = "UseExpiryDateCheckBox";
            this.UseExpiryDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
            this.UseExpiryDateCheckBox.TabIndex = 11;
            this.UseExpiryDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // UsePartAttrib3CheckBox
            // 
            this.UsePartAttrib3CheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UsePartAttrib3CheckBox, "RelatedOrganisations.OU_UsePartAttrib3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePartAttrib3)));
            this.UsePartAttrib3CheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|28fe1328-e7e2-432f-a2c4-9ff4dade25e7", "Use Part Attribute 3");
			this.UsePartAttrib3CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UsePartAttrib3CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 66, true);
            this.UsePartAttrib3CheckBox.Name = "UsePartAttrib3CheckBox";
            this.UsePartAttrib3CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
            this.UsePartAttrib3CheckBox.TabIndex = 4;
            this.UsePartAttrib3CheckBox.UseVisualStyleBackColor = true;
            // 
            // UsePackingDateCheckBox
            // 
            this.UsePackingDateCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.UsePackingDateCheckBox, "RelatedOrganisations.OU_UsePackingDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_UsePackingDate)));
			this.UsePackingDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UsePackingDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 112, true);
            this.UsePackingDateCheckBox.Name = "UsePackingDateCheckBox";
            this.UsePackingDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
            this.UsePackingDateCheckBox.TabIndex = 8;
            this.UsePackingDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // OtherDetailsTabPage
            // 
            this.OtherDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|a577d006-69a0-4a53-a8f4-ff9677f732af", "Other Details");
            this.OtherDetailsTabPage.Controls.Add(this.CartonizationGroupBox);
            this.OtherDetailsTabPage.Controls.Add(this.DefaultChargesGroupBox);
            this.OtherDetailsTabPage.Controls.Add(this.LandedCostingGroupBox);
            this.OtherDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.OtherDetailsTabPage.Name = "OtherDetailsTabPage";
            this.OtherDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.OtherDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 213, true);
            this.OtherDetailsTabPage.TabIndex = 2;
            // 
            // CartonizationGroupBox
            // 
            this.CartonizationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19084c8b-5dd0-49ba-99ed-a09d54e68725", "Cartonization");
            this.CartonizationGroupBox.Controls.Add(this.CartonGroupGuidFindBox);
            this.CartonizationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 104, true);
            this.CartonizationGroupBox.Name = "CartonizationGroupBox";
            this.CartonizationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 84, true);
            this.CartonizationGroupBox.TabIndex = 4;
            this.CartonizationGroupBox.TabStop = false;
            // 
            // CartonGroupGuidFindBox
            // 
            this.CartonGroupGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CartonGroupGuidFindBox, "RelatedOrganisations.OU_WCG_CartonGroup");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_WCG_CartonGroup)));
            this.CartonGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 39, true);
            this.CartonGroupGuidFindBox.Name = "CartonGroupGuidFindBox";
            this.CartonGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
            this.CartonGroupGuidFindBox.TabIndex = 5;
            // 
            // DefaultChargesGroupBox
            // 
            this.DefaultChargesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|17c1666e-f2d0-4b3b-ad56-d832ed9b1b1a", "Default Charges");
            this.DefaultChargesGroupBox.Controls.Add(this.RoyaltyFlatAmountCalcFindBox);
            this.DefaultChargesGroupBox.Controls.Add(this.RoyaltyPercentageNumericUpDown);
            this.DefaultChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 6, true);
            this.DefaultChargesGroupBox.Name = "DefaultChargesGroupBox";
            this.DefaultChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 94, true);
            this.DefaultChargesGroupBox.TabIndex = 3;
            this.DefaultChargesGroupBox.TabStop = false;
            // 
            // RoyaltyFlatAmountCalcFindBox
            // 
            this.RoyaltyFlatAmountCalcFindBox.AllowDrop = true;
            this.RoyaltyFlatAmountCalcFindBox.BindToAmount = "RelatedOrganisations.OU_RoyaltyFlatAmount";
            this.RoyaltyFlatAmountCalcFindBox.BindToUnit = "RelatedOrganisations.OU_RX_NKRoyaltyCurrency";
            this.RoyaltyFlatAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.RoyaltyFlatAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 47, true);
            this.RoyaltyFlatAmountCalcFindBox.Name = "RoyaltyFlatAmountCalcFindBox";
            this.RoyaltyFlatAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
            this.RoyaltyFlatAmountCalcFindBox.TabIndex = 3;
            // 
            // RoyaltyPercentageNumericUpDown
            // 
            this.BindingSource.SetBindingMember(this.RoyaltyPercentageNumericUpDown, "RelatedOrganisations.OU_RoyaltyPercent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_RoyaltyPercent)));
            this.RoyaltyPercentageNumericUpDown.BindTo = "RelatedOrganisations.OU_RoyaltyPercent";
            this.RoyaltyPercentageNumericUpDown.DecimalPlaces = 3;
            this.RoyaltyPercentageNumericUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.RoyaltyPercentageNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 21, true);
            this.RoyaltyPercentageNumericUpDown.Name = "RoyaltyPercentageNumericUpDown";
            this.RoyaltyPercentageNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
            this.RoyaltyPercentageNumericUpDown.TabIndex = 1;
            this.RoyaltyPercentageNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LandedCostingGroupBox
            // 
            this.LandedCostingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|ba7ebe26-89d3-46ee-a92a-2c507e5e7c0c", "Landed Costing");
            this.LandedCostingGroupBox.Controls.Add(this.LandedCostMarginPercent3CalcEdit);
            this.LandedCostingGroupBox.Controls.Add(this.LandedCostMarginPercent1CalcEdit);
            this.LandedCostingGroupBox.Controls.Add(this.LandedCostMarginPercent2CalcEdit);
            this.LandedCostingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
            this.LandedCostingGroupBox.Name = "LandedCostingGroupBox";
            this.LandedCostingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 94, true);
            this.LandedCostingGroupBox.TabIndex = 2;
            this.LandedCostingGroupBox.TabStop = false;
            // 
            // LandedCostMarginPercent3CalcEdit
            // 
            this.BindingSource.SetBindingMember(this.LandedCostMarginPercent3CalcEdit, "RelatedOrganisations.OU_LandedCostMarginPercent3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LandedCostMarginPercent3)));
            this.LandedCostMarginPercent3CalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|c3486090-8e43-40d6-8a9e-c25e66b7050c", "Landed Costing Mark-up % 3");
            this.LandedCostMarginPercent3CalcEdit.DecimalPlaces = 2;
            this.LandedCostMarginPercent3CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 62, true);
            this.LandedCostMarginPercent3CalcEdit.Name = "LandedCostMarginPercent3CalcEdit";
            this.LandedCostMarginPercent3CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.LandedCostMarginPercent3CalcEdit.TabIndex = 5;
            this.LandedCostMarginPercent3CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LandedCostMarginPercent1CalcEdit
            // 
            this.BindingSource.SetBindingMember(this.LandedCostMarginPercent1CalcEdit, "RelatedOrganisations.OU_LandedCostMarginPercent1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LandedCostMarginPercent1)));
            this.LandedCostMarginPercent1CalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|320d5fc6-4577-4025-9eac-da94eec43543", "Landed Costing Mark-up % 1");
            this.LandedCostMarginPercent1CalcEdit.DecimalPlaces = 2;
            this.LandedCostMarginPercent1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 16, true);
            this.LandedCostMarginPercent1CalcEdit.Name = "LandedCostMarginPercent1CalcEdit";
            this.LandedCostMarginPercent1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.LandedCostMarginPercent1CalcEdit.TabIndex = 1;
            this.LandedCostMarginPercent1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LandedCostMarginPercent2CalcEdit
            // 
            this.BindingSource.SetBindingMember(this.LandedCostMarginPercent2CalcEdit, "RelatedOrganisations.OU_LandedCostMarginPercent2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartRelation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).RelatedOrganisations)).SyncRoot)).OU_LandedCostMarginPercent2)));
            this.LandedCostMarginPercent2CalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|a31eb062-1a7e-438e-9990-4860d7a1777f", "Landed Costing Mark-up % 2");
            this.LandedCostMarginPercent2CalcEdit.DecimalPlaces = 2;
            this.LandedCostMarginPercent2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 39, true);
            this.LandedCostMarginPercent2CalcEdit.Name = "LandedCostMarginPercent2CalcEdit";
            this.LandedCostMarginPercent2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.LandedCostMarginPercent2CalcEdit.TabIndex = 3;
            this.LandedCostMarginPercent2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CustomFieldsTabPage
            // 
            this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelatedOrganizationUserControl|B628C6A1-9D9A-4B40-AD6D-D57B7AFE083C", "Custom Fields");
            this.CustomFieldsTabPage.Controls.Add(this.CustomFieldsControl1);
            this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
            this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 213, true);
            this.CustomFieldsTabPage.TabIndex = 3;
            // 
            // CustomFieldsControl1
            // 
            this.CustomFieldsControl1.AllowDrop = true;
            this.CustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.CustomFieldsControl1.Name = "CustomFieldsControl1";
            this.CustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 207, true);
            this.CustomFieldsControl1.TabIndex = 0;
            // 
            // RelatedOrganizationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.RelatedOrganizationPanel);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 310, true);
            this.Name = "RelatedOrganizationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 378, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.RelatedOrganizationPanel.ResumeLayout(false);
            this.RelatedOrganizationPanel.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridPartRelations)).EndInit();
            this.GridPartRelations.ResumeLayout(false);
            this.GridPartRelations.PerformLayout();
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.RelationshipTabPage.ResumeLayout(false);
            this.RelationshipTabPage.PerformLayout();
            this.RelationGroupBox.ResumeLayout(false);
            this.RelationGroupBox.PerformLayout();
            this.UnitPriceCurrencyBox.ResumeLayout(true);
            this.UnitPriceCurrencyBox.PerformLayout();
            this.DefaultHoldCodeFindBox.ResumeLayout(true);
            this.DefaultHoldCodeFindBox.PerformLayout();
            this.ClientUQDropEdit.ResumeLayout(true);
            this.ClientUQDropEdit.PerformLayout();
            this.OrganisationFindBox.ResumeLayout(true);
            this.OrganisationFindBox.PerformLayout();
            this.RelationshipDropEdit.ResumeLayout(true);
            this.RelationshipDropEdit.PerformLayout();
            this.AttributesTabPage.ResumeLayout(false);
            this.AttributesTabPage.PerformLayout();
            this.AttributesGroupBox.ResumeLayout(false);
            this.AttributesGroupBox.PerformLayout();
            this.JulianBatchNumberFormatDropEdit.ResumeLayout(true);
            this.JulianBatchNumberFormatDropEdit.PerformLayout();
            this.OP_PickModeDropEdit.ResumeLayout(true);
            this.OP_PickModeDropEdit.PerformLayout();
            this.RFConfirmDropEdit.ResumeLayout(true);
            this.RFConfirmDropEdit.PerformLayout();
            this.OtherDetailsTabPage.ResumeLayout(false);
            this.OtherDetailsTabPage.PerformLayout();
            this.CartonizationGroupBox.ResumeLayout(false);
            this.CartonizationGroupBox.PerformLayout();
            this.CartonGroupGuidFindBox.ResumeLayout(true);
            this.CartonGroupGuidFindBox.PerformLayout();
            this.DefaultChargesGroupBox.ResumeLayout(false);
            this.DefaultChargesGroupBox.PerformLayout();
            this.RoyaltyFlatAmountCalcFindBox.ResumeLayout(true);
            this.RoyaltyFlatAmountCalcFindBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RoyaltyPercentageNumericUpDown)).EndInit();
            this.RoyaltyPercentageNumericUpDown.ResumeLayout(false);
            this.RoyaltyPercentageNumericUpDown.PerformLayout();
            this.LandedCostingGroupBox.ResumeLayout(false);
            this.LandedCostingGroupBox.PerformLayout();
            this.CustomFieldsTabPage.ResumeLayout(false);
            this.CustomFieldsTabPage.PerformLayout();
            this.CustomFieldsControl1.ResumeLayout(true);
            this.CustomFieldsControl1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel RelatedOrganizationPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage RelationshipTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RelationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ClientUQDropEdit;
		private Enterprise.ZArchitecture.ZTextBox LocalPartDescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox LocalPartNumberTextBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox OrganisationFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RelationshipDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox FormLayoutControllerCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage AttributesTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AttributesGroupBox;
		private Enterprise.ZArchitecture.ZTextBox ExpiryDateFormatStringTextBox;
		private Enterprise.ZArchitecture.ZTextBox PackingDateFormatStringTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RFConfirmDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UsePartAttrib1CheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UsePartAttrib2CheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UseExpiryDateCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UsePartAttrib3CheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UsePackingDateCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage OtherDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DefaultChargesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox RoyaltyFlatAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown RoyaltyPercentageNumericUpDown;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LandedCostingGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit LandedCostMarginPercent3CalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LandedCostMarginPercent1CalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LandedCostMarginPercent2CalcEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		public Enterprise.ZArchitecture.ZGrid GridPartRelations;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OP_PickModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OU_CompletePalletPickingCheckBox;
		private ZArchitecture.GUI.ZCheckBox OU_RollUpAttributesOnDocumentsCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsPartAttrib3ReleaseCapturedCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsPartAttrib2ReleaseCapturedCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsPartAttrib1ReleaseCapturedCheckBox;
		private ZArchitecture.GUI.ZDropEdit JulianBatchNumberFormatDropEdit;
		private ZArchitecture.GUI.ZLinkLabel barcodeParsingLinkLabel;
		private Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CustomFieldsControl1;
		private ZArchitecture.GUI.ZGroupBox CartonizationGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox CartonGroupGuidFindBox;
		private ZArchitecture.ZCalcEdit OU_HiCalcEdit;
		private ZArchitecture.ZCalcEdit OU_TiCalcEdit;
		private ZArchitecture.ZCalcEdit ConsigneeMinShelfLifeAcceptedDaysCalcEdit;
		private ZArchitecture.GUI.ZGuidFindBox DefaultHoldCodeFindBox;
		private ZArchitecture.ZCalcEdit UntiPriceBox;
		private ZArchitecture.GUI.ZCodeFindBox UnitPriceCurrencyBox;
		private ZArchitecture.GUI.ZCheckBox IsSerialNumberReleaseCapturedCheckBox;
		private ZArchitecture.GUI.ZCheckBox UseSerialNumberCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PreventReceivingOversCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit ReceiveOverageTolerancePercentCalcEdit;
	}
}
