namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class DocumentCartageLegsForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Designer generated code

		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.ZGrid ContainerLegsGrid;
		Enterprise.ZArchitecture.ZLabel SelectContainerLegsLabel;
		System.ComponentModel.IContainer components = null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContainerLegsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectContainerLegsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainerLegsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 296, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(244);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(245);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|b0f7028b-3a2e-4448-9368-6d1845f7e770", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 268, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 2;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|9494cf3d-b510-4f7d-8920-7d86c448fd62", "Cancel");
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 268, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 3;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// ContainerLegsGrid
			// 
			this.ContainerLegsGrid.AllowNavigation = false;
			this.ContainerLegsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContainerLegsGrid, "CartageLegs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.JU_PlannedPickupTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.Container.JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).PrintCartageLeg)));
			this.ContainerLegsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ContainerLegsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|60c73dc3-9363-4dd1-98be-5b43ec9e26e2", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "CartageLeg+JU_PlannedPickupTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|88b8b63e-9655-4cc8-a8c8-1a873ed4528a", "Container #");
			zTextBoxColumnStyleInfo1.ColumnName = "CartageLeg+Container+JC_ContainerNum";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|3d29c9a5-e1b9-443b-8fb4-b2cb585dd2a6", "Print");
			zCheckBoxColumnStyleInfo1.ColumnName = "PrintCartageLeg";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.ContainerLegsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ContainerLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainerLegsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ContainerLegsGrid.GridId = "687ddd1d-4836-45a9-9f1c-4779d1d690d1";
			this.ContainerLegsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerLegsGrid.LayoutKey = "zGrid1";
			this.ContainerLegsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 30, true);
			this.ContainerLegsGrid.Name = "ContainerLegsGrid";
			this.ContainerLegsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 227, true);
			this.ContainerLegsGrid.TabIndex = 1;
			// 
			// SelectContainerLegsLabel
			// 
			this.SelectContainerLegsLabel.AutoSize = true;
			this.SelectContainerLegsLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|393e4ad6-c7a3-47bf-a5ad-55c4adf57c47", "Select Port Transport Legs to print");
			this.SelectContainerLegsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SelectContainerLegsLabel.Name = "SelectContainerLegsLabel";
			this.SelectContainerLegsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 13, true);
			this.SelectContainerLegsLabel.TabIndex = 5;
			// 
			// DocumentCartageLegsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 318, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentCartageLegsForm|f12a1eda-8251-499b-ae6d-995fd86c74eb", "Select Port Transport Legs to Print");
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.ContainerLegsGrid);
			this.Controls.Add(this.SelectContainerLegsLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions);
			this.DataSourceTypeName = "Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions";
			this.Name = "DocumentCartageLegsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SelectContainerLegsLabel, 0);
			this.Controls.SetChildIndex(this.ContainerLegsGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainerLegsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
