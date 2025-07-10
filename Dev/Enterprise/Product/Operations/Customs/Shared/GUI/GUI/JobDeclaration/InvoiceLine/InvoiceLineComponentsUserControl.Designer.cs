
namespace Enterprise.Customs.GUI
{
	partial class InvoiceLineComponentsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.SelectedInventoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SelectedInventoryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SelectedInventoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedInventoryGrid)).BeginInit();
			this.SelectedInventoryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceLine);
			// 
			// SelectedInventoryGroupBox
			// 
			this.SelectedInventoryGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b265dd1f-0605-45e2-a96a-000843f28ad8", "Selected Inventory");
			this.SelectedInventoryGroupBox.Controls.Add(this.SelectedInventoryGrid);
			this.SelectedInventoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedInventoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectedInventoryGroupBox.Name = "SelectedInventoryGroupBox";
			this.SelectedInventoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 417, true);
			this.SelectedInventoryGroupBox.TabIndex = 7;
			this.SelectedInventoryGroupBox.TabStop = false;
			// 
			// SelectedInventoryGrid
			// 
			this.SelectedInventoryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SelectedInventoryGrid, "ComponentInventoryCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).ComponentInventoryCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.JobComInvLineComponentInventory)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).ComponentInventoryCollection)).SyncRoot)).QuantityOnHand)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.JobComInvLineComponentInventory)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).ComponentInventoryCollection)).SyncRoot)).JIV_QuantityToDraw)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.JobComInvLineComponentInventory)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).ComponentInventoryCollection)).SyncRoot)).PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.JobComInvLineComponentInventory)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).ComponentInventoryCollection)).SyncRoot)).CustomsEntryKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.JobComInvLineComponentInventory)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).ComponentInventoryCollection)).SyncRoot)).ArrivalDate)));
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "QuantityOnHand";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JIV_QuantityToDraw";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "PackType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CustomsEntryKey";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "ArrivalDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SelectedInventoryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SelectedInventoryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SelectedInventoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SelectedInventoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SelectedInventoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SelectedInventoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedInventoryGrid.GridId = "9897fd88-ec01-4fb9-b3c7-f0e79001b057";
			this.SelectedInventoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedInventoryGrid.LayoutKey = "SelectedInventoryGrid";
			this.SelectedInventoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 20, true);
			this.SelectedInventoryGrid.Name = "SelectedInventoryGrid";
			this.SelectedInventoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 394, true);
			this.SelectedInventoryGrid.TabIndex = 0;
			// 
			// InvoiceLineComponentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SelectedInventoryGroupBox);
			this.Name = "InvoiceLineComponentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 417, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SelectedInventoryGroupBox.ResumeLayout(false);
			this.SelectedInventoryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedInventoryGrid)).EndInit();
			this.SelectedInventoryGrid.ResumeLayout(false);
			this.SelectedInventoryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public Enterprise.ZArchitecture.GUI.ZGroupBox SelectedInventoryGroupBox;
		public Enterprise.ZArchitecture.ZGrid SelectedInventoryGrid;
	}
}
