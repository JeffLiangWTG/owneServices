using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceLineApportionChargeValidation : EU.Business.Declaration.InvoiceLineApportionChargeValidation
	{
		public InvoiceLineApportionChargeValidation(InvoiceLineApportionCharge invoiceLineApportionCharge) : base(invoiceLineApportionCharge)
		{
		}

		public new InvoiceLineApportionCharge Parent => (InvoiceLineApportionCharge)base.Parent;

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();

			var chargeType = Parent.J7_ChargeType;

			if (TRIncotermChargeCodeList.IsTotalCharge(chargeType))
			{
				var invoiceLineCharges = Parent.InvoiceLine.Charges.Cast<JobComInvCharge>().Union(Parent.InvoiceLine.ApportionedCharges).Where(charge => charge != Parent);
				var isLocalTotal = chargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges;

				var invoiceLineOtherCharges = isLocalTotal
					? invoiceLineCharges.Where(charge => charge.IsLocalCharge())
					: invoiceLineCharges.Where(charge => charge.IsForeignCharge());

				var invoiceLineOtherChargesAmount = invoiceLineOtherCharges.Sum(charge => charge.J7_Amount);

				if (invoiceLineOtherChargesAmount != Parent.J7_Amount)
				{
					var localOrForeign = isLocalTotal ? Res.GetString("817A409B-1151-4D52-B430-9691B2938023", "Local") : Res.GetString("08EA04B6-67C1-4E13-8A74-A0D92F5FCD59", "Foreign");

					Parent.J7_AmountInfo.AddMessageError(Res.GetString(
						"BC4DD15F-AF2E-413C-81AB-C07529C64D2C",
						@"Sum of {0} charges should be equal to the amount of {1} charge.
Summed {0} amount: {2}, {1}: {3}",
						localOrForeign,
						chargeType,
						invoiceLineOtherChargesAmount,
						Parent.J7_Amount
					));
				}
			}
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();

			var chargeType = Parent.J7_ChargeType;

			if ((Parent.IsLocalCharge() || chargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges) && Parent.J7_RX_NKCurrency != Core.Constants.CurrencyCodes.Turkey)
			{
				Parent.J7_RX_NKCurrencyInfo.AddMessageError(Res.GetString(
					"F1FED553-6A65-4D18-9E49-0A867C30C57A", "Currency of {0} can only be {1}.",
					Parent.Lookups.ChargeTypeList.GetDescriptionFromCode(Parent.J7_ChargeType),
					Core.Constants.CurrencyCodes.Turkey
				));
			}
		}
	}
}
