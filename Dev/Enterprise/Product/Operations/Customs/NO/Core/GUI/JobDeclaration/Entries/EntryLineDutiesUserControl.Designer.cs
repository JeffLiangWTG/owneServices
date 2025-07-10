namespace Enterprise.Customs.NO.GUI
{
	partial class EntryLineDutiesUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.EntryLineInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.EntryLineDutiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineDutiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryLineExtendedInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ExtendedInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryLineInfoTabControl.SuspendLayout();
			this.EntryLineDutiesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutiesGrid)).BeginInit();
			this.EntryLineDutiesGrid.SuspendLayout();
			this.ExtendedInfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// EntryLineInfoTabControl
			// 
			this.EntryLineInfoTabControl.Controls.Add(this.EntryLineDutiesTabPage);
			this.EntryLineInfoTabControl.Controls.Add(this.EntryLineExtendedInfoTabPage);
			this.EntryLineInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.EntryLineInfoTabControl.Name = "EntryLineInfoTabControl";
			this.EntryLineInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1055, 201, true);
			this.EntryLineInfoTabControl.TabIndex = 3;
			// 
			// EntryLineDutiesTabPage
			// 
			this.EntryLineDutiesTabPage.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("d88c6bfc-576e-4d80-8ff6-853798768758", "Duties and VAT");
			this.EntryLineDutiesTabPage.Controls.Add(this.EntryLineDutiesGrid);
			this.EntryLineDutiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryLineDutiesTabPage.Name = "EntryLineDutiesTabPage";
			this.EntryLineDutiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryLineDutiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1047, 174, true);
			this.EntryLineDutiesTabPage.TabIndex = 0;
			this.EntryLineDutiesTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryLineDutiesGrid
			// 
			this.EntryLineDutiesGrid.AllowNavigation = false;
			this.EntryLineDutiesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EntryLineDutiesGrid, "CustomsEntryHeaders.AllEntryLines.Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_DutyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_RateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).IsLandedCostOnlyAsText)));
			this.EntryLineDutiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CF_DutyCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CF_Sequence";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CF_RateType";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "IsLandedCostOnlyAsText";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLineDutiesGrid.GridId = "6ca6311e-bfe9-4505-87a3-2fea6b1807f7";
			this.EntryLineDutiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutiesGrid.LayoutKey = "EntryLineDutiesGrid";
			this.EntryLineDutiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineDutiesGrid.Name = "EntryLineDutiesGrid";
			this.EntryLineDutiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 168, true);
			this.EntryLineDutiesGrid.TabIndex = 0;
			// 
			// EntryLineExtendedInfoTabPage
			// 
			this.EntryLineExtendedInfoTabPage.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("69DC8ECB-8934-478B-8886-C5DEE49323D0", "Extended Information");
			this.EntryLineExtendedInfoTabPage.Controls.Add(this.ExtendedInfoGroupBox);
			this.EntryLineExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryLineExtendedInfoTabPage.Name = "EntryLineExtendedInfoTabPage";
			this.EntryLineExtendedInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryLineExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1047, 174, true);
			this.EntryLineExtendedInfoTabPage.TabIndex = 1;
			this.EntryLineExtendedInfoTabPage.UseVisualStyleBackColor = true;
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("e7cf7978-52a9-4406-9aba-0708173a1075", "Extended Information");
			this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodeTextBox);
			this.ExtendedInfoGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ExtendedInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendedInfoGroupBox.Name = "ExtendedInfoGroupBox";
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 293, true);
			this.ExtendedInfoGroupBox.TabIndex = 2;
			this.ExtendedInfoGroupBox.TabStop = false;
			// 
			// TariffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
			this.TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("75c19559-4551-4100-a0b7-c57110243c0b", "Tariff Code:");
			this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
			this.TariffCodeTextBox.Name = "TariffCodeTextBox";
			this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.TariffCodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("3ce3f3ed-fe10-4799-9a29-32699f04574b", "Description:");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 41, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 137, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// EntryLineDutiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.EntryLineInfoTabControl);
			this.Name = "EntryLineDutiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1061, 205, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			this.EntryLineInfoTabControl.ResumeLayout(false);
			this.EntryLineInfoTabControl.PerformLayout();
			this.EntryLineDutiesTabPage.ResumeLayout(false);
			this.EntryLineDutiesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutiesGrid)).EndInit();
			this.EntryLineDutiesGrid.ResumeLayout(false);
			this.EntryLineDutiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZTabControl EntryLineInfoTabControl;
		public ZArchitecture.GUI.ZTabPage EntryLineDutiesTabPage;
		public ZArchitecture.ZGrid EntryLineDutiesGrid;
		public ZArchitecture.GUI.ZTabPage EntryLineExtendedInfoTabPage;
		private ZArchitecture.GUI.ZGroupBox ExtendedInfoGroupBox;
		private ZArchitecture.ZTextBox TariffCodeTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
