using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentContainersForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo containerNumberColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo sealNumberColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo containerModeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo printCheckbox = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SelectContainersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.containersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IncludeUnContainerisedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SelectNoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
			this.containersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 314, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DocumentContainers);
			// 
			// SelectContainersLabel
			// 
			this.SelectContainersLabel.AutoSize = true;
			this.SelectContainersLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|41427c93-28a4-4471-839b-b2fea626ebfa", "Select Containers");
			this.SelectContainersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.SelectContainersLabel.Name = "SelectContainersLabel";
			this.SelectContainersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.SelectContainersLabel.TabIndex = 1;
			// 
			// containersGrid
			// 
			this.containersGrid.AllowNavigation = false;
			this.containersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.containersGrid, "ContainersToSelectFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentContainers)(null)).ContainersToSelectFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentContainers)(null)).ContainersToSelectFrom)).SyncRoot)).Container.JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentContainers)(null)).ContainersToSelectFrom)).SyncRoot)).Container.JC_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentContainers)(null)).ContainersToSelectFrom)).SyncRoot)).Container.JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.ContainerToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentContainers)(null)).ContainersToSelectFrom)).SyncRoot)).JC_Calc_PrintDocumentForContainer)));
			this.containersGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.containersGrid.CaptionVisible = false;
			containerNumberColumn.ColumnName = "ContainerNumber";
			containerNumberColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|9dddbce2-9411-4462-90e4-fb0998cb57ef", "Container Number");
			containerNumberColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			containerNumberColumn.IsReadOnly = true;
			sealNumberColumn.ColumnName = "Container.JC_SealNum";
			sealNumberColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			sealNumberColumn.IsReadOnly = true;
			containerModeColumn.ColumnName = "Container.JC_ContainerMode";
			containerModeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			containerModeColumn.IsReadOnly = true;
			printCheckbox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|c786620f-c03c-4696-a81d-3d03f5abe400", "Print Document");
			printCheckbox.ColumnName = "JC_Calc_PrintDocumentForContainer";
			printCheckbox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.containersGrid.ColumnStyles.Add(containerNumberColumn);
			this.containersGrid.ColumnStyles.Add(sealNumberColumn);
			this.containersGrid.ColumnStyles.Add(containerModeColumn);
			this.containersGrid.ColumnStyles.Add(printCheckbox);
			this.containersGrid.GridId = "d2c91840-3205-4cfb-a3c1-9541cf5bb68d";
			this.containersGrid.CopySelectedRowsAllowed = true;
			this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containersGrid.LayoutKey = "containersGrid";
			this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.containersGrid.Name = "containersGrid";
			this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 244, true);
			this.containersGrid.TabIndex = 2;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|00d14078-3f11-46fa-816b-87e83f22b2ff", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 287, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 5;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|c151742d-1d26-4afe-8acd-064a818c679a", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 287, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 4;
			// 
			// IncludeUnContainerisedCheckBox
			// 
			this.IncludeUnContainerisedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IncludeUnContainerisedCheckBox.AutoSize = true;
			this.IncludeUnContainerisedCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|f7a2ddcf-5419-4bca-8883-e1d6ea187bdf", "Include Uncontainerized Shipments");
			this.IncludeUnContainerisedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeUnContainerisedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 287, true);
			this.IncludeUnContainerisedCheckBox.Name = "IncludeUnContainerisedCheckBox";
			this.IncludeUnContainerisedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 17, true);
			this.IncludeUnContainerisedCheckBox.TabIndex = 3;
			// 
			// SelectNoneButton
			// 
			this.SelectNoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectNoneButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("f53af991-b266-43ee-84bf-f8a7f98b6a52", "Select None");
			this.SelectNoneButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 287, true);
			this.SelectNoneButton.Name = "SelectNoneButton";
			this.SelectNoneButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectNoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.SelectNoneButton.TabIndex = 6;
			this.SelectNoneButton.ToolTipCaption = null;
			this.SelectNoneButton.Visible = false;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("025796be-006a-4307-86ae-59de11346121", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 287, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SelectAllButton.TabIndex = 7;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Visible = false;
			// 
			// DocumentContainersForm
			// 
			this.AcceptButton = this.PrintButton;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 336, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentContainersForm|7816ffbb-332e-4113-b291-e11b7230d0de", "Select containers to print.");
			this.Controls.Add(this.IncludeUnContainerisedCheckBox);
			this.Controls.Add(this.SelectAllButton);
			this.Controls.Add(this.SelectNoneButton);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.containersGrid);
			this.Controls.Add(this.SelectContainersLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.DocumentContainers);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentContainers";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 364, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 364, true);
			this.Name = "DocumentContainersForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.SelectContainersLabel, 0);
			this.Controls.SetChildIndex(this.containersGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.SelectNoneButton, 0);
			this.Controls.SetChildIndex(this.SelectAllButton, 0);
			this.Controls.SetChildIndex(this.IncludeUnContainerisedCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
			this.containersGrid.ResumeLayout(false);
			this.containersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZLabel SelectContainersLabel;
		Enterprise.ZArchitecture.ZGrid containersGrid;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		Enterprise.ZArchitecture.GUI.ZCheckBox IncludeUnContainerisedCheckBox;
		private ZArchitecture.GUI.ZButton SelectNoneButton;
		private ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
