namespace Enterprise.Customs.GUI
{
	partial class NumbersUserControl
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
			CustomsReferenceDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new CustomsReferenceDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DateEditIssued = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TextBoxInfo = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.DropEditType = new CustomsReferenceDropEdit();
			this.NumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumbersGroupBox.SuspendLayout();
			this.DateEditIssued.SuspendLayout();
			this.DropEditType.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).BeginInit();
			this.NumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// ReferenceNumbersGroupBox
			// 
			this.ReferenceNumbersGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E064DDD3-C584-4AD7-8DAE-D2C0B9C62A9A", "Reference Numbers");
			this.ReferenceNumbersGroupBox.Controls.Add(this.DateEditIssued);
			this.ReferenceNumbersGroupBox.Controls.Add(this.TextBoxInfo);
			this.ReferenceNumbersGroupBox.Controls.Add(this.TextBoxNumber);
			this.ReferenceNumbersGroupBox.Controls.Add(this.DropEditType);
			this.ReferenceNumbersGroupBox.Controls.Add(this.NumbersGrid);
			this.ReferenceNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbersGroupBox.Name = "ReferenceNumbersGroupBox";
			this.ReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			this.ReferenceNumbersGroupBox.TabIndex = 0;
			this.ReferenceNumbersGroupBox.TabStop = false;
			// 
			// DateEditIssued
			// 
			this.DateEditIssued.AllowDrop = true;
			this.DateEditIssued.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DateEditIssued.AutoCompleteMonthThreshold = 1;
			this.DateEditIssued.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditIssued, "AdditionalReferenceNumbers.CE_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_IssueDate)));
			this.DateEditIssued.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 284, true);
			this.DateEditIssued.Name = "DateEditIssued";
			this.DateEditIssued.TabIndex = 4;
			// 
			// TextBoxInfo
			// 
			this.TextBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxInfo, "AdditionalReferenceNumbers.CE_EntryLineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryLineReference)));
			this.TextBoxInfo.CaptionResourceString = null;
			this.TextBoxInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 262, true);
			this.TextBoxInfo.Name = "TextBoxInfo";
			this.TextBoxInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.TextBoxInfo.TabIndex = 3;
			// 
			// TextBoxNumber
			// 
			this.TextBoxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxNumber, "AdditionalReferenceNumbers.CE_EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryNum)));
			this.TextBoxNumber.CaptionResourceString = null;
			this.TextBoxNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 240, true);
			this.TextBoxNumber.Name = "TextBoxNumber";
			this.TextBoxNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.TextBoxNumber.TabIndex = 2;
			// 
			// DropEditType
			// 
			this.DropEditType.AllowDrop = true;
			this.DropEditType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DropEditType, "AdditionalReferenceNumbers.CE_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).Lookups.AdditionalReferenceNumberTypes)));
			this.DropEditType.BindToList = "AdditionalReferenceNumbers.Lookups+AdditionalReferenceNumberTypes";
			this.DropEditType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 219, true);
			this.DropEditType.Name = "DropEditType";
			this.DropEditType.ShouldResizeByMaxLength = true;
			this.DropEditType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.DropEditType.TabIndex = 1;

			// 
			// NumbersGrid
			// 
			this.NumbersGrid.AllowNavigation = false;
			this.NumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NumbersGrid, "AdditionalReferenceNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).Lookups.AdditionalReferenceNumberTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).AdditionalReferenceNumberTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_EntryLineReference)));
			this.NumbersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.AdditionalReferenceNumberTypes";
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("NumbersUserControl|afc672c7-3236-4764-ae25-0d783c594454", "Description", "Indicates the full description of the Reference Type entered.");
			zTextBoxColumnStyleInfo1.ColumnName = "AdditionalReferenceNumberTypeDescription";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.ColumnName = "CE_IssueDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CE_EntryLineReference";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.ToolTip = "Additional Information related to this specific number.";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.NumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NumbersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NumbersGrid.GridId = "bb30e775-b325-4d67-9888-abd533d6b12c";
			this.NumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NumbersGrid.LayoutKey = "NumbersGrid";
			this.NumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NumbersGrid.Name = "NumbersGrid";
			this.NumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 200, true);
			this.NumbersGrid.TabIndex = 0;
			// 
			// NumbersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumbersGroupBox);
			this.Name = "NumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumbersGroupBox.ResumeLayout(false);
			this.ReferenceNumbersGroupBox.PerformLayout();
			this.DateEditIssued.ResumeLayout(true);
			this.DateEditIssued.PerformLayout();
			this.DropEditType.ResumeLayout(true);
			this.DropEditType.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).EndInit();
			this.NumbersGrid.ResumeLayout(false);
			this.NumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox ReferenceNumbersGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit DateEditIssued;
		protected Enterprise.ZArchitecture.ZTextBox TextBoxInfo;
		protected Enterprise.ZArchitecture.ZTextBox TextBoxNumber;
		protected CustomsReferenceDropEdit DropEditType;
		public Enterprise.ZArchitecture.ZGrid NumbersGrid;


	}
}
