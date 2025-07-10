using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Module
{
	partial class ExportOperationalActionUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SelectAllCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransactionsDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.LockInfoLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsDisplayGrid)).BeginInit();
			this.TransactionsDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.BaseExportApplicator);
			//
			// LockInfoLabel
			//
			this.LockInfoLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LockInfoLabel, "LockInfoLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).LockInfoLabel)));
			this.LockInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LockInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.LockInfoLabel.Name = "LockInfoLabel";
			this.LockInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 150, true);
			this.LockInfoLabel.TabIndex = 0;
			this.LockInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LockInfoLabel.ForeColor = System.Drawing.Color.Red;
			this.LockInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.SelectAllCheckBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 17, true);
			this.TopPanel.TabIndex = 1;
			// 
			// SelectAllCheckBox
			// 
			this.SelectAllCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SelectAllCheckBox, "SelectAll");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).SelectAll)));
			this.SelectAllCheckBox.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("b29b68c7-b64a-46f2-90a2-64daa034fcd9", "Select All");
			this.SelectAllCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectAllCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectAllCheckBox.Name = "SelectAllCheckBox";
			this.SelectAllCheckBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, 3, 0, 0, true);
			this.SelectAllCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 17, true);
			this.SelectAllCheckBox.TabIndex = 2;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).Select)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).ExportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).OwnerReference)));
			this.TransactionsDisplayGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Select";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDateEditColumnStyleInfo1.ColumnName = "TransactionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.ColumnName = "ExportType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo3.ColumnName = "OwnerReference";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionsDisplayGrid.GridId = "4e3324b8-56cf-4dd1-b69b-98e529f3ba59";
			this.TransactionsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionsDisplayGrid.LayoutKey = "TransactionsDisplayGrid";
			this.TransactionsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.TransactionsDisplayGrid.Name = "TransactionsDisplayGrid";
			this.TransactionsDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.TransactionsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 376, true);
			this.TransactionsDisplayGrid.TabIndex = 0;
			// 
			// ExportOperationalActionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.LockInfoLabel);
			this.Controls.Add(this.TransactionsDisplayGrid);
			this.Name = "ExportOperationalActionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 399, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsDisplayGrid)).EndInit();
			this.TransactionsDisplayGrid.ResumeLayout(false);
			this.TransactionsDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDisplayGrid TransactionsDisplayGrid;
		private ZArchitecture.ZLabel LockInfoLabel;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZCheckBox SelectAllCheckBox;
	}
}
