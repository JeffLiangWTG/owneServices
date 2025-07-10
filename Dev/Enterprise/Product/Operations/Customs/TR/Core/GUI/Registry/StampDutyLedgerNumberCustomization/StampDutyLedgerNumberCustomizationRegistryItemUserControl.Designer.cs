namespace Enterprise.Customs.TR.GUI
{
	partial class StampDutyLedgerNumberCustomizationRegistryItemUserControl
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
            this.FiscalYearGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ExpiredYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
            this.StartNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.SequenceNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.FiscalYearGroupBox.SuspendLayout();
            this.EndDateEdit.SuspendLayout();
            this.StartDateEdit.SuspendLayout();
            this.SequenceNumberGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.StampDutyLedgerNumberCustomizationRegistrySetting);
            // 
            // FiscalYearGroupBox
            // 
            this.FiscalYearGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FiscalYearGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("f7b4e6d8-9a2b-4432-b5c3-464b75e01277", "Fiscal Year");
            this.FiscalYearGroupBox.Controls.Add(this.EndDateEdit);
            this.FiscalYearGroupBox.Controls.Add(this.StartDateEdit);
            this.FiscalYearGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.FiscalYearGroupBox.Name = "FiscalYearGroupBox";
            this.FiscalYearGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 89, true);
            this.FiscalYearGroupBox.TabIndex = 0;
            this.FiscalYearGroupBox.TabStop = false;
            // 
            // EndDateEdit
            // 
            this.EndDateEdit.AllowDrop = true;
            this.EndDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EndDateEdit, "EndDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.StampDutyLedgerNumberCustomizationRegistrySetting)(null)).EndDate)));
            this.EndDateEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("22083e90-c8d1-4cf1-a9d5-381baa9a8f0a", "End Date");
            this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 54, true);
            this.EndDateEdit.Name = "EndDateEdit";
            this.EndDateEdit.TabIndex = 1;
            // 
            // StartDateEdit
            // 
            this.StartDateEdit.AllowDrop = true;
            this.StartDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.StartDateEdit, "StartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.StampDutyLedgerNumberCustomizationRegistrySetting)(null)).StartDate)));
            this.StartDateEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("9b8e7ba8-72d0-4573-a181-016272018388", "Start Date");
            this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 19, true);
            this.StartDateEdit.Name = "StartDateEdit";
            this.StartDateEdit.TabIndex = 0;
            // 
            // ExpiredYearEdit
            // 
            this.BindingSource.SetBindingMember(this.ExpiredYearEdit, "ExpiredYear");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.StampDutyLedgerNumberCustomizationRegistrySetting)(null)).ExpiredYear)));
            this.ExpiredYearEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("0dc74d9c-1612-4b1f-8570-c22a1776dc89", "Expired End of Fiscal Year");
            this.ExpiredYearEdit.DecimalPlaces = 0;
            this.ExpiredYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 52, true);
            this.ExpiredYearEdit.Name = "ExpiredYearEdit";
            this.ExpiredYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.ExpiredYearEdit.TabIndex = 1;
            this.ExpiredYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // StartNumberCalcEdit
            // 
            this.StartNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.StartNumberCalcEdit, "StartNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.StampDutyLedgerNumberCustomizationRegistrySetting)(null)).StartNumber)));
            this.StartNumberCalcEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("f03fcd3f-713b-4d35-b2e6-d5b6f8bf177c", "Start Number");
            this.StartNumberCalcEdit.DecimalPlaces = 0;
            this.StartNumberCalcEdit.Decimals = 0;
            this.StartNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 19, true);
            this.StartNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.StartNumberCalcEdit.Name = "StartNumberCalcEdit";
            this.StartNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.StartNumberCalcEdit.TabIndex = 0;
            this.StartNumberCalcEdit.Text = "0";
            this.StartNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SequenceNumberGroupBox
            // 
            this.SequenceNumberGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SequenceNumberGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("cdc04aac-531d-44f7-a639-54490df2e310", "Sequence Number");
            this.SequenceNumberGroupBox.Controls.Add(this.StartNumberCalcEdit);
            this.SequenceNumberGroupBox.Controls.Add(this.ExpiredYearEdit);
            this.SequenceNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
            this.SequenceNumberGroupBox.Name = "SequenceNumberGroupBox";
            this.SequenceNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 89, true);
            this.SequenceNumberGroupBox.TabIndex = 1;
            this.SequenceNumberGroupBox.TabStop = false;
			// 
			// StampDutyLedgerNumberCustomizationRegistryItemUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SequenceNumberGroupBox);
            this.Controls.Add(this.FiscalYearGroupBox);
            this.Name = "StampDutyLedgerNumberCustomizationRegistryItemUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 289, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.FiscalYearGroupBox.ResumeLayout(false);
            this.FiscalYearGroupBox.PerformLayout();
            this.EndDateEdit.ResumeLayout(true);
            this.EndDateEdit.PerformLayout();
            this.StartDateEdit.ResumeLayout(true);
            this.StartDateEdit.PerformLayout();
            this.SequenceNumberGroupBox.ResumeLayout(false);
            this.SequenceNumberGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox FiscalYearGroupBox;
		internal ZArchitecture.GUI.ZYearEdit ExpiredYearEdit;
		internal ZArchitecture.ZCalcEdit StartNumberCalcEdit;
		internal ZArchitecture.GUI.ZGroupBox SequenceNumberGroupBox;
		internal ZArchitecture.GUI.ZDateEdit EndDateEdit;
		internal ZArchitecture.GUI.ZDateEdit StartDateEdit;
	}
}
