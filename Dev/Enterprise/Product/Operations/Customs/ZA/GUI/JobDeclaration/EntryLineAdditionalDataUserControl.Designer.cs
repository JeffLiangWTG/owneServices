using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class EntryLineAdditionalDataUserControl
	{


		private System.ComponentModel.IContainer components;


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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>	
		#region InitializeComponent

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.ExtendedInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.feeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.penaltyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.customsDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.s1P2BCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.vATCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.provisionalPaymentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.quantityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.customsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.additionalQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.classificationQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.warehouseCountablyQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
            this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
            this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
            this.tradeAgreementLabel = new Enterprise.ZArchitecture.ZLabel();
            this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.tariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ExtendInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.zTabPageAdditionalInformation = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zGridAdditionalInformation = new Enterprise.ZArchitecture.ZGrid();
            this.ExtendedInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.taxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.dutyAndFeeSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.entryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.entryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
            this.provisionalPaymentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.provisionalPaymentsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ExtendedInfoGroupBox.SuspendLayout();
            this.feeGroupBox.SuspendLayout();
            this.quantityGroupBox.SuspendLayout();
            this.customsQuantityCalcDropEdit.SuspendLayout();
            this.additionalQuantityCalcDropEdit.SuspendLayout();
            this.classificationQuantityCalcDropEdit.SuspendLayout();
            this.warehouseCountablyQuantityCalcDropEdit.SuspendLayout();
            this.zCodeFindBox1.SuspendLayout();
            this.ExtendInfoTabControl.SuspendLayout();
            this.zTabPageAdditionalInformation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGridAdditionalInformation)).BeginInit();
            this.zGridAdditionalInformation.SuspendLayout();
            this.ExtendedInfoTabPage.SuspendLayout();
            this.taxOrFeeTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dutyAndFeeSplitContainer)).BeginInit();
            this.dutyAndFeeSplitContainer.Panel1.SuspendLayout();
            this.dutyAndFeeSplitContainer.Panel2.SuspendLayout();
            this.dutyAndFeeSplitContainer.SuspendLayout();
            this.entryLineDutyAndTaxGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.entryLineDutyAndTaxGrid)).BeginInit();
            this.entryLineDutyAndTaxGrid.SuspendLayout();
            this.provisionalPaymentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.provisionalPaymentsGrid)).BeginInit();
            this.provisionalPaymentsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobDeclaration);
            // 
            // ExtendedInfoGroupBox
            // 
            this.ExtendedInfoGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f2b69e31-7d20-4b51-88fe-18d6e5f4a5d1", "Extended Information");
            this.ExtendedInfoGroupBox.Controls.Add(this.feeGroupBox);
            this.ExtendedInfoGroupBox.Controls.Add(this.quantityGroupBox);
            this.ExtendedInfoGroupBox.Controls.Add(this.zTextBox4);
            this.ExtendedInfoGroupBox.Controls.Add(this.zTextBox3);
            this.ExtendedInfoGroupBox.Controls.Add(this.zCalcEdit1);
            this.ExtendedInfoGroupBox.Controls.Add(this.zTextBox2);
            this.ExtendedInfoGroupBox.Controls.Add(this.tradeAgreementLabel);
            this.ExtendedInfoGroupBox.Controls.Add(this.zCodeFindBox1);
            this.ExtendedInfoGroupBox.Controls.Add(this.tariffCodeTextBox);
            this.ExtendedInfoGroupBox.Controls.Add(this.descriptionTextBox);
            this.ExtendedInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ExtendedInfoGroupBox.Name = "ExtendedInfoGroupBox";
            this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
            this.ExtendedInfoGroupBox.TabIndex = 2;
            this.ExtendedInfoGroupBox.TabStop = false;
            // 
            // feeGroupBox
            // 
            this.feeGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("958dd5ef-660c-4a1d-9c49-f9175cce1dea", "Calculated Fees");
            this.feeGroupBox.Controls.Add(this.penaltyCalcEdit);
            this.feeGroupBox.Controls.Add(this.customsDutyCalcEdit);
            this.feeGroupBox.Controls.Add(this.s1P2BCalcEdit);
            this.feeGroupBox.Controls.Add(this.vATCalcEdit);
            this.feeGroupBox.Controls.Add(this.provisionalPaymentCalcEdit);
            this.feeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 124, true);
            this.feeGroupBox.Name = "feeGroupBox";
            this.feeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 54, true);
            this.feeGroupBox.TabIndex = 8;
            this.feeGroupBox.TabStop = false;
            // 
            // penaltyCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.penaltyCalcEdit, "CustomsEntryHeaders.AllEntryLines.PenaltyAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).PenaltyAmount)));
            this.penaltyCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("c28d77a8-15d4-45a8-a7b4-a755ae96d852", "Penalty");
            this.penaltyCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.penaltyCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.penaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 31, true);
            this.penaltyCalcEdit.Name = "penaltyCalcEdit";
            this.penaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
            this.penaltyCalcEdit.TabIndex = 4;
            this.penaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.penaltyCalcEdit.TrackDisposedAccess = true;
            // 
            // customsDutyCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.customsDutyCalcEdit, "CustomsEntryHeaders.AllEntryLines.CustomsDutyExcluding12B");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CustomsDutyExcluding12B)));
            this.customsDutyCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("03118d68-6446-48d1-9114-e8f09f418e3e", "Customs Duty");
            this.customsDutyCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.customsDutyCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.customsDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 31, true);
            this.customsDutyCalcEdit.Name = "customsDutyCalcEdit";
            this.customsDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
            this.customsDutyCalcEdit.TabIndex = 0;
            this.customsDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.customsDutyCalcEdit.TrackDisposedAccess = true;
            // 
            // s1P2BCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.s1P2BCalcEdit, "CustomsEntryHeaders.AllEntryLines.DutySch1P2B");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).DutySch1P2B)));
            this.s1P2BCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("c96c9bb5-8d0f-4304-ad1e-9a1d1becd709", "Sch 1 P 2B");
            this.s1P2BCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.s1P2BCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.s1P2BCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 31, true);
            this.s1P2BCalcEdit.Name = "s1P2BCalcEdit";
            this.s1P2BCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
            this.s1P2BCalcEdit.TabIndex = 1;
            this.s1P2BCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.s1P2BCalcEdit.TrackDisposedAccess = true;
            // 
            // vATCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.vATCalcEdit, "CustomsEntryHeaders.AllEntryLines.VAT");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).VAT)));
            this.vATCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("10cdb5ac-136f-450c-a9c2-b46bc8f87dd0", "VAT");
            this.vATCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.vATCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.vATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 31, true);
            this.vATCalcEdit.Name = "vATCalcEdit";
            this.vATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
            this.vATCalcEdit.TabIndex = 2;
            this.vATCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.vATCalcEdit.TrackDisposedAccess = true;
            // 
            // provisionalPaymentCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.provisionalPaymentCalcEdit, "CustomsEntryHeaders.AllEntryLines.ProvisionalPaymentAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ProvisionalPaymentAmount)));
            this.provisionalPaymentCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("77279b9d-3924-4058-9c4d-1beb01014d60", "Provisional Payment");
            this.provisionalPaymentCalcEdit.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.provisionalPaymentCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.provisionalPaymentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 31, true);
            this.provisionalPaymentCalcEdit.Name = "provisionalPaymentCalcEdit";
            this.provisionalPaymentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
            this.provisionalPaymentCalcEdit.TabIndex = 3;
            this.provisionalPaymentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.provisionalPaymentCalcEdit.TrackDisposedAccess = true;
            // 
            // quantityGroupBox
            // 
            this.quantityGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3f1f8d3c-c392-4bec-9a9d-2b38e79f52b5", "Quantities");
            this.quantityGroupBox.Controls.Add(this.customsQuantityCalcDropEdit);
            this.quantityGroupBox.Controls.Add(this.additionalQuantityCalcDropEdit);
            this.quantityGroupBox.Controls.Add(this.classificationQuantityCalcDropEdit);
            this.quantityGroupBox.Controls.Add(this.warehouseCountablyQuantityCalcDropEdit);
            this.quantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 17, true);
            this.quantityGroupBox.Name = "quantityGroupBox";
            this.quantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 103, true);
            this.quantityGroupBox.TabIndex = 7;
            this.quantityGroupBox.TabStop = false;
            // 
            // customsQuantityCalcDropEdit
            // 
            this.customsQuantityCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.customsQuantityCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcCustomsQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcCustomsUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Lookups.QuantityUnitCodeList)));
            this.customsQuantityCalcDropEdit.BindToAmount = "CustomsEntryHeaders.AllEntryLines.CalcCustomsQuantity";
            this.customsQuantityCalcDropEdit.BindToList = "CustomsEntryHeaders.AllEntryLines.Lookups.QuantityUnitCodeList";
            this.customsQuantityCalcDropEdit.BindToUnit = "CustomsEntryHeaders.AllEntryLines.CalcCustomsUnitQty";
            this.customsQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f3c6cbdd-a286-4db4-9048-33bd8d226b5f", "Customs Qty.");
            this.customsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 14, true);
            this.customsQuantityCalcDropEdit.Name = "customsQuantityCalcDropEdit";
            this.customsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
            this.customsQuantityCalcDropEdit.TabIndex = 0;
            this.customsQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
            // 
            // additionalQuantityCalcDropEdit
            // 
            this.additionalQuantityCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.additionalQuantityCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcAdditionalQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcAdditionalUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Lookups.QuantityUnitCodeList)));
            this.additionalQuantityCalcDropEdit.BindToAmount = "CustomsEntryHeaders.AllEntryLines.CalcAdditionalQuantity";
            this.additionalQuantityCalcDropEdit.BindToList = "CustomsEntryHeaders.AllEntryLines.Lookups.QuantityUnitCodeList";
            this.additionalQuantityCalcDropEdit.BindToUnit = "CustomsEntryHeaders.AllEntryLines.CalcAdditionalUnitQty";
            this.additionalQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("B166A30A-F0B0-4FD1-B291-6F9D0AB5D226", "Additional Qty. 1");
            this.additionalQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 35, true);
            this.additionalQuantityCalcDropEdit.Name = "additionalQuantityCalcDropEdit";
            this.additionalQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
            this.additionalQuantityCalcDropEdit.TabIndex = 1;
            this.additionalQuantityCalcDropEdit.UnitPreBoundMaxLength = 4;
            // 
            // classificationQuantityCalcDropEdit
            // 
            this.classificationQuantityCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.classificationQuantityCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcClassificationQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcClassificationUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Lookups.QuantityUnitCodeList)));
            this.classificationQuantityCalcDropEdit.BindToAmount = "CustomsEntryHeaders.AllEntryLines.CalcClassificationQuantity";
            this.classificationQuantityCalcDropEdit.BindToList = "CustomsEntryHeaders.AllEntryLines.Lookups.QuantityUnitCodeList";
            this.classificationQuantityCalcDropEdit.BindToUnit = "CustomsEntryHeaders.AllEntryLines.CalcClassificationUnitQty";
            this.classificationQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("945FFC1E-4989-4BD2-A26B-5F8B03CD5A42", "Additional Qty. 2");
            this.classificationQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 57, true);
            this.classificationQuantityCalcDropEdit.Name = "classificationQuantityCalcDropEdit";
            this.classificationQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
            this.classificationQuantityCalcDropEdit.TabIndex = 2;
            this.classificationQuantityCalcDropEdit.UnitPreBoundMaxLength = 4;
            // 
            // warehouseCountablyQuantityCalcDropEdit
            // 
            this.warehouseCountablyQuantityCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.warehouseCountablyQuantityCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcWarehouseCountableQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcWarehouseCountableUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Lookups.CountableUnitCodeList)));
            this.warehouseCountablyQuantityCalcDropEdit.BindToAmount = "CustomsEntryHeaders.AllEntryLines.CalcWarehouseCountableQuantity";
            this.warehouseCountablyQuantityCalcDropEdit.BindToList = "CustomsEntryHeaders.AllEntryLines.Lookups.CountableUnitCodeList";
            this.warehouseCountablyQuantityCalcDropEdit.BindToUnit = "CustomsEntryHeaders.AllEntryLines.CalcWarehouseCountableUnitQty";
            this.warehouseCountablyQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("d2a8ff1d-7712-4464-8961-1621a70ee0e2", "Countable Qty.");
            this.warehouseCountablyQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 78, true);
            this.warehouseCountablyQuantityCalcDropEdit.Name = "warehouseCountablyQuantityCalcDropEdit";
            this.warehouseCountablyQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
            this.warehouseCountablyQuantityCalcDropEdit.TabIndex = 3;
            this.warehouseCountablyQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
            // 
            // zTextBox4
            // 
            this.BindingSource.SetBindingMember(this.zTextBox4, "CustomsEntryHeaders.AllEntryLines.CalcPreviousMRN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcPreviousMRN)));
            this.zTextBox4.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("22eacc68-e697-42b8-97fd-80768f21fed7", "Previous MRN");
            this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 38, true);
            this.zTextBox4.Name = "zTextBox4";
            this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zTextBox4.TabIndex = 3;
            // 
            // zTextBox3
            // 
            this.BindingSource.SetBindingMember(this.zTextBox3, "CustomsEntryHeaders.AllEntryLines.CalcROOCert");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcROOCert)));
            this.zTextBox3.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("45c87eb9-d1a8-4e37-89b9-81aca27e613e", "ROO Certificate");
            this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 103, true);
            this.zTextBox3.Name = "zTextBox3";
            this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zTextBox3.TabIndex = 6;
            // 
            // zCalcEdit1
            // 
            this.BindingSource.SetBindingMember(this.zCalcEdit1, "CustomsEntryHeaders.AllEntryLines.CalcPreviousLineNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcPreviousLineNumber)));
            this.zCalcEdit1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("9943580d-46ad-41ee-a1b7-0f787d0177bf", "Previous Line No.");
            this.zCalcEdit1.DecimalPlaces = 0;
            this.zCalcEdit1.Decimals = 0;
            this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 60, true);
            this.zCalcEdit1.Name = "zCalcEdit1";
            this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
            this.zCalcEdit1.TabIndex = 4;
            this.zCalcEdit1.Text = "0";
            this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEdit1.TrackDisposedAccess = true;
            // 
            // zTextBox2
            // 
            this.BindingSource.SetBindingMember(this.zTextBox2, "CustomsEntryHeaders.AllEntryLines.CalcPreference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcPreference)));
            this.zTextBox2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("A2AB8DBA-6200-4756-AE09-B0011B8CE421", "Preference");
            this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 81, true);
            this.zTextBox2.Name = "zTextBox2";
            this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
            this.zTextBox2.TabIndex = 5;
            // 
            // tradeAgreementLabel
            // 
            this.BindingSource.SetBindingMember(this.tradeAgreementLabel, "CustomsEntryHeaders.AllEntryLines.CalcTradeAgreementLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcTradeAgreementLabel)));
            this.tradeAgreementLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.tradeAgreementLabel, false);
            this.tradeAgreementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 81, true);
            this.tradeAgreementLabel.Name = "tradeAgreementLabel";
            this.tradeAgreementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 17, true);
            this.tradeAgreementLabel.TabIndex = 9;
            this.tradeAgreementLabel.UseMnemonic = false;
            // 
            // zCodeFindBox1
            // 
            this.zCodeFindBox1.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBox1, "CustomsEntryHeaders.AllEntryLines.CalcGoodsOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CalcGoodsOrigin)));
            this.zCodeFindBox1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b9c10198-ac88-42c3-8163-229c71b335c8", "Country/Region Of Origin");
            this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 17, true);
            this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            this.zCodeFindBox1.Name = "zCodeFindBox1";
            this.zCodeFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBox1.ParentType = null;
            this.zCodeFindBox1.PreBoundMaxLength = 2;
            this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zCodeFindBox1.TabIndex = 2;
            // 
            // tariffCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.tariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
            this.tariffCodeTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("58c6c301-5885-4cec-89f6-56fea69e4058", "Tariff Code");
            this.tariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
            this.tariffCodeTextBox.Name = "tariffCodeTextBox";
            this.tariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
            this.tariffCodeTextBox.TabIndex = 0;
            // 
            // descriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.descriptionTextBox, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
            this.descriptionTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("865c128f-1628-4265-8351-aa868b3b4ab2", "Description");
            this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 41, true);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 137, true);
            this.descriptionTextBox.TabIndex = 1;
            // 
            // ExtendInfoTabControl
            // 
            this.ExtendInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.ExtendInfoTabControl.Controls.Add(this.zTabPageAdditionalInformation);
            this.ExtendInfoTabControl.Controls.Add(this.ExtendedInfoTabPage);
            this.ExtendInfoTabControl.Controls.Add(this.taxOrFeeTabPage);
            this.ExtendInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtendInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ExtendInfoTabControl.Name = "ExtendInfoTabControl";
            this.ExtendInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
            this.ExtendInfoTabControl.TabIndex = 15;
            // 
            // zTabPageAdditionalInformation
            // 
            this.zTabPageAdditionalInformation.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3935db4f-85fd-4b33-bcee-2d926d269268", "Additional Information");
            this.zTabPageAdditionalInformation.Controls.Add(this.zGridAdditionalInformation);
            this.zTabPageAdditionalInformation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPageAdditionalInformation.Name = "zTabPageAdditionalInformation";
            this.zTabPageAdditionalInformation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
            this.zTabPageAdditionalInformation.TabIndex = 0;
            this.zTabPageAdditionalInformation.Text = Res.GetString("920E2EFB-FAB6-4E13-995E-DE83086A7B11", "Additional Information");
            // 
            // zGridAdditionalInformation
            // 
            this.zGridAdditionalInformation.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.zGridAdditionalInformation, "CustomsEntryHeaders.AllEntryLines.AdditionalInformationCodes");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)).SyncRoot)).CY_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)).SyncRoot)).DecimalPlaces)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)).SyncRoot)).CY_FormattedData)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)).SyncRoot)).CY_FormattedData_FieldType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)).SyncRoot)).Grouping)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AdditionalInformationCodes)).SyncRoot)).Description)));
            this.zGridAdditionalInformation.CaptionVisible = false;
            zDropEditColumnStyleInfo1.Caption = "";
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7528ec91-d808-43cd-94e6-4b40d3cb6729", "Code");
            zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zMultiControlColumnStyleInfo1.BindToDecimalPlaces = "DecimalPlaces";
            zMultiControlColumnStyleInfo1.Caption = "";
            zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("0c7af0ce-4d19-4074-bd2e-17d88491bac0", "Data");
            zMultiControlColumnStyleInfo1.ColumnName = "CY_FormattedData";
            zMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
            zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CY_FormattedData_FieldType";
            zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2b1c2fef-64cb-4021-8025-eeea756770f1", "Grouping");
            zTextBoxColumnStyleInfo1.ColumnName = "Grouping";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.Caption = "";
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ddb9e710-a119-4f8d-a2ea-3b31b595bad8", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "Description";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            this.zGridAdditionalInformation.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.zGridAdditionalInformation.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
            this.zGridAdditionalInformation.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGridAdditionalInformation.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.zGridAdditionalInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGridAdditionalInformation.GridId = "f2427af8-78a7-4163-9f1a-1b061aac0fec";
            this.zGridAdditionalInformation.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGridAdditionalInformation.LayoutKey = "zGridAdditionalInformation";
            this.zGridAdditionalInformation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGridAdditionalInformation.Name = "zGridAdditionalInformation";
            this.zGridAdditionalInformation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
            this.zGridAdditionalInformation.TabIndex = 0;
            // 
            // ExtendedInfoTabPage
            // 
            this.ExtendedInfoTabPage.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ac26851f-5e80-4712-96d9-f9e63f57d2b4", "Extended Information");
            this.ExtendedInfoTabPage.Controls.Add(this.ExtendedInfoGroupBox);
            this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ExtendedInfoTabPage.Name = "ExtendedInfoTabPage";
            this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
            this.ExtendedInfoTabPage.TabIndex = 1;
            this.ExtendedInfoTabPage.Text = Res.GetString("D83A95A2-7303-4801-AAA8-53765E6A2C2A", "Extended Information");
            // 
            // taxOrFeeTabPage
            // 
            this.taxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3bfaf450-05ff-4206-9ad2-23dffe376209", "Tax Or Fee");
            this.taxOrFeeTabPage.Controls.Add(this.dutyAndFeeSplitContainer);
            this.taxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.taxOrFeeTabPage.Name = "taxOrFeeTabPage";
            this.taxOrFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.taxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
            this.taxOrFeeTabPage.TabIndex = 2;
            this.taxOrFeeTabPage.UseVisualStyleBackColor = true;
            // 
            // dutyAndFeeSplitContainer
            // 
            this.dutyAndFeeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dutyAndFeeSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.dutyAndFeeSplitContainer.Name = "dutyAndFeeSplitContainer";
            // 
            // dutyAndFeeSplitContainer.Panel1
            // 
            this.dutyAndFeeSplitContainer.Panel1.Controls.Add(this.entryLineDutyAndTaxGroupBox);
            this.dutyAndFeeSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 178, true);
            this.dutyAndFeeSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
            // 
            // dutyAndFeeSplitContainer.Panel2
            // 
            this.dutyAndFeeSplitContainer.Panel2.Controls.Add(this.provisionalPaymentGroupBox);
            this.dutyAndFeeSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
            this.dutyAndFeeSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(413);
            this.dutyAndFeeSplitContainer.TabIndex = 1;
            // 
            // entryLineDutyAndTaxGroupBox
            // 
            this.entryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("26288b9d-9e85-487d-9c5a-704fdac646e4", "Duty And Tax");
            this.entryLineDutyAndTaxGroupBox.Controls.Add(this.entryLineDutyAndTaxGrid);
            this.entryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.entryLineDutyAndTaxGroupBox, true);
            this.entryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.entryLineDutyAndTaxGroupBox.Name = "entryLineDutyAndTaxGroupBox";
            this.entryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 178, true);
            this.entryLineDutyAndTaxGroupBox.TabIndex = 4;
            this.entryLineDutyAndTaxGroupBox.TabStop = false;
            // 
            // entryLineDutyAndTaxGrid
            // 
            this.entryLineDutyAndTaxGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.entryLineDutyAndTaxGrid, "CustomsEntryHeaders.AllEntryLines.Fees");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_IsLandedCostOnly)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_RateOverrideReasonCode)));
            this.entryLineDutyAndTaxGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("781f7500-8425-41dd-a3a0-17ae8aa2c20d", "Type");
            zTextBoxColumnStyleInfo3.ColumnName = "CF_ChargeType";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("542af801-5954-4951-9651-cdbd03e6e7d3", "Amount");
            zCalcEditColumnStyleInfo1.ColumnName = "CF_ChargeAmount";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a03717e6-ccee-42fc-8620-3e3eea1e5021", "Is Landed Cost Only");
            zCheckBoxColumnStyleInfo1.ColumnName = "CF_IsLandedCostOnly";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("0034FA67-23B7-4525-BC11-DEB7C1F94983", "Action");
            zDropEditColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            this.entryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.entryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.entryLineDutyAndTaxGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.entryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.entryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entryLineDutyAndTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
            this.entryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.entryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
            this.entryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.entryLineDutyAndTaxGrid.Name = "entryLineDutyAndTaxGrid";
            this.entryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 159, true);
            this.entryLineDutyAndTaxGrid.TabIndex = 0;
            // 
            // provisionalPaymentGroupBox
            // 
            this.provisionalPaymentGroupBox.Controls.Add(this.provisionalPaymentsGrid);
            this.provisionalPaymentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.provisionalPaymentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.provisionalPaymentGroupBox.Name = "provisionalPaymentGroupBox";
            this.provisionalPaymentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 178, true);
            this.provisionalPaymentGroupBox.TabIndex = 0;
            this.provisionalPaymentGroupBox.TabStop = false;
            // 
            // provisionalPaymentsGrid
            // 
            this.provisionalPaymentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.provisionalPaymentsGrid, "CustomsEntryHeaders.AllEntryLines.ProvisionalPayments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ProvisionalPayments)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.ProvisionalPaymentAmountCodeData)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ProvisionalPayments)).SyncRoot)).CY_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.ProvisionalPaymentAmountCodeData)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ProvisionalPayments)).SyncRoot)).CY_Value)));
            this.provisionalPaymentsGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a55db5b6-bfe4-45ee-b69a-56ae29316533", "Type");
            zDropEditColumnStyleInfo3.ColumnName = "CY_Code";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("96338e3f-d90e-4823-a2f7-d778dbd3f41e", "Value");
            zCalcEditColumnStyleInfo2.ColumnName = "CY_Value";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            this.provisionalPaymentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.provisionalPaymentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.provisionalPaymentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.provisionalPaymentsGrid.GridId = "96a79ed1-7fce-494c-b846-acb761ede24c";
            this.provisionalPaymentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.provisionalPaymentsGrid.LayoutKey = "ProvisionalPaymentsGrid";
            this.provisionalPaymentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.provisionalPaymentsGrid.Name = "provisionalPaymentsGrid";
            this.provisionalPaymentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 159, true);
            this.provisionalPaymentsGrid.TabIndex = 0;
            // 
            // EntryLineAdditionalDataUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ExtendInfoTabControl);
            this.Name = "EntryLineAdditionalDataUserControl";
            this.ShouldSerializeTabPageMethods = false;
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ExtendedInfoGroupBox.ResumeLayout(false);
            this.ExtendedInfoGroupBox.PerformLayout();
            this.feeGroupBox.ResumeLayout(false);
            this.feeGroupBox.PerformLayout();
            this.quantityGroupBox.ResumeLayout(false);
            this.quantityGroupBox.PerformLayout();
            this.customsQuantityCalcDropEdit.ResumeLayout(true);
            this.customsQuantityCalcDropEdit.PerformLayout();
            this.additionalQuantityCalcDropEdit.ResumeLayout(true);
            this.additionalQuantityCalcDropEdit.PerformLayout();
            this.classificationQuantityCalcDropEdit.ResumeLayout(true);
            this.classificationQuantityCalcDropEdit.PerformLayout();
            this.warehouseCountablyQuantityCalcDropEdit.ResumeLayout(true);
            this.warehouseCountablyQuantityCalcDropEdit.PerformLayout();
            this.zCodeFindBox1.ResumeLayout(true);
            this.zCodeFindBox1.PerformLayout();
            this.ExtendInfoTabControl.ResumeLayout(false);
            this.ExtendInfoTabControl.PerformLayout();
            this.zTabPageAdditionalInformation.ResumeLayout(false);
            this.zTabPageAdditionalInformation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGridAdditionalInformation)).EndInit();
            this.zGridAdditionalInformation.ResumeLayout(false);
            this.zGridAdditionalInformation.PerformLayout();
            this.ExtendedInfoTabPage.ResumeLayout(false);
            this.ExtendedInfoTabPage.PerformLayout();
            this.taxOrFeeTabPage.ResumeLayout(false);
            this.taxOrFeeTabPage.PerformLayout();
            this.dutyAndFeeSplitContainer.Panel1.ResumeLayout(false);
            this.dutyAndFeeSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dutyAndFeeSplitContainer)).EndInit();
            this.dutyAndFeeSplitContainer.ResumeLayout(false);
            this.dutyAndFeeSplitContainer.PerformLayout();
            this.entryLineDutyAndTaxGroupBox.ResumeLayout(false);
            this.entryLineDutyAndTaxGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.entryLineDutyAndTaxGrid)).EndInit();
            this.entryLineDutyAndTaxGrid.ResumeLayout(false);
            this.entryLineDutyAndTaxGrid.PerformLayout();
            this.provisionalPaymentGroupBox.ResumeLayout(false);
            this.provisionalPaymentGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.provisionalPaymentsGrid)).EndInit();
            this.provisionalPaymentsGrid.ResumeLayout(false);
            this.provisionalPaymentsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		#endregion

		private ZArchitecture.ZGrid provisionalPaymentsGrid;
		private CargoWise.Windows.UI.KSplitContainer dutyAndFeeSplitContainer;
		private ZGroupBox provisionalPaymentGroupBox;
		private ZArchitecture.ZTextBox zTextBox3;
		private ZArchitecture.ZCalcEdit zCalcEdit1;
		private ZArchitecture.ZTextBox zTextBox2;
		private ZArchitecture.ZLabel tradeAgreementLabel;
		private ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private ZArchitecture.ZTextBox zTextBox4;
		private ZArchitecture.ZCalcEdit penaltyCalcEdit;
		private ZArchitecture.ZCalcEdit provisionalPaymentCalcEdit;
		private ZArchitecture.ZCalcEdit vATCalcEdit;
		private ZArchitecture.ZCalcEdit s1P2BCalcEdit;
		private ZArchitecture.ZCalcEdit customsDutyCalcEdit;
		private ZCalcDropEdit warehouseCountablyQuantityCalcDropEdit;
		private ZCalcDropEdit classificationQuantityCalcDropEdit;
		private ZCalcDropEdit additionalQuantityCalcDropEdit;
		private ZCalcDropEdit customsQuantityCalcDropEdit;
		private ZGroupBox quantityGroupBox;
		private ZGroupBox feeGroupBox;
		public ZArchitecture.ZGrid entryLineDutyAndTaxGrid;
		private ZArchitecture.ZTextBox tariffCodeTextBox;
		private ZArchitecture.ZTextBox descriptionTextBox;
		protected internal ZTemplateTabControl ExtendInfoTabControl;
		protected internal ZTabPage zTabPageAdditionalInformation;
		private ZArchitecture.ZGrid zGridAdditionalInformation;
		protected internal ZTabPage ExtendedInfoTabPage;
		private ZTabPage taxOrFeeTabPage;
		public ZGroupBox ExtendedInfoGroupBox;
		private ZGroupBox entryLineDutyAndTaxGroupBox;
	}
}
