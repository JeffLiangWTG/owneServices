using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public static class BaseJobComInvHeaderChargeExtensionMethods
	{
		public static bool IsAdjustmentChargeToInvoiceAmount(this BaseJobComInvHeaderCharge charge)
		{
			return charge.J7_IsDutiable && charge.J7_IsNotIncludedInInvoice || !charge.J7_IsDutiable && !charge.J7_IsNotIncludedInInvoice || charge.IsDiscount;
		}
	}

	public class EntrySummary7501Invoice : NonPersistentBusinessObject, IObsoleteValidation
	{
		public EntrySummary7501Invoice(JobComInvoiceHeader invoice, ZInt invSeq, ZBool mixedRelationship)
			: base(invoice.Factory)
		{
			Argument.NotNull(invoice, "invoice"); // Used for an exception message
			this.invoice = invoice;
			this.invoiceSequence = invSeq;
			this.mixedRelationship = mixedRelationship;

			CombineCharges(invoice.Charges);
			CombineCharges(invoice.GroupCharges);
			CombineAMMVLineCharges(invoice);
		}

		void CombineCharges(IEnumerable<JobComInvCharge> charges)
		{
			foreach (BaseJobComInvHeaderCharge charge in charges)
			{
				if (charge.J7_Amount > 0 && charge.Currency != null && charge.IsAdjustmentChargeToInvoiceAmount())
				{
					ChargeWrapper chargeWrapper = null;

					if (AdjustmentCharges.TryGetValue(GetAdjustmentChargeKey(charge), out chargeWrapper))
					{
						chargeWrapper.Amount += charge.J7_Amount;
						chargeWrapper.AmountInUSD += charge.MoneyInLocalCurrency.Amount;
					}

					if (chargeWrapper == null)
					{
						chargeWrapper = new ChargeWrapper(charge);
						invoiceCharges.Add(chargeWrapper);

						AdjustmentCharges.Add(GetAdjustmentChargeKey(charge), chargeWrapper);
					}
				}
			}
		}

		void CombineAMMVLineCharges(JobComInvoiceHeader invoice)
		{
			var uSDAdjustmentWithAnyAMMV = GetCombinedAMMVLineLevelChargesInUSD(invoice);
			if (uSDAdjustmentWithAnyAMMV > 0)
			{
				var ammvCharge = new ChargeWrapper();
				ammvCharge.ChargeType = USCustomsChargeTypeList.Codes.AdditionCharge;
				ammvCharge.ChargeDescription = ChargeWrapper.AdditionalCharge;
				ammvCharge.Amount = uSDAdjustmentWithAnyAMMV;
				ammvCharge.Currency = Core.Constants.CurrencyCodes.UnitedStates;
				ammvCharge.IsIncludedInInvoice = false;
				ammvCharge.IsDutiable = true;
				ammvCharge.ExchangeRate = 1m;
				ammvCharge.IsDiscount = false;

				ChargeWrapper usdAdjWrapper = null;
				if (AdjustmentCharges.TryGetValue("ADD|USD|1.000000", out usdAdjWrapper))
				{
					usdAdjWrapper.Amount += ammvCharge.Amount;
					usdAdjWrapper.AmountInUSD += ammvCharge.Amount;
				}

				if (usdAdjWrapper == null)
				{
					invoiceCharges.Add(ammvCharge);
					AdjustmentCharges.Add("ADD|USD|1.000000", ammvCharge);
				}
			}
		}

		ZDecimal GetCombinedAMMVLineLevelChargesInUSD(JobComInvoiceHeader invoice)
		{
			ZDecimal accumulatedAMMV = 0m;

			foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
			{
				if (invoiceLine.US_AMMVPerUnit > 0 || invoiceLine.US_AMMVPercentage > 0)
				{
					foreach (InvoiceLineApportionCharge charge in invoiceLine.ApportionedCharges)
					{
						if (charge.IsAMMV())
						{
							accumulatedAMMV += charge.J7_Amount;
						}
					}
				}
			}

			return accumulatedAMMV;
		}

		Dictionary<ZString, ChargeWrapper> AdjustmentCharges
		{
			get
			{
				return adjustmentCharges ?? (adjustmentCharges = new Dictionary<ZString, ChargeWrapper>());
			}
		}
		Dictionary<ZString, ChargeWrapper> adjustmentCharges;

		ZString GetAdjustmentChargeKey(BaseJobComInvHeaderCharge charge)
		{
			var result = charge.J7_ChargeType + "|" + charge.J7_RX_NKCurrency + "|" + charge.J7_ExchangeRate.ToString(6);
			if (!charge.IsDiscount)
			{
				result += "|" + charge.J7_IsDutiable.ToString();
			}
			return result;
		}

		internal class ChargeWrapper
		{
			public ChargeWrapper()
			{
			}

			public ChargeWrapper(BaseJobComInvHeaderCharge charge)
			{
				ChargeType = charge.J7_ChargeType;
				ChargeDescription = GetChargeDescription(charge.J7_ChargeDescription, charge.J7_ChargeType);
				Amount = charge.J7_Amount;
				Currency = charge.J7_RX_NKCurrency;
				AmountInUSD = charge.MoneyInLocalCurrency.Amount;
				IsDutiable = charge.J7_IsDutiable;
				IsIncludedInInvoice = !charge.J7_IsNotIncludedInInvoice;
				IsIncludedInLine = charge.J7_IsIncludedInITOT;
				ExchangeRate = charge.J7_ExchangeRate;
				IsDiscount = charge.IsDiscount;
			}

			public ZString ChargeType;
			public ZString ChargeDescription;
			public ZDecimal Amount;
			public ZString Currency;

			public ZDecimal AmountInUSD;
			public ZDecimal ExchangeRate;

			public ZBool IsDutiable;
			public ZBool IsIncludedInInvoice;
			public ZBool IsIncludedInLine;
			public ZBool IsDiscount;

#if DEBUG
			public
#endif
			string GetChargeDescription(string chargeDesc, string chargeCode)
			{
				switch (chargeCode)
				{
					case USCustomsChargeTypeList.Codes.AdditionCharge:
						return AdditionalCharge;

					case USCustomsChargeTypeList.Codes.DeductionCharge:
						return DeductionCharge;

					default:
						return chargeDesc;
				}
			}

			internal const string AdditionalCharge = "Additional Charge";
			internal const string DeductionCharge = "Deduction Charge";
		}

		readonly JobComInvoiceHeader invoice;
		readonly ZInt invoiceSequence;
		readonly ZBool mixedRelationship;

		readonly List<ChargeWrapper> invoiceCharges = new List<ChargeWrapper>();

		#region IEntrySummaryInvoices Members

		public ZString InvoiceNo
		{
			get
			{
				ZString result = ZString.Empty;

				result = invoiceSequence.ToString().PadLeft(3, '0');
				if (!invoice.JZ_InvoiceNumber.IsEmpty)
				{
					result = result + "/" + invoice.JZ_InvoiceNumber.KeepChars(ZString.AlphanumericCharacters + "-");
				}

				return result;
			}
		}

		public ZString RelEntryNo
		{
			get { return invoice.US_ReleaseEntryNumber.Left(14); }
		}

		public ZDecimal InvoiceValue
		{
			get { return InvoiceCurrency.IsEmpty ? ZDecimal.Zero : InvoiceValueCore; }
		}

		ZDecimal InvoiceValueCore
		{
			get { return invoice.JZ_InvoiceAmount + AnyDiscountIncludedInInvoiceAmount; }
		}

		public ZString InvoiceCurrency
		{
			get
			{
				ZString result = ZString.Empty;

				if (!invoice.JZ_RX_NKInvoice_Currency.IsEmpty && invoice.Invoice_Currency.RX_Code != "USD")
				{
					result = "Invoice Value " + invoice.Invoice_Currency.RX_Code;
				}

				return result;
			}
		}

		public ZDecimal ExchangeRate
		{
			get
			{
				ZDecimal exchangeRate = 0;

				if (!invoice.JZ_RX_NKInvoice_Currency.IsEmpty && invoice.Invoice_Currency.RX_Code != "USD")
				{
					exchangeRate = invoice.JZ_InvoiceCurrExRate;
				}

				return exchangeRate;
			}
		}

		public ZDecimal InvoiceValueInUSD
		{
			get
			{
				return invoice.JZ_InvoiceCurrExRate > 0 ? new ZDecimal(InvoiceValueCore * invoice.JZ_InvoiceCurrExRate).Round(2) : InvoiceValueCore;
			}
		}

		ZDecimal AnyDiscountIncludedInInvoiceAmount
		{
			get
			{
				ZDecimal value = 0;

				foreach (ChargeWrapper invoiceCharge in invoiceCharges)
				{
					if (invoiceCharge.IsDiscount && invoiceCharge.IsIncludedInInvoice)
					{
						value += invoiceCharge.Amount;
					}
				}

				return value;
			}
		}

		public ZString TransactionsRelatedIndicator
		{
			get { return mixedRelationship ? ZString.Empty : invoice.InvoiceLines.Count > 0 ? invoice.InvoiceLines[0].US_TransactionsRelated : ZString.Empty; }
		}

		#region Adjustment Item 1

		public ZString AdjustmentItem1
		{
			get { return GetChargeDescription(0); }
		}

		public ZDecimal Adjustment1
		{
			get { return GetAdjustmentAmount(0); }
		}

		public ZDecimal Adjustment1ExchangeRate
		{
			get { return GetAdjustmentExchangeRate(0); }
		}

		public ZString AdjustmentItem1USD
		{
			get { return AdjustmentItemDesc(0); }
		}

		public ZDecimal Adjustment1USD
		{
			get { return GetAdjustmentUSD(0); }
		}

		#endregion

		#region Adjustment Item 2

		public ZString AdjustmentItem2
		{
			get { return GetChargeDescription(1); }
		}

		public ZDecimal Adjustment2
		{
			get { return GetAdjustmentAmount(1); }
		}

		public ZDecimal Adjustment2ExchangeRate
		{
			get { return GetAdjustmentExchangeRate(1); }
		}

		public ZString AdjustmentItem2USD
		{
			get { return AdjustmentItemDesc(1); }
		}

		public ZDecimal Adjustment2USD
		{
			get { return GetAdjustmentUSD(1); }
		}

		#endregion

		#region Adjustment Item 3

		public ZString AdjustmentItem3
		{
			get { return GetChargeDescription(2); }
		}

		public ZDecimal Adjustment3
		{
			get { return GetAdjustmentAmount(2); }
		}

		public ZDecimal Adjustment3ExchangeRate
		{
			get { return GetAdjustmentExchangeRate(2); }
		}

		public ZString AdjustmentItem3USD
		{
			get { return AdjustmentItemDesc(2); }
		}

		public ZDecimal Adjustment3USD
		{
			get { return GetAdjustmentUSD(2); }
		}

		#endregion

		#region Adjustment Item 4

		public ZString AdjustmentItem4
		{
			get { return GetChargeDescription(3); }
		}

		public ZDecimal Adjustment4
		{
			get { return GetAdjustmentAmount(3); }
		}

		public ZDecimal Adjustment4ExchangeRate
		{
			get { return GetAdjustmentExchangeRate(3); }
		}

		public ZString AdjustmentItem4USD
		{
			get { return AdjustmentItemDesc(3); }
		}

		public ZDecimal Adjustment4USD
		{
			get { return GetAdjustmentUSD(3); }
		}

		#endregion

		#region Adjustment Item 5

		public ZString AdjustmentItem5
		{
			get { return GetChargeDescription(4); }
		}

		public ZDecimal Adjustment5
		{
			get { return GetAdjustmentAmount(4); }
		}

		public ZDecimal Adjustment5ExchangeRate
		{
			get { return GetAdjustmentExchangeRate(4); }
		}

		public ZString AdjustmentItem5USD
		{
			get { return AdjustmentItemDesc(4); }
		}

		public ZDecimal Adjustment5USD
		{
			get { return GetAdjustmentUSD(4); }
		}

		#endregion

		public ZDecimal USOriginalGoodsValue
		{
			get
			{
				if (fUSOriginalGoodsValue == null)
				{
					fUSOriginalGoodsValue = ZDecimal.Zero;

					foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
					{
						fUSOriginalGoodsValue += invoiceLine.US_98GoodsValue;
					}
				}
				return fUSOriginalGoodsValue.Value;
			}
		}
		ZDecimal? fUSOriginalGoodsValue;

		public ZDecimal TEV
		{
			get
			{
				if (!tev.HasValue)
				{
					tev = ZDecimal.Zero;

					var declaration = invoice.JobDeclaration;
					if (declaration != null)
					{
						var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
						if (entry != null)
						{
							foreach (CusEntryLine entryLine in entry.MergedLines)
							{
								if (entryLine.RandomLine.JI_JZ == invoice.PK && !entryLine.IsSetXLine)
								{
									tev += entryLine.CL_CustomsValue;
								}
							}
						}
					}
				}
				return tev.Value;
			}
		}
		ZDecimal? tev;

		#endregion

		#region Implementation

		ZDecimal GetAdjustmentUSD(int index)
		{
			var result = ZDecimal.Zero;

			if (invoiceCharges.Count > index)
			{
				var charge = invoiceCharges[index];
				if (charge.ExchangeRate != 1 && charge.ExchangeRate > 0)
				{
					result = new ZDecimal(charge.Amount * charge.ExchangeRate).Round(2);
				}
				else
				{
					result = charge.Amount;
				}
			}

			return result;
		}

		ZString GetChargeDescription(int index)
		{
			var result = ZString.Empty;

			if (invoiceCharges.Count > index)
			{
				var charge = invoiceCharges[index];
				result = charge.ChargeDescription + (charge.ExchangeRate != 1m ? " " + charge.Currency : string.Empty);
			}

			return result;
		}

		ZDecimal GetAdjustmentAmount(int index)
		{
			var result = ZDecimal.Zero;

			if (invoiceCharges.Count > index)
			{
				var charge = invoiceCharges[index];
				result = charge.ExchangeRate != 1m ? charge.Amount : ZDecimal.Zero;
			}

			return result;
		}

		ZDecimal GetAdjustmentExchangeRate(int index)
		{
			var result = ZDecimal.Zero;

			if (invoiceCharges.Count > index)
			{
				var charge = invoiceCharges[index];
				result = charge.ExchangeRate != 1m ? charge.ExchangeRate : ZDecimal.Zero;
			}

			return result;
		}

		ZString AdjustmentItemDesc(int index)
		{
			var result = ZString.Empty;

			if (invoiceCharges.Count > index)
			{
				var charge = invoiceCharges[index];
				ZString adjType = ZString.Empty;

				if (charge.IsDutiable && !charge.IsIncludedInInvoice)
				{
					adjType = "(+) ";
				}

				if ((!charge.IsDutiable && charge.IsIncludedInInvoice) || charge.IsDiscount)
				{
					adjType = "(-) ";
				}

				ZString currCode = charge.Currency != "USD" ? " USD" : "";

				result = adjType + charge.ChargeDescription + currCode;
			}

			return result;
		}

		#endregion
	}
}
