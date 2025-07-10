using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuoteTabControl
	{
		private QuoteSignaturesControl quoteSignaturesControl1;
		private DocumentSelectionControl documentSelectionControl1;
		private SummaryEntryPanel summaryEntryPanel1;
		private RateEntryPanelWithRelatedLines rateEntryPanelWithRelatedLines1;
		private ZCheckBox PrintDestinationCheckBox;
		private ZCheckBox PrintOriginCheckBox;
		private ZCheckBox PrintInheritedOriginCheckBox;
		private ZCheckBox PrintInheritedDestinationCheckBox;
		private ZTemplateTabControl TabControl;
		private ZTabPage QuoteFormatTabPage;
		private ZArchitecture.ZGrid QuoteFormatEntryGrid;
		private ZArchitecture.ZTextBox PageHeaderTextBox;
		private ZArchitecture.ZTextBox PageOpeningTextBox;
		private ZArchitecture.ZTextBox PageClosingTextBox;
		private ZTabPage DocumentLayoutTabPage;
		private ZTabPage CustomFieldTabPage;
		private ZLogsTabPage zEventTabPage1;
		private ZTabPage SummaryTabPage;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZArchitecture.ZLabel QuoteFormatInstructionsLabel;
		private ZGuidFindBox AgentOverrideFindBox;
		private ZTabPage TemplateTabPage;
		private ZDropEdit PaymentTermDropEdit;
		private ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl;
		public ZWorkflowTabPage WorkflowTabPage;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			this.TabControl = new ZTemplateTabControl();
			this.TemplateTabPage = new ZTabPage();
			this.rateEntryPanelWithRelatedLines1 = new RateEntryPanelWithRelatedLines();
			this.QuoteFormatTabPage = new ZTabPage();
			this.quoteSignaturesControl1 = new QuoteSignaturesControl();
			this.PrintDestinationCheckBox = new ZCheckBox();
			this.PrintOriginCheckBox = new ZCheckBox();
			this.PrintInheritedDestinationCheckBox = new ZCheckBox();
			this.PrintInheritedOriginCheckBox = new ZCheckBox();
			this.PaymentTermDropEdit = new ZDropEdit();
			this.AgentOverrideFindBox = new ZGuidFindBox();
			this.QuoteFormatInstructionsLabel = new ZArchitecture.ZLabel();
			this.PageClosingTextBox = new ZArchitecture.ZTextBox();
			this.PageOpeningTextBox = new ZArchitecture.ZTextBox();
			this.PageHeaderTextBox = new ZArchitecture.ZTextBox();
			this.QuoteFormatEntryGrid = new ZArchitecture.ZGrid();
			this.DocumentLayoutTabPage = new ZTabPage();
			this.documentSelectionControl1 = new DocumentSelectionControl();
			this.SummaryTabPage = new ZTabPage();
			this.CustomFieldTabPage = new ZTabPage();
			this.summaryEntryPanel1 = new SummaryEntryPanel();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.processTemplateCustomFieldsControl = new ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.TemplateTabPage.SuspendLayout();
			this.QuoteFormatTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuoteFormatEntryGrid)).BeginInit();
			this.DocumentLayoutTabPage.SuspendLayout();
			this.CustomFieldTabPage.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Quote);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TemplateTabPage);
			this.TabControl.Controls.Add(this.QuoteFormatTabPage);
			this.TabControl.Controls.Add(this.DocumentLayoutTabPage);
			this.TabControl.Controls.Add(this.CustomFieldTabPage);
			this.TabControl.Controls.Add(this.SummaryTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TabControl.TabIndex = 0;
			// 
			// TemplateTabPage
			// 
			this.TemplateTabPage.Controls.Add(this.rateEntryPanelWithRelatedLines1);
			this.TemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateTabPage.Name = "TemplateTabPage";
			this.TemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.TemplateTabPage.TabIndex = 0;
			this.TemplateTabPage.Text = "Template";
			// 
			// rateEntryPanelWithRelatedLines1
			// 
			this.rateEntryPanelWithRelatedLines1.AllowDrop = true;
			this.rateEntryPanelWithRelatedLines1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.rateEntryPanelWithRelatedLines1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryPanelWithRelatedLines1.Category = "";
			this.rateEntryPanelWithRelatedLines1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rateEntryPanelWithRelatedLines1.Name = "rateEntryPanelWithRelatedLines1";
			this.rateEntryPanelWithRelatedLines1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.rateEntryPanelWithRelatedLines1.TabIndex = 0;
			// 
			// QuoteFormatTabPage
			// 
			this.QuoteFormatTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|5c630951-04ce-40d5-be64-d93f0da6f4fd", "Document Format");
			this.QuoteFormatTabPage.Controls.Add(this.quoteSignaturesControl1);
			this.QuoteFormatTabPage.Controls.Add(this.PrintDestinationCheckBox);
			this.QuoteFormatTabPage.Controls.Add(this.PrintOriginCheckBox);
			this.QuoteFormatTabPage.Controls.Add(this.PrintInheritedDestinationCheckBox);
			this.QuoteFormatTabPage.Controls.Add(this.PrintInheritedOriginCheckBox);
			this.QuoteFormatTabPage.Controls.Add(this.PaymentTermDropEdit);
			this.QuoteFormatTabPage.Controls.Add(this.AgentOverrideFindBox);
			this.QuoteFormatTabPage.Controls.Add(this.QuoteFormatInstructionsLabel);
			this.QuoteFormatTabPage.Controls.Add(this.PageClosingTextBox);
			this.QuoteFormatTabPage.Controls.Add(this.PageOpeningTextBox);
			this.QuoteFormatTabPage.Controls.Add(this.PageHeaderTextBox);
			this.QuoteFormatTabPage.Controls.Add(this.QuoteFormatEntryGrid);
			this.QuoteFormatTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuoteFormatTabPage.Name = "QuoteFormatTabPage";
			this.QuoteFormatTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.QuoteFormatTabPage.TabIndex = 5;
			// 
			// quoteSignaturesControl1
			// 
			this.quoteSignaturesControl1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.quoteSignaturesControl1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.quoteSignaturesControl1, ".");
			this.quoteSignaturesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 261, true);
			this.quoteSignaturesControl1.Name = "quoteSignaturesControl1";
			this.quoteSignaturesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 75, true);
			this.quoteSignaturesControl1.TabIndex = 18;
			// 
			// PrintDestinationCheckBox
			// 
			this.PrintDestinationCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintDestinationCheckBox, "TH_PrintRateLevelDestinationCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_PrintRateLevelDestinationCharges);
			this.PrintDestinationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintDestinationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 228, true);
			this.PrintDestinationCheckBox.Name = "PrintDestinationCheckBox";
			this.PrintDestinationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 18, true);
			this.PrintDestinationCheckBox.TabIndex = 8;
			// 
			// PrintOriginCheckBox
			// 
			this.PrintOriginCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintOriginCheckBox, "TH_PrintRateLevelOriginCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_PrintRateLevelOriginCharges);
			this.PrintOriginCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintOriginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 194, true);
			this.PrintOriginCheckBox.Name = "PrintOriginCheckBox";
			this.PrintOriginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 18, true);
			this.PrintOriginCheckBox.TabIndex = 6;
			// 
			// PrintInheritedDestinationCheckBox
			// 
			this.PrintInheritedDestinationCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintInheritedDestinationCheckBox, "TH_PrintInheritedDestinationCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_PrintInheritedDestinationCharges);
			this.PrintInheritedDestinationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintInheritedDestinationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 245, true);
			this.PrintInheritedDestinationCheckBox.Name = "PrintInheritedDestinationCheckBox";
			this.PrintInheritedDestinationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 18, true);
			this.PrintInheritedDestinationCheckBox.TabIndex = 9;
			// 
			// PrintInheritedOriginCheckBox
			// 
			this.PrintInheritedOriginCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PrintInheritedOriginCheckBox, "TH_PrintInheritedOriginCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_PrintInheritedOriginCharges);
			this.PrintInheritedOriginCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintInheritedOriginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 211, true);
			this.PrintInheritedOriginCheckBox.Name = "PrintInheritedOriginCheckBox";
			this.PrintInheritedOriginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 18, true);
			this.PrintInheritedOriginCheckBox.TabIndex = 7;
			// 
			// PaymentTermDropEdit
			// 
			this.PaymentTermDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PaymentTermDropEdit, "QuoteFormatEntries.TI_QuotePageIncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_QuotePageIncoTerm);
			this.PaymentTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 228, true);
			this.PaymentTermDropEdit.Name = "PaymentTermDropEdit";
			this.PaymentTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.PaymentTermDropEdit.TabIndex = 5;
			// 
			// AgentOverrideFindBox
			// 
			this.AgentOverrideFindBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.AgentOverrideFindBox, "QuoteFormatEntries.TI_OH_AgentOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_OH_AgentOverride);
			this.AgentOverrideFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 200, true);
			this.AgentOverrideFindBox.Name = "AgentOverrideFindBox";
			this.AgentOverrideFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 20, true);
			this.AgentOverrideFindBox.TabIndex = 3;
			// 
			// QuoteFormatInstructionsLabel
			// 
			this.QuoteFormatInstructionsLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|970fb273-1fd8-43f0-a02c-df5490906cab", "", "You can specify heading, opening and closing text for each freight trade lane specified in your quotation. This information will appear on the printed document.");
			this.QuoteFormatInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 0, true);
			this.QuoteFormatInstructionsLabel.Name = "QuoteFormatInstructionsLabel";
			this.QuoteFormatInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 24, true);
			this.QuoteFormatInstructionsLabel.TabIndex = 0;
			// 
			// PageClosingTextBox
			// 
			this.PageClosingTextBox.AcceptsReturn = true;
			this.PageClosingTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PageClosingTextBox, "QuoteFormatEntries.ClosingText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).ClosingText);
			this.PageClosingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PageClosingTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|60bf1ba2-3917-404e-8510-7f7f4bfd6e8f", "Page Closing Text", "This text will appear at the bottom of the pricing page.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PageClosingTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PageClosingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 360, true);
			this.PageClosingTextBox.Multiline = true;
			this.PageClosingTextBox.Name = "PageClosingTextBox";
			this.PageClosingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.PageClosingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 120, true);
			this.PageClosingTextBox.TabIndex = 17;
			// 
			// PageOpeningTextBox
			// 
			this.PageOpeningTextBox.AcceptsReturn = true;
			this.PageOpeningTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.PageOpeningTextBox, "QuoteFormatEntries.OpeningText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).OpeningText);
			this.PageOpeningTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PageOpeningTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|f5808e70-1e72-463a-bc65-1a8e93e5f306", "Page Opening Text", "This text will appear at the top of the pricing page.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PageOpeningTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PageOpeningTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 360, true);
			this.PageOpeningTextBox.Multiline = true;
			this.PageOpeningTextBox.Name = "PageOpeningTextBox";
			this.PageOpeningTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.PageOpeningTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 120, true);
			this.PageOpeningTextBox.TabIndex = 15;
			// 
			// PageHeaderTextBox
			// 
			this.PageHeaderTextBox.AcceptsReturn = true;
			this.PageHeaderTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.BindingSource.SetBindingMember(this.PageHeaderTextBox, "QuoteFormatEntries.PageHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).PageHeader);
			this.PageHeaderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PageHeaderTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|2b8f12c8-037c-4ca7-976b-f9c7da73a6e0", "Heading Text", "This text will replace the default heading text on each pricing page. Default text contains the Transport mode, Origin and Destination ports.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PageHeaderTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PageHeaderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 264, true);
			this.PageHeaderTextBox.Multiline = true;
			this.PageHeaderTextBox.Name = "PageHeaderTextBox";
			this.PageHeaderTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.PageHeaderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 64, true);
			this.PageHeaderTextBox.TabIndex = 13;
			// 
			// QuoteFormatEntryGrid
			// 
			this.QuoteFormatEntryGrid.AllowNavigation = false;
			this.QuoteFormatEntryGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.QuoteFormatEntryGrid, "QuoteFormatEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).QuoteFormatEntries);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_Mode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_OriginLRC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_DestinationLRC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_ViaLRC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_RX_NKCurrency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_OH_TransportProvider);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).Lookups.ShippingProviders);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_RS_NKServiceLevel_NI);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_RH_NKCommodityCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).Unit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_TransitTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_Frequency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuoteEntry)(((System.Collections.IList)(((Quote)(null)).QuoteFormatEntries)).SyncRoot)).TI_FrequencyUnit);
			this.QuoteFormatEntryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "TI_Mode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TI_OriginLRC";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "TI_DestinationLRC";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "TI_ViaLRC";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "TI_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ShippingProviders";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TI_OH_TransportProvider";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo5.ColumnName = "TI_RS_NKServiceLevel_NI";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "TI_RH_NKCommodityCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo3.ColumnName = "CommodityCode+RH_DescriptionMultilingual";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("88CA0691-8709-4CCE-8F50-26A70AD3B5CF", "Comm. Desc.", "Commodity Description", "Description of the commodity of this rate");
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "Unit";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo2.ColumnName = "TI_TransitTime";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TI_Frequency";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "TI_FrequencyUnit";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.QuoteFormatEntryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.QuoteFormatEntryGrid.GridId = "b8eca8ef-ce4a-480c-8c23-a6592cd902db";
			this.QuoteFormatEntryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuoteFormatEntryGrid.LayoutKey = "AIRRateEntryGrid";
			this.QuoteFormatEntryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.QuoteFormatEntryGrid.Name = "QuoteFormatEntryGrid";
			this.QuoteFormatEntryGrid.ReadOnly = true;
			this.QuoteFormatEntryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 168, true);
			this.QuoteFormatEntryGrid.TabIndex = 1;
			// 
			// DocumentLayoutTabPage
			// 
			this.DocumentLayoutTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|f2c0443e-bdd8-42b8-b15a-83a2a07ee84c", "Document Selection");
			this.DocumentLayoutTabPage.Controls.Add(this.documentSelectionControl1);
			this.DocumentLayoutTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentLayoutTabPage.Name = "DocumentLayoutTabPage";
			this.DocumentLayoutTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.DocumentLayoutTabPage.TabIndex = 6;
			// 
			// documentSelectionControl1
			// 
			this.documentSelectionControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.documentSelectionControl1, ".");
			this.documentSelectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.documentSelectionControl1.Name = "documentSelectionControl1";
			this.documentSelectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.documentSelectionControl1.TabIndex = 0;
			//
			//	ProcessTemplateCustomFieldsControl
			//
			this.processTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.processTemplateCustomFieldsControl.Name = "processTemplateCustomFieldsControl";
			this.processTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 500);
			this.processTemplateCustomFieldsControl.TabIndex = 0;
			this.processTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("6fa3809b-8324-4182-8feb-ffbc9c3eabac", "To make use of this tab, please setup quotation custom fields in Workflow Manager.");
			// 
			// CustomFieldTabPage
			// 
			this.CustomFieldTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|db5613e6-fa44-4ca3-9e9c-3f4fb29653fb", "Custom Fields");
			this.CustomFieldTabPage.Controls.Add(this.summaryEntryPanel1);
			this.CustomFieldTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.CustomFieldTabPage.UseVisualStyleBackColor = true;
			this.CustomFieldTabPage.TabIndex = 7;
			this.CustomFieldTabPage.Controls.Add(this.processTemplateCustomFieldsControl);
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteTabControl|8c7a2164-6142-47bb-8eeb-38dd2b5aa69a", "Summary");
			this.SummaryTabPage.Controls.Add(this.summaryEntryPanel1);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.SummaryTabPage.TabIndex = 8;
			// 
			// summaryEntryPanel1
			// 
			this.summaryEntryPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.summaryEntryPanel1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.summaryEntryPanel1.Category = "";
			this.summaryEntryPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.summaryEntryPanel1.Name = "summaryEntryPanel1";
			this.summaryEntryPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.summaryEntryPanel1.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.WorkflowTabPage.TabIndex = 12;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zStmNoteTabPage1.TabIndex = 10;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			this.zEventTabPage1.TabIndex = 9;
			// 
			// QuoteTabControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "QuoteTabControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 582, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TemplateTabPage.ResumeLayout(false);
			this.QuoteFormatTabPage.ResumeLayout(false);
			this.QuoteFormatTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuoteFormatEntryGrid)).EndInit();
			this.DocumentLayoutTabPage.ResumeLayout(false);
			this.CustomFieldTabPage.ResumeLayout(false);
			this.SummaryTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
