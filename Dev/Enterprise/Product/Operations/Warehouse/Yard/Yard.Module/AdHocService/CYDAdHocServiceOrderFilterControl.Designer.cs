using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Module
{
	partial class CYDAdHocServiceOrderFilterControl
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

		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BindingSource.SetBindingMember(this.grid, ".");

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Yard.Business.CYDAdHocServiceOrder)(null)).YAO_JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Yard.Business.CYDAdHocServiceOrder)(null)).Client?.Address?.Header?.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Yard.Business.CYDAdHocServiceOrder)(null)).YAO_ClientReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Yard.Business.CYDAdHocServiceOrder)(null)).YAO_BillingDate)));

			zTextBoxColumnStyleInfo1.ColumnName = "YAO_JobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Yard.Module.Res.GetData("e0a78d72-e026-4cf0-90d0-7ff452b08a1b", "Service order ID");

			zTextBoxColumnStyleInfo2.ColumnName = "Client+Address+Header+OH_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Yard.Module.Res.GetData("805d5207-2ee6-4c85-b59b-bbf1ba2c1b49", "Client");

			zTextBoxColumnStyleInfo3.ColumnName = "YAO_ClientReference";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Yard.Module.Res.GetData("e52bce41-8de3-4010-9a60-f5dd6d240a13", "Client Reference");

			zTextBoxColumnStyleInfo4.ColumnName = "YAO_BillingDate";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Yard.Module.Res.GetData("bca1c54b-0043-4414-bfac-c2bcff900723", "Billing Date");


			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			this.CaptionRenderingEnabled = true;
			this.Name = "CYDAdHocServiceOrderFilterControl";

			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
