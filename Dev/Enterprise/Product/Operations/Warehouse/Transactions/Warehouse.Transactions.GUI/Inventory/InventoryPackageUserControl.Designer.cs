using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class InventoryPackageUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PackageDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackageDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackageDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageDetailsGrid)).BeginInit();
			this.PackageDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketLine);
			// 
			// PackageDetailsGroupBox
			// 
			this.PackageDetailsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryPackageUserControl|adc96721-91db-471a-8936-b97201ca2f50", "Package Details");
			this.PackageDetailsGroupBox.Controls.Add(this.PackageDetailsGrid);
			this.PackageDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageDetailsGroupBox.Name = "PackageDetailsGroupBox";
			this.PackageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 563, true);
			this.PackageDetailsGroupBox.TabIndex = 0;
			this.PackageDetailsGroupBox.TabStop = false;
			// 
			// PackageDetailsGrid
			// 
			this.PackageDetailsGrid.AllowDrop = true;
			this.PackageDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackageDetailsGrid, "Inventory.PackageDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).Inventory)).SyncRoot)).PackageDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryPackageDetail)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).Inventory)).SyncRoot)).PackageDetails)).SyncRoot)).PackageID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryPackageDetail)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).Inventory)).SyncRoot)).PackageDetails)).SyncRoot)).OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryPackageDetail)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).Inventory)).SyncRoot)).PackageDetails)).SyncRoot)).IsTote)));
			this.PackageDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "PackageID";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "OrderNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsTote";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PackageDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackageDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackageDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PackageDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageDetailsGrid.GridId = "0e2b8c79-b147-444c-9c19-7027cee0aecb";
			this.PackageDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageDetailsGrid.LayoutKey = "PackageDetailsGrid";
			this.PackageDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackageDetailsGrid.Name = "PackageDetailsGrid";
			this.PackageDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 544, true);
			this.PackageDetailsGrid.TabIndex = 0;
			this.PackageDetailsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PackageInfoGrid_MouseDoubleClick);
			// 
			// InventoryPackageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackageDetailsGroupBox);
			this.Name = "InventoryPackageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 563, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackageDetailsGroupBox.ResumeLayout(false);
			this.PackageDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageDetailsGrid)).EndInit();
			this.PackageDetailsGrid.ResumeLayout(false);
			this.PackageDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region PackageInfoGrid_MouseDoubleClick

		WhsDocketLine docketLine => CurrentDataItem as WhsDocketLine;

		void PackageInfoGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var currentPackageDetail = (WhsInventoryPackageDetail)PackageDetailsGrid.ListManager.GetCurrent();

			if (currentPackageDetail != null)
			{
				InventoryHelper.ViewReleaseForPackage(currentPackageDetail.Order);
			}
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PackageDetailsGroupBox;
		private ZArchitecture.ZGrid PackageDetailsGrid;
	}
}
