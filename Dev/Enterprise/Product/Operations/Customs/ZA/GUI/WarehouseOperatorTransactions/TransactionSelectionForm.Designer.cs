namespace Enterprise.Customs.ZA.GUI
{
	partial class TransactionSelectionForm
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SelectAllCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransactionsDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.FormOkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FormCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsDisplayGrid)).BeginInit();
			this.TransactionsDisplayGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 437, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.TransactionSelectionController);
			// 
			// SelectAllCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SelectAllCheckBox, "SelectAll");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).SelectAll)));
			this.SelectAllCheckBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b8016a7a-5a59-48d3-b586-af07621eccd2", "Select All");
			this.SelectAllCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectAllCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectAllCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectAllCheckBox.Name = "SelectAllCheckBox";
			this.SelectAllCheckBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, 3, 0, 3, true);
			this.SelectAllCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 23, true);
			this.SelectAllCheckBox.TabIndex = 0;
			this.SelectAllCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransactionsDisplayGrid
			// 
			this.TransactionsDisplayGrid.AllowNavigation = false;
			this.TransactionsDisplayGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionsDisplayGrid, "Records");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).Select)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).ExportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).OwnerReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).BatchLineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.TransactionSelectionController)(null)).Records)).SyncRoot)).LineReference)));
			this.TransactionsDisplayGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Select";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDateEditColumnStyleInfo1.ColumnName = "TransactionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.ColumnName = "ExportType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo3.ColumnName = "OwnerReference";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "ProductCode";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BatchLineNo";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo5.ColumnName = "LineReference";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransactionsDisplayGrid.GridId = "4e3324b8-56cf-4dd1-b69b-98e529f3ba59";
			this.TransactionsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionsDisplayGrid.LayoutKey = "TransactionsDisplayGrid";
			this.TransactionsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.TransactionsDisplayGrid.Name = "TransactionsDisplayGrid";
			this.TransactionsDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.TransactionsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 380, true);
			this.TransactionsDisplayGrid.TabIndex = 0;
			// 
			// FormOkButton
			// 
			this.FormOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FormOkButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e5ac4186-c9b3-4f75-b62c-d1ed1aafc51d", "OK");
			this.FormOkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(573, 409, true);
			this.FormOkButton.Name = "FormOkButton";
			this.FormOkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FormOkButton.TabIndex = 2;
			this.FormOkButton.ToolTipCaption = null;
			this.FormOkButton.UseVisualStyleBackColor = true;
			this.FormOkButton.Click += new System.EventHandler(this.FormOkButton_Click);
			// 
			// FormCancelButton
			// 
			this.FormCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FormCancelButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("0b142bf5-30e4-4de2-905f-1d6ff13a78bb", "Cancel");
			this.FormCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.FormCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(649, 409, true);
			this.FormCancelButton.Name = "FormCancelButton";
			this.FormCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FormCancelButton.TabIndex = 3;
			this.FormCancelButton.ToolTipCaption = null;
			this.FormCancelButton.UseVisualStyleBackColor = true;
			// 
			// TopPanel
			// 
			this.TopPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TopPanel.Controls.Add(this.SelectAllCheckBox);
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 23, true);
			this.TopPanel.TabIndex = 1;
			// 
			// TransactionSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.FormCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("aea9dabb-b11d-4d58-b057-90fcd4835233", "Select Item");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 461, true);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.FormCancelButton);
			this.Controls.Add(this.FormOkButton);
			this.Controls.Add(this.TransactionsDisplayGrid);
			this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.TransactionSelectionController);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 500, true);
			this.Name = "TransactionSelectionForm";
			this.Resize += new System.EventHandler(this.TransactionSelectionForm_Resize);
			this.Controls.SetChildIndex(this.TransactionsDisplayGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FormOkButton, 0);
			this.Controls.SetChildIndex(this.FormCancelButton, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsDisplayGrid)).EndInit();
			this.TransactionsDisplayGrid.ResumeLayout(false);
			this.TransactionsDisplayGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDisplayGrid TransactionsDisplayGrid;
		private ZArchitecture.GUI.ZCheckBox SelectAllCheckBox;
		private ZArchitecture.GUI.ZButton FormOkButton;
		private ZArchitecture.GUI.ZButton FormCancelButton;
		private ZArchitecture.GUI.ZPanel TopPanel;
	}
}
