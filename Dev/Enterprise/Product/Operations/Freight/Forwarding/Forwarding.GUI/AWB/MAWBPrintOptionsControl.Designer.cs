namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class MAWBPrintOptionsControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PrintOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MAWBUseEPrintCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CarrierAWBRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.LaserAWBRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NeutralAWBRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PrintedDateTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrintedDateContentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrintAWBCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrintConsignmentSecurityDeclarationCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.PrinterDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrintOptionsGroupBox.SuspendLayout();
			this.PrinterDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB);
			// 
			// PrintOptionsGroupBox
			// 
			this.PrintOptionsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("6683d2f5-0b9f-442b-8edc-7c53805c321a", "Master Air Waybill Print Options");
			this.PrintOptionsGroupBox.Controls.Add(this.MAWBUseEPrintCheckBox);
			this.PrintOptionsGroupBox.Controls.Add(this.CarrierAWBRadioButton);
			this.PrintOptionsGroupBox.Controls.Add(this.LaserAWBRadioButton);
			this.PrintOptionsGroupBox.Controls.Add(this.NeutralAWBRadioButton);
			this.PrintOptionsGroupBox.Controls.Add(this.PrintedDateTitleLabel);
			this.PrintOptionsGroupBox.Controls.Add(this.PrintedDateContentLabel);
			this.PrintOptionsGroupBox.Controls.Add(this.PrintAWBCheckBox);
			this.PrintOptionsGroupBox.Controls.Add(this.PrintConsignmentSecurityDeclarationCheckBox);
			this.PrintOptionsGroupBox.Controls.Add(this.PrinterDropEdit);
			this.PrintOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PrintOptionsGroupBox.Name = "PrintOptionsGroupBox";
			this.PrintOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 142, true);
			this.PrintOptionsGroupBox.TabIndex = 2;
			this.PrintOptionsGroupBox.TabStop = false;
			// 
			// MAWBUseEPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBUseEPrintCheckBox, "MAWBUseEPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).MAWBUseEPrint)));
			this.MAWBUseEPrintCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("a653aa1a-b4bd-408e-a880-20d03850f605", "ePrint");
			this.MAWBUseEPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MAWBUseEPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 112, true);
			this.MAWBUseEPrintCheckBox.Name = "MAWBUseEPrintCheckBox";
			this.MAWBUseEPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 24, true);
			this.MAWBUseEPrintCheckBox.TabIndex = 24;
			// 
			// CarrierAWBRadioButton
			// 
			this.CarrierAWBRadioButton.AutoCheck = false;
			this.CarrierAWBRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CarrierAWBRadioButton, "CarrierAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).CarrierAWB)));
			this.CarrierAWBRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("56ee4b93-97e9-483a-a8fb-05f9b6d06689", "Carrier", "Carrier AWB", "Should print a Carrier AWB.");
			this.CarrierAWBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CarrierAWBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 57, true);
			this.CarrierAWBRadioButton.Name = "CarrierAWBRadioButton";
			this.CarrierAWBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.CarrierAWBRadioButton.TabIndex = 2;
			// 
			// LaserAWBRadioButton
			// 
			this.LaserAWBRadioButton.AutoCheck = false;
			this.LaserAWBRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LaserAWBRadioButton, "LaserAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).LaserAWB)));
			this.LaserAWBRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4e7abd05-ca1f-4b94-a5b8-20e71dd46fcb", "Laser", "Laser AWB", "Should print a Laser AWB.");
			this.LaserAWBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LaserAWBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 57, true);
			this.LaserAWBRadioButton.Name = "LaserAWBRadioButton";
			this.LaserAWBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.LaserAWBRadioButton.TabIndex = 3;
			// 
			// NeutralAWBRadioButton
			// 
			this.NeutralAWBRadioButton.AutoCheck = false;
			this.NeutralAWBRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NeutralAWBRadioButton, "NeutralAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).NeutralAWB)));
			this.NeutralAWBRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("fd362fa3-90e8-4210-8a80-c067a96dab71", "Neutral", "Neutral AWB", "Should print a Neutral AWB.");
			this.NeutralAWBRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NeutralAWBRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 57, true);
			this.NeutralAWBRadioButton.Name = "NeutralAWBRadioButton";
			this.NeutralAWBRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.NeutralAWBRadioButton.TabIndex = 1;
			// 
			// PrintedDateTitleLabel
			// 
			this.PrintedDateTitleLabel.AutoSize = true;
			this.PrintedDateTitleLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c18e84a8-d680-4a1e-ab1d-bfeb0803b40a", "Printed Date:");
			this.PrintedDateTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 27, true);
			this.PrintedDateTitleLabel.Name = "PrintedDateTitleLabel";
			this.PrintedDateTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.PrintedDateTitleLabel.TabIndex = 7;
			// 
			// PrintedDateContentLabel
			// 
			this.PrintedDateContentLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintedDateContentLabel, "DatePrinted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).DatePrinted)));
			this.PrintedDateContentLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3d4df86f-4d11-471a-b7ed-c955fc6db9cb", "Date");
			this.PrintedDateContentLabel.IsFontBold = true;
			this.PrintedDateContentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 27, true);
			this.PrintedDateContentLabel.Name = "PrintedDateContentLabel";
			this.PrintedDateContentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 13, true);
			this.PrintedDateContentLabel.TabIndex = 8;
			// 
			// PrintAWBCheckBox
			// 
			this.PrintAWBCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintAWBCheckBox, "PrintMasterAirWaybill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).PrintMasterAirWaybill)));
			this.PrintAWBCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b4572614-d25d-465a-9e09-60bed48db630", "Print Master Air Waybill", "Specifies whether the Master Air Waybill should be printed.");
			this.PrintAWBCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintAWBCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 27, true);
			this.PrintAWBCheckBox.Name = "PrintAWBCheckBox";
			this.PrintAWBCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
			this.PrintAWBCheckBox.TabIndex = 0;
			// 
			// PrintConsignmentSecurityDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PrintConsignmentSecurityDeclarationCheckBox, "PrintConsignmentSecurityDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).PrintConsignmentSecurityDeclaration)));
			this.PrintConsignmentSecurityDeclarationCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("314bec7b-fbfd-410a-a108-f6ae6a8b804c", "Print CSD", "Print Consignment Security Declaration", "Specifies whether the Consignment Security Declaration should be printed.");
			this.PrintConsignmentSecurityDeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintConsignmentSecurityDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 112, true);
			this.PrintConsignmentSecurityDeclarationCheckBox.Name = "PrintConsignmentSecurityDeclarationCheckBox";
			this.PrintConsignmentSecurityDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 24, true);
			this.PrintConsignmentSecurityDeclarationCheckBox.TabIndex = 25;
			// 
			// PrinterDropEdit
			// 
			this.PrinterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrinterDropEdit, "MAWBPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.IPrintMAWB)(null)).MAWBPrinter)));
			this.PrinterDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("408328d2-ca3e-4599-8f24-b1a4159d2d97", "Printer", "The printer to use when printing the Master Air Waybill.");
			this.PrinterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 89, true);
			this.PrinterDropEdit.Name = "PrinterDropEdit";
			this.PrinterDropEdit.PreBoundMaxLength = 52;
			this.PrinterDropEdit.ShowDescriptionBox = false;
			this.PrinterDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PrinterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.PrinterDropEdit.TabIndex = 4;
			// 
			// MAWBPrintOptionsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrintOptionsGroupBox);
			this.Name = "MAWBPrintOptionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 148, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrintOptionsGroupBox.ResumeLayout(false);
			this.PrintOptionsGroupBox.PerformLayout();
			this.PrinterDropEdit.ResumeLayout(true);
			this.PrinterDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PrintOptionsGroupBox;
		private ZArchitecture.GUI.ZGuidDropEdit PrinterDropEdit;
		private ZArchitecture.GUI.ZRadioButton CarrierAWBRadioButton;
		private ZArchitecture.GUI.ZRadioButton LaserAWBRadioButton;
		private ZArchitecture.GUI.ZRadioButton NeutralAWBRadioButton;
		private ZArchitecture.ZLabel PrintedDateTitleLabel;
		private ZArchitecture.ZLabel PrintedDateContentLabel;
		private ZArchitecture.GUI.ZCheckBox PrintAWBCheckBox;
		private ZArchitecture.GUI.ZCheckBox PrintConsignmentSecurityDeclarationCheckBox;
		private ZArchitecture.GUI.ZCheckBox MAWBUseEPrintCheckBox;
	}
}
