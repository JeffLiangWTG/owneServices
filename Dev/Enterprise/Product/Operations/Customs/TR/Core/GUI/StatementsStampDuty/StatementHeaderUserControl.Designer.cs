using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	partial class StatementHeaderUserControl
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
            this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.StatementNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PaymentTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PrintDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.StatementStatusZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PaymentStatusZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PaymentPartyZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TotalChargeAmountZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DetailsGroupBox.SuspendLayout();
            this.PaymentTypeZDropEdit.SuspendLayout();
            this.PrintDateZDateEdit.SuspendLayout();
            this.StatementStatusZDropEdit.SuspendLayout();
            this.PaymentStatusZDropEdit.SuspendLayout();
            this.PaymentPartyZDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.CusStatementHeader);
            // 
            // DetailsGroupBox
            // 
            this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("4A3457E4-F0E1-4CDC-9A91-DD932C8B150D", "Details");
            this.DetailsGroupBox.Controls.Add(this.StatementNumberZTextBox);
            this.DetailsGroupBox.Controls.Add(this.PaymentTypeZDropEdit);
            this.DetailsGroupBox.Controls.Add(this.PrintDateZDateEdit);
            this.DetailsGroupBox.Controls.Add(this.StatementStatusZDropEdit);
            this.DetailsGroupBox.Controls.Add(this.PaymentStatusZDropEdit);
            this.DetailsGroupBox.Controls.Add(this.PaymentPartyZDropEdit);
            this.DetailsGroupBox.Controls.Add(this.TotalChargeAmountZCalcEdit);
            this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsGroupBox.Name = "DetailsGroupBox";
            this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 106, true);
            this.DetailsGroupBox.TabIndex = 0;
            this.DetailsGroupBox.TabStop = false;
            // 
            // StatementNumberZTextBox
            // 
            this.BindingSource.SetBindingMember(this.StatementNumberZTextBox, "B2_StatementNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).B2_StatementNumber)));
            this.StatementNumberZTextBox.CaptionResourceString = null;
            this.StatementNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 20, true);
            this.StatementNumberZTextBox.Name = "StatementNumberZTextBox";
            this.StatementNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 18, true);
            this.StatementNumberZTextBox.TabIndex = 0;
            this.StatementNumberZTextBox.TabStop = false;
            // 
            // PaymentTypeZDropEdit
            // 
            this.PaymentTypeZDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentTypeZDropEdit, "B2_PaymentType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).B2_PaymentType)));
            this.PaymentTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 40, true);
            this.PaymentTypeZDropEdit.Name = "PaymentTypeZDropEdit";
            this.PaymentTypeZDropEdit.PreBoundMaxLength = 1;
            this.PaymentTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 18, true);
            this.PaymentTypeZDropEdit.TabIndex = 1;
            // 
            // PrintDateZDateEdit
            // 
            this.PrintDateZDateEdit.AllowDrop = true;
            this.PrintDateZDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.PrintDateZDateEdit, "B2_PrintDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).B2_PrintDate)));
            this.PrintDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 60, true);
            this.PrintDateZDateEdit.Name = "PrintDateZDateEdit";
            this.PrintDateZDateEdit.TabIndex = 2;
            // 
            // StatementStatusZDropEdit
            // 
            this.StatementStatusZDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatementStatusZDropEdit, "B2_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).B2_Status)));
            this.StatementStatusZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 20, true);
            this.StatementStatusZDropEdit.Name = "StatementStatusZDropEdit";
            this.StatementStatusZDropEdit.PreBoundMaxLength = 2;
            this.StatementStatusZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
            this.StatementStatusZDropEdit.TabIndex = 3;
            // 
            // PaymentStatusZDropEdit
            // 
            this.PaymentStatusZDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentStatusZDropEdit, "B2_PaymentStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).B2_PaymentStatus)));
            this.PaymentStatusZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 40, true);
            this.PaymentStatusZDropEdit.Name = "PaymentStatusZDropEdit";
            this.PaymentStatusZDropEdit.PreBoundMaxLength = 2;
            this.PaymentStatusZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
            this.PaymentStatusZDropEdit.TabIndex = 4;
            // 
            // PaymentPartyZDropEdit
            // 
            this.PaymentPartyZDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentPartyZDropEdit, "B2_PaymentParty");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).B2_PaymentParty)));
            this.PaymentPartyZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 60, true);
            this.PaymentPartyZDropEdit.Name = "PaymentPartyZDropEdit";
            this.PaymentPartyZDropEdit.PreBoundMaxLength = 2;
            this.PaymentPartyZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
            this.PaymentPartyZDropEdit.TabIndex = 5;
            // 
            // TotalChargeAmountZCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalChargeAmountZCalcEdit, "TotalChargeAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.CusStatementHeader)(null)).TotalChargeAmount)));
            this.TotalChargeAmountZCalcEdit.CaptionResourceString = null;
            this.TotalChargeAmountZCalcEdit.DecimalPlaces = 2;
            this.TotalChargeAmountZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 80, true);
            this.TotalChargeAmountZCalcEdit.Name = "TotalChargeAmountZCalcEdit";
            this.TotalChargeAmountZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
            this.TotalChargeAmountZCalcEdit.TabIndex = 6;
            this.TotalChargeAmountZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // StatementHeaderUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.DetailsGroupBox);
            this.Name = "StatementHeaderUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 107, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DetailsGroupBox.ResumeLayout(false);
            this.DetailsGroupBox.PerformLayout();
            this.PaymentTypeZDropEdit.ResumeLayout(true);
            this.PaymentTypeZDropEdit.PerformLayout();
            this.PrintDateZDateEdit.ResumeLayout(true);
            this.PrintDateZDateEdit.PerformLayout();
            this.StatementStatusZDropEdit.ResumeLayout(true);
            this.StatementStatusZDropEdit.PerformLayout();
            this.PaymentStatusZDropEdit.ResumeLayout(true);
            this.PaymentStatusZDropEdit.PerformLayout();
            this.PaymentPartyZDropEdit.ResumeLayout(true);
            this.PaymentPartyZDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.ZTextBox StatementNumberZTextBox;
		private ZArchitecture.GUI.ZDropEdit PaymentTypeZDropEdit;
		private ZArchitecture.GUI.ZDateEdit PrintDateZDateEdit;
		private ZArchitecture.GUI.ZDropEdit StatementStatusZDropEdit;
		private ZArchitecture.GUI.ZDropEdit PaymentStatusZDropEdit;
		private ZArchitecture.GUI.ZDropEdit PaymentPartyZDropEdit;
		private ZArchitecture.ZCalcEdit TotalChargeAmountZCalcEdit;
	}
}
