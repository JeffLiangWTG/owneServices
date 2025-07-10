using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class ReceiveProductSummaryGridUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SummaryLinesGrid = new ReceiveProductSummaryGridUserControl.ProductSummaryGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SummaryLinesGrid)).BeginInit();
			this.SummaryLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsReceive);
			// 
			// Grid
			// 
			this.SummaryLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SummaryLinesGrid, "ReceiveProductSummaryCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).ReceiveProductSummaryCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveProductSummary)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).ReceiveProductSummaryCollection)).SyncRoot)).ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveProductSummary)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).ReceiveProductSummaryCollection)).SyncRoot)).ProductDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveProductSummary)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).ReceiveProductSummaryCollection)).SyncRoot)).ExpectedQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveProductSummary)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).ReceiveProductSummaryCollection)).SyncRoot)).ReceivedQuantity)));
			this.SummaryLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ProductCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ProductDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ExpectedQuantity";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ReceivedQuantity";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SummaryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SummaryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SummaryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SummaryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryLinesGrid.GridId = "f588a032-40fd-4a26-b3e9-a9aa3aec1128";
			this.SummaryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SummaryLinesGrid.LayoutKey = "Grid";
			this.SummaryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryLinesGrid.Name = "Grid";
			this.SummaryLinesGrid.ReadOnly = true;
			this.SummaryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 150, true);
			this.SummaryLinesGrid.TabIndex = 2;
			// 
			// ReceiveProductSummaryGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SummaryLinesGrid);
			this.Name = "ReceiveProductSummaryGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SummaryLinesGrid)).EndInit();
			this.SummaryLinesGrid.ResumeLayout(false);
			this.SummaryLinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ReceiveProductSummaryGridUserControl.ProductSummaryGrid SummaryLinesGrid;
	}
}
