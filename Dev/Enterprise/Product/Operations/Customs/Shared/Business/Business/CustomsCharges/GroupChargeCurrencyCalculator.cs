using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business
{
	public class GroupChargeCurrencyCalculator
	{
		public GroupChargeCurrencyCalculator(BaseJobComInvoiceGroupHeader groupHeader)
		{
			this.groupHeader = groupHeader;
			this.declaration = groupHeader.JobDeclaration;
		}

		public ZString GetDefaultCurrency(ICustomsChargeCode chargeCode)
		{
			ZString result = "";

			if (groupHeader != null && chargeCode != null)
			{
				string[] invoiceCurrencies = groupHeader.JobComInvoiceHeaders.InvoiceCurrencies;

				if (invoiceCurrencies.Length == 1)
				{
					if (DoInvoicesIncotermsAllowThisChargePartOfInvoice(chargeCode) || declaration.IsAir)
					{
						result = invoiceCurrencies[0];
					}
				}

				if (result == "" &&
					(chargeCode.Code == CustomsChargeTypeList.Codes.OverseasFreight || chargeCode.Code == CustomsChargeTypeList.Codes.OverseasInsurance) &&
					declaration.IsSea)
				{
					result = Constants.CurrencyCodes.UnitedStates;
				}

				if (result == "" && groupHeader.Charges.Count > 0)
				{
					result = groupHeader.Charges[0].J7_RX_NKCurrency;
				}
			}
			return result;
		}

		#region Implementation

		internal bool DoInvoicesIncotermsAllowThisChargePartOfInvoice(ICustomsChargeCode chargeCode)
		{
			var invoiceIncoTerms = groupHeader.JobComInvoiceHeaders.InvoiceIncoterms;
			var incoTermAndCustomsChargeFactory = ((IApportionInvoiceHolder)declaration).IncoTermAndChargeFactory;
			foreach (var incoTerm in invoiceIncoTerms)
			{
				if (!incoTermAndCustomsChargeFactory.CanThisIncoTermHaveThisCharge(incoTerm, chargeCode))
				{
					return false;
				}
			}
			return invoiceIncoTerms.Length != 0;
		}

		readonly BaseJobComInvoiceGroupHeader groupHeader;
		readonly BaseJobDeclaration declaration;

		#endregion
	}
}
