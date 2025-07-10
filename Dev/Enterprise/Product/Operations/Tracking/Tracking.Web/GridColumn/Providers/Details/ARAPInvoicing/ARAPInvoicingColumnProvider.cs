using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ARAPInvoicingColumnProvider : GridColumnProvider
	{
		public ARAPInvoicingColumnProvider(TrackingSiteUser siteUser)
		{
			this.siteUser = siteUser;
		}

		public const string ViewInvoiceCommand = "ViewInvoice";

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((InvoicingBase)null).InvoiceNumber);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			ZLinkButtonColumn invoiceNumColumn = new ZLinkButtonColumn(Res.GetString("bce7a511-ee6b-4842-b5b8-5bbd7fe189a3", "Invoice #"), "InvoiceNumber") { ColumnKey = WebTracker.Grids.ARAPInvoicing.InvoiceNumber };
			invoiceNumColumn.Command = ViewInvoiceCommand;
			if (SiteUser != null && SiteUser.IsShipmentQuickViewUser)
			{
				invoiceNumColumn.ClientClickHandler = BasePage.ShipmentQuickViewUserLoginRequest;
			}
			AddToDictionaryAsDefault(invoiceNumColumn);
			AddToDictionaryAsDefault(new ZFindBoxColumn(Res.GetString("2ebd319c-16e5-4893-9602-aa65af92792b", "Issuer"), InvoicingBase.Schema.AH_GC, "Lookups.Companies", typeof(InvoicingBase))
			{
				ColumnKey = WebTracker.Grids.ARAPInvoicing.Issuer,
				DisplayStyle = Enterprise.ZArchitecture.Core.OComboBoxDropDownStyle.DescriptionOnly
			});

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("c3c6b079-289c-4aa5-b5ba-b9bcd37ff1b9", "Type"), InvoicingBase.Schema.AH_TransactionType) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Type });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("ccd57221-46b7-4081-a4e4-2df904e04778", "Terms"), InvoicingBase.Schema.AH_InvoiceTerm) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Terms });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("15bc2392-9f38-47a4-9775-69081da9c068", "Inv. Date"), InvoicingBase.Schema.AH_InvoiceDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ARAPInvoicing.InvoiceDate });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("9cc86f51-e40e-437c-a5ec-9d50d771a4d2", "Due Date"), InvoicingBase.Schema.AH_DueDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ARAPInvoicing.DueDate });
			AddToDictionaryAsDefault(new ZCodeFindBoxColumn(Res.GetString("2cb87554-d474-4042-a470-4d0f3e53c011", "Currency"), InvoicingBase.Schema.AH_RX_NKTransactionCurrency, "Lookups.TransactionCurrencies", typeof(InvoicingBase)) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Currency });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("c53c52a0-8b99-475e-aad6-11528b53c50e", "Amount"), InvoicingBase.Schema.AH_OSTotalAmount, InvoicingBase.Schema.AH_Calc_RXDecimals, AccTransactionHeaderSchema.AH_OSTotal.Name) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Amount });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("82efd883-de5a-406f-955c-c6a664f97468", "Outstanding Amt."), InvoicingBase.Schema.AH_Calc_OSOutstandingAmount, InvoicingBase.Schema.AH_Calc_RXDecimals, AccTransactionHeaderSchema.AH_OutstandingAmount.Name) { ColumnKey = WebTracker.Grids.ARAPInvoicing.OutstandingAmount });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("1265001b-3b2a-4b95-97d1-fd54640cd609", "Paid Date"), InvoicingBase.Schema.AH_FullyPaidDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.ARAPInvoicing.PaidDate });

			if (GetAnyCompanySupportsComplianceSubType())
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("fa276cc3-027c-4f6e-bbb1-6b975906675f", "Compliance Number"), InvoicingBase.Schema.AH_TransactionReference) { ColumnKey = WebTracker.Grids.ARAPInvoicing.ComplianceNumber });
			}
		}

		bool GetAnyCompanySupportsComplianceSubType()
		{
			var transactionCompanies = SiteUser?.GetTransactionCompanies(new BusinessObjectFactory());
			if (transactionCompanies != null)
			{
				foreach (var transactionCompany in transactionCompanies)
				{
					var country = transactionCompany?.Country;
					if (country != null)
					{
						var countryCode = country.Code;
						var complianceSubTypeConfig = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(transactionCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
						foreach (ComplianceSubTypeAttributionRuleConfiguration config in complianceSubTypeConfig)
						{
							if (countryCode == config.Country)
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		protected TrackingSiteUser SiteUser
		{
			get { return siteUser; }
		}

		readonly TrackingSiteUser siteUser;
	}
}
