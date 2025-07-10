namespace Enterprise.Customs.US.GUI
{
	partial class PSTEditForm
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntendedUseDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExamLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ShipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.IsPSTLabelsSentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConfidentialityRemarkText = new Enterprise.ZArchitecture.ZTextBox();
			this.CBIIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NotifyPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertifyingIndividualDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zCalcDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.UnitsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoOfUnit1Field = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoOfUnit2Field = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoOfUnit3Field = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoOfUnit4Field = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoOfUnit5Field = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NoOfUnit6Field = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PSTQtyTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProducerEstForeignNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProducerEstNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LPCONumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonRemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnregReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ExamLocationAddressControl.SuspendLayout();
			this.ShipperAddressControl.SuspendLayout();
			this.NotifyPartyDropEdit.SuspendLayout();
			this.CertifyingIndividualDropEdit.SuspendLayout();
			this.zCalcDropEdit2.SuspendLayout();
			this.UnitsGroupBox.SuspendLayout();
			this.NoOfUnit1Field.SuspendLayout();
			this.NoOfUnit2Field.SuspendLayout();
			this.NoOfUnit3Field.SuspendLayout();
			this.NoOfUnit4Field.SuspendLayout();
			this.NoOfUnit5Field.SuspendLayout();
			this.NoOfUnit6Field.SuspendLayout();
			this.UnregReasonDropEdit.SuspendLayout();
			this.ProductTypeDropEdit.SuspendLayout();
			this.IntendedUseCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 682, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.Pesticide);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6171d8c2-35d5-4aea-8d54-71e85538c662", "Pesticide");
			this.DetailsGroupBox.Controls.Add(this.IntendedUseDescTextBox);
			this.DetailsGroupBox.Controls.Add(this.ExamLocationAddressControl);
			this.DetailsGroupBox.Controls.Add(this.ShipperAddressControl);
			this.DetailsGroupBox.Controls.Add(this.IsPSTLabelsSentCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ConfidentialityRemarkText);
			this.DetailsGroupBox.Controls.Add(this.CBIIndicatorCheckBox);
			this.DetailsGroupBox.Controls.Add(this.NotifyPartyDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CertifyingIndividualDropEdit);
			this.DetailsGroupBox.Controls.Add(this.zCalcDropEdit2);
			this.DetailsGroupBox.Controls.Add(this.UnitsGroupBox);
			this.DetailsGroupBox.Controls.Add(this.OKButton);
			this.DetailsGroupBox.Controls.Add(this.ProducerEstForeignNoTextBox);
			this.DetailsGroupBox.Controls.Add(this.ProducerEstNoTextBox);
			this.DetailsGroupBox.Controls.Add(this.LPCONumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.BrandNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.ReasonRemarksTextBox);
			this.DetailsGroupBox.Controls.Add(this.UnregReasonDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ProductTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.IntendedUseCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.LineNoTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 682, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// IntendedUseDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseDescTextBox, "US_IntendedUseDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_IntendedUseDesc)));
			this.IntendedUseDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("44e5720f-927d-4a1f-82ae-d2ea9c9064ba", "Intended Use Description");
			this.IntendedUseDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 69, true);
			this.IntendedUseDescTextBox.Name = "IntendedUseDescTextBox";
			this.IntendedUseDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 18, true);
			this.IntendedUseDescTextBox.TabIndex = 2;
			// 
			// ExamLocationAddressControl
			// 
			this.ExamLocationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExamLocationAddressControl, "US_OA_ExaminationLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_OA_ExaminationLocation)));
			this.ExamLocationAddressControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.ExamLocationAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d0ec7ec9-9517-4fed-bce2-1ccab23120ab", "Exam. Location");
			this.ExamLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 326, true);
			this.ExamLocationAddressControl.Name = "ExamLocationAddressControl";
			this.ExamLocationAddressControl.PopupCaption = "Exam. Location";
			this.ExamLocationAddressControl.ReadOnly = false;
			this.ExamLocationAddressControl.ShowAddress = false;
			this.ExamLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.ExamLocationAddressControl.TabIndex = 12;
			// 
			// ShipperAddressControl
			// 
			this.ShipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressControl, "US_OA_ShipperAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_OA_ShipperAddress)));
			this.ShipperAddressControl.BindToOrgList = "AddInfoLookups+Organizations";
			this.ShipperAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9afbe26d-cf56-46a5-a97d-2388e7a06531", "Shipper");
			this.ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 353, true);
			this.ShipperAddressControl.Name = "ShipperAddressControl";
			this.ShipperAddressControl.PopupCaption = "Shipper";
			this.ShipperAddressControl.ReadOnly = false;
			this.ShipperAddressControl.ShowAddress = false;
			this.ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.ShipperAddressControl.TabIndex = 13;
			// 
			// IsPSTLabelsSentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPSTLabelsSentCheckBox, "US_PSTLabelsSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_PSTLabelsSent)));
			this.IsPSTLabelsSentCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ab10c17d-2fb1-45bd-91ae-8096f21754c8", "Elec. Image Submitted");
			this.IsPSTLabelsSentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPSTLabelsSentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPSTLabelsSentCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsPSTLabelsSentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 658, true);
			this.IsPSTLabelsSentCheckBox.Name = "IsPSTLabelsSentCheckBox";
			this.IsPSTLabelsSentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.IsPSTLabelsSentCheckBox.TabIndex = 24;
			this.IsPSTLabelsSentCheckBox.Text = "Elec. Image Submitted";
			this.IsPSTLabelsSentCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsPSTLabelsSentCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConfidentialityRemarkText
			// 
			this.BindingSource.SetBindingMember(this.ConfidentialityRemarkText, "US_ConfidentialityRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_ConfidentialityRemarks)));
			this.ConfidentialityRemarkText.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("584a6e2f-a24c-4e16-a904-a60f08b48c61", "EPA NOA Comments");
			this.ConfidentialityRemarkText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 191, true);
			this.ConfidentialityRemarkText.Name = "ConfidentialityRemarkText";
			this.ConfidentialityRemarkText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 18, true);
			this.ConfidentialityRemarkText.TabIndex = 7;
			// 
			// CBIIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CBIIndicatorCheckBox, "US_CBIIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_CBIIndicator)));
			this.CBIIndicatorCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4500d794-a1e8-4cf9-a869-d9d9ee2f3bbf", "Confidential Info Included");
			this.CBIIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CBIIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CBIIndicatorCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CBIIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 144, true);
			this.CBIIndicatorCheckBox.Name = "CBIIndicatorCheckBox";
			this.CBIIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.CBIIndicatorCheckBox.TabIndex = 5;
			this.CBIIndicatorCheckBox.Text = "Confidential Info Included";
			this.CBIIndicatorCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CBIIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// NotifyPartyDropEdit
			// 
			this.NotifyPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyDropEdit, "US_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NotifyParty)));
			this.NotifyPartyDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bea01de7-5646-4ba2-bf42-3e870207b4e9", "Notify Party");
			this.NotifyPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 631, true);
			this.NotifyPartyDropEdit.Name = "NotifyPartyDropEdit";
			this.NotifyPartyDropEdit.PreBoundMaxLength = 3;
			this.NotifyPartyDropEdit.ShowDescriptionBox = false;
			this.NotifyPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.NotifyPartyDropEdit.TabIndex = 23;
			// 
			// CertifyingIndividualDropEdit
			// 
			this.CertifyingIndividualDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertifyingIndividualDropEdit, "US_CertifyingIndividual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_CertifyingIndividual)));
			this.CertifyingIndividualDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("175e013d-fb0e-458e-b4cd-3e7633e1ab4a", "Certifying Individual");
			this.CertifyingIndividualDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 604, true);
			this.CertifyingIndividualDropEdit.Name = "CertifyingIndividualDropEdit";
			this.CertifyingIndividualDropEdit.PreBoundMaxLength = 3;
			this.CertifyingIndividualDropEdit.ShowDescriptionBox = false;
			this.CertifyingIndividualDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.CertifyingIndividualDropEdit.TabIndex = 22;
			// 
			// zCalcDropEdit2
			// 
			this.zCalcDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_WeightUQ)));
			this.zCalcDropEdit2.BindToAmount = "US_NetWeight";
			this.zCalcDropEdit2.BindToUnit = "US_WeightUQ";
			this.zCalcDropEdit2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2765f663-761f-4799-aca6-7d308ef1ecaf", "Net Weight");
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 577, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.zCalcDropEdit2.TabIndex = 21;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 2;
			// 
			// UnitsGroupBox
			// 
			this.UnitsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0955A9FA-62D1-4A07-8C4A-6B72A8EDAE2A", "EPA Quantities");
			this.UnitsGroupBox.Controls.Add(this.NoOfUnit1Field);
			this.UnitsGroupBox.Controls.Add(this.NoOfUnit2Field);
			this.UnitsGroupBox.Controls.Add(this.NoOfUnit3Field);
			this.UnitsGroupBox.Controls.Add(this.NoOfUnit4Field);
			this.UnitsGroupBox.Controls.Add(this.NoOfUnit5Field);
			this.UnitsGroupBox.Controls.Add(this.NoOfUnit6Field);
			this.UnitsGroupBox.Controls.Add(this.PSTQtyTotalLabel);
			this.UnitsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 390, true);
			this.UnitsGroupBox.Name = "UnitsGroupBox";
			this.UnitsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 170, true);
			this.UnitsGroupBox.TabIndex = 14;
			this.UnitsGroupBox.TabStop = false;
			// 
			// NoOfUnit1Field
			// 
			this.NoOfUnit1Field.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfUnit1Field, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NoOfUnit1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UQ1)));
			this.NoOfUnit1Field.BindToAmount = "US_NoOfUnit1";
			this.NoOfUnit1Field.BindToUnit = "US_UQ1";
			this.NoOfUnit1Field.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e32ba0f1-0c9b-48a8-8cfa-6cc507794101", "Outermost Qty");
			this.NoOfUnit1Field.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 20, true);
			this.NoOfUnit1Field.Name = "NoOfUnit1Field";
			this.NoOfUnit1Field.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.NoOfUnit1Field.TabIndex = 14;
			this.NoOfUnit1Field.UnitPreBoundMaxLength = 2;
			// 
			// NoOfUnit2Field
			// 
			this.NoOfUnit2Field.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfUnit2Field, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NoOfUnit2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UQ2)));
			this.NoOfUnit2Field.BindToAmount = "US_NoOfUnit2";
			this.NoOfUnit2Field.BindToUnit = "US_UQ2";
			this.NoOfUnit2Field.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("60c186ad-00d8-4db9-9126-5c401acd5b15", "Next Inner Qty");
			this.NoOfUnit2Field.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 45, true);
			this.NoOfUnit2Field.Name = "NoOfUnit2Field";
			this.NoOfUnit2Field.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.NoOfUnit2Field.TabIndex = 15;
			this.NoOfUnit2Field.UnitPreBoundMaxLength = 2;
			// 
			// NoOfUnit3Field
			// 
			this.NoOfUnit3Field.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfUnit3Field, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NoOfUnit3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UQ3)));
			this.NoOfUnit3Field.BindToAmount = "US_NoOfUnit3";
			this.NoOfUnit3Field.BindToUnit = "US_UQ3";
			this.NoOfUnit3Field.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("A1EBAAEB-1FFC-4156-949A-ED6B029E72D4", "Next Inner Qty");
			this.NoOfUnit3Field.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 70, true);
			this.NoOfUnit3Field.Name = "NoOfUnit3Field";
			this.NoOfUnit3Field.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.NoOfUnit3Field.TabIndex = 16;
			this.NoOfUnit3Field.UnitPreBoundMaxLength = 2;
			// 
			// NoOfUnit4Field
			// 
			this.NoOfUnit4Field.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfUnit4Field, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NoOfUnit4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UQ4)));
			this.NoOfUnit4Field.BindToAmount = "US_NoOfUnit4";
			this.NoOfUnit4Field.BindToUnit = "US_UQ4";
			this.NoOfUnit4Field.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6D5EBF5D-35D6-4986-A2F6-396398D3D76B", "Next Inner Qty");
			this.NoOfUnit4Field.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 95, true);
			this.NoOfUnit4Field.Name = "NoOfUnit4Field";
			this.NoOfUnit4Field.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.NoOfUnit4Field.TabIndex = 17;
			this.NoOfUnit4Field.UnitPreBoundMaxLength = 2;
			// 
			// NoOfUnit5Field
			// 
			this.NoOfUnit5Field.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfUnit5Field, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NoOfUnit5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UQ5)));
			this.NoOfUnit5Field.BindToAmount = "US_NoOfUnit5";
			this.NoOfUnit5Field.BindToUnit = "US_UQ5";
			this.NoOfUnit5Field.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("E957AE0B-5E7C-4909-AB6D-E2A9E26F889E", "Next Inner Qty");
			this.NoOfUnit5Field.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 120, true);
			this.NoOfUnit5Field.Name = "NoOfUnit5Field";
			this.NoOfUnit5Field.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.NoOfUnit5Field.TabIndex = 18;
			this.NoOfUnit5Field.UnitPreBoundMaxLength = 2;
			// 
			// NoOfUnit6Field
			// 
			this.NoOfUnit6Field.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfUnit6Field, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_NoOfUnit6)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UQ6)));
			this.NoOfUnit6Field.BindToAmount = "US_NoOfUnit6";
			this.NoOfUnit6Field.BindToUnit = "US_UQ6";
			this.NoOfUnit6Field.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5EC0EF00-509C-4174-92C0-761CCB0512C3", "Next Inner Qty");
			this.NoOfUnit6Field.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 145, true);
			this.NoOfUnit6Field.Name = "NoOfUnit6Field";
			this.NoOfUnit6Field.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.NoOfUnit6Field.TabIndex = 19;
			this.NoOfUnit6Field.UnitPreBoundMaxLength = 2;
			// 
			// PSTQtyTotalLabel
			// 
			this.BindingSource.SetBindingMember(this.PSTQtyTotalLabel, "PSTQtyRunningTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).PSTQtyRunningTotal)));
			this.PSTQtyTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PSTQtyTotalLabel.IsFontBold = true;
			this.PSTQtyTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 20, true);
			this.PSTQtyTotalLabel.Name = "PSTQtyTotalLabel";
			this.PSTQtyTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.PSTQtyTotalLabel.TabIndex = 20;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0998c0fc-bab4-4524-892e-a64aa9684dfd", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 652, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 25;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ProducerEstForeignNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProducerEstForeignNoTextBox, "US_ProducerEstNoForeign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_ProducerEstNoForeign)));
			this.ProducerEstForeignNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("678f0c3a-6dc5-42de-9014-a582ab485c88", "Producer Est. No. (Foreign)");
			this.ProducerEstForeignNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 272, true);
			this.ProducerEstForeignNoTextBox.Name = "ProducerEstForeignNoTextBox";
			this.ProducerEstForeignNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 18, true);
			this.ProducerEstForeignNoTextBox.TabIndex = 10;
			// 
			// ProducerEstNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProducerEstNoTextBox, "US_ProducerEstNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_ProducerEstNo)));
			this.ProducerEstNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("58912d75-c984-4225-b5b5-6f594ef6fccc", "Producer Est. No. (Domestic)");
			this.ProducerEstNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 299, true);
			this.ProducerEstNoTextBox.Name = "ProducerEstNoTextBox";
			this.ProducerEstNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 18, true);
			this.ProducerEstNoTextBox.TabIndex = 11;
			// 
			// LPCONumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LPCONumberTextBox, "US_LPCONumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_LPCONumber)));
			this.LPCONumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3a03a9ee-9c2b-4592-a287-35b0234893b2", "EPA Registration Num.");
			this.LPCONumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 245, true);
			this.LPCONumberTextBox.Name = "LPCONumberTextBox";
			this.LPCONumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 18, true);
			this.LPCONumberTextBox.TabIndex = 9;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "US_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_BrandName)));
			this.BrandNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cd157a7c-f73d-4481-b3ac-3222d03359e6", "Brand Name");
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 218, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 18, true);
			this.BrandNameTextBox.TabIndex = 8;
			// 
			// ReasonRemarksTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonRemarksTextBox, "US_UnregReasonRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UnregReasonRemarks)));
			this.ReasonRemarksTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c5a39471-e6cc-4477-b4eb-5f5a9d582d2e", "General Remarks");
			this.ReasonRemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 119, true);
			this.ReasonRemarksTextBox.Name = "ReasonRemarksTextBox";
			this.ReasonRemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 18, true);
			this.ReasonRemarksTextBox.TabIndex = 4;
			// 
			// UnregReasonDropEdit
			// 
			this.UnregReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnregReasonDropEdit, "US_UnregReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_UnregReasonCode)));
			this.UnregReasonDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b415c671-f649-4b73-89c5-ed48a3deb1fd", "Reason Not Registered");
			this.UnregReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 164, true);
			this.UnregReasonDropEdit.Name = "UnregReasonDropEdit";
			this.UnregReasonDropEdit.PreBoundMaxLength = 3;
			this.UnregReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 18, true);
			this.UnregReasonDropEdit.TabIndex = 6;
			// 
			// ProductTypeDropEdit
			// 
			this.ProductTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductTypeDropEdit, "US_ProductType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_ProductType)));
			this.ProductTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("dd2bc430-afee-472b-932c-31c69103b2e4", "Product Type");
			this.ProductTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 94, true);
			this.ProductTypeDropEdit.Name = "ProductTypeDropEdit";
			this.ProductTypeDropEdit.PreBoundMaxLength = 3;
			this.ProductTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 18, true);
			this.ProductTypeDropEdit.TabIndex = 3;
			// 
			// IntendedUseCodeDropEdit
			// 
			this.IntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "US_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_IntendedUseCode)));
			this.IntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6dc45f97-4826-4677-9e7b-1e6836472b5e", "Intended Use Code");
			this.IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 44, true);
			this.IntendedUseCodeDropEdit.Name = "IntendedUseCodeDropEdit";
			this.IntendedUseCodeDropEdit.PreBoundMaxLength = 7;
			this.IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 18, true);
			this.IntendedUseCodeDropEdit.TabIndex = 1;
			// 
			// LineNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.LineNoTextBox, "US_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.US.Business.Pesticide)(null)).US_LineNo)));
			this.LineNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6ae7ec3b-fff2-4723-8d38-4457d58e5570", "Line No.");
			this.LineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 19, true);
			this.LineNoTextBox.Name = "LineNoTextBox";
			this.LineNoTextBox.ReadOnly = true;
			this.LineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18, true);
			this.LineNoTextBox.TabIndex = 0;
			// 
			// PSTEditForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("31b31bf9-92f8-4052-b1d4-b7cff23e1023", "Pesticide");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 706, true);
			this.Controls.Add(this.DetailsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.Pesticide);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimizeBox = false;
			this.Name = "PSTEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ExamLocationAddressControl.ResumeLayout(true);
			this.ExamLocationAddressControl.PerformLayout();
			this.ShipperAddressControl.ResumeLayout(true);
			this.ShipperAddressControl.PerformLayout();
			this.NotifyPartyDropEdit.ResumeLayout(true);
			this.NotifyPartyDropEdit.PerformLayout();
			this.CertifyingIndividualDropEdit.ResumeLayout(true);
			this.CertifyingIndividualDropEdit.PerformLayout();
			this.zCalcDropEdit2.ResumeLayout(true);
			this.zCalcDropEdit2.PerformLayout();
			this.UnitsGroupBox.ResumeLayout(false);
			this.UnitsGroupBox.PerformLayout();
			this.NoOfUnit1Field.ResumeLayout(true);
			this.NoOfUnit1Field.PerformLayout();
			this.NoOfUnit2Field.ResumeLayout(true);
			this.NoOfUnit2Field.PerformLayout();
			this.NoOfUnit3Field.ResumeLayout(true);
			this.NoOfUnit3Field.PerformLayout();
			this.NoOfUnit4Field.ResumeLayout(true);
			this.NoOfUnit4Field.PerformLayout();
			this.NoOfUnit5Field.ResumeLayout(true);
			this.NoOfUnit5Field.PerformLayout();
			this.NoOfUnit6Field.ResumeLayout(true);
			this.NoOfUnit6Field.PerformLayout();
			this.UnregReasonDropEdit.ResumeLayout(true);
			this.UnregReasonDropEdit.PerformLayout();
			this.ProductTypeDropEdit.ResumeLayout(true);
			this.ProductTypeDropEdit.PerformLayout();
			this.IntendedUseCodeDropEdit.ResumeLayout(true);
			this.IntendedUseCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox UnitsGroupBox;
		private ZArchitecture.ZTextBox LineNoTextBox;
		private ZArchitecture.GUI.ZDropEdit IntendedUseCodeDropEdit;
		private ZArchitecture.ZTextBox ReasonRemarksTextBox;
		private ZArchitecture.GUI.ZDropEdit UnregReasonDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProductTypeDropEdit;
		private ZArchitecture.ZTextBox BrandNameTextBox;
		private ZArchitecture.ZTextBox ProducerEstNoTextBox;
		private ZArchitecture.ZTextBox LPCONumberTextBox;
		private ZArchitecture.ZTextBox ProducerEstForeignNoTextBox;
		private ZArchitecture.GUI.ZButton OKButton;
		protected ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit2;
		protected ZArchitecture.GUI.ZCalcDropEdit NoOfUnit1Field;
		protected ZArchitecture.GUI.ZCalcDropEdit NoOfUnit2Field;
		protected ZArchitecture.GUI.ZCalcDropEdit NoOfUnit3Field;
		protected ZArchitecture.GUI.ZCalcDropEdit NoOfUnit4Field;
		protected ZArchitecture.GUI.ZCalcDropEdit NoOfUnit5Field;
		protected ZArchitecture.GUI.ZCalcDropEdit NoOfUnit6Field;
		private ZArchitecture.ZLabel PSTQtyTotalLabel;
		private ZArchitecture.GUI.ZDropEdit CertifyingIndividualDropEdit;
		private ZArchitecture.GUI.ZDropEdit NotifyPartyDropEdit;
		private ZArchitecture.GUI.ZCheckBox CBIIndicatorCheckBox;
		private ZArchitecture.ZTextBox ConfidentialityRemarkText;
		private ZArchitecture.GUI.ZCheckBox IsPSTLabelsSentCheckBox;
		private ZArchitecture.GUI.ZAddressControl ShipperAddressControl;
		private ZArchitecture.GUI.ZAddressControl ExamLocationAddressControl;
		private ZArchitecture.ZTextBox IntendedUseDescTextBox;
	}
}