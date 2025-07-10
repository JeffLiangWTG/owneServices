using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ImportInvoiceLineHelper
	{
		internal ImportInvoiceLineHelper(IInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly IInvoiceLine invoiceLine;

		public ZString GetPartNo()
		{
			var result = ZString.Empty;

			if (invoiceLine.ProductParentTariffLine is IInvoiceLine productParentTariffLine)
			{
				result = productParentTariffLine.JI_PartNo;
			}
			else if (invoiceLine.ParentTariffLine is IInvoiceLine parentTariffLine)
			{
				result = parentTariffLine.JI_PartNo;
			}
			else
			{
				result = invoiceLine.BaseJI_PartNo;
			}

			return result;
		}

		public USCTariff ImportSupTariff
		{
			get
			{
				var factory = invoiceLine.Factory;
				var supTariff = invoiceLine.US_SupTariff;
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				return factory.GetCachedValue(supTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + effectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(factory).LoadBestMatch(supTariff, effectiveDateForDutyRate);
					});
			}
		}

		public USCTariff ImportSupAdditionalTariff1
		{
			get
			{
				var factory = invoiceLine.Factory;
				var supAdditionalTariff = invoiceLine.US_SupAdditionalTariff1;
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				return factory.GetCachedValue(supAdditionalTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + effectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(factory).LoadBestMatch(supAdditionalTariff, effectiveDateForDutyRate);
					});
			}
		}

		public USCTariff ImportSupAdditionalTariff2
		{
			get
			{
				var factory = invoiceLine.Factory;
				var supAdditionalTariff = invoiceLine.US_SupAdditionalTariff2;
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				return factory.GetCachedValue(supAdditionalTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + effectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(factory).LoadBestMatch(supAdditionalTariff, effectiveDateForDutyRate);
					});
			}
		}

		public USCTariff ImportSupAdditionalTariff3
		{
			get
			{
				var factory = invoiceLine.Factory;
				var supAdditionalTariff = invoiceLine.US_SupAdditionalTariff3;
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				return factory.GetCachedValue(supAdditionalTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + effectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(factory).LoadBestMatch(supAdditionalTariff, effectiveDateForDutyRate);
					});
			}
		}

		public USCTariff ImportSupAdditionalTariff4
		{
			get
			{
				var factory = invoiceLine.Factory;
				var supAdditionalTariff = invoiceLine.US_SupAdditionalTariff4;
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				return factory.GetCachedValue(supAdditionalTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + effectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(factory).LoadBestMatch(supAdditionalTariff, effectiveDateForDutyRate);
					});
			}
		}

		public USCTariff ImportSupAdditionalTariff5
		{
			get
			{
				var factory = invoiceLine.Factory;
				var supAdditionalTariff = invoiceLine.US_SupAdditionalTariff5;
				var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
				return factory.GetCachedValue(supAdditionalTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + effectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(factory).LoadBestMatch(supAdditionalTariff, effectiveDateForDutyRate);
					});
			}
		}

		public ZDecimal GetTotalOriginalGoodsValueInUSD()
		{
			var amount = ZDecimal.Zero;

			if (invoiceLine.IsSetXLine && !invoiceLine.IsRecon)
			{
				foreach (var childLine in invoiceLine.ChildLines)
				{
					if (childLine.IsVParentLine)
					{
						foreach (var vChild in childLine.ChildLines)
						{
							amount += vChild.TotalOriginalGoodsValueInUSD;
						}
					}
					else if (childLine.IsSetVLine)
					{
						amount += childLine.TotalOriginalGoodsValueInUSD;
					}
				}
			}
			else
			{
				amount = invoiceLine.US_98GoodsValue;
				var f98ValueInvCurr = invoiceLine.US_98ValueInvCurr;
				if (!f98ValueInvCurr.IsEmpty && invoiceLine.CurrencyConverter is CurrencyConverter currencyConverter && invoiceLine.Invoice_Currency is RefCurrency invoice_Currency)
				{
					amount += currencyConverter.ConvertExact(new Money(f98ValueInvCurr, invoice_Currency), USD).Amount;
				}
			}

			return amount;
		}

		public RefCurrency USD
		{
			get
			{
				var factory = invoiceLine.Factory;
				return factory.GetCachedValue("USD", () => RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.UnitedStates));
			}
		}

		public ZDate GetEffectiveDateForDutyRate()
		{
			var date = invoiceLine.FTZAdmissionEffectiveDateForDutyRate;
			if (!date.IsValid)
			{
				var privilegedStatusDate = invoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign ? invoiceLine.US_PrivilegedStatusDate : ZDateTime.Invalid;
				if (privilegedStatusDate.IsValid)
				{
					date = privilegedStatusDate.Date;
				}
				else
				{
					date = invoiceLine.ImportEffectiveDateForDutyRate;
				}
			}
			return date;
		}

		public ZBool GetIsVChildLine() => invoiceLine.IsSetVLine && invoiceLine.ParentTariffLine is IInvoiceLine parentTariffLine && parentTariffLine.IsSetVLine;

		public IEnumerable<IInvoiceLine> GetChildVLines() => invoiceLine.ChildLines.Where(x => x.IsSetVLine);

		public ZBool GetIsSetXLine() => invoiceLine.HasDeclaration && (invoiceLine.IsACE ?
						invoiceLine.US_SetInd == SecondarySpecProgIndicatorList.Codes.X :
						invoiceLine.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.X);

		public ZBool GetIsSetVLine() => invoiceLine.HasDeclaration && (invoiceLine.IsACE ?
						invoiceLine.US_SetInd == SecondarySpecProgIndicatorList.Codes.V :
						invoiceLine.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.V);

		public ZBool GetIsVParentLine() => invoiceLine.IsSetVLine && invoiceLine.ChildVLines.Any();
	}
}
