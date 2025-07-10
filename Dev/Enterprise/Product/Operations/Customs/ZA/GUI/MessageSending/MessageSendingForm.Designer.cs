using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class MessageSendingForm
	{
		new void InitializeComponent()
		{
            this.vOCPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.vOCValuesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.amountDueLabel = new Enterprise.ZArchitecture.ZLabel();
            this.penaltyLabel = new Enterprise.ZArchitecture.ZLabel();
            this.provisionalPaymentLabel = new Enterprise.ZArchitecture.ZLabel();
            this.valuAddedTaxLabel = new Enterprise.ZArchitecture.ZLabel();
            this.sch1Pt2BDutyLabel = new Enterprise.ZArchitecture.ZLabel();
            this.customsDutyLabel = new Enterprise.ZArchitecture.ZLabel();
            this.customsValueLabel = new Enterprise.ZArchitecture.ZLabel();
            this.cIFValueLabel = new Enterprise.ZArchitecture.ZLabel();
            this.vOCDifferenceLabel = new Enterprise.ZArchitecture.ZLabel();
            this.vOCBeforeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.vOCAfterLabel = new Enterprise.ZArchitecture.ZLabel();
            this.amountDueDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.amountDueBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.amountDueAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.penaltyDefferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.penaltyBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.penaltyAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.provisionalPaymentDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.provisionalPaymentBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.provisionalPaymentAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.valueAddedTaxDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.valueAddedTaxBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.valueAddedTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.s1P2BDutyDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.s1P2BDutyBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.s1P2BDutyAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsDutyNoS1P2BDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsDutyNoS1P2BBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsDutyNoS1P2BAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsValueDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsValueBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsValueAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.cIFValueDifferenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.cIFValueBeforeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.cIFValueAfterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.vOCReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.vOCReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.messageSendingObjectsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
            this.MessageSendingObjectsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.vOCPanel.SuspendLayout();
            this.vOCValuesGroupBox.SuspendLayout();
            this.vOCReasonGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendButton
            // 
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(976, 329, true);
            this.SendButton.TabIndex = 3;
            // 
            // CancelButton2
            // 
            this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1072, 329, true);
            this.CancelButton2.TabIndex = 4;
            // 
            // messageSendingObjectsGroupBox
            // 
            this.messageSendingObjectsGroupBox.Controls.Add(this.vOCPanel);
            this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1152, 314, true);
            this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.vOCPanel, 0);
            this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.MessageSendingObjectsGrid, 0);
            // 
            // MessageSendingObjectsGrid
            // 
            this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 156, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 360, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 23, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent);
            // 
            // vOCPanel
            // 
            this.vOCPanel.Controls.Add(this.vOCValuesGroupBox);
            this.vOCPanel.Controls.Add(this.vOCReasonGroupBox);
            this.vOCPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.vOCPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 172, true);
            this.vOCPanel.Name = "vOCPanel";
            this.vOCPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 139, true);
            this.vOCPanel.TabIndex = 2;
            // 
            // vOCValuesGroupBox
            // 
            this.vOCValuesGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b4ff6b76-8949-4e6d-92f2-38fd0a1ba8ae", "VOC Summary");
            this.vOCValuesGroupBox.Controls.Add(this.amountDueLabel);
            this.vOCValuesGroupBox.Controls.Add(this.penaltyLabel);
            this.vOCValuesGroupBox.Controls.Add(this.provisionalPaymentLabel);
            this.vOCValuesGroupBox.Controls.Add(this.valuAddedTaxLabel);
            this.vOCValuesGroupBox.Controls.Add(this.sch1Pt2BDutyLabel);
            this.vOCValuesGroupBox.Controls.Add(this.customsDutyLabel);
            this.vOCValuesGroupBox.Controls.Add(this.customsValueLabel);
            this.vOCValuesGroupBox.Controls.Add(this.cIFValueLabel);
            this.vOCValuesGroupBox.Controls.Add(this.vOCDifferenceLabel);
            this.vOCValuesGroupBox.Controls.Add(this.vOCBeforeLabel);
            this.vOCValuesGroupBox.Controls.Add(this.vOCAfterLabel);
            this.vOCValuesGroupBox.Controls.Add(this.amountDueDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.amountDueBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.amountDueAfterCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.penaltyDefferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.penaltyBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.penaltyAfterCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.provisionalPaymentDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.provisionalPaymentBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.provisionalPaymentAfterCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.valueAddedTaxDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.valueAddedTaxBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.valueAddedTaxCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.s1P2BDutyDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.s1P2BDutyBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.s1P2BDutyAfterCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.customsDutyNoS1P2BDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.customsDutyNoS1P2BBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.customsDutyNoS1P2BAfterCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.customsValueDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.customsValueBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.customsValueAfterCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.cIFValueDifferenceCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.cIFValueBeforeCalcEdit);
            this.vOCValuesGroupBox.Controls.Add(this.cIFValueAfterCalcEdit);
            this.vOCValuesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vOCValuesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 0, true);
            this.vOCValuesGroupBox.Name = "vOCValuesGroupBox";
            this.vOCValuesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 139, true);
            this.vOCValuesGroupBox.TabIndex = 1;
            this.vOCValuesGroupBox.TabStop = false;
            // 
            // amountDueLabel
            // 
            this.amountDueLabel.AutoSize = true;
            this.amountDueLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e3679d7f-e87f-4257-b920-847df7522cf2", "Amount Due");
            this.amountDueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.amountDueLabel.IsFontBold = true;
            this.amountDueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(830, 18, true);
            this.amountDueLabel.Name = "amountDueLabel";
            this.amountDueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.amountDueLabel.TabIndex = 31;
            this.amountDueLabel.UseMnemonic = false;
            // 
            // penaltyLabel
            // 
            this.penaltyLabel.AutoSize = true;
            this.penaltyLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("78214a7e-effa-41c9-af01-88a7c735efe6", "Penalty");
            this.penaltyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.penaltyLabel.IsFontBold = true;
            this.penaltyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 18, true);
            this.penaltyLabel.Name = "penaltyLabel";
            this.penaltyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.penaltyLabel.TabIndex = 27;
            this.penaltyLabel.UseMnemonic = false;
            // 
            // provisionalPaymentLabel
            // 
            this.provisionalPaymentLabel.AutoSize = true;
            this.provisionalPaymentLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("355b56e0-975e-4835-9200-9ee7ffc35df0", "Provisional Payment");
            this.provisionalPaymentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.provisionalPaymentLabel.IsFontBold = true;
            this.provisionalPaymentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 18, true);
            this.provisionalPaymentLabel.Name = "provisionalPaymentLabel";
            this.provisionalPaymentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.provisionalPaymentLabel.TabIndex = 23;
            this.provisionalPaymentLabel.UseMnemonic = false;
            // 
            // valuAddedTaxLabel
            // 
            this.valuAddedTaxLabel.AutoSize = true;
            this.valuAddedTaxLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("4846220f-698d-4e3b-a999-774e9603173d", "Value Added Tax");
            this.valuAddedTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.valuAddedTaxLabel.IsFontBold = true;
            this.valuAddedTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 18, true);
            this.valuAddedTaxLabel.Name = "valuAddedTaxLabel";
            this.valuAddedTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.valuAddedTaxLabel.TabIndex = 19;
            this.valuAddedTaxLabel.UseMnemonic = false;
            // 
            // sch1Pt2BDutyLabel
            // 
            this.sch1Pt2BDutyLabel.AutoSize = true;
            this.sch1Pt2BDutyLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f96e4421-a51c-41cb-a740-a9e075cca502", "Sch 1 Pt 2 B");
            this.sch1Pt2BDutyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.sch1Pt2BDutyLabel.IsFontBold = true;
            this.sch1Pt2BDutyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 18, true);
            this.sch1Pt2BDutyLabel.Name = "sch1Pt2BDutyLabel";
            this.sch1Pt2BDutyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.sch1Pt2BDutyLabel.TabIndex = 15;
            this.sch1Pt2BDutyLabel.UseMnemonic = false;
            // 
            // customsDutyLabel
            // 
            this.customsDutyLabel.AutoSize = true;
            this.customsDutyLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("87a1e31e-2698-4f59-bd97-4180d8061f1c", "Customs Duty");
            this.customsDutyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.customsDutyLabel.IsFontBold = true;
            this.customsDutyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 18, true);
            this.customsDutyLabel.Name = "customsDutyLabel";
            this.customsDutyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.customsDutyLabel.TabIndex = 11;
            this.customsDutyLabel.UseMnemonic = false;
            // 
            // customsValueLabel
            // 
            this.customsValueLabel.AutoSize = true;
            this.customsValueLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e5952ebc-501b-4587-a473-2cfc7372e8ef", "Customs Value");
            this.customsValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.customsValueLabel.IsFontBold = true;
            this.customsValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 18, true);
            this.customsValueLabel.Name = "customsValueLabel";
            this.customsValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.customsValueLabel.TabIndex = 7;
            this.customsValueLabel.UseMnemonic = false;
            // 
            // cIFValueLabel
            // 
            this.cIFValueLabel.AutoSize = true;
            this.cIFValueLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f66d802d-ee8b-42c8-8c23-46e735630005", "CIF");
            this.cIFValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.cIFValueLabel.IsFontBold = true;
            this.cIFValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 18, true);
            this.cIFValueLabel.Name = "cIFValueLabel";
            this.cIFValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.cIFValueLabel.TabIndex = 3;
            this.cIFValueLabel.UseMnemonic = false;
            // 
            // vOCDifferenceLabel
            // 
            this.vOCDifferenceLabel.AutoSize = true;
            this.vOCDifferenceLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("fa3f6996-ba3f-4d99-9f68-533f8971ca48", "Difference:");
            this.vOCDifferenceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.vOCDifferenceLabel.IsFontBold = true;
            this.vOCDifferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 93, true);
            this.vOCDifferenceLabel.Name = "vOCDifferenceLabel";
            this.vOCDifferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.vOCDifferenceLabel.TabIndex = 2;
            this.vOCDifferenceLabel.UseMnemonic = false;
            // 
            // vOCBeforeLabel
            // 
            this.vOCBeforeLabel.AutoSize = true;
            this.vOCBeforeLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("c62028be-cfb2-4305-9acc-2d749707e82c", "Before:");
            this.vOCBeforeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.vOCBeforeLabel.IsFontBold = true;
            this.vOCBeforeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 67, true);
            this.vOCBeforeLabel.Name = "vOCBeforeLabel";
            this.vOCBeforeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.vOCBeforeLabel.TabIndex = 1;
            this.vOCBeforeLabel.UseMnemonic = false;
            // 
            // vOCAfterLabel
            // 
            this.vOCAfterLabel.AutoSize = true;
            this.vOCAfterLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ad1c0a05-4d4b-4fa6-af15-d0222c03ea1b", "After:");
            this.vOCAfterLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.vOCAfterLabel.IsFontBold = true;
            this.vOCAfterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 41, true);
            this.vOCAfterLabel.Name = "vOCAfterLabel";
            this.vOCAfterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.vOCAfterLabel.TabIndex = 0;
            this.vOCAfterLabel.UseMnemonic = false;
            // 
            // amountDueDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.amountDueDifferenceCalcEdit, "SendingObjectsCollection.AmountDueDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmountDueDifference)));
            this.amountDueDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.amountDueDifferenceCalcEdit, false);
            this.amountDueDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(830, 91, true);
            this.amountDueDifferenceCalcEdit.Name = "amountDueDifferenceCalcEdit";
            this.amountDueDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.amountDueDifferenceCalcEdit.TabIndex = 34;
            this.amountDueDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amountDueDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // amountDueBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.amountDueBeforeCalcEdit, "SendingObjectsCollection.AmountDueBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmountDueBefore)));
            this.amountDueBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.amountDueBeforeCalcEdit, false);
            this.amountDueBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(830, 65, true);
            this.amountDueBeforeCalcEdit.Name = "amountDueBeforeCalcEdit";
            this.amountDueBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.amountDueBeforeCalcEdit.TabIndex = 33;
            this.amountDueBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amountDueBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // amountDueAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.amountDueAfterCalcEdit, "SendingObjectsCollection.AmountDueAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmountDueAfter)));
            this.amountDueAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.amountDueAfterCalcEdit, false);
            this.amountDueAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(830, 38, true);
            this.amountDueAfterCalcEdit.Name = "amountDueAfterCalcEdit";
            this.amountDueAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.amountDueAfterCalcEdit.TabIndex = 32;
            this.amountDueAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amountDueAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // penaltyDefferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.penaltyDefferenceCalcEdit, "SendingObjectsCollection.PenaltyAmountDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyAmountDifference)));
            this.penaltyDefferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.penaltyDefferenceCalcEdit, false);
            this.penaltyDefferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 90, true);
            this.penaltyDefferenceCalcEdit.Name = "penaltyDefferenceCalcEdit";
            this.penaltyDefferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.penaltyDefferenceCalcEdit.TabIndex = 30;
            this.penaltyDefferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.penaltyDefferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // penaltyBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.penaltyBeforeCalcEdit, "SendingObjectsCollection.PenaltyAmountBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyAmountBefore)));
            this.penaltyBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.penaltyBeforeCalcEdit, false);
            this.penaltyBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 64, true);
            this.penaltyBeforeCalcEdit.Name = "penaltyBeforeCalcEdit";
            this.penaltyBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.penaltyBeforeCalcEdit.TabIndex = 29;
            this.penaltyBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.penaltyBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // penaltyAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.penaltyAfterCalcEdit, "SendingObjectsCollection.PenaltyAmountAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PenaltyAmountAfter)));
            this.penaltyAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.penaltyAfterCalcEdit, false);
            this.penaltyAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 38, true);
            this.penaltyAfterCalcEdit.Name = "penaltyAfterCalcEdit";
            this.penaltyAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.penaltyAfterCalcEdit.TabIndex = 28;
            this.penaltyAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.penaltyAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // provisionalPaymentDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.provisionalPaymentDifferenceCalcEdit, "SendingObjectsCollection.ProvisionalPaymentAmountDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ProvisionalPaymentAmountDifference)));
            this.provisionalPaymentDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.provisionalPaymentDifferenceCalcEdit, false);
            this.provisionalPaymentDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 90, true);
            this.provisionalPaymentDifferenceCalcEdit.Name = "provisionalPaymentDifferenceCalcEdit";
            this.provisionalPaymentDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
            this.provisionalPaymentDifferenceCalcEdit.TabIndex = 26;
            this.provisionalPaymentDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.provisionalPaymentDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // provisionalPaymentBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.provisionalPaymentBeforeCalcEdit, "SendingObjectsCollection.ProvisionalPaymentAmountBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ProvisionalPaymentAmountBefore)));
            this.provisionalPaymentBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.provisionalPaymentBeforeCalcEdit, false);
            this.provisionalPaymentBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 64, true);
            this.provisionalPaymentBeforeCalcEdit.Name = "provisionalPaymentBeforeCalcEdit";
            this.provisionalPaymentBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
            this.provisionalPaymentBeforeCalcEdit.TabIndex = 25;
            this.provisionalPaymentBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.provisionalPaymentBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // provisionalPaymentAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.provisionalPaymentAfterCalcEdit, "SendingObjectsCollection.ProvisionalPaymentAmountAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ProvisionalPaymentAmountAfter)));
            this.provisionalPaymentAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.provisionalPaymentAfterCalcEdit, false);
            this.provisionalPaymentAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 38, true);
            this.provisionalPaymentAfterCalcEdit.Name = "provisionalPaymentAfterCalcEdit";
            this.provisionalPaymentAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
            this.provisionalPaymentAfterCalcEdit.TabIndex = 24;
            this.provisionalPaymentAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.provisionalPaymentAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // valueAddedTaxDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.valueAddedTaxDifferenceCalcEdit, "SendingObjectsCollection.ValueAddedTaxDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ValueAddedTaxDifference)));
            this.valueAddedTaxDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.valueAddedTaxDifferenceCalcEdit, false);
            this.valueAddedTaxDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 90, true);
            this.valueAddedTaxDifferenceCalcEdit.Name = "valueAddedTaxDifferenceCalcEdit";
            this.valueAddedTaxDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.valueAddedTaxDifferenceCalcEdit.TabIndex = 22;
            this.valueAddedTaxDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.valueAddedTaxDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // valueAddedTaxBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.valueAddedTaxBeforeCalcEdit, "SendingObjectsCollection.ValueAddedTaxBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ValueAddedTaxBefore)));
            this.valueAddedTaxBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.valueAddedTaxBeforeCalcEdit, false);
            this.valueAddedTaxBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 64, true);
            this.valueAddedTaxBeforeCalcEdit.Name = "valueAddedTaxBeforeCalcEdit";
            this.valueAddedTaxBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.valueAddedTaxBeforeCalcEdit.TabIndex = 21;
            this.valueAddedTaxBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.valueAddedTaxBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // valueAddedTaxCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.valueAddedTaxCalcEdit, "SendingObjectsCollection.ValueAddedTaxAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ValueAddedTaxAfter)));
            this.valueAddedTaxCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.valueAddedTaxCalcEdit, false);
            this.valueAddedTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 38, true);
            this.valueAddedTaxCalcEdit.Name = "valueAddedTaxCalcEdit";
            this.valueAddedTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.valueAddedTaxCalcEdit.TabIndex = 20;
            this.valueAddedTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.valueAddedTaxCalcEdit.TrackDisposedAccess = true;
            // 
            // s1P2BDutyDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.s1P2BDutyDifferenceCalcEdit, "SendingObjectsCollection.S1P2BDutyDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).S1P2BDutyDifference)));
            this.s1P2BDutyDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.s1P2BDutyDifferenceCalcEdit, false);
            this.s1P2BDutyDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 90, true);
            this.s1P2BDutyDifferenceCalcEdit.Name = "s1P2BDutyDifferenceCalcEdit";
            this.s1P2BDutyDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.s1P2BDutyDifferenceCalcEdit.TabIndex = 18;
            this.s1P2BDutyDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.s1P2BDutyDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // s1P2BDutyBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.s1P2BDutyBeforeCalcEdit, "SendingObjectsCollection.S1P2BDutyBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).S1P2BDutyBefore)));
            this.s1P2BDutyBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.s1P2BDutyBeforeCalcEdit, false);
            this.s1P2BDutyBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 64, true);
            this.s1P2BDutyBeforeCalcEdit.Name = "s1P2BDutyBeforeCalcEdit";
            this.s1P2BDutyBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.s1P2BDutyBeforeCalcEdit.TabIndex = 17;
            this.s1P2BDutyBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.s1P2BDutyBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // s1P2BDutyAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.s1P2BDutyAfterCalcEdit, "SendingObjectsCollection.S1P2BDutyAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).S1P2BDutyAfter)));
            this.s1P2BDutyAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.s1P2BDutyAfterCalcEdit, false);
            this.s1P2BDutyAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 38, true);
            this.s1P2BDutyAfterCalcEdit.Name = "s1P2BDutyAfterCalcEdit";
            this.s1P2BDutyAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.s1P2BDutyAfterCalcEdit.TabIndex = 16;
            this.s1P2BDutyAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.s1P2BDutyAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // customsDutyNoS1P2BDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsDutyNoS1P2BDifferenceCalcEdit, "SendingObjectsCollection.CustomsDutyNoS1P2BDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsDutyNoS1P2BDifference)));
            this.customsDutyNoS1P2BDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.customsDutyNoS1P2BDifferenceCalcEdit, false);
            this.customsDutyNoS1P2BDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 90, true);
            this.customsDutyNoS1P2BDifferenceCalcEdit.Name = "customsDutyNoS1P2BDifferenceCalcEdit";
            this.customsDutyNoS1P2BDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.customsDutyNoS1P2BDifferenceCalcEdit.TabIndex = 14;
            this.customsDutyNoS1P2BDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsDutyNoS1P2BDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // customsDutyNoS1P2BBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsDutyNoS1P2BBeforeCalcEdit, "SendingObjectsCollection.CustomsDutyNoS1P2BBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsDutyNoS1P2BBefore)));
            this.customsDutyNoS1P2BBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.customsDutyNoS1P2BBeforeCalcEdit, false);
            this.customsDutyNoS1P2BBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 64, true);
            this.customsDutyNoS1P2BBeforeCalcEdit.Name = "customsDutyNoS1P2BBeforeCalcEdit";
            this.customsDutyNoS1P2BBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.customsDutyNoS1P2BBeforeCalcEdit.TabIndex = 13;
            this.customsDutyNoS1P2BBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsDutyNoS1P2BBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // customsDutyNoS1P2BAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsDutyNoS1P2BAfterCalcEdit, "SendingObjectsCollection.CustomsDutyNoS1P2BAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsDutyNoS1P2BAfter)));
            this.customsDutyNoS1P2BAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.customsDutyNoS1P2BAfterCalcEdit, false);
            this.customsDutyNoS1P2BAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 38, true);
            this.customsDutyNoS1P2BAfterCalcEdit.Name = "customsDutyNoS1P2BAfterCalcEdit";
            this.customsDutyNoS1P2BAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.customsDutyNoS1P2BAfterCalcEdit.TabIndex = 12;
            this.customsDutyNoS1P2BAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsDutyNoS1P2BAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // customsValueDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsValueDifferenceCalcEdit, "SendingObjectsCollection.CustomsValueDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsValueDifference)));
            this.customsValueDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.customsValueDifferenceCalcEdit, false);
            this.customsValueDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 90, true);
            this.customsValueDifferenceCalcEdit.Name = "customsValueDifferenceCalcEdit";
            this.customsValueDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.customsValueDifferenceCalcEdit.TabIndex = 10;
            this.customsValueDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsValueDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // customsValueBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsValueBeforeCalcEdit, "SendingObjectsCollection.CustomsValueBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsValueBefore)));
            this.customsValueBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.customsValueBeforeCalcEdit, false);
            this.customsValueBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 64, true);
            this.customsValueBeforeCalcEdit.Name = "customsValueBeforeCalcEdit";
            this.customsValueBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.customsValueBeforeCalcEdit.TabIndex = 9;
            this.customsValueBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsValueBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // customsValueAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsValueAfterCalcEdit, "SendingObjectsCollection.CustomsValueAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsValueAfter)));
            this.customsValueAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.customsValueAfterCalcEdit, false);
            this.customsValueAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 38, true);
            this.customsValueAfterCalcEdit.Name = "customsValueAfterCalcEdit";
            this.customsValueAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.customsValueAfterCalcEdit.TabIndex = 8;
            this.customsValueAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsValueAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // cIFValueDifferenceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.cIFValueDifferenceCalcEdit, "SendingObjectsCollection.CIFValueDifference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CIFValueDifference)));
            this.cIFValueDifferenceCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cIFValueDifferenceCalcEdit, false);
            this.cIFValueDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 90, true);
            this.cIFValueDifferenceCalcEdit.Name = "cIFValueDifferenceCalcEdit";
            this.cIFValueDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.cIFValueDifferenceCalcEdit.TabIndex = 6;
            this.cIFValueDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cIFValueDifferenceCalcEdit.TrackDisposedAccess = true;
            // 
            // cIFValueBeforeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.cIFValueBeforeCalcEdit, "SendingObjectsCollection.CIFValueBefore");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CIFValueBefore)));
            this.cIFValueBeforeCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cIFValueBeforeCalcEdit, false);
            this.cIFValueBeforeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 64, true);
            this.cIFValueBeforeCalcEdit.Name = "cIFValueBeforeCalcEdit";
            this.cIFValueBeforeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.cIFValueBeforeCalcEdit.TabIndex = 5;
            this.cIFValueBeforeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cIFValueBeforeCalcEdit.TrackDisposedAccess = true;
            // 
            // cIFValueAfterCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.cIFValueAfterCalcEdit, "SendingObjectsCollection.CIFValueAfter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CIFValueAfter)));
            this.cIFValueAfterCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cIFValueAfterCalcEdit, false);
            this.cIFValueAfterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
            this.cIFValueAfterCalcEdit.Name = "cIFValueAfterCalcEdit";
            this.cIFValueAfterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.cIFValueAfterCalcEdit.TabIndex = 4;
            this.cIFValueAfterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cIFValueAfterCalcEdit.TrackDisposedAccess = true;
            // 
            // vOCReasonGroupBox
            // 
            this.vOCReasonGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("17cd894a-3841-4eb2-aff3-354da5230438", "VOC Reason");
            this.vOCReasonGroupBox.Controls.Add(this.vOCReasonTextBox);
            this.vOCReasonGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.vOCReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.vOCReasonGroupBox.Name = "vOCReasonGroupBox";
            this.vOCReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 139, true);
            this.vOCReasonGroupBox.TabIndex = 0;
            this.vOCReasonGroupBox.TabStop = false;
            // 
            // vOCReasonTextBox
            // 
            this.BindingSource.SetBindingMember(this.vOCReasonTextBox, "SendingObjectsCollection.VOCReason");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).VOCReason)));
            this.vOCReasonTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vOCReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.vOCReasonTextBox.Multiline = true;
            this.vOCReasonTextBox.Name = "vOCReasonTextBox";
            this.vOCReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.vOCReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 120, true);
            this.vOCReasonTextBox.TabIndex = 0;
            // 
            // MessageSendingForm
            // 
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 383, true);
            this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1175, 350, true);
            this.Name = "MessageSendingForm";
            this.messageSendingObjectsGroupBox.ResumeLayout(false);
            this.messageSendingObjectsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
            this.MessageSendingObjectsGrid.ResumeLayout(false);
            this.MessageSendingObjectsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.vOCPanel.ResumeLayout(false);
            this.vOCPanel.PerformLayout();
            this.vOCValuesGroupBox.ResumeLayout(false);
            this.vOCValuesGroupBox.PerformLayout();
            this.vOCReasonGroupBox.ResumeLayout(false);
            this.vOCReasonGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
