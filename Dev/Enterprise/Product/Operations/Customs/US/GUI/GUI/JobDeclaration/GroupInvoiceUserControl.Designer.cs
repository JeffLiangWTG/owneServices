namespace Enterprise.Customs.US.GUI
{
	partial class GroupInvoiceUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SelectedInvoiceGroupGroupBox.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargeGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SelectedInvoiceGroupGroupBox
			// 
			this.SelectedInvoiceGroupGroupBox.Controls.SetChildIndex(this.ChargesGroupBox, 0);
			this.SelectedInvoiceGroupGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberBoundGroupTextBox1, 0);
			// 
			// GroupChargeGrid
			// 
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Exchange Rate";
			zCalcEditColumnStyleInfo1.ColumnName = "J7_ExchangeRate";
			zCalcEditColumnStyleInfo1.Decimals = 6;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("GroupInvoice|f68a6622-fd7b-4686-90f2-299b6b3d5977", "Exchange Rate");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCheckBoxColumnStyleInfo1.Caption = "Fixed Rate";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsJ7_ExchangeRateUserEnterable";
			zCheckBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("GroupInvoice|f68a6622-fd7b-4686-90f2-299b6b3d5977", "Exchange Rate");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zTextBoxColumnStyleInfo1.Caption = "AII Charge Desc.";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "J7_ChargeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCheckBoxColumnStyleInfo2.Caption = "Override";
			zCheckBoxColumnStyleInfo2.ColumnName = "J7_AdjustedCharge";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			this.GroupChargeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// GroupInvoiceUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "GroupInvoiceUserControl";
			this.SelectedInvoiceGroupGroupBox.ResumeLayout(false);
			this.SelectedInvoiceGroupGroupBox.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GroupChargeGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
