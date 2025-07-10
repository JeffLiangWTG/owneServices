using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public sealed class UniversalRateCustomsUnitDefaultingStrategy<TJobComInvoiceLine> : TariffCustomsUnitDefaultingStrategy<TJobComInvoiceLine>
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public UniversalRateCustomsUnitDefaultingStrategy(IsConvertibleFrom isConvertibleFrom = null)
			: this((invoiceLine) => invoiceLine.UniversalTariff, (invoiceLine) => [invoiceLine.UniversalDutyRate], isConvertibleFrom: isConvertibleFrom)
		{
		}

		public UniversalRateCustomsUnitDefaultingStrategy(Func<TJobComInvoiceLine, IEnumerable<RateView>> getRates, IEnumerable<ZPropertyInfo> propertyInfos, IsConvertibleFrom isConvertibleFrom = null)
			: this((invoiceLine) => invoiceLine.UniversalTariff, getRates, propertyInfos: propertyInfos, isConvertibleFrom: isConvertibleFrom)
		{
		}

		public UniversalRateCustomsUnitDefaultingStrategy(
			Func<TJobComInvoiceLine, TariffView> getUniversalTariff,
			Func<TJobComInvoiceLine, IEnumerable<RateView>> getRates,
			IEnumerable<ZPropertyInfo> propertyInfos = null,
			IsConvertibleFrom isConvertibleFrom = null)
			: base(getUniversalTariff)
		{
			this.getRates = getRates;
			PropertyInfos = propertyInfos;
			IsConvertibleFrom = isConvertibleFrom ?? ((unitQty, list, countryCode, factory) => false);
		}

		readonly Func<TJobComInvoiceLine, IEnumerable<RateView>> getRates;

		public IsConvertibleFrom IsConvertibleFrom { get; }

		IEnumerable<ZPropertyInfo> PropertyInfos { get; }

		ZString diagnoseInfo;

		public override void Initialise(TJobComInvoiceLine invoiceLine)
		{
			//This is diagnose message for WI00384643. We might be able to track stacktrace later if we can get narrow down information.
			diagnoseInfo = $@"HasInv={invoiceLine.InvoiceHeader != null};HasDec={invoiceLine.Declaration != null};DecApp={invoiceLine.Declaration?.JE_ApplicationCode}";
			base.Initialise(invoiceLine);

			invoiceLine.JI_CountryOfOriginInfo.ValueChanged += ValueChanged;
			invoiceLine.JI_PrimaryPreferenceInfo.ValueChanged += ValueChanged;
			invoiceLine.JI_ConcessionOrderInfo.ValueChanged += ValueChanged;

			deinitialiseActions.Add(() =>
			{
				invoiceLine.JI_CountryOfOriginInfo.ValueChanged -= ValueChanged;
				invoiceLine.JI_PrimaryPreferenceInfo.ValueChanged -= ValueChanged;
				invoiceLine.JI_ConcessionOrderInfo.ValueChanged -= ValueChanged;
			});

			if (PropertyInfos != null)
			{
				foreach (var propertyInfo in PropertyInfos)
				{
					propertyInfo.ValueChanged += ValueChanged;
					deinitialiseActions.Add(() =>
					{
						propertyInfo.ValueChanged -= ValueChanged;
					});
				}
			}

			void ValueChanged(object sender, EventArgs args)
			{
				DefaultUOMs(invoiceLine);
			}
		}

		public override void DefaultUOMs(TJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.UseUniversalTariff)
			{
				var itariff = getTariff(invoiceLine);
				var rates = getRates(invoiceLine)?.WhereNotNull();
				if (itariff is TariffView tariff)
				{
					var countryOfOrigin = invoiceLine.JI_CountryOfOrigin;

					var listOfUOMType = new ZString[]
					{
						Constants.UnitOfMeasureTypes.StatisticalUOMType,
						Constants.UnitOfMeasureTypes.AdditionalUOMType,
						Constants.UnitOfMeasureTypes.CustomsUOM3Type,
						Constants.UnitOfMeasureTypes.CustomsUOM4Type,
						Constants.UnitOfMeasureTypes.CustomsUOM5Type
					}.ToHashSet();

					var tariffUoms = tariff.UnitsOfMeasure
						.Where(uom => listOfUOMType.Contains(uom.ZZ8_Type.ToUpperInvariant()));

					if (!countryOfOrigin.IsEmpty)
					{
						tariffUoms = tariffUoms.Where(uom => uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == countryOfOrigin) ?? true);
					}

					var tariffUomsString = tariffUoms.OrderBy(x => x.ZZ8_Type).ThenBy(x => x.ZZ8_UOM).Select(x => x.ZZ8_UOM).ToArray();

					var rateUoms = rates?.SelectMany(x => x.UnitsOfMeasure).Select(x => x.ZXG_UOM).Where(x => !IsConvertibleFrom(x, tariffUomsString, invoiceLine.CustomsCountryCode, invoiceLine.Factory)).OrderBy(x => x) ?? Enumerable.Empty<ZString>();
					var distinctUOMs = new Queue<ZString>(tariffUomsString.Union(rateUoms.Where(x => !tariffUomsString.Contains(x))).Distinct());

					SetUnit(invoiceLine.JI_CustomsUnitQtyInfo);
					SetUnit(invoiceLine.JI_CustomsSecondUnitQtyInfo);
					SetUnit(invoiceLine.JI_CustomsThirdUnitQtyInfo);
					SetUnit(invoiceLine.JI_CustomsFourthUnitQtyInfo);
					SetUnit(invoiceLine.JI_CustomsFifthUnitQtyInfo);

					void SetUnit(ZPropertyInfo info)
					{
						info.Value = (distinctUOMs.Any() ? distinctUOMs.Dequeue() : ZString.Empty).SubstringSafe(0, info.MaxLength);
					}
				}
			}
			else
			{
				ErrorReporter.ReportOnce("The UniversalRateCustomsUnitDefaultingStrategy cannot be used with a JobComInvoiceLine where UseUniversalTariff is set to False. " + $"Initialise: {diagnoseInfo} Exception occur: DecApp={invoiceLine.Declaration?.JE_ApplicationCode}");
			}
		}
	}
}
