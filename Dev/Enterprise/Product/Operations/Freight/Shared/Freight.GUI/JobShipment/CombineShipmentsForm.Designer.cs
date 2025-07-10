namespace Enterprise.Freight.GUI
{
	public partial class CombineShipmentsForm
	{
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

		Enterprise.ZArchitecture.GUI.ZGroupBox ShipmentsGroupBox;
		protected Enterprise.ZArchitecture.ZGrid ShipmentsGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton CombineButton;
		new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		System.ComponentModel.IContainer components = null;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ShipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CombineButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CombineShipmentsHelper);
			// 
			// ShipmentsGroupBox
			// 
			this.ShipmentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ShipmentsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("CombineShipmentsForm|c935a110-1878-416c-a04d-489e6d031ea1", "Shipments");
			this.ShipmentsGroupBox.Controls.Add(this.ShipmentsGrid);
			this.ShipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.ShipmentsGroupBox.Name = "ShipmentsGroupBox";
			this.ShipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 158, true);
			this.ShipmentsGroupBox.TabIndex = 1;
			this.ShipmentsGroupBox.TabStop = false;
			// 
			// ShipmentsGrid
			// 
			this.ShipmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ShipmentsGrid, "RelatedShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CombineShipmentsHelper)(null)).RelatedShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonShipment)(((System.Collections.IList)(((Enterprise.Freight.Business.CombineShipmentsHelper)(null)).RelatedShipments)).SyncRoot)).JS_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonShipment)(((System.Collections.IList)(((Enterprise.Freight.Business.CombineShipmentsHelper)(null)).RelatedShipments)).SyncRoot)).JS_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(((System.Collections.IList)(((Enterprise.Freight.Business.CombineShipmentsHelper)(null)).RelatedShipments)).SyncRoot)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(((System.Collections.IList)(((Enterprise.Freight.Business.CombineShipmentsHelper)(null)).RelatedShipments)).SyncRoot)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonShipment)(((System.Collections.IList)(((Enterprise.Freight.Business.CombineShipmentsHelper)(null)).RelatedShipments)).SyncRoot)).JS_OuterPacks)));
			this.ShipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "JS_HouseBill";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JS_ActualVolume";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JS_ActualWeight";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("CombineShipmentsForm|734e1f16-28cf-4035-a568-a7e068a9bb10", "Packs");
			zCalcEditColumnStyleInfo3.ColumnName = "JS_OuterPacks";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ShipmentsGrid.GridId = "17739c1c-048d-4246-b649-9c6778a41341";
			this.ShipmentsGrid.CopySelectedRowsAllowed = true;
			this.ShipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsGrid.IsWholeRowSelectedOnClick = true;
			this.ShipmentsGrid.LayoutKey = "ShipmentsGrid";
			this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ShipmentsGrid.Name = "ShipmentsGrid";
			this.ShipmentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 139, true);
			this.ShipmentsGrid.TabIndex = 0;
			// 
			// CombineButton
			// 
			this.CombineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CombineButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("CombineShipmentsForm|083ef9b8-3458-4f06-b6eb-3a9c09eee9a4", "Combine");
			this.CombineButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 172, true);
			this.CombineButton.Name = "CombineButton";
			this.CombineButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CombineButton.TabIndex = 2;
			this.CombineButton.Click += new System.EventHandler(this.CombineButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("CombineShipmentsForm|b33983b2-e282-4b41-a07e-6ea601b99786", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 172, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CombineShipmentsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 222, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("CombineShipmentsForm|514fb868-65ef-459e-9e47-88d2980f7ae7", "Combine Shipments");
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.CombineButton);
			this.Controls.Add(this.ShipmentsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.CombineShipmentsHelper);
			this.DataSourceTypeName = "Enterprise.Freight.Business.CombineShipmentsHelper";
			this.Name = "CombineShipmentsForm";
			this.Controls.SetChildIndex(this.ShipmentsGroupBox, 0);
			this.Controls.SetChildIndex(this.CombineButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
