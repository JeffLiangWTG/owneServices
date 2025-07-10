
namespace Enterprise.Customs.TR.GUI
{
	partial class AviationFuelTypeUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AviationFuelTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AviationFuelTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AviationFuelTypeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AviationFuelTypeGrid)).BeginInit();
			this.AviationFuelTypeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// AviationFuelTypeGroupBox
			// 
			this.AviationFuelTypeGroupBox.Controls.Add(this.AviationFuelTypeGrid);
			this.AviationFuelTypeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AviationFuelTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AviationFuelTypeGroupBox.Name = "AviationFuelTypeGroupBox";
			this.AviationFuelTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 284, true);
			this.AviationFuelTypeGroupBox.TabIndex = 9;
			this.AviationFuelTypeGroupBox.TabStop = false;
			// 
			// AviationFuelTypeGrid
			// 
			this.AviationFuelTypeGrid.AllowNavigation = false;
			this.AviationFuelTypeGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.AviationFuelTypeGrid, "FilteredInvoiceLines.AviationFuelTypeCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.AviationFuelType)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TR.Business.Declaration.AviationFuelType)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.AviationFuelType)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.AviationFuelType)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.AviationFuelType)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)).SyncRoot)).CSI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.AviationFuelType)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AviationFuelTypeCollection)).SyncRoot)).CSI_Description)));
			this.AviationFuelTypeGrid.CaptionText = "Aviation Fuel Type";
			this.AviationFuelTypeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("891D8E6B-F01B-468B-9077-0FDEC449D940", "Total Amount");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_RX_NKCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.TR.GUI.Res.GetData("891D8E6B-F01B-468B-9077-0FDEC449D940", "Total Amount");
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.AviationFuelTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AviationFuelTypeGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AviationFuelTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AviationFuelTypeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AviationFuelTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AviationFuelTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AviationFuelTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AviationFuelTypeGrid.GridId = "69f97afa-c479-45ee-8a4f-930bc139d976";
			this.AviationFuelTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AviationFuelTypeGrid.LayoutKey = "AviationFuelTypeGrid";
			this.AviationFuelTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AviationFuelTypeGrid.Name = "AviationFuelTypeGrid";
			this.AviationFuelTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 265, true);
			this.AviationFuelTypeGrid.TabIndex = 8;
			// 
			// AviationFuelTypeUserControl
			// 
			this.Controls.Add(this.AviationFuelTypeGroupBox);
			this.Name = "AviationFuelTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 284, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AviationFuelTypeGroupBox.ResumeLayout(false);
			this.AviationFuelTypeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AviationFuelTypeGrid)).EndInit();
			this.AviationFuelTypeGrid.ResumeLayout(false);
			this.AviationFuelTypeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AviationFuelTypeGroupBox;
		private ZArchitecture.ZGrid AviationFuelTypeGrid;
	}
}
