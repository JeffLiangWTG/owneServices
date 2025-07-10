namespace Enterprise.Customs.TW.GUI
{
	partial class EntryLineAdditionalDataUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EntryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryLineDutyAndTaxGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
			this.EntryLineDutyAndTaxGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// EntryLineDutyAndTaxGroupBox
			// 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("5ce8da1f-c6bc-4f8e-bd57-1e6641421cb6", "Duties, Taxes and Fees");
			this.EntryLineDutyAndTaxGroupBox.Controls.Add(this.EntryLineDutyAndTaxGrid);
			this.EntryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineDutyAndTaxGroupBox, true);
			this.EntryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.EntryLineDutyAndTaxGroupBox.Name = "EntryLineDutyAndTaxGroupBox";
			this.EntryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 173, true);
			this.EntryLineDutyAndTaxGroupBox.TabIndex = 5;
			this.EntryLineDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineDutyAndTaxGrid
			// 
			this.EntryLineDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineDutyAndTaxGrid, "CustomsEntryHeaders.MergedLines.Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_RateOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_BaseAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_RateDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_RateSuspension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLines)).SyncRoot)).Fees)).SyncRoot)).TW_MethodOfPayment)));
			this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TW_Type";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("a77dcba7-557b-47fb-b483-fee350c22dc3", "Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TW_TypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("a77dcba7-557b-47fb-b483-fee350c22dc3", "Type");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.ColumnName = "TW_RateOverride";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TW_BaseAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "TW_RateDuty";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "TW_RateSuspension";
			zTextBoxColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "TW_Amount";
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "TW_MethodOfPayment";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineDutyAndTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
			this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 156, true);
			this.EntryLineDutyAndTaxGrid.TabIndex = 0;
			// 
			// EntryLineAdditionalDataUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryLineDutyAndTaxGroupBox);
			this.Name = "EntryLineAdditionalDataUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 0, true);
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 175, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryLineDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineDutyAndTaxGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
			this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineDutyAndTaxGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox EntryLineDutyAndTaxGroupBox;
		protected ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
	}
}
