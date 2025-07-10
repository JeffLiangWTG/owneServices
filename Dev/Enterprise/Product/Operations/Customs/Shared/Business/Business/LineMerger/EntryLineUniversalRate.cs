using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class EntryLineUniversalRate : IUniversalRateCalcData
	{
		public EntryLineUniversalRate(CusEntryLine entryLine)
			: this(entryLine, entryLine.CL_CustomsValue)
		{
		}

		public EntryLineUniversalRate(CusEntryLine entryLine, ZDecimal customsValue, int valueOfDutyDecimalPlaces = 2)
		{
			this.EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			this.RandomLine = Argument.NotNull(entryLine.RandomLine, "entryLine.RandomLine");
			this.customsValue = customsValue;
			this.valueOfDutyDecimalPlaces = valueOfDutyDecimalPlaces;
			dateOfValuation = RandomLine.EffectiveAssessmentDate.ToDateTime();
		}
		protected readonly CusEntryLine EntryLine;
		protected readonly BaseJobComInvoiceLine RandomLine;
		readonly int valueOfDutyDecimalPlaces;

		public decimal CustomsValue => customsValue;
		readonly decimal customsValue;

		public DateTime DateOfValuation => dateOfValuation;
		readonly DateTime dateOfValuation;

		public decimal ValueForDuty
		{
			get
			{
				var result = customsValue;
				if (!CustomsValueFormula.IsEmpty)
				{
					if (!cachedValueForDuty.HasValue)
					{
						var vfdCalcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
						cachedValueForDuty = Utilities.Round(vfdCalcResult, valueOfDutyDecimalPlaces);
					}
					result = cachedValueForDuty.Value;
				}
				return result;
			}
		}
		ZDecimal? cachedValueForDuty;

		public ZString CustomsValueFormula
		{
			get => customsValueFormula;
			set
			{
				customsValueFormula = value;
				cachedValueForDuty = null;
			}
		}
		ZString customsValueFormula;

		public IDictionary<string, decimal> CountrySpecificValueList
		{
			get
			{
				if (fCountrySpecificValueList == null)
				{
					fCountrySpecificValueList = new Dictionary<string, decimal>();
					foreach (var data in GetCountrySpecificValues())
					{
						fCountrySpecificValueList.AddNewKeyOrAccumulateValue(data.uq, data.qty);
					}
				}
				return fCountrySpecificValueList;
			}
		}
		IDictionary<string, decimal> fCountrySpecificValueList;

		protected virtual IEnumerable<(ZString uq, ZDecimal qty)> GetCountrySpecificValues() => Enumerable.Empty<(ZString uq, ZDecimal qty)>();

		public IDictionary<string, decimal> UnitOfMeasureValueList
		{
			get
			{
				if (fUnitOfMeasureValueList == null)
				{
					fUnitOfMeasureValueList = new Dictionary<string, decimal>();
					foreach (var data in GetUnitOfMeasureValues())
					{
						fUnitOfMeasureValueList.AddNewKeyOrAccumulateValue(data.uq, data.qty);
					}
				}
				return fUnitOfMeasureValueList;
			}
		}
		IDictionary<string, decimal> fUnitOfMeasureValueList;

		protected virtual IEnumerable<(ZString uq, ZDecimal qty)> GetUnitOfMeasureValues()
		{
			foreach (BaseJobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
			{
				var uq = invoiceLine.JI_CustomsUnitQty;
				if (!uq.IsEmpty)
				{
					yield return (uq, invoiceLine.JI_CustomsQuantity);
				}
				uq = invoiceLine.JI_CustomsSecondUnitQty;
				if (!uq.IsEmpty)
				{
					yield return (uq, invoiceLine.JI_CustomsSecondQuantity);
				}
				uq = invoiceLine.JI_CustomsThirdUnitQty;
				if (!uq.IsEmpty)
				{
					yield return (uq, invoiceLine.JI_CustomsThirdQuantity);
				}
			}
		}

		public IList<Tuple<string, string>> AdditionalInformationList => fAdditionalInformationList ?? (fAdditionalInformationList = new List<Tuple<string, string>>());
		IList<Tuple<string, string>> fAdditionalInformationList;

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();
	}
}
