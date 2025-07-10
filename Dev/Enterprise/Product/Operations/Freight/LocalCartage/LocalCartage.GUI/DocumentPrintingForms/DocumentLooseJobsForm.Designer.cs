using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class DocumentLooseJobsForm
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

		ZButton PrintButton;
		ZButton CancelPrintButton;
		ZGrid LooseLegsGrid;
		ZLabel SelectLooseLegsLabel;
		System.ComponentModel.IContainer components = null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LooseLegsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectLooseLegsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LooseLegsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 22, true);
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
			this.PrintButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|BB3D3AF3-A261-4EED-8303-1116079596CD", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(889, 412, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 2;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|8E128399-6EAF-45FD-8531-01263D80D6CE", "Cancel");
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(969, 412, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 3;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// LooseLegsGrid
			// 
			this.LooseLegsGrid.AllowNavigation = false;
			this.LooseLegsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LooseLegsGrid.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.LooseLegsGrid, "CartageLegs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).PrintCartageLeg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.PickupFromDocAddress.E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.PickupFromCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.JU_PlannedPickupTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.DeliverToDocAddress.E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.DeliverToCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.JU_EstimatedDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).TotalPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).TotalPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLeg)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions)(null)).CartageLegs)).SyncRoot)).CartageLeg.BookedCtgMove.EW_DropMode)));

			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|33F28E2A-3B50-46AF-B9E2-B6E20E1A1CBD", "Print");
			zCheckBoxColumnStyleInfo2.ColumnName = "PrintCartageLeg";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.LooseLegsGrid.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.LooseLegsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|351D156D-830F-48F7-86E6-8728C9E5F2E7", "Pickup Company Name");
			zTextBoxColumnStyleInfo8.ColumnName = "CartageLeg+PickupFromDocAddress+E2_CompanyName";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|EAB89B58-9829-4377-AD2C-1896A8C5788F", "Pickup City");
			zTextBoxColumnStyleInfo9.ColumnName = "CartageLeg+PickupFromCity";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|63ED923E-7F09-41F9-B4F8-867BE736DC6F", "Planned Pick Up Time");
			zDateEditColumnStyleInfo3.ColumnName = "CartageLeg+JU_PlannedPickupTime";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|C167B43F-DCB7-4CB4-B82F-FF85CC135F3F", "Delivery Company Name");
			zTextBoxColumnStyleInfo10.ColumnName = "CartageLeg+DeliverToDocAddress+E2_CompanyName";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|F12B1B54-AF66-44A3-88C8-0326D0B2B720", "Delivery City");
			zTextBoxColumnStyleInfo11.ColumnName = "CartageLeg+DeliverToCity";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.Caption = null;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|BE631DF7-01AC-4100-B5D5-E474C8A67F93", "Planned Delivery Time");
			zDateEditColumnStyleInfo4.ColumnName = "CartageLeg+JU_EstimatedDeliveryTime";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zTextBoxColumnStyleInfo12.Caption = null;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|2E048DCF-3D0E-4DF2-9E25-0A8C24D84BB4", "Pack Count");
			zTextBoxColumnStyleInfo12.ColumnName = "TotalPackCount";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo13.Caption = null;
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|C49CA362-1795-45A1-84C6-EA10D615AA31", "Pack Type");
			zTextBoxColumnStyleInfo13.ColumnName = "TotalPackType";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo14.Caption = null;
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|C12C9A02-D600-47C7-8149-A6E472914C3D", "Drop Mode");
			zTextBoxColumnStyleInfo14.ColumnName = "CartageLeg+BookedCtgMove+EW_DropMode";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zCheckBoxColumnStyleInfo2.Caption = null;

			this.LooseLegsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.LooseLegsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.LooseLegsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);

			this.LooseLegsGrid.CopySelectedRowsAllowed = true;
			this.LooseLegsGrid.GridId = "81D1A85F-0E95-4773-945D-7DD1E970C50D";
			this.LooseLegsGrid.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.LooseLegsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LooseLegsGrid.LayoutKey = "zGrid1";
			this.LooseLegsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.LooseLegsGrid.Name = "LooseLegsGrid";
			this.LooseLegsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 383, true);
			this.LooseLegsGrid.TabIndex = 1;
			// 
			// SelectLooseLegsLabel
			// 
			this.SelectLooseLegsLabel.AutoSize = true;
			this.SelectLooseLegsLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("DocumentLooseJobsForm|5EC9ECB8-119E-400E-B7C5-A9004A30B70E", "Select Loose Job Legs to print");
			this.SelectLooseLegsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SelectLooseLegsLabel.Name = "SelectLooseLegsLabel";
			this.SelectLooseLegsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 13, true);
			this.SelectLooseLegsLabel.TabIndex = 5;
			// 
			// DocumentLooseJobsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("bee98c59-2e4b-45ee-a40a-c23270b4ae99", "Select Loose Job Legs to print");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1057, 462, true);
			this.ControlBox = false;
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.LooseLegsGrid);
			this.Controls.Add(this.SelectLooseLegsLabel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions);
			this.DataSourceTypeName = "Enterprise.Freight.LocalCartage.Business.DocumentCartageLegOptions";
			this.Name = "DocumentLooseJobsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SelectLooseLegsLabel, 0);
			this.Controls.SetChildIndex(this.LooseLegsGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LooseLegsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
