namespace Enterprise.MasterFiles.GUI
{
	partial class GLJournalExchangeRateTypeControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BalanceSheetAccountTypeExchangeRateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GLJournalExchangeRateType);
			// 
			// GroupBox
			// 
			this.GroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GLJournalExchangeRateTypeControl|AAF5C006-E8D1-4352-AEA5-A02D6F05AE77", "Settings");
			this.GroupBox.Controls.Add(this.BalanceSheetAccountTypeExchangeRateTypeDropEdit);
			this.GroupBox.Controls.Add(this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit);
			this.GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 130, true);
			this.GroupBox.TabIndex = 1;
			this.GroupBox.TabStop = false;
			// 
			// BalanceSheetAccountTypeExchangeRateTypeDropEdit
			//
			this.BindingSource.SetBindingMember(this.BalanceSheetAccountTypeExchangeRateTypeDropEdit, "BalanceSheetAccountTypeExchangeRateType");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLJournalExchangeRateType)(null)).BalanceSheetAccountTypeExchangeRateType)));
			this.BalanceSheetAccountTypeExchangeRateTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GLJournalExchangeRateTypeControl|0B540EEF-A76F-4C68-99D2-ACAA4876DB4C", "Balance Sheet Account Type");
			this.BalanceSheetAccountTypeExchangeRateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 28, true);
			this.BalanceSheetAccountTypeExchangeRateTypeDropEdit.Name = "BalanceSheetAccountTypeExchangeRateTypeDropEdit";
			this.BalanceSheetAccountTypeExchangeRateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.BalanceSheetAccountTypeExchangeRateTypeDropEdit.TabIndex = 2;
			// 
			// ProfitAndLossAccountTypeExchangeRateTypeDropEdit
			//
			this.BindingSource.SetBindingMember(this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit, "ProfitAndLossAccountTypeExchangeRateType");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLJournalExchangeRateType)(null)).ProfitAndLossAccountTypeExchangeRateType)));
			this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GLJournalExchangeRateTypeControl|878798A7-56B7-415D-A795-EB39B21F368F", "Profit and Loss Account Type");
			this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 67, true);
			this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit.Name = "ProfitAndLossAccountTypeExchangeRateTypeDropEdit";
			this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.ProfitAndLossAccountTypeExchangeRateTypeDropEdit.TabIndex = 3;
			// 
			// GLJournalExchangeRateTypeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "GLJournalExchangeRateTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 133, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZArchitecture.GUI.ZGroupBox GroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit BalanceSheetAccountTypeExchangeRateTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ProfitAndLossAccountTypeExchangeRateTypeDropEdit;

		#endregion
	}
}
