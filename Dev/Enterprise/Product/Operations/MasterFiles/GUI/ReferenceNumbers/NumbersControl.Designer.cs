namespace Enterprise.MasterFiles.GUI
{
	partial class NumbersControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.numbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.numberDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.issueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.entryLineReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.entryNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.entryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numbersGrid)).BeginInit();
			this.numbersGrid.SuspendLayout();
			this.numberDetailsPanel.SuspendLayout();
			this.issueDateEdit.SuspendLayout();
			this.entryTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection);
			// 
			// ReferenceNumbersGroupBox
			// 
			this.ReferenceNumbersGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NumbersControl|e1afede6-362c-4fc2-abf5-515abdcb5112", "Reference Numbers");
			this.ReferenceNumbersGroupBox.Controls.Add(this.numbersGrid);
			this.ReferenceNumbersGroupBox.Controls.Add(this.numberDetailsPanel);
			this.ReferenceNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbersGroupBox.Name = "ReferenceNumbersGroupBox";
			this.ReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 184, true);
			this.ReferenceNumbersGroupBox.TabIndex = 0;
			this.ReferenceNumbersGroupBox.TabStop = false;
			// 
			// numbersGrid
			// 
			this.numbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.numbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).AdditionalReferenceNumberTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryLineReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryIsSystemGenerated)));
			this.numbersGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CE_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.IsVisible = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NumbersControl|8b430b2a-9684-43b5-9e1e-3823aa2d074f", "Type Description");
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
			this.numbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.numbersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.numbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.numbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.numbersGrid.GridId = "d7f2d6aa-d6bd-48d2-bbf0-05b3281d39e0";
			this.numbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.numbersGrid.LayoutKey = "NumbersGrid";
			this.numbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.numbersGrid.Name = "numbersGrid";
			this.numbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 65, true);
			this.numbersGrid.TabIndex = 0;
			// 
			// numberDetailsPanel
			// 
			this.numberDetailsPanel.Controls.Add(this.issueDateEdit);
			this.numberDetailsPanel.Controls.Add(this.entryLineReferenceTextBox);
			this.numberDetailsPanel.Controls.Add(this.entryNumTextBox);
			this.numberDetailsPanel.Controls.Add(this.entryTypeDropEdit);
			this.numberDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.numberDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 69, true);
			this.numberDetailsPanel.Name = "numberDetailsPanel";
			this.numberDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 100, true);
			this.numberDetailsPanel.TabIndex = 5;
			// 
			// issueDateEdit
			// 
			this.issueDateEdit.AllowDrop = true;
			this.issueDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.issueDateEdit.AutoCompleteMonthThreshold = 1;
			this.issueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.issueDateEdit, "CE_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_IssueDate)));
			this.issueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 79, true);
			this.issueDateEdit.Name = "issueDateEdit";
			this.issueDateEdit.TabIndex = 4;
			// 
			// entryLineReferenceTextBox
			// 
			this.entryLineReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.entryLineReferenceTextBox, "CE_EntryLineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryLineReference)));
			this.entryLineReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 55, true);
			this.entryLineReferenceTextBox.Name = "entryLineReferenceTextBox";
			this.entryLineReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.entryLineReferenceTextBox.TabIndex = 3;
			// 
			// entryNumTextBox
			// 
			this.entryNumTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.entryNumTextBox, "CE_EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryNum)));
			this.entryNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 31, true);
			this.entryNumTextBox.Name = "entryNumTextBox";
			this.entryNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.entryNumTextBox.TabIndex = 2;
			// 
			// entryTypeDropEdit
			// 
			this.entryTypeDropEdit.AllowDrop = true;
			this.entryTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.entryTypeDropEdit, "CE_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Integration.Customs.ICusEntryNumber)(null)).CE_EntryType)));
			this.entryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 5, true);
			this.entryTypeDropEdit.Name = "entryTypeDropEdit";
			this.entryTypeDropEdit.PreBoundMaxLength = 3;
			this.entryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.entryTypeDropEdit.TabIndex = 1;
			// 
			// NumbersControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumbersGroupBox);
			this.Name = "NumbersControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 184, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumbersGroupBox.ResumeLayout(false);
			this.ReferenceNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numbersGrid)).EndInit();
			this.numbersGrid.ResumeLayout(false);
			this.numbersGrid.PerformLayout();
			this.numberDetailsPanel.ResumeLayout(false);
			this.numberDetailsPanel.PerformLayout();
			this.issueDateEdit.ResumeLayout(true);
			this.issueDateEdit.PerformLayout();
			this.entryTypeDropEdit.ResumeLayout(true);
			this.entryTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ReferenceNumbersGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit issueDateEdit;
		private Enterprise.ZArchitecture.ZTextBox entryLineReferenceTextBox;
		private Enterprise.ZArchitecture.ZTextBox entryNumTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit entryTypeDropEdit;
		private Enterprise.ZArchitecture.ZGrid numbersGrid;
		private ZArchitecture.GUI.ZPanel numberDetailsPanel;

	}
}
