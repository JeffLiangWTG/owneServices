using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class ConditionCalcDataForInvoiceLine : IUniversalRateCalcData
	{
		public ConditionCalcDataForInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		protected readonly BaseJobComInvoiceLine InvoiceLine;

		public DateTime DateOfValuation => InvoiceLine.EffectiveAssessmentDate.ToDateTime();

		public decimal ValueForDuty => InvoiceLine.JI_CustomsValue;

		public decimal CustomsValue => InvoiceLine.JI_CustomsValue;

		public IDictionary<string, decimal> UnitOfMeasureValueList
		{
			get
			{
				if (fUnitOfMeasureValueList == null)
				{
					fUnitOfMeasureValueList = new Dictionary<string, decimal>();
					AddToDictionaryIfNotExists(fUnitOfMeasureValueList, InvoiceLine.JI_CustomsUnitQty, InvoiceLine.JI_CustomsQuantity);
					AddToDictionaryIfNotExists(fUnitOfMeasureValueList, InvoiceLine.JI_CustomsSecondUnitQty, InvoiceLine.JI_CustomsSecondQuantity);
					AddToDictionaryIfNotExists(fUnitOfMeasureValueList, InvoiceLine.JI_CustomsThirdUnitQty, InvoiceLine.JI_CustomsThirdQuantity);
					AddUnitOfMeasureValueList(fUnitOfMeasureValueList);
					AddConviableUnitOfMeasureValues(fUnitOfMeasureValueList);
				}
				return fUnitOfMeasureValueList;
			}
		}
		IDictionary<string, decimal> fUnitOfMeasureValueList;

		protected virtual void AddUnitOfMeasureValueList(IDictionary<string, decimal> unitOfMeasureValueList) { }

		public IDictionary<string, decimal> CountrySpecificValueList
		{
			get
			{
				if (fCountrySpecificValueList == null)
				{
					fCountrySpecificValueList = new Dictionary<string, decimal>();
					AddCountrySpecificValues(fCountrySpecificValueList);
				}
				return fCountrySpecificValueList;
			}
		}
		IDictionary<string, decimal> fCountrySpecificValueList;

		protected virtual void AddCountrySpecificValues(IDictionary<string, decimal> countrySpecificValueList) { }

		protected void AddToDictionaryIfNotExists(IDictionary<string, decimal> keyValuePairs, ZString key, ZDecimal value)
		{
			if (!key.IsEmpty && !keyValuePairs.ContainsKey(key))
			{
				keyValuePairs.Add(key, value);
			}
		}

		protected virtual bool CustomsUnitConvertible(ZString fromUQ, ZString toUQ) => false;

		protected virtual ZDecimal CustomsUnitConvert(ZDecimal qty, ZString fromUQ, ZString toUQ) => 0m;

		protected virtual IEnumerable<ZString> CustomsUQList => InvoiceLine.Lookups.CustomsUQList.GetAllCodesZString();

		public IList<Tuple<string, string>> AdditionalInformationList => null;

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();

		protected void AddConviableUnitOfMeasureValues(IDictionary<string, decimal> unitOfMeasureValueList)
		{
			var customsUQs = CustomsUQList;

			foreach (var fromUQ in unitOfMeasureValueList.Keys.ToArray())
			{
				foreach (var toUQ in customsUQs.Where(x => !unitOfMeasureValueList.ContainsKey(x)).ToArray())
				{
					if (CustomsUnitConvertible(fromUQ, toUQ))
					{
						unitOfMeasureValueList.Add(toUQ, CustomsUnitConvert(unitOfMeasureValueList[fromUQ], fromUQ, toUQ));
					}
				}
			}
		}
	}
}
