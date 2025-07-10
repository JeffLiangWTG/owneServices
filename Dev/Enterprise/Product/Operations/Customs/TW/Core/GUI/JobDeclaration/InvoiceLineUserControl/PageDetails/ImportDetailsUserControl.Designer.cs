namespace Enterprise.Customs.TW.GUI
{
	partial class ImportDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ImportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SpecialDutiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_RetaliatoryDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_AntiDumpingDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_AdditionalDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_CountervailingDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsRegulationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsRegulationsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TW_VatPymntMthdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TW_TpfPymntMthdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_CusValueConvRatioCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_ProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FormattedSpecificDutyRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FormattedAdValoremDutyRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TW_DtyPymntMthdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TW_ImportTariffDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RAPRORGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_UseOneTenthCVCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TW_RAPRORPriceConvertToLocalCurrencyControl = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JI_ConcessionOrderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_AlcoholPercentageTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_TariffAdditionalCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_PrimaryPreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.JI_CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SecondaryTariffsUserControl = new Enterprise.Customs.TW.GUI.SecondaryTariffsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImportPanel.SuspendLayout();
			this.SpecialDutiesGroupBox.SuspendLayout();
			this.CustomsRegulationsGroupBox.SuspendLayout();
			this.FeesGroupBox.SuspendLayout();
			this.TW_VatPymntMthdDropEdit.SuspendLayout();
			this.TW_TpfPymntMthdDropEdit.SuspendLayout();
			this.JI_ProcedureDropEdit.SuspendLayout();
			this.DutyRatesGroupBox.SuspendLayout();
			this.TW_DtyPymntMthdDropEdit.SuspendLayout();
			this.RAPRORGroupBox.SuspendLayout();
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_ConcessionOrderDropEdit.SuspendLayout();
			this.JI_PrimaryPreferenceDropEdit.SuspendLayout();
			this.JI_TariffFindBox.SuspendLayout();
			this.JI_CountryOfOriginCodeFindBox.SuspendLayout();
			this.SecondaryTariffsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// ImportPanel
			// 
			this.ImportPanel.Controls.Add(this.SpecialDutiesGroupBox);
			this.ImportPanel.Controls.Add(this.CustomsRegulationsGroupBox);
			this.ImportPanel.Controls.Add(this.FeesGroupBox);
			this.ImportPanel.Controls.Add(this.JI_CusValueConvRatioCalcEdit);
			this.ImportPanel.Controls.Add(this.JI_ProcedureDropEdit);
			this.ImportPanel.Controls.Add(this.DutyRatesGroupBox);
			this.ImportPanel.Controls.Add(this.TW_ImportTariffDescriptionTextBox);
			this.ImportPanel.Controls.Add(this.RAPRORGroupBox);
			this.ImportPanel.Controls.Add(this.JI_ConcessionOrderDropEdit);
			this.ImportPanel.Controls.Add(this.JI_AlcoholPercentageTextBox);
			this.ImportPanel.Controls.Add(this.JI_TariffAdditionalCodeTextBox);
			this.ImportPanel.Controls.Add(this.JI_PrimaryPreferenceDropEdit);
			this.ImportPanel.Controls.Add(this.JI_TariffFindBox);
			this.ImportPanel.Controls.Add(this.JI_CountryOfOriginCodeFindBox);
			this.ImportPanel.Controls.Add(this.SecondaryTariffsUserControl);
			this.ImportPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportPanel.Name = "ImportPanel";
			this.ImportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 369, true);
			this.ImportPanel.TabIndex = 0;
			// 
			// SpecialDutiesGroupBox
			// 
			this.SpecialDutiesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8553658e-9a33-46c1-8305-4c7b8edde96e", "Special Duties");
			this.SpecialDutiesGroupBox.Controls.Add(this.JI_RetaliatoryDutyRateCalcEdit);
			this.SpecialDutiesGroupBox.Controls.Add(this.JI_AntiDumpingDutyRateCalcEdit);
			this.SpecialDutiesGroupBox.Controls.Add(this.JI_AdditionalDutyRateCalcEdit);
			this.SpecialDutiesGroupBox.Controls.Add(this.JI_CountervailingDutyRateCalcEdit);
			this.SpecialDutiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 115, true);
			this.SpecialDutiesGroupBox.Name = "SpecialDutiesGroupBox";
			this.SpecialDutiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 127, true);
			this.SpecialDutiesGroupBox.TabIndex = 10;
			this.SpecialDutiesGroupBox.TabStop = false;
			// 
			// JI_RetaliatoryDutyRateCalcEdit
			// 
			this.JI_RetaliatoryDutyRateCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_RetaliatoryDutyRateCalcEdit, "FilteredInvoiceLines.JI_RetaliatoryDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RetaliatoryDutyRate)));
			this.JI_RetaliatoryDutyRateCalcEdit.DecimalPlaces = 5;
			this.JI_RetaliatoryDutyRateCalcEdit.Decimals = 5;
			this.JI_RetaliatoryDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 94, true);
			this.JI_RetaliatoryDutyRateCalcEdit.Name = "JI_RetaliatoryDutyRateCalcEdit";
			this.JI_RetaliatoryDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JI_RetaliatoryDutyRateCalcEdit.TabIndex = 4;
			this.JI_RetaliatoryDutyRateCalcEdit.Text = "0.00000";
			this.JI_RetaliatoryDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_RetaliatoryDutyRateCalcEdit.TrackDisposedAccess = true;
			// 
			// JI_AntiDumpingDutyRateCalcEdit
			// 
			this.JI_AntiDumpingDutyRateCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_AntiDumpingDutyRateCalcEdit, "FilteredInvoiceLines.JI_AntiDumpingDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AntiDumpingDutyRate)));
			this.JI_AntiDumpingDutyRateCalcEdit.DecimalPlaces = 5;
			this.JI_AntiDumpingDutyRateCalcEdit.Decimals = 5;
			this.JI_AntiDumpingDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 18, true);
			this.JI_AntiDumpingDutyRateCalcEdit.Name = "JI_AntiDumpingDutyRateCalcEdit";
			this.JI_AntiDumpingDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JI_AntiDumpingDutyRateCalcEdit.TabIndex = 1;
			this.JI_AntiDumpingDutyRateCalcEdit.Text = "0.00000";
			this.JI_AntiDumpingDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_AntiDumpingDutyRateCalcEdit.TrackDisposedAccess = true;
			// 
			// JI_AdditionalDutyRateCalcEdit
			// 
			this.JI_AdditionalDutyRateCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_AdditionalDutyRateCalcEdit, "FilteredInvoiceLines.JI_AdditionalDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AdditionalDutyRate)));
			this.JI_AdditionalDutyRateCalcEdit.DecimalPlaces = 5;
			this.JI_AdditionalDutyRateCalcEdit.Decimals = 5;
			this.JI_AdditionalDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 68, true);
			this.JI_AdditionalDutyRateCalcEdit.Name = "JI_AdditionalDutyRateCalcEdit";
			this.JI_AdditionalDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JI_AdditionalDutyRateCalcEdit.TabIndex = 3;
			this.JI_AdditionalDutyRateCalcEdit.Text = "0.00000";
			this.JI_AdditionalDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_AdditionalDutyRateCalcEdit.TrackDisposedAccess = true;
			// 
			// JI_CountervailingDutyRateCalcEdit
			// 
			this.JI_CountervailingDutyRateCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_CountervailingDutyRateCalcEdit, "FilteredInvoiceLines.JI_CountervailingDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountervailingDutyRate)));
			this.JI_CountervailingDutyRateCalcEdit.DecimalPlaces = 5;
			this.JI_CountervailingDutyRateCalcEdit.Decimals = 5;
			this.JI_CountervailingDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 44, true);
			this.JI_CountervailingDutyRateCalcEdit.Name = "JI_CountervailingDutyRateCalcEdit";
			this.JI_CountervailingDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JI_CountervailingDutyRateCalcEdit.TabIndex = 2;
			this.JI_CountervailingDutyRateCalcEdit.Text = "0.00000";
			this.JI_CountervailingDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_CountervailingDutyRateCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomsRegulationsGroupBox
			// 
			this.CustomsRegulationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CustomsRegulationsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f20a1bcc-0fd4-4a1a-b41b-505af6482796", "Customs Regulations");
			this.CustomsRegulationsGroupBox.Controls.Add(this.CustomsRegulationsTextBox);
			this.CustomsRegulationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(872, 115, true);
			this.CustomsRegulationsGroupBox.Name = "CustomsRegulationsGroupBox";
			this.CustomsRegulationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 254, true);
			this.CustomsRegulationsGroupBox.TabIndex = 13;
			this.CustomsRegulationsGroupBox.TabStop = false;
			// 
			// CustomsRegulationsTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsRegulationsTextBox, "FilteredInvoiceLines.CustomsRegulations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CustomsRegulations)));
			this.CustomsRegulationsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomsRegulationsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsRegulationsTextBox, false);
			this.CustomsRegulationsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomsRegulationsTextBox.Multiline = true;
			this.CustomsRegulationsTextBox.Name = "CustomsRegulationsTextBox";
			this.CustomsRegulationsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CustomsRegulationsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 235, true);
			this.CustomsRegulationsTextBox.TabIndex = 0;
			// 
			// FeesGroupBox
			// 
			this.FeesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("02b948e8-ebe5-4cb0-bbed-e770034af46f", "Fees");
			this.FeesGroupBox.Controls.Add(this.TW_VatPymntMthdDropEdit);
			this.FeesGroupBox.Controls.Add(this.TW_TpfPymntMthdDropEdit);
			this.FeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 10, true);
			this.FeesGroupBox.Name = "FeesGroupBox";
			this.FeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 95, true);
			this.FeesGroupBox.TabIndex = 11;
			this.FeesGroupBox.TabStop = false;
			// 
			// TW_VatPymntMthdDropEdit
			// 
			this.TW_VatPymntMthdDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_VatPymntMthdDropEdit, "FilteredInvoiceLines.JI_VatPymntMthd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_VatPymntMthd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_VatPymntMthdDescription)));
			this.TW_VatPymntMthdDropEdit.BindToForDescription = "FilteredInvoiceLines.JI_VatPymntMthdDescription";
			this.TW_VatPymntMthdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 19, true);
			this.TW_VatPymntMthdDropEdit.Name = "TW_VatPymntMthdDropEdit";
			this.TW_VatPymntMthdDropEdit.PreBoundMaxLength = 3;
			this.TW_VatPymntMthdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.TW_VatPymntMthdDropEdit.TabIndex = 0;
			// 
			// TW_TpfPymntMthdDropEdit
			// 
			this.TW_TpfPymntMthdDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_TpfPymntMthdDropEdit, "FilteredInvoiceLines.JI_TpfPymntMthd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TpfPymntMthd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TpfPymntMthdDescription)));
			this.TW_TpfPymntMthdDropEdit.BindToForDescription = "FilteredInvoiceLines.JI_TpfPymntMthdDescription";
			this.TW_TpfPymntMthdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 45, true);
			this.TW_TpfPymntMthdDropEdit.Name = "TW_TpfPymntMthdDropEdit";
			this.TW_TpfPymntMthdDropEdit.PreBoundMaxLength = 3;
			this.TW_TpfPymntMthdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.TW_TpfPymntMthdDropEdit.TabIndex = 1;
			// 
			// JI_CusValueConvRatioCalcEdit
			// 
			this.JI_CusValueConvRatioCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_CusValueConvRatioCalcEdit, "FilteredInvoiceLines.JI_CusValueConvRatio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CusValueConvRatio)));
			this.JI_CusValueConvRatioCalcEdit.DecimalPlaces = 4;
			this.JI_CusValueConvRatioCalcEdit.Decimals = 4;
			this.JI_CusValueConvRatioCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 137, true);
			this.JI_CusValueConvRatioCalcEdit.Name = "JI_CusValueConvRatioCalcEdit";
			this.JI_CusValueConvRatioCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.JI_CusValueConvRatioCalcEdit.TabIndex = 7;
			this.JI_CusValueConvRatioCalcEdit.Text = "0.0000";
			this.JI_CusValueConvRatioCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_CusValueConvRatioCalcEdit.TrackDisposedAccess = true;
			// 
			// JI_ProcedureDropEdit
			// 
			this.JI_ProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_ProcedureDropEdit, "FilteredInvoiceLines.JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Procedure)));
			this.JI_ProcedureDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("4d9d9218-2dc9-4226-89d3-edd84ea97fe8", "Duty Treatment", "The code of duty treatment.");
			this.JI_ProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 85, true);
			this.JI_ProcedureDropEdit.Name = "JI_ProcedureDropEdit";
			this.JI_ProcedureDropEdit.PreBoundMaxLength = 3;
			this.JI_ProcedureDropEdit.ShouldResizeByMaxLength = false;
			this.JI_ProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.JI_ProcedureDropEdit.TabIndex = 4;
			// 
			// DutyRatesGroupBox
			// 
			this.DutyRatesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e74151d7-4135-4766-9161-8e1b8606f50b", "Duties");
			this.DutyRatesGroupBox.Controls.Add(this.FormattedSpecificDutyRateTextBox);
			this.DutyRatesGroupBox.Controls.Add(this.FormattedAdValoremDutyRateTextBox);
			this.DutyRatesGroupBox.Controls.Add(this.TW_DtyPymntMthdDropEdit);
			this.DutyRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 10, true);
			this.DutyRatesGroupBox.Name = "DutyRatesGroupBox";
			this.DutyRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 95, true);
			this.DutyRatesGroupBox.TabIndex = 9;
			this.DutyRatesGroupBox.TabStop = false;
			// 
			// FormattedSpecificDutyRateTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormattedSpecificDutyRateTextBox, "FilteredInvoiceLines.FormattedSpecificDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).FormattedSpecificDutyRate)));
			this.FormattedSpecificDutyRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 67, true);
			this.FormattedSpecificDutyRateTextBox.Name = "FormattedSpecificDutyRateTextBox";
			this.FormattedSpecificDutyRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.FormattedSpecificDutyRateTextBox.TabIndex = 3;
			// 
			// FormattedAdValoremDutyRateTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormattedAdValoremDutyRateTextBox, "FilteredInvoiceLines.FormattedAdValoremDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).FormattedAdValoremDutyRate)));
			this.FormattedAdValoremDutyRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 42, true);
			this.FormattedAdValoremDutyRateTextBox.Name = "FormattedAdValoremDutyRateTextBox";
			this.FormattedAdValoremDutyRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.FormattedAdValoremDutyRateTextBox.TabIndex = 2;
			// 
			// TW_DtyPymntMthdDropEdit
			// 
			this.TW_DtyPymntMthdDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_DtyPymntMthdDropEdit, "FilteredInvoiceLines.JI_DtyPymntMthd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DtyPymntMthd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DtyPymntMthdDescription)));
			this.TW_DtyPymntMthdDropEdit.BindToForDescription = "FilteredInvoiceLines.JI_DtyPymntMthdDescription";
			this.TW_DtyPymntMthdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 18, true);
			this.TW_DtyPymntMthdDropEdit.Name = "TW_DtyPymntMthdDropEdit";
			this.TW_DtyPymntMthdDropEdit.PreBoundMaxLength = 3;
			this.TW_DtyPymntMthdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.TW_DtyPymntMthdDropEdit.TabIndex = 1;
			// 
			// TW_ImportTariffDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.TW_ImportTariffDescriptionTextBox, "FilteredInvoiceLines.JI_TariffDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TariffDescription)));
			this.TW_ImportTariffDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 10, true);
			this.TW_ImportTariffDescriptionTextBox.Name = "TW_ImportTariffDescriptionTextBox";
			this.TW_ImportTariffDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.TW_ImportTariffDescriptionTextBox.TabIndex = 1;
			// 
			// RAPRORGroupBox
			// 
			this.RAPRORGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("09d5898e-c1af-46c1-b4eb-04286f397ba8", "Repair / Assembly / Processing");
			this.RAPRORGroupBox.Controls.Add(this.JI_UseOneTenthCVCheckBox);
			this.RAPRORGroupBox.Controls.Add(this.TW_RAPRORPriceConvertToLocalCurrencyControl);
			this.RAPRORGroupBox.Controls.Add(this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl);
			this.RAPRORGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 115, true);
			this.RAPRORGroupBox.Name = "RAPRORGroupBox";
			this.RAPRORGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 127, true);
			this.RAPRORGroupBox.TabIndex = 12;
			this.RAPRORGroupBox.TabStop = false;
			// 
			// JI_UseOneTenthCVCheckBox
			// 
			this.BindingSource.SetBindingMember(this.JI_UseOneTenthCVCheckBox, "FilteredInvoiceLines.JI_UseOneTenthCV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_UseOneTenthCV)));
			this.JI_UseOneTenthCVCheckBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8636c182-6e4d-4f83-a06f-9159a199a8f0", "Use 10% CV");
			this.JI_UseOneTenthCVCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JI_UseOneTenthCVCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 71, true);
			this.JI_UseOneTenthCVCheckBox.Name = "JI_UseOneTenthCVCheckBox";
			this.JI_UseOneTenthCVCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.JI_UseOneTenthCVCheckBox.TabIndex = 2;
			this.JI_UseOneTenthCVCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.JI_UseOneTenthCVCheckBox.UseVisualStyleBackColor = true;
			// 
			// TW_RAPRORPriceConvertToLocalCurrencyControl
			// 
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_RAPRORPriceConvertToLocalCurrencyControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RAPPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RAPCurr)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.RAPRORCurrencyList)));
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_RAPPrice";
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups.RAPRORCurrencyList";
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RAPCurr";
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.Decimals = 6;
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 45, true);
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.Name = "TW_RAPRORPriceConvertToLocalCurrencyControl";
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.TabIndex = 1;
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.UnitPreBoundMaxLength = 3;
			// 
			// JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_RAPRORUnitPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Calc_RAPRORUnitCurr)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.RAPRORCurrencyList)));
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_RAPRORUnitPrice";
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups.RAPRORCurrencyList";
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_Calc_RAPRORUnitCurr";
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Decimals = 6;
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 19, true);
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Name = "JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl";
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.TabIndex = 0;
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.UnitPreBoundMaxLength = 3;
			// 
			// JI_ConcessionOrderDropEdit
			// 
			this.JI_ConcessionOrderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_ConcessionOrderDropEdit, "FilteredInvoiceLines.JI_ConcessionOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ConcessionOrder)));
			this.JI_ConcessionOrderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 168, true);
			this.JI_ConcessionOrderDropEdit.Name = "JI_ConcessionOrderDropEdit";
			this.JI_ConcessionOrderDropEdit.PreBoundMaxLength = 20;
			this.JI_ConcessionOrderDropEdit.ShouldResizeByMaxLength = false;
			this.JI_ConcessionOrderDropEdit.ShowDescriptionBox = false;
			this.JI_ConcessionOrderDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.JI_ConcessionOrderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.JI_ConcessionOrderDropEdit.TabIndex = 8;
			// 
			// JI_AlcoholPercentageTextBox
			// 
			this.JI_AlcoholPercentageTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_AlcoholPercentageTextBox, "FilteredInvoiceLines.JI_AlcoholPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AlcoholPercentage)));
			this.JI_AlcoholPercentageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JI_AlcoholPercentageTextBox.DecimalPlaces = 3;
			this.JI_AlcoholPercentageTextBox.Decimals = 3;
			this.JI_AlcoholPercentageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 111, true);
			this.JI_AlcoholPercentageTextBox.Name = "JI_AlcoholPercentageTextBox";
			this.JI_AlcoholPercentageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.JI_AlcoholPercentageTextBox.TabIndex = 5;
			this.JI_AlcoholPercentageTextBox.Text = "0.000";
			this.JI_AlcoholPercentageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JI_AlcoholPercentageTextBox.TrackDisposedAccess = true;
			// 
			// JI_TariffAdditionalCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_TariffAdditionalCodeTextBox, "FilteredInvoiceLines.JI_TariffAdditionalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TariffAdditionalCode)));
			this.JI_TariffAdditionalCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 111, true);
			this.JI_TariffAdditionalCodeTextBox.Name = "JI_TariffAdditionalCodeTextBox";
			this.JI_TariffAdditionalCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.JI_TariffAdditionalCodeTextBox.TabIndex = 6;
			// 
			// JI_PrimaryPreferenceDropEdit
			// 
			this.JI_PrimaryPreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_PrimaryPreferenceDropEdit, "FilteredInvoiceLines.JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PrimaryPreference)));
			this.JI_PrimaryPreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 60, true);
			this.JI_PrimaryPreferenceDropEdit.Name = "JI_PrimaryPreferenceDropEdit";
			this.JI_PrimaryPreferenceDropEdit.PreBoundMaxLength = 3;
			this.JI_PrimaryPreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.JI_PrimaryPreferenceDropEdit.TabIndex = 3;
			// 
			// JI_TariffFindBox
			// 
			this.JI_TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_TariffFindBox, "FilteredInvoiceLines.JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			this.JI_TariffFindBox.ErrorForUnsupportedCountry = null;
			this.JI_TariffFindBox.GetEffectiveDate = null;
			this.JI_TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 10, true);
			this.JI_TariffFindBox.Name = "JI_TariffFindBox";
			this.JI_TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_TariffFindBox.ParentType = null;
			this.JI_TariffFindBox.PreBoundMaxLength = 11;
			this.JI_TariffFindBox.SelectNomenclatureModes = null;
			this.JI_TariffFindBox.ShouldResize = false;
			this.JI_TariffFindBox.ShowDescriptionBox = false;
			this.JI_TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.JI_TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JI_TariffFindBox.TabIndex = 0;
			this.JI_TariffFindBox.TariffType = null;
			// 
			// JI_CountryOfOriginCodeFindBox
			// 
			this.JI_CountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CountryOfOriginCodeFindBox, "FilteredInvoiceLines.JI_CountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountryOfOrigin)));
			this.JI_CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 35, true);
			this.JI_CountryOfOriginCodeFindBox.Name = "JI_CountryOfOriginCodeFindBox";
			this.JI_CountryOfOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_CountryOfOriginCodeFindBox.ParentType = null;
			this.JI_CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
			this.JI_CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.JI_CountryOfOriginCodeFindBox.TabIndex = 2;
			// 
			// SecondaryTariffsUserControl
			// 
			this.SecondaryTariffsUserControl.AllowDrop = true;
			this.SecondaryTariffsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SecondaryTariffsUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.SecondaryTariffsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 248, true);
			this.SecondaryTariffsUserControl.Name = "SecondaryTariffsUserControl";
			this.SecondaryTariffsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 121, true);
			this.SecondaryTariffsUserControl.TabIndex = 14;
			// 
			// ImportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImportPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 335, true);
			this.Name = "ImportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 369, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImportPanel.ResumeLayout(false);
			this.ImportPanel.PerformLayout();
			this.SpecialDutiesGroupBox.ResumeLayout(false);
			this.SpecialDutiesGroupBox.PerformLayout();
			this.CustomsRegulationsGroupBox.ResumeLayout(false);
			this.CustomsRegulationsGroupBox.PerformLayout();
			this.FeesGroupBox.ResumeLayout(false);
			this.FeesGroupBox.PerformLayout();
			this.TW_VatPymntMthdDropEdit.ResumeLayout(true);
			this.TW_VatPymntMthdDropEdit.PerformLayout();
			this.TW_TpfPymntMthdDropEdit.ResumeLayout(true);
			this.TW_TpfPymntMthdDropEdit.PerformLayout();
			this.JI_ProcedureDropEdit.ResumeLayout(true);
			this.JI_ProcedureDropEdit.PerformLayout();
			this.DutyRatesGroupBox.ResumeLayout(false);
			this.DutyRatesGroupBox.PerformLayout();
			this.TW_DtyPymntMthdDropEdit.ResumeLayout(true);
			this.TW_DtyPymntMthdDropEdit.PerformLayout();
			this.RAPRORGroupBox.ResumeLayout(false);
			this.RAPRORGroupBox.PerformLayout();
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.TW_RAPRORPriceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_ConcessionOrderDropEdit.ResumeLayout(true);
			this.JI_ConcessionOrderDropEdit.PerformLayout();
			this.JI_PrimaryPreferenceDropEdit.ResumeLayout(true);
			this.JI_PrimaryPreferenceDropEdit.PerformLayout();
			this.JI_TariffFindBox.ResumeLayout(true);
			this.JI_TariffFindBox.PerformLayout();
			this.JI_CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.JI_CountryOfOriginCodeFindBox.PerformLayout();
			this.SecondaryTariffsUserControl.ResumeLayout(true);
			this.SecondaryTariffsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ImportPanel;
		protected SecondaryTariffsUserControl SecondaryTariffsUserControl;
		internal Universal.GUI.TariffFindBox JI_TariffFindBox;
		private ZArchitecture.GUI.ZCodeFindBox JI_CountryOfOriginCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit JI_PrimaryPreferenceDropEdit;
		public ZArchitecture.ZCalcEdit JI_AlcoholPercentageTextBox;
		public ZArchitecture.ZTextBox JI_TariffAdditionalCodeTextBox;
		public ZArchitecture.GUI.ZDropEdit JI_ConcessionOrderDropEdit;
		public ZArchitecture.GUI.ZGroupBox RAPRORGroupBox;
		internal ZArchitecture.GUI.ZCheckBox JI_UseOneTenthCVCheckBox;
		public ZArchitecture.GUI.ZCalcDropEdit TW_RAPRORPriceConvertToLocalCurrencyControl;
		public ZArchitecture.GUI.ZCalcDropEdit JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl;
		private ZArchitecture.ZTextBox TW_ImportTariffDescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit JI_ProcedureDropEdit;
		public ZArchitecture.GUI.ZGroupBox DutyRatesGroupBox;
		public ZArchitecture.ZCalcEdit JI_AdditionalDutyRateCalcEdit;
		public ZArchitecture.ZCalcEdit JI_CountervailingDutyRateCalcEdit;
		public ZArchitecture.ZCalcEdit JI_AntiDumpingDutyRateCalcEdit;
		public ZArchitecture.ZCalcEdit JI_RetaliatoryDutyRateCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit TW_DtyPymntMthdDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TW_TpfPymntMthdDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TW_VatPymntMthdDropEdit;
		public ZArchitecture.ZCalcEdit JI_CusValueConvRatioCalcEdit;
		private ZArchitecture.ZTextBox FormattedAdValoremDutyRateTextBox;
		public ZArchitecture.GUI.ZGroupBox FeesGroupBox;
		private ZArchitecture.GUI.ZGroupBox CustomsRegulationsGroupBox;
		private ZArchitecture.ZTextBox CustomsRegulationsTextBox;
		private ZArchitecture.ZTextBox FormattedSpecificDutyRateTextBox;
		private ZArchitecture.GUI.ZGroupBox SpecialDutiesGroupBox;
	}
}
