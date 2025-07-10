namespace Enterprise.Warehouse.Transactions.Module
{
	partial class VASOrderFilterControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            this.RecentItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.BindingSource.SetBindingMember(this.grid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WVO_JobID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WVO_CustomerReferenceNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Client.OH_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WarehousePK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Status)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).ServiceArea.WA_NameMultilingual)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).ProductCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).ProductDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).ProductQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Client.OH_FullName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Job.JH_ProfitLossReasonCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Job.JH_TotalProfitRevenueMargin)));
            zTextBoxColumnStyleInfo1.ColumnName = "WVO_JobID";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "WVO_CustomerReferenceNo";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("100332cc-cf83-4dfa-a6af-d4a4ea5279af", "Client");
            zTextBoxColumnStyleInfo3.ColumnName = "Client+OH_Code";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo1.ColumnName = "WarehousePK";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "Status";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "ServiceArea+WA_NameMultilingual";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "ProductCode";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.ColumnName = "ProductDescription";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "ProductQty";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("795a9a31-68fa-41fc-92dd-8c21f6e12bad", "Client Full Name");
            zTextBoxColumnStyleInfo8.ColumnName = "Client+OH_FullName";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.IsVisible = false;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
            zTextBoxColumnStyleInfo9.ColumnName = "Job+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.IsVisible = false;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.ColumnName = "Job+JH_TotalProfitRevenueMargin";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.IsVisible = false;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsVASOrder);
            // 
            // VASOrderFilterControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Name = "VASOrderFilterControl";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.RecentItemsPanel.ResumeLayout(false);
            this.RecentItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}
