namespace Enterprise.Customs.ZA.GUI
{
	partial class VAT404FilterUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ImportersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LocalReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LocalReferenceNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReceiptNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReceiptNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.DateRangeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImportersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportersGrid)).BeginInit();
			this.ImportersGrid.SuspendLayout();
			this.LocalReferenceNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocalReferenceNumbersGrid)).BeginInit();
			this.LocalReferenceNumbersGrid.SuspendLayout();
			this.ReceiptNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptNumbersGrid)).BeginInit();
			this.ReceiptNumbersGrid.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.DateRangeGroupBox.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.VAT404DocumentInstruction);
			// 
			// ImportersGroupBox
			// 
			this.ImportersGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("cde4fe62-6b4e-480d-a227-d0dc0b8c54b2", "Importers");
			this.ImportersGroupBox.Controls.Add(this.ImportersGrid);
			this.ImportersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 94, true);
			this.ImportersGroupBox.Name = "ImportersGroupBox";
			this.ImportersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 372, true);
			this.ImportersGroupBox.TabIndex = 2;
			this.ImportersGroupBox.TabStop = false;
			// 
			// ImportersGrid
			// 
			this.ImportersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ImportersGrid, "ImporterForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).ImporterForFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.ImporterHolder)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).ImporterForFilter)).SyncRoot)).ImporterPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.ImporterHolder)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).ImporterForFilter)).SyncRoot)).Importer.OH_FullName)));
			this.ImportersGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ImporterPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a7974552-b79e-4947-8934-46599c636cdd", "Importer Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Importer+OH_FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			this.ImportersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ImportersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ImportersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportersGrid.GridId = "25c2cbba-770b-4fd4-89cd-7ac455c08534";
			this.ImportersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportersGrid.LayoutKey = "ImportersGrid";
			this.ImportersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ImportersGrid.Name = "ImportersGrid";
			this.ImportersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 356, true);
			this.ImportersGrid.TabIndex = 0;
			// 
			// LocalReferenceNumbersGroupBox
			// 
			this.LocalReferenceNumbersGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2d3c704d-7917-4e36-abef-5ef1a70e21cd", "Local Reference Numbers");
			this.LocalReferenceNumbersGroupBox.Controls.Add(this.LocalReferenceNumbersGrid);
			this.LocalReferenceNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 94, true);
			this.LocalReferenceNumbersGroupBox.Name = "LocalReferenceNumbersGroupBox";
			this.LocalReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 372, true);
			this.LocalReferenceNumbersGroupBox.TabIndex = 3;
			this.LocalReferenceNumbersGroupBox.TabStop = false;
			// 
			// LocalReferenceNumbersGrid
			// 
			this.LocalReferenceNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocalReferenceNumbersGrid, "LocalReferenceNumbersForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).LocalReferenceNumbersForFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.LocalReferenceNumberHolder)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).LocalReferenceNumbersForFilter)).SyncRoot)).LocalReferenceNumber)));
			this.LocalReferenceNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "LocalReferenceNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			this.LocalReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LocalReferenceNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalReferenceNumbersGrid.GridId = "25c2cbba-770b-4fd4-89cd-7ac455c08534";
			this.LocalReferenceNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocalReferenceNumbersGrid.LayoutKey = "zGrid1";
			this.LocalReferenceNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.LocalReferenceNumbersGrid.Name = "LocalReferenceNumbersGrid";
			this.LocalReferenceNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 356, true);
			this.LocalReferenceNumbersGrid.TabIndex = 0;
			// 
			// ReceiptNumbersGroupBox
			// 
			this.ReceiptNumbersGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a7a35404-f177-49ac-a837-158f2a4ef334", "Receipt Numbers");
			this.ReceiptNumbersGroupBox.Controls.Add(this.ReceiptNumbersGrid);
			this.ReceiptNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiptNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 94, true);
			this.ReceiptNumbersGroupBox.Name = "ReceiptNumbersGroupBox";
			this.ReceiptNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 372, true);
			this.ReceiptNumbersGroupBox.TabIndex = 4;
			this.ReceiptNumbersGroupBox.TabStop = false;
			// 
			// ReceiptNumbersGrid
			// 
			this.ReceiptNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReceiptNumbersGrid, "ReceiptNumbersForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).ReceiptNumbersForFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.ReceiptNumberHolder)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).ReceiptNumbersForFilter)).SyncRoot)).ReceiptNumber)));
			this.ReceiptNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "ReceiptNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(154);
			this.ReceiptNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReceiptNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiptNumbersGrid.GridId = "25c2cbba-770b-4fd4-89cd-7ac455c08534";
			this.ReceiptNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceiptNumbersGrid.LayoutKey = "zGrid1";
			this.ReceiptNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ReceiptNumbersGrid.Name = "ReceiptNumbersGrid";
			this.ReceiptNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 356, true);
			this.ReceiptNumbersGrid.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.Controls.Add(this.zLabel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ImportersGroupBox, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.ReceiptNumbersGroupBox, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.LocalReferenceNumbersGroupBox, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.DateRangeGroupBox, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(46)));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(46)));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 469, true);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("86c10310-4d36-4853-b9b9-36693fa8442f", "Please specify the corresponding Importer, Local Reference Number or Receipt Numbers in below grids if you want to deliver Proof of Payments for specific records. Otherwise, all eligible records for the specified transaction dates will be printed");
			this.tableLayoutPanel1.SetColumnSpan(this.zLabel1, 3);
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 46, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 46, true);
			this.zLabel1.TabIndex = 1;
			// 
			// DateRangeGroupBox
			// 
			this.DateRangeGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("38914fac-944d-4d43-be7a-e768ff19ab9d", "Payment Date Range");
			this.tableLayoutPanel1.SetColumnSpan(this.DateRangeGroupBox, 3);
			this.DateRangeGroupBox.Controls.Add(this.zDateEdit2);
			this.DateRangeGroupBox.Controls.Add(this.zDateEdit1);
			this.DateRangeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DateRangeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DateRangeGroupBox.Name = "DateRangeGroupBox";
			this.DateRangeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 41, true);
			this.DateRangeGroupBox.TabIndex = 0;
			this.DateRangeGroupBox.TabStop = false;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).EndDate)));
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 16, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 1;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).StartDate)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 16, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 0;
			// 
			// VAT404FilterUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "VAT404FilterUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 469, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImportersGroupBox.ResumeLayout(false);
			this.ImportersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportersGrid)).EndInit();
			this.ImportersGrid.ResumeLayout(false);
			this.ImportersGrid.PerformLayout();
			this.LocalReferenceNumbersGroupBox.ResumeLayout(false);
			this.LocalReferenceNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocalReferenceNumbersGrid)).EndInit();
			this.LocalReferenceNumbersGrid.ResumeLayout(false);
			this.LocalReferenceNumbersGrid.PerformLayout();
			this.ReceiptNumbersGroupBox.ResumeLayout(false);
			this.ReceiptNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptNumbersGrid)).EndInit();
			this.ReceiptNumbersGrid.ResumeLayout(false);
			this.ReceiptNumbersGrid.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.DateRangeGroupBox.ResumeLayout(false);
			this.DateRangeGroupBox.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ImportersGroupBox;
		private ZArchitecture.ZGrid ImportersGrid;
		private ZArchitecture.GUI.ZGroupBox LocalReferenceNumbersGroupBox;
		private ZArchitecture.ZGrid LocalReferenceNumbersGrid;
		private ZArchitecture.GUI.ZGroupBox ReceiptNumbersGroupBox;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel1;
		private ZArchitecture.ZGrid ReceiptNumbersGrid;
		private ZArchitecture.GUI.ZGroupBox DateRangeGroupBox;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private ZArchitecture.GUI.ZDateEdit zDateEdit1;
	}
}
