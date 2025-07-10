namespace Enterprise.Customs.ZA.Module
{
	partial class WarehouseOperatorTransactionsFilterStripControl
	{
		private System.ComponentModel.IContainer components = null;

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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_TransactionType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_Status)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_ExportType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_OwnerReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).Product.OP_PartNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_Quantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_IsCustomsControlled)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_TotalValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_RX_NKCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_RN_NKOrigin)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).Batch.WOB_Batch)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_BatchLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_TransactionDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_LineReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusWHSOperatorTransaction)(null)).WOT_CustomsEntryNumber)));
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("6C947E82-9750-475F-96DB-0D9F3C5D1A2E", "Transaction Type");
            zTextBoxColumnStyleInfo1.ColumnName = "WOT_TransactionType";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("01BC0369-2356-4AA8-A382-2153E9D66A34", "Status");
            zTextBoxColumnStyleInfo2.ColumnName = "WOT_Status";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("96B16577-27F1-4122-8D6B-B9BEB342492F", "Export Type");
            zTextBoxColumnStyleInfo3.ColumnName = "WOT_ExportType";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("BD1DC189-F652-4A50-8845-A6DF34468996", "Owner Reference");
            zTextBoxColumnStyleInfo4.ColumnName = "WOT_OwnerReference";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("8854b186-7c47-4ed6-9518-a6b3ff045bdb", "Product Code");
            zTextBoxColumnStyleInfo5.ColumnName = "Product+OP_PartNum";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("C63D8136-E778-4EA9-B1F3-B26FBB302510", "Quantity");
            zCalcEditColumnStyleInfo1.ColumnName = "WOT_Quantity";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("0B277755-DDDA-48B4-A075-78A2E081505D", "Is Customs Controlled");
            zCheckBoxColumnStyleInfo1.ColumnName = "WOT_IsCustomsControlled";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("DE4598F9-9EE6-4ED5-93DE-B5445979215B", "Value");
            zCalcEditColumnStyleInfo2.ColumnName = "WOT_TotalValue";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("715AAA87-7ACD-449C-B9CB-BEE013AD0FE4", "Currency");
            zTextBoxColumnStyleInfo6.ColumnName = "WOT_RX_NKCurrency";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("FAB560AB-E323-476E-9F51-4475E8C82D0B", "Country/Region Origin");
            zTextBoxColumnStyleInfo7.ColumnName = "WOT_RN_NKOrigin";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("C0EB3F02-8110-4F37-8FEE-001F04E36AA0", "Batch");
            zTextBoxColumnStyleInfo8.ColumnName = "Batch+WOB_Batch";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("A308A914-BAED-43F0-902A-C6DA6ADC4227", "Batch Line No");
            zCalcEditColumnStyleInfo3.ColumnName = "WOT_BatchLineNo";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("48B3E317-8075-4C7A-B7B9-DBD279D3CDF5", "Transaction Date");
            zTextBoxColumnStyleInfo9.ColumnName = "WOT_TransactionDate";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("B4148724-FADE-42B6-AABF-B4CB0B8406FA", "Line Reference");
            zTextBoxColumnStyleInfo10.ColumnName = "WOT_LineReference";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("73E0B6A8-FB17-40FB-8529-88D535D63BDD", "Customs Reference Number");
            zTextBoxColumnStyleInfo11.ColumnName = "WOT_CustomsEntryNumber";
            zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 22, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusWHSOperatorTransaction);
            // 
            // WarehouseOperatorTransactionsFilterStripControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Name = "WarehouseOperatorTransactionsFilterStripControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 174, true);
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
