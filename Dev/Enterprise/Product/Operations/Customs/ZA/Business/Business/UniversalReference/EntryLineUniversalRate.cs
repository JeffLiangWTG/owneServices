using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business
{
	abstract class UniversalRateCalcData : IUniversalRateCalcData
	{
		protected UniversalRateCalcData(BusinessObjectFactory factory, ZDateTime dateOfValuation, ZDecimal customsValue, ZString countryOfOrigin, ZString preference)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			DateOfValuation = dateOfValuation.ToDateTime();
			CustomsValue = customsValue;
			CountryOfOrigin = countryOfOrigin;
			Preference = preference;
		}

		public readonly BusinessObjectFactory Factory;

		public IList<ZString> CountrySpecificTypeList
		{
			get { return countrySpecificTypeList ?? (countrySpecificTypeList = new List<ZString>()); }
		}
		List<ZString> countrySpecificTypeList;

		public IDictionary<string, decimal> CountrySpecificValueList
		{
			get
			{
				if (countrySpecificValueList == null)
				{
					countrySpecificValueList = new Dictionary<string, decimal>();
					countrySpecificValueList.Add(UniversalReferenceConstants.CusTariffCode.Schedule1Part1, decimal.Zero);
					foreach (ICodeDescription pair in AdditionalDutiesTariffTypeList)
					{
						countrySpecificValueList.Add(pair.Code, decimal.Zero);
					}
				}
				return countrySpecificValueList;
			}
		}
		Dictionary<string, decimal> countrySpecificValueList;

		public IList<Tuple<string, string>> AdditionalInformationList => new List<Tuple<string, string>>();

		AdditionalDutiesTariffTypeList AdditionalDutiesTariffTypeList => additionalDutiesTariffTypeList ?? (additionalDutiesTariffTypeList = Factory.GetCachedValue("ZAAdditionalDutiesTariffTypeList", () => new AdditionalDutiesTariffTypeList(Factory)));
		AdditionalDutiesTariffTypeList additionalDutiesTariffTypeList;

		public DateTime DateOfValuation { get; private set; }

		IDictionary<string, decimal> IUniversalRateCalcData.UnitOfMeasureValueList => OriginalUnitOfMeasureValueList.ToDictionary(x => x.Key, x => ZArchitecture.Core.Utilities.Round(x.Value, 2));

		protected Dictionary<string, decimal> OriginalUnitOfMeasureValueList => originalUunitOfMeasureValueList ?? (originalUunitOfMeasureValueList = new Dictionary<string, decimal>());
		Dictionary<string, decimal> originalUunitOfMeasureValueList;

		public decimal ValueForDuty
		{
			get
			{
				var result = CustomsValue;
				if (!CustomsValueFormula.IsEmpty)
				{
					if (!cachedValueForDuty.HasValue)
					{
						var vfdCalcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
						cachedValueForDuty = new ZDecimal(vfdCalcResult).RoundUsingCustomsValueRule();
					}
					result = cachedValueForDuty.Value;
				}
				return result;
			}
		}
		ZDecimal? cachedValueForDuty;

		internal ZString CustomsValueFormula
		{
			get { return customsValueFormula; }
			set
			{
				customsValueFormula = value;
				cachedValueForDuty = null;
			}
		}
		ZString customsValueFormula;

		public decimal CustomsValue { get; set; }
		public ZString CountryOfOrigin { get; private set; }
		public ZString Preference { get; private set; }

		internal void ResetCountrySpecificValueListEntry(ZString key)
		{
			var list = CountrySpecificValueList;
			if (!key.IsEmpty && list.ContainsKey(key))
			{
				list[key] = ZDecimal.Zero;
			}
		}

		internal void CountrySpecificValueListUpdateOrAddNew(ZString key, ZDecimal value)
		{
			UpdateDictionaryOrAddNew(CountrySpecificValueList, key, value);
		}

		protected void UpdateDictionaryOrAddNew(IDictionary<string, decimal> result, ZString key, ZDecimal value)
		{
			if (!key.IsEmpty)
			{
				if (!result.ContainsKey(key))
				{
					result.Add(key, value);
				}
				else
				{
					result[key] += value;
				}
			}
		}

		internal virtual void SetVPBAmount(decimal valueForDuty)
		{
		}

		IDictionary<string, string> IUniversalRateCalcData.MeursingExpressionList { get; } = new Dictionary<string, string>();
	}

	class EntryLineUniversalRate : UniversalRateCalcData
	{
		internal EntryLineUniversalRate(CusEntryLine entryLine, ZDecimal customsValue)
			: this(entryLine, customsValue, entryLine.RandomLine.JI_PrimaryPreference)
		{
		}
		internal EntryLineUniversalRate(CusEntryLine entryLine, ZDecimal customsValue, ZString preference)
			: this(entryLine.RandomLine, customsValue, preference)
		{
			this.entryLine = entryLine;
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				UpdateDictionaryOrAddNew(OriginalUnitOfMeasureValueList, invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsQuantity);
				UpdateDictionaryOrAddNew(OriginalUnitOfMeasureValueList, invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondQuantity);
				UpdateDictionaryOrAddNew(OriginalUnitOfMeasureValueList, invoiceLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsThirdQuantity);
			}
		}
		readonly CusEntryLine entryLine;

		EntryLineUniversalRate(JobComInvoiceLine invoiceLine, ZDecimal customsValue, ZString preference)
			: base(invoiceLine.Factory, invoiceLine.EffectiveAssessmentDate, customsValue, invoiceLine.JI_CountryOfOrigin, preference)
		{
		}

		internal override void SetVPBAmount(decimal valueForDuty)
		{
			entryLine.CL_VPBAmount = valueForDuty;
		}

		public void UpdateCustomsValue(decimal updatedCustomsValue)
		{
			CustomsValue = updatedCustomsValue;
		}
	}
}
