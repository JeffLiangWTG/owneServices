using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.Business.Declaration
{
	[UserDefinedValues]
	public class InvoiceLineCharge : EU.Business.Declaration.InvoiceLineCharge, Integration.Customs.TR.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : JobComInvCharge.Schema
		{
			public const string Explanation = nameof(InvoiceLineCharge.Explanation);
		}

		[MaxLength(100)]
		[ReadOnlyMember(nameof(ExplanationReadOnly))]
		public ZString Explanation
		{
			get => this.GetUserDefinedValue<ZString>(nameof(Explanation));
			set
			{
				if (value != Explanation && !IsCopying)
				{
					CheckMaximumLength(ExplanationInfo, value);
					this.SetUserDefinedValue(nameof(Explanation), value);
					ExplanationInfo.RefreshBinding(value);
				}
			}
		}

		public ZPropertyInfo ExplanationInfo => GetZPropertyInfo(nameof(Explanation));

		bool IsChargeTypeOther => J7_ChargeType == TRIncotermChargeCodeList.Codes.OTH || J7_ChargeType == TRIncotermChargeCodeList.Codes.LOT;
		bool ExplanationReadOnly => !IsChargeTypeOther;

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				base.J7_ChargeType = value;
				if (TRIncotermChargeCodeList.IsLocalCharge(J7_ChargeType))
				{
					if (J7_RX_NKCurrency.IsEmpty)
					{
						J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
					}
				}
				else if (TRIncotermChargeCodeList.IsForeignCharge(J7_ChargeType))
				{
					var currencyFromInvoiceChargeWithSameChargeType = GetCurrencyFromInvoiceChargeWithSameChargeType();
					if (!currencyFromInvoiceChargeWithSameChargeType.IsEmpty && currencyFromInvoiceChargeWithSameChargeType != J7_RX_NKCurrency)
					{
						J7_RX_NKCurrency = currencyFromInvoiceChargeWithSameChargeType;
					}
				}

				ClearExplanationIfRequired();
			}
		}

		void ClearExplanationIfRequired()
		{
			if (!Explanation.IsEmpty && ExplanationReadOnly)
			{
				Explanation = ZString.Empty;
			}
		}

		public ZString GetCurrencyFromInvoiceChargeWithSameChargeType()
		{
			var result = ZString.Empty;
			if (InvoiceLine?.InvoiceHeader?.Charges.Cast<InvoiceCharge>().FirstOrDefault(charge => charge.J7_ChargeType == J7_ChargeType) is InvoiceCharge invoiceCharge)
			{
				result = invoiceCharge.J7_RX_NKCurrency;
			}
			return result;
		}

		protected override bool GetJ7_RX_NKCurrency_ReadOnly()
		{
			var result = base.GetJ7_RX_NKCurrency_ReadOnly();
			if (!result)
			{
				result = !J7_RX_NKCurrency.IsEmpty && TRIncotermChargeCodeList.IsForeignCharge(J7_ChargeType) && GetCurrencyFromInvoiceChargeWithSameChargeType() == J7_RX_NKCurrency;
			}
			return result;
		}

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);
	}
}
