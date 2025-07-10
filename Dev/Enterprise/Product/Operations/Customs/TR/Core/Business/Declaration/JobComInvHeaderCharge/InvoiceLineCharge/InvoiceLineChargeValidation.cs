namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceLineChargeValidation : EU.Business.Declaration.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();

			var currency = Parent.J7_RX_NKCurrency;
			var chargeType = Parent.J7_ChargeType;
			var targetInfo = Parent.J7_RX_NKCurrencyInfo;
			if (Parent.IsLocalCharge())
			{
				if (currency != Core.Constants.CurrencyCodes.Turkey)
				{
					targetInfo.AddMessageError(Res.GetString(
						"75AD416A-77CC-4BD9-AFE1-7D8561F36598", "Currency of {0} can only be {1}.",
						Parent.Lookups.ChargeTypeList.GetDescriptionFromCode(chargeType),
						Core.Constants.CurrencyCodes.Turkey
					));
				}
			}
			else if (Parent.IsForeignCharge())
			{
				var invoiceChargeCurrency = Parent.GetCurrencyFromInvoiceChargeWithSameChargeType();
				if (!invoiceChargeCurrency.IsEmpty && invoiceChargeCurrency != currency)
				{
					targetInfo.AddMessageError(Res.GetString(
							"69CB73FF-A66E-47F2-807B-B1F51AEA95F6",
							@"{0} charges in the Invoice Lines should be equal to the currency of {0} charge in the Invoice Header. Invoice Lines: {1}, Invoice Header input: {2}", chargeType, currency, invoiceChargeCurrency
						));
				}
			}
		}

		new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
