namespace Enterprise.Customs.Module
{
	public partial class CommercialInvoiceFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceHeader);
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("FFFE70D7-C6EB-4745-8E93-4CA30737CA92", "Invoice Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JZ_InvoiceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.SupplierList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("8F7C0626-A54C-4CC3-835E-CE98C5D5F9B3", "Supplier Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JZ_OH_Supplier";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo2.BindToList = "Lookups.ImporterList";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("893F5368-2B0D-4E13-A7AC-0E4DB6716C91", "Importer Code");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JZ_OH_Buyer";
			zGuidFindBoxColumnStyleInfo2.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zCodeFindBoxColumnStyleInfo3.BindToList = "Lookups.CurrencyList";
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("8AEF7FCE-5E71-4A97-8C47-9B5360A784DE", "Currency");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JZ_RX_NKInvoice_Currency";
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("866EA716-AEC0-9EA2-4531-7F5E4ED0E6BA", "Incoterm");
			zTextBoxColumnStyleInfo2.ColumnName = "JZ_IncoTerm";
			zGuidFindBoxColumnStyleInfo4.BindToList = "Lookups.BranchList";
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("5D45A5E7-B2E0-42A3-BD65-19F7F3325B95", "Branch");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JZ_GB";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("48543FB6-D15F-4BB7-9C23-59F28712B08E", "Shipment Type");
			zTextBoxColumnStyleInfo3.ColumnName = "JZ_MessageType";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("A769257B-1E3E-4DC4-942C-D771CFB18194", "Importer Name");
			zTextBoxColumnStyleInfo4.ColumnName = "Importer_Effective+OH_FullName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("A2D8A1A8-ADB2-47D7-8141-E7370D0DBE70", "Supplier Name");
			zTextBoxColumnStyleInfo5.ColumnName = "Supplier_Effective+OH_FullName";
			zTextBoxColumnStyleInfo6.ColumnName = "JobDeclaration+JE_DeclarationReference";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 264, true);
			this.FilteredGrid.TabIndex = 5;
			// 
			// CommercialInvoiceFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "EnterpriseEnterprise.Customs.Business.BaseJobComInvoiceHeader";
			this.Name = "CommercialInvoiceFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
