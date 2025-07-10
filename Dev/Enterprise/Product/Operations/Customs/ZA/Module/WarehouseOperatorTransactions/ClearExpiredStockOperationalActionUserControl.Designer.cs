using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Module
{
	partial class ClearExpiredStockOperationalActionUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.TransactionsDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.LockInfoLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsDisplayGrid)).BeginInit();
			this.TransactionsDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.OperationalActions.ClearExpiredStockApplicator);
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
			// TransactionsDisplayGrid
			// 
			this.TransactionsDisplayGrid.AllowNavigation = false;
			this.TransactionsDisplayGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionsDisplayGrid, "Records");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).CustomsEntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).OwnerReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.OperatorTransactionSelection)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseExportApplicator)(null)).Records)).SyncRoot)).IntoBondDate)));
			this.TransactionsDisplayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CustomsEntryNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "OwnerReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "IntoBondDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionsDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionsDisplayGrid.GridId = "4e3324b8-56cf-4dd1-b69b-98e529f3ba5a";
			this.TransactionsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionsDisplayGrid.LayoutKey = "ClearExpiredStockTransactionsDisplayGrid";
			this.TransactionsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionsDisplayGrid.Name = "TransactionsDisplayGrid";
			this.TransactionsDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.TransactionsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 399, true);
			this.TransactionsDisplayGrid.TabIndex = 0;
			// 
			// ExportOperationalActionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LockInfoLabel);
			this.Controls.Add(this.TransactionsDisplayGrid);
			this.Name = "ExportOperationalActionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 399, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsDisplayGrid)).EndInit();
			this.TransactionsDisplayGrid.ResumeLayout(false);
			this.TransactionsDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDisplayGrid TransactionsDisplayGrid;
		private ZArchitecture.ZLabel LockInfoLabel;
	}
}
