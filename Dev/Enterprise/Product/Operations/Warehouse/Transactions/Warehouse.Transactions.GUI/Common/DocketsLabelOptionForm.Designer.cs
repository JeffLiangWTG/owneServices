using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class DocketsLabelOptionForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DocketLabelLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DocketLabelLinesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 162, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl);
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocketsLabelOptionForm|c40a64f7-5de8-4ed4-8f1b-334d34b33b53", "Cancel", "Cancel", "Don\'t print labels");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 132, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPrintButton.TabIndex = 4;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocketsLabelOptionForm|9f0441ca-bbfb-47ef-a9ae-67e1757b1de7", "Print", "Print", "Print labels.");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 132, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 3;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// DocketLabelLinesGrid
			// 
			this.DocketLabelLinesGrid.AllowNavigation = false;
			this.DocketLabelLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocketLabelLinesGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLabelLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl)(null)).Lines)).SyncRoot)).OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLabelLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl)(null)).Lines)).SyncRoot)).NumberOfLabelsToPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLabelLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl)(null)).Lines)).SyncRoot)).TotalNumberOfLabels)));
			this.DocketLabelLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocketsLabelOptionForm|517959ca-4195-4aef-936b-3386719615f2", "Order", "Order Number", "");
			zTextBoxColumnStyleInfo1.ColumnName = "OrderNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocketsLabelOptionForm|df0e9990-8f01-41c0-baf2-5086fb58ca36", "Labels to Print", "Number of Labels to Print", "Number of labels you actually want to print.");
			zCalcEditColumnStyleInfo1.ColumnName = "NumberOfLabelsToPrint";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocketsLabelOptionForm|110f175b-02e0-443a-aea5-9e674bd4de84", "Available Labels", "Total Labels to Print", "Total number of labels that can be printed.");
			zCalcEditColumnStyleInfo2.ColumnName = "TotalNumberOfLabels";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.DocketLabelLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocketLabelLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DocketLabelLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DocketLabelLinesGrid.GridId = "5d2a29ff-3041-4a93-9fa9-d804eb8d3dd9";
			this.DocketLabelLinesGrid.CopySelectedRowsAllowed = true;
			this.DocketLabelLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocketLabelLinesGrid.LayoutKey = "DocketLabelLinesGrid";
			this.DocketLabelLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 12, true);
			this.DocketLabelLinesGrid.Name = "DocketLabelLinesGrid";
			this.DocketLabelLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 108, true);
			this.DocketLabelLinesGrid.TabIndex = 1;
			// 
			// DocketsLabelOptionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 186, true);
			this.ControlBox = false;
			this.Controls.Add(this.DocketLabelLinesGrid);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PrintButton);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocketsLabelControl";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "DocketsLabelOptionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.DocketLabelLinesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DocketLabelLinesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		private Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		private ZArchitecture.ZGrid DocketLabelLinesGrid;
	}
}
