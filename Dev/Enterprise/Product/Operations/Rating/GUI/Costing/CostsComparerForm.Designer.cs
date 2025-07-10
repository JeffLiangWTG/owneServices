using System;
using System.ComponentModel;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CostsComparerForm
	{
		private ZDropEdit ModeDropEdit;
		private ZButton CancelButtonX;
		internal ZTemplateTabControl MainTabControl;
		private ZTabPage FilterTabPage;
		private ZCodeFindBox OriginCodeFindBox;
		private ZCodeFindBox DestinationCodeFindBox;
		private ZCodeFindBox CommodityCodeCodeFindBox;
		internal ZButton NextButton;
		internal ZButton PreviousButton;
		private ZTabPage CostsTabPage;
		internal ZGrid CostsGrid;
		internal RateLinesAndItemsControl RateLinesAndItemsControl;
		private ZGuidFindBox ContainerGuidFindBox;
		private ZCheckBox ShowOriginDestinationCheckBox;
		private ZCodeFindBox CurrencyCodeFindBox;
		private ZLabel label1;
		private ZRadioButton ShowAllChargesRadioButton;
		private ZRadioButton ShowDestinationChargesOnlyRadioButton;
		private ZRadioButton ShowOriginChargesOnlyRadioButton;
		private ZRadioButton SingleChargeCodeComparisonOnlyRadioButton;
		private ZGuidFindBox ChargeCodeFindBox;
		private ZPanel zPanel1;
		private ZDateEdit ValidToDateEdit;
		private ZDateEdit ValidFromDateEdit;
		private ZTextBox ContractNumberTextBox;
		private IContainer components;
		internal ZLabel SecurityMessageLabel;
		private ZTextBox FreightRatingClassTextBox;
		private ZTextBox HandlingRatingClassTextBox;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.CancelButtonX = new ZButton();
			this.MainTabControl = new ZTemplateTabControl();
			this.FilterTabPage = new ZTabPage();
			this.CostsTabPage = new ZTabPage();
			this.NextButton = new ZButton();
			this.PreviousButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 442, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CostsComparer);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.CancelButtonX.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|a790dd02-eb44-4fc5-95e2-c6db4a997cb8", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(872, 412, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.CancelButtonX.TabIndex = 3;
			this.CancelButtonX.ToolTipCaption = null;
			this.CancelButtonX.Click += new EventHandler(this.CancelButtonX_Click);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.FilterTabPage);
			this.MainTabControl.Controls.Add(this.CostsTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 404, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.SelectedIndexChanging += new EventHandler(this.MainTabControl_SelectedIndexChanging);
			// 
			// FilterTabPage
			// 
			this.FilterTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|9eeb9c7d-d445-49d9-80bf-00e30987de61", "Filter");
			this.FilterTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FilterTabPage.Name = "FilterTabPage";
			this.FilterTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 377, true);
			this.FilterTabPage.TabIndex = 0;
			this.FilterTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.FilterTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ChargeCodePK);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).SingleChargeCodeComparisonOnly);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ShowOriginChargesOnly);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ShowDestinationChargesOnly);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ShowAllCharges);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ShowOriginDestination);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ValidFromDate);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).CommodityCode);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).Destination);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).Origin);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).Mode);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ContainerType);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ContainerFreightRatingClass);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ContainerHandlingRatingClass);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ValidToDate);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).ContractNumber);
			// 
			// CostsTabPage
			// 
			this.CostsTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|a53e67ef-2e4e-491e-825f-c84e5a5bbfe5", "Costs");
			this.CostsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CostsTabPage.Name = "CostsTabPage";
			this.CostsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 377, true);
			this.CostsTabPage.TabIndex = 1;
			this.CostsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.CostsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).Currency);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparer)(null)).Costs);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.Organisation);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.Parent.TH_ClientFullName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_OriginLRC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_DestinationLRC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_ViaLRC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_RC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_PL_NKCarrierServiceLevel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_RH_NKCommodityCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_OH_TransportProvider);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_OH_Consignor);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_OH_Consignee);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_OH_ControllingCustomer);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_TransitTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_Frequency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_FrequencyUnit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.Unit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_RateStartDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_RateEndDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CostsComparerEntry)(((System.Collections.IList)(((CostsComparer)(null)).Costs)).SyncRoot)).Entry.TI_ContractNumber);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.NextButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|7c02c552-5b2c-4324-9991-174ec41ad605", "Next");
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 412, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.NextButton.TabIndex = 2;
			this.NextButton.ToolTipCaption = null;
			this.NextButton.Click += new EventHandler(this.NextButton_Click);
			// 
			// PreviousButton
			// 
			this.PreviousButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.PreviousButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|d383199f-809a-4569-a927-51921fc73294", "Previous");
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 412, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.PreviousButton.TabIndex = 1;
			this.PreviousButton.ToolTipCaption = null;
			this.PreviousButton.Click += new EventHandler(this.PreviousButton_Click);
			// 
			// CostsComparerForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("1412b0c9-0b1a-42d2-bb86-788dfee05a8e", "Freight Costs Comparison");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 466, true);
			this.Controls.Add(this.PreviousButton);
			this.Controls.Add(this.NextButton);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.CancelButtonX);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(CostsComparer);
			this.DataSourceTypeName = "Enterprise.Rating.Business.CostsComparer";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 500, true);
			this.Name = "CostsComparerForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.NextButton, 0);
			this.Controls.SetChildIndex(this.PreviousButton, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void FilterTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ChargeCodeFindBox = new ZGuidFindBox();
			this.SingleChargeCodeComparisonOnlyRadioButton = new ZRadioButton();
			this.ShowOriginChargesOnlyRadioButton = new ZRadioButton();
			this.ShowDestinationChargesOnlyRadioButton = new ZRadioButton();
			this.ShowAllChargesRadioButton = new ZRadioButton();
			this.label1 = new ZLabel();
			this.ShowOriginDestinationCheckBox = new ZCheckBox();
			this.ValidFromDateEdit = new ZDateEdit();
			this.CommodityCodeCodeFindBox = new ZCodeFindBox();
			this.DestinationCodeFindBox = new ZCodeFindBox();
			this.OriginCodeFindBox = new ZCodeFindBox();
			this.ModeDropEdit = new ZDropEdit();
			this.ContainerGuidFindBox = new ZGuidFindBox();
			this.FreightRatingClassTextBox = new ZTextBox();
			this.HandlingRatingClassTextBox = new ZTextBox();
			this.ValidToDateEdit = new ZDateEdit();
			this.ContractNumberTextBox = new ZTextBox();
			this.FilterTabPage.SuspendLayout();
			this.ChargeCodeFindBox.SuspendLayout();
			this.ValidFromDateEdit.SuspendLayout();
			this.CommodityCodeCodeFindBox.SuspendLayout();
			this.DestinationCodeFindBox.SuspendLayout();
			this.OriginCodeFindBox.SuspendLayout();
			this.ModeDropEdit.SuspendLayout();
			this.ContainerGuidFindBox.SuspendLayout();
			this.ValidToDateEdit.SuspendLayout();
			this.FilterTabPage.Controls.Add(this.ContractNumberTextBox);
			this.FilterTabPage.Controls.Add(this.ValidToDateEdit);
			this.FilterTabPage.Controls.Add(this.ChargeCodeFindBox);
			this.FilterTabPage.Controls.Add(this.SingleChargeCodeComparisonOnlyRadioButton);
			this.FilterTabPage.Controls.Add(this.ShowOriginChargesOnlyRadioButton);
			this.FilterTabPage.Controls.Add(this.ShowDestinationChargesOnlyRadioButton);
			this.FilterTabPage.Controls.Add(this.ShowAllChargesRadioButton);
			this.FilterTabPage.Controls.Add(this.label1);
			this.FilterTabPage.Controls.Add(this.ShowOriginDestinationCheckBox);
			this.FilterTabPage.Controls.Add(this.ValidFromDateEdit);
			this.FilterTabPage.Controls.Add(this.CommodityCodeCodeFindBox);
			this.FilterTabPage.Controls.Add(this.DestinationCodeFindBox);
			this.FilterTabPage.Controls.Add(this.OriginCodeFindBox);
			this.FilterTabPage.Controls.Add(this.ModeDropEdit);
			this.FilterTabPage.Controls.Add(this.ContainerGuidFindBox);
			this.FilterTabPage.Controls.Add(this.FreightRatingClassTextBox);
			this.FilterTabPage.Controls.Add(this.HandlingRatingClassTextBox);
			// 
			// ChargeCodeFindBox
			// 
			this.ChargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeCodeFindBox, "ChargeCodePK");
			this.ChargeCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ac24fd89-a9b6-4d07-8456-3a4f609e58c7", "Charge Code for Comparison");
			this.ChargeCodeFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ChargeCodeFindBox, false);
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 315, true);
			this.ChargeCodeFindBox.Name = "ChargeCodeFindBox";
			this.ChargeCodeFindBox.ShouldResize = true;
			this.ChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 20, true);
			this.ChargeCodeFindBox.TabIndex = 17;
			// 
			// SingleChargeCodeComparisonOnlyRadioButton
			// 
			this.SingleChargeCodeComparisonOnlyRadioButton.AutoCheck = false;
			this.SingleChargeCodeComparisonOnlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SingleChargeCodeComparisonOnlyRadioButton, "SingleChargeCodeComparisonOnly");
			this.SingleChargeCodeComparisonOnlyRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|e173ea99-fc98-43be-87da-e98b32a9ade6", "Single Charge Code Comparison Only");
			this.SingleChargeCodeComparisonOnlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SingleChargeCodeComparisonOnlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 292, true);
			this.SingleChargeCodeComparisonOnlyRadioButton.Name = "SingleChargeCodeComparisonOnlyRadioButton";
			this.SingleChargeCodeComparisonOnlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 17, true);
			this.SingleChargeCodeComparisonOnlyRadioButton.TabIndex = 16;
			// 
			// ShowOriginChargesOnlyRadioButton
			// 
			this.ShowOriginChargesOnlyRadioButton.AutoCheck = false;
			this.ShowOriginChargesOnlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowOriginChargesOnlyRadioButton, "ShowOriginChargesOnly");
			this.ShowOriginChargesOnlyRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|5e10ac2d-2e29-460e-aaef-c9a61615b703", "Origin Charges Only");
			this.ShowOriginChargesOnlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowOriginChargesOnlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 246, true);
			this.ShowOriginChargesOnlyRadioButton.Name = "ShowOriginChargesOnlyRadioButton";
			this.ShowOriginChargesOnlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
			this.ShowOriginChargesOnlyRadioButton.TabIndex = 14;
			// 
			// ShowDestinationChargesOnlyRadioButton
			// 
			this.ShowDestinationChargesOnlyRadioButton.AutoCheck = false;
			this.ShowDestinationChargesOnlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowDestinationChargesOnlyRadioButton, "ShowDestinationChargesOnly");
			this.ShowDestinationChargesOnlyRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|f7bb6a14-6afa-4785-91d5-67ec5fa813b7", "Destination Charges Only");
			this.ShowDestinationChargesOnlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowDestinationChargesOnlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 269, true);
			this.ShowDestinationChargesOnlyRadioButton.Name = "ShowDestinationChargesOnlyRadioButton";
			this.ShowDestinationChargesOnlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.ShowDestinationChargesOnlyRadioButton.TabIndex = 15;
			// 
			// ShowAllChargesRadioButton
			// 
			this.ShowAllChargesRadioButton.AutoCheck = false;
			this.ShowAllChargesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowAllChargesRadioButton, "ShowAllCharges");
			this.ShowAllChargesRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|0165101d-bf83-4f54-8db2-d406c55f8622", "All Freight Charges");
			this.ShowAllChargesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowAllChargesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 200, true);
			this.ShowAllChargesRadioButton.Name = "ShowAllChargesRadioButton";
			this.ShowAllChargesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
			this.ShowAllChargesRadioButton.TabIndex = 12;
			// 
			// label1
			// 
			this.label1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|9c0e5e57-bf42-4d09-962a-4446fca0cd50", "", "Enter details of the costs that you wish to compare. You can also choose whether applicable origin / destination charges are included when calculating comparison rates. If not included, only freight and related charges are included when calculating comparison rates.");
			this.label1.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 32, true);
			this.label1.TabIndex = 0;
			// 
			// ShowOriginDestinationCheckBox
			// 
			this.ShowOriginDestinationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowOriginDestinationCheckBox, "ShowOriginDestination");
			this.ShowOriginDestinationCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|c1f8f9ca-1c0b-4af6-a004-03079fc3893f", "Include Origin/Destination rates in comparison rates.");
			this.ShowOriginDestinationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowOriginDestinationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 223, true);
			this.ShowOriginDestinationCheckBox.Name = "ShowOriginDestinationCheckBox";
			this.ShowOriginDestinationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.ShowOriginDestinationCheckBox.TabIndex = 13;
			// 
			// ValidFromDateEdit
			// 
			this.ValidFromDateEdit.AllowDrop = true;
			this.ValidFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.ValidFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ValidFromDateEdit, "ValidFromDate");
			this.ValidFromDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|99d7129f-8762-4ed1-913a-73a522cb5df9", "Valid From");
			this.ValidFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
			this.ValidFromDateEdit.Name = "ValidFromDateEdit";
			this.ValidFromDateEdit.TabIndex = 2;
			// 
			// CommodityCodeCodeFindBox
			// 
			this.CommodityCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeCodeFindBox, "CommodityCode");
			this.CommodityCodeCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|2b1994cf-52c2-44ab-a00c-c0339d2f5bd1", "Commodity Code");
			this.CommodityCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 117, true);
			this.CommodityCodeCodeFindBox.Name = "CommodityCodeCodeFindBox";
			this.CommodityCodeCodeFindBox.PreBoundMaxLength = 4;
			this.CommodityCodeCodeFindBox.ShouldResize = true;
			this.CommodityCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CommodityCodeCodeFindBox.TabIndex = 8;
			// 
			// DestinationCodeFindBox
			// 
			this.DestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCodeFindBox, "Destination");
			this.DestinationCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|da49eaf4-fdb1-4bd8-a6d9-0250177327e8", "Destination");
			this.DestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 90, true);
			this.DestinationCodeFindBox.Name = "DestinationCodeFindBox";
			this.DestinationCodeFindBox.PreBoundMaxLength = 5;
			this.DestinationCodeFindBox.ShouldResize = true;
			this.DestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.DestinationCodeFindBox.TabIndex = 6;
			// 
			// OriginCodeFindBox
			// 
			this.OriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "Origin");
			this.OriginCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|c141c069-deb5-4029-9a78-fd29245cd556", "Origin");
			this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 64, true);
			this.OriginCodeFindBox.Name = "OriginCodeFindBox";
			this.OriginCodeFindBox.PreBoundMaxLength = 5;
			this.OriginCodeFindBox.ShouldResize = true;
			this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.OriginCodeFindBox.TabIndex = 5;
			// 
			// ModeDropEdit
			// 
			this.ModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModeDropEdit, "Mode");
			this.ModeDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|70678e6e-71dc-46a8-9bc5-e6b8e77b5d43", "Mode");
			this.ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 90, true);
			this.ModeDropEdit.Name = "ModeDropEdit";
			this.ModeDropEdit.PreBoundMaxLength = 3;
			this.ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ModeDropEdit.TabIndex = 4;
			// 
			// ContainerGuidFindBox
			// 
			this.ContainerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerGuidFindBox, "ContainerType");
			this.ContainerGuidFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|f6d443fe-ec45-4c8d-bac1-cddd7bfbebab", "Container");
			this.ContainerGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ContainerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 116, true);
			this.ContainerGuidFindBox.Name = "ContainerGuidFindBox";
			this.ContainerGuidFindBox.PreBoundMaxLength = 5;
			this.ContainerGuidFindBox.ShouldResize = true;
			this.ContainerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ContainerGuidFindBox.TabIndex = 7;
			// 
			// FreightRatingClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.FreightRatingClassTextBox, "ContainerFreightRatingClass");
			this.FreightRatingClassTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|12aa7440-d136-4642-9245-00f148c9952b", "Freight Rate Class");
			this.FreightRatingClassTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FreightRatingClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 143, true);
			this.FreightRatingClassTextBox.Name = "FreightRatingClassTextBox";
			this.FreightRatingClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.FreightRatingClassTextBox.TabIndex = 9;
			// 
			// HandlingRatingClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.HandlingRatingClassTextBox, "ContainerHandlingRatingClass");
			this.HandlingRatingClassTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|2b1389c1-8942-4bfa-ba86-dccae36eb31e", "Handling Rate Class");
			this.HandlingRatingClassTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HandlingRatingClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 170, true);
			this.HandlingRatingClassTextBox.Name = "HandlingRatingClassTextBox";
			this.HandlingRatingClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.HandlingRatingClassTextBox.TabIndex = 10;
			// 
			// ValidToDateEdit
			// 
			this.ValidToDateEdit.AllowDrop = true;
			this.ValidToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ValidToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ValidToDateEdit, "ValidToDate");
			this.ValidToDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|13d76ea4-ce4b-480d-99a1-62e6de463797", "To");
			this.ValidToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 64, true);
			this.ValidToDateEdit.Name = "ValidToDateEdit";
			this.ValidToDateEdit.TabIndex = 3;
			// 
			// ContractNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContractNumberTextBox, "ContractNumber");
			this.ContractNumberTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|08094752-b53f-48f5-9efe-8f13fecdc0f8", "Contract No.");
			this.ContractNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContractNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 143, true);
			this.ContractNumberTextBox.Name = "ContractNumberTextBox";
			this.ContractNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.ContractNumberTextBox.TabIndex = 11;
			this.FilterTabPage.PerformLayout();
			this.ChargeCodeFindBox.ResumeLayout(true);
			this.ChargeCodeFindBox.PerformLayout();
			this.ValidFromDateEdit.ResumeLayout(true);
			this.ValidFromDateEdit.PerformLayout();
			this.CommodityCodeCodeFindBox.ResumeLayout(true);
			this.CommodityCodeCodeFindBox.PerformLayout();
			this.DestinationCodeFindBox.ResumeLayout(true);
			this.DestinationCodeFindBox.PerformLayout();
			this.OriginCodeFindBox.ResumeLayout(true);
			this.OriginCodeFindBox.PerformLayout();
			this.ModeDropEdit.ResumeLayout(true);
			this.ModeDropEdit.PerformLayout();
			this.ContainerGuidFindBox.ResumeLayout(true);
			this.ContainerGuidFindBox.PerformLayout();
			this.ValidToDateEdit.ResumeLayout(true);
			this.ValidToDateEdit.PerformLayout();
			this.FilterTabPage.ResumeLayout(true);
		}

		private void CostsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			this.zPanel1 = new ZPanel();
			this.CurrencyCodeFindBox = new ZCodeFindBox();
			this.RateLinesAndItemsControl = new RateLinesAndItemsControl();
			this.CostsGrid = new ZGrid();
			this.SecurityMessageLabel = new ZLabel();
			this.CostsTabPage.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.RateLinesAndItemsControl.SuspendLayout();
			((ISupportInitialize)(this.CostsGrid)).BeginInit();
			this.CostsGrid.SuspendLayout();
			this.CostsTabPage.Controls.Add(this.zPanel1);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CurrencyCodeFindBox);
			this.zPanel1.Controls.Add(this.RateLinesAndItemsControl);
			this.zPanel1.Controls.Add(this.CostsGrid);
			this.zPanel1.Controls.Add(this.SecurityMessageLabel);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 377, true);
			this.zPanel1.TabIndex = 4;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "Currency");
			this.CurrencyCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|c094655a-96b9-4486-826c-9de11419f31f", "View Costs in Currency");
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 6, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.CurrencyCodeFindBox.ShouldResize = true;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.CurrencyCodeFindBox.TabIndex = 5;
			// 
			// RateLinesAndItemsControl
			// 
			this.RateLinesAndItemsControl.AllowDrop = true;
			this.RateLinesAndItemsControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.RateLinesAndItemsControl, ".");
			this.RateLinesAndItemsControl.BindTo = "Costs.RateLines";
			this.RateLinesAndItemsControl.CalculatorPanelAgentRatesCheckBoxVisible = false;
			this.RateLinesAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 210, true);
			this.RateLinesAndItemsControl.Name = "RateLinesAndItemsControl";
			this.RateLinesAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RateLinesAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 160, true);
			this.RateLinesAndItemsControl.TabIndex = 7;
			// 
			// CostsGrid
			// 
			this.CostsGrid.AllowNavigation = false;
			this.CostsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.CostsGrid, "Costs");
			this.CostsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|3172460d-7252-4ef1-be55-31dd0dfd8de9", "Svc. Prov. Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Entry+Organisation";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "Entry+TI_OriginLRC";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|b3ee64e8-063a-4d3c-981b-9b01379080b5", "Destination");
			zTextBoxColumnStyleInfo3.ColumnName = "Entry+TI_DestinationLRC";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.ColumnName = "Entry+TI_ViaLRC";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.ColumnName = "Entry+TI_PL_NKCarrierServiceLevel";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = null;
			zTextBoxColumnStyleInfo6.ColumnName = "Entry+TI_RH_NKCommodityCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = null;
			zTextBoxColumnStyleInfo7.ColumnName = "Entry+TI_TransitTime";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = null;
			zTextBoxColumnStyleInfo8.ColumnName = "Entry+TI_FrequencyUnit";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.ColumnName = "Entry+Unit";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = null;
			zTextBoxColumnStyleInfo10.ColumnName = "Entry+TI_ContractNumber";
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CostsComparerForm|823BBFC3-377F-4F04-8A50-31FCBAADE81C", "Service Provider Name");
			zTextBoxColumnStyleInfo11.ColumnName = "Entry+Parent+TH_ClientFullName";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Entry+TI_RC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Entry+TI_OH_TransportProvider";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo3.Caption = null;
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Entry+TI_OH_Consignor";
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo4.Caption = null;
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo4.ColumnName = "Entry+TI_OH_Consignee";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo5.Caption = null;
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo5.ColumnName = "Entry+TI_OH_ControllingCustomer";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Entry+TI_Frequency";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.ColumnName = "Entry+TI_RateStartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = null;
			zDateEditColumnStyleInfo2.ColumnName = "Entry+TI_RateEndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsVisible = false;
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.CostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.CostsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CostsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CostsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CostsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CostsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CostsGrid.CopySelectedRowsAllowed = true;
			this.CostsGrid.GridId = "e2bd6a93-ec4e-44e4-b7a6-661555cf5767";
			this.CostsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CostsGrid.IsWholeRowSelectedOnClick = true;
			this.CostsGrid.LayoutKey = "CostsGrid";
			this.CostsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 38, true);
			this.CostsGrid.Name = "CostsGrid";
			this.CostsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.CostsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 160, true);
			this.CostsGrid.TabIndex = 6;
			this.CostsGrid.AfterBind += CostsGridAfterBind;
			// 
			// SecurityMessageLabel
			// 
			this.SecurityMessageLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SecurityMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 200, true);
			this.SecurityMessageLabel.Name = "SecurityMessageLabel";
			this.SecurityMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 160, true);
			this.SecurityMessageLabel.TabIndex = 3;
			this.SecurityMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.SecurityMessageLabel.Visible = false;
			this.CostsTabPage.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.RateLinesAndItemsControl.ResumeLayout(true);
			this.RateLinesAndItemsControl.PerformLayout();
			((ISupportInitialize)(this.CostsGrid)).EndInit();
			this.CostsGrid.ResumeLayout(false);
			this.CostsGrid.PerformLayout();
			this.CostsTabPage.ResumeLayout(true);
		}
	}
}
