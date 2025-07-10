using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class InvoiceGroupingUserControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.GroupChargeGrid)).BeginInit();
			// 
			// JZ_InvoiceNumberBoundGroupTextBox1
			// 
			this.JZ_InvoiceNumberBoundGroupTextBox1.Name = "JZ_InvoiceNumberBoundGroupTextBox1";
			this.ContainerControlToolTip.SetToolTip(this.JZ_InvoiceNumberBoundGroupTextBox1, "Invoice Number for this supplier");
			// 
			// GroupChargeGrid
			// 
			this.GroupChargeGrid.Name = "GroupChargeGrid";
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_ChargeTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_ChargeType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).Lookups.ChargeTypeList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).ChargeCodeDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).ChargeCodeDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_Amount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_AmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_RX_NKCurrencyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_RX_NKCurrency)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).Lookups.Currencies)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_IsDutiable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_IsDutiableInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_IsIncludedInITOT)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_IsIncludedInITOTInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_PrepaidCollectInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).J7_PrepaidCollect)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseGroupInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceGroupHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)))).Charges)))).Lookups.PrepaidCollectList)));
			// 
			// InvoiceGroupingUserControl
			// 
			this.Name = "InvoiceGroupingUserControl";
			((System.ComponentModel.ISupportInitialize)(this.GroupChargeGrid)).EndInit();
		}

	}
}
