using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccBankAccountFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|9fa45264-c328-4913-9b2a-81e66ad00e7e", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "AB_Code";
			zTextBoxColumnStyleInfo2.ColumnName = "AB_Desc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "AB_BankName";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|83feead9-4598-4a7e-8347-1bd861da0cee", "Account Num.", "Account Number");
			zTextBoxColumnStyleInfo4.ColumnName = "AB_AccountNum";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AB_RX_NKAccountCurrency";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|c5939e15-0a68-4f80-9e9e-837280035b6f", "Bank Abbreviation");
			zTextBoxColumnStyleInfo5.ColumnName = "AB_BankAbbreviation";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|c4e47ac7-97b7-433d-a688-06e4bcdb0fc8", "Account EFT");
			zTextBoxColumnStyleInfo6.ColumnName = "AB_AccountEFTUserID";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|be9dc706-a812-4366-94eb-19096ccb32e0", "GL Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AB_AG";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|2a439ea9-7a81-4165-84cb-881cfb36e52f", "Allow Auto DDR");
			zCheckBoxColumnStyleInfo1.ColumnName = "AB_AllowAutoDDR";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|10e862d3-3053-44fe-8633-d8b96dac192d", "DDR Format");
			zTextBoxColumnStyleInfo7.ColumnName = "AB_AutoDDRFormat";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|a9fb1de5-068b-4bb8-acf0-9797236d0488", "Bank Address");
			zTextBoxColumnStyleInfo8.ColumnName = "AB_BankAddress";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|94b4eb84-c5f2-4ae0-8df2-ddbf3634c6b8", "BSB");
			zTextBoxColumnStyleInfo9.ColumnName = "AB_BSB";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|9a530257-9555-4490-b1ec-64a4482062b7", "Detailed Deposit Slip");
			zCheckBoxColumnStyleInfo2.ColumnName = "AB_DetailedDepositSlip";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|1a34f22a-c1e5-41a2-9be2-e01d2b513732", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AB_GB";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|b777c7b1-e44e-4394-9d1f-e5581ba94e73", "Company");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AB_GC";
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|ef0da86d-8c9c-41fb-8802-9852ca5bd06c", "Is Active");
			zCheckBoxColumnStyleInfo3.ColumnName = "AB_IsActive";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|1e2c9168-5b62-428c-861a-53a481bfa194", "Is Default Receipt Bank Account");
			zCheckBoxColumnStyleInfo4.ColumnName = "AB_IsDefaultReceiptBankAccount";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|84e9d70b-cc2b-479e-8e44-f6aae9f61997", "Open Balance");
			zCalcEditColumnStyleInfo1.ColumnName = "AB_OpenBalance";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|3da9c09d-1e6d-4086-be81-5860b114987f", "Open OS Balance");
			zCalcEditColumnStyleInfo2.ColumnName = "AB_OpenOSBalance";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|dd9cddac-f48d-4681-886b-6d231b2a364f", "Statement Balance");
			zCalcEditColumnStyleInfo3.ColumnName = "AB_StatementBalance";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccBankAccountFilterControl|bb7d891c-cfbb-4b82-ada4-7cc334a8476e", "SWIFT");
			zTextBoxColumnStyleInfo10.ColumnName = "AB_SWIFT";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "AB_AccountType";
			zTextBoxColumnStyleInfo11.IsVisible = true;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 328, true);
			this.FilteredGrid.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.AccBankAccountFilterBusinessObject);
			// 
			// AccBankAccountFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "AccBankAccountFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
