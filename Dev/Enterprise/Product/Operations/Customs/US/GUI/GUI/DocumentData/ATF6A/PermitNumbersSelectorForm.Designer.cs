using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Customs.US.GUI
{
	public partial class PermitNumbersSelectorForm
	{
		System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SelectPermitNumbersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PermitNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectNoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PermitNumbersGrid)).BeginInit();
			this.PermitNumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 303, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.PermitNumbersToSelectFromForPrintingCollection);
			// 
			// SelectPermitNumbersLabel
			// 
			this.SelectPermitNumbersLabel.AutoSize = true;
			this.SelectPermitNumbersLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("014e47c6-6720-459e-976f-e2dfbe190497", "Select Permit Numbers");
			this.SelectPermitNumbersLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SelectPermitNumbersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.SelectPermitNumbersLabel.Name = "SelectPermitNumbersLabel";
			this.SelectPermitNumbersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 13, true);
			this.SelectPermitNumbersLabel.TabIndex = 1;
			this.SelectPermitNumbersLabel.UseMnemonic = false;
			// 
			// PermitNumbersGrid
			// 
			this.PermitNumbersGrid.AllowNavigation = false;
			this.PermitNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PermitNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.PermitNumberToSelectFromForPrinting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.PermitNumberToSelectFromForPrinting)(null)).NeedPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.PermitNumberToSelectFromForPrinting)(null)).PermitNumber)));
			this.PermitNumbersGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d092e106-419c-4ba2-8f48-b54fc8576b36", "Print");
			zCheckBoxColumnStyleInfo1.ColumnName = "NeedPrint";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c939a9d5-771d-46a7-9bf8-ab19f9110db3", "Permit Number");
			zTextBoxColumnStyleInfo1.ColumnName = "PermitNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PermitNumbersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PermitNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PermitNumbersGrid.GridId = "7bfba12f-a24b-4bb9-aa63-d934ced370bc";
			this.PermitNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitNumbersGrid.LayoutKey = "containersGrid";
			this.PermitNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.PermitNumbersGrid.Name = "PermitNumbersGrid";
			this.PermitNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 244, true);
			this.PermitNumbersGrid.TabIndex = 2;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9cc16abc-1f1e-4413-86a3-bb25a835fd90", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 280, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 7;
			this.CancelPrintButton.ToolTipCaption = null;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("65ff6773-c210-4363-9e83-d4e3c564ae09", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 280, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 6;
			this.PrintButton.ToolTipCaption = null;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// SelectNoneButton
			// 
			this.SelectNoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectNoneButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0fe53188-be65-4eb7-8fb7-6d2470a84bed", "Select None");
			this.SelectNoneButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 280, true);
			this.SelectNoneButton.Name = "SelectNoneButton";
			this.SelectNoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.SelectNoneButton.TabIndex = 5;
			this.SelectNoneButton.ToolTipCaption = null;
			this.SelectNoneButton.Click += new System.EventHandler(this.SelectNoneButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1bae14c3-2cf8-40cb-922e-da66bb8c4a2d", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 280, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SelectAllButton.TabIndex = 4;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// PermitNumbersSelectorForm
			// 
			this.AcceptButton = this.PrintButton;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4afbcd58-68b8-43c8-b8cb-673aca857d60", "Select permit numbers to print.");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 325, true);
			this.Controls.Add(this.SelectAllButton);
			this.Controls.Add(this.SelectNoneButton);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PermitNumbersGrid);
			this.Controls.Add(this.SelectPermitNumbersLabel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.PermitNumbersToSelectFromForPrintingCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 364, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 364, true);
			this.Name = "PermitNumbersSelectorForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.SelectPermitNumbersLabel, 0);
			this.Controls.SetChildIndex(this.PermitNumbersGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.SelectNoneButton, 0);
			this.Controls.SetChildIndex(this.SelectAllButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PermitNumbersGrid)).EndInit();
			this.PermitNumbersGrid.ResumeLayout(false);
			this.PermitNumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZLabel SelectPermitNumbersLabel;
		Enterprise.ZArchitecture.ZGrid PermitNumbersGrid;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		private ZArchitecture.GUI.ZButton SelectNoneButton;
		private ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
