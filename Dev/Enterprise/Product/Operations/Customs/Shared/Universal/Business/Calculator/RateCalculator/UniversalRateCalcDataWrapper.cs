using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.Universal
{
	public class UniversalRateCalcDataWrapper
	{
		public UniversalRateCalcDataWrapper(IUniversalRateCalcData data, FormulaErrorListener errorListener)
		{
			this.data = Argument.NotNull(data, "data");
			this.errorListener = Argument.NotNull(errorListener, "errorListener");
		}
		internal readonly FormulaErrorListener errorListener;
		readonly IUniversalRateCalcData data;

		public DateTime DateOfValuation { get { return data.DateOfValuation; } }
		public decimal ValueForDuty { get { return data.ValueForDuty; } }
		public decimal CustomsValue { get { return data.CustomsValue; } }

		public IDictionary<string, decimal> UnitOfMeasureValueList
		{
			get
			{
				if (unitOfMeasureValueList == null)
				{
					unitOfMeasureValueList = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
					CopyData(data.UnitOfMeasureValueList, unitOfMeasureValueList, "UnitOfMeasureValueList");
				}
				return unitOfMeasureValueList;
			}
		}
		IDictionary<string, decimal> unitOfMeasureValueList;

		public IDictionary<string, decimal> CountrySpecificValueList
		{
			get
			{
				if (countrySpecificValueList == null)
				{
					countrySpecificValueList = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
					CopyData(data.CountrySpecificValueList, countrySpecificValueList, "CountrySpecificValueList");
				}
				return countrySpecificValueList;
			}
		}
		IDictionary<string, decimal> countrySpecificValueList;

		public IDictionary<string, string> MeursingExpressionList
		{
			get
			{
				if (meursingExpressionList == null)
				{
					meursingExpressionList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
					CopyData(data.MeursingExpressionList, meursingExpressionList, "MeursingExpressionList");
				}
				return meursingExpressionList;
			}
		}
		IDictionary<string, string> meursingExpressionList;

		public IDictionary<string, QuestionForFormulaSpecificValue> FormulaSpecificValueList
		{
			get
			{
				if (formulaSpecificValueList == null)
				{
					formulaSpecificValueList = new Dictionary<string, QuestionForFormulaSpecificValue>(StringComparer.OrdinalIgnoreCase);
				}
				return formulaSpecificValueList;
			}
		}
		IDictionary<string, QuestionForFormulaSpecificValue> formulaSpecificValueList;

		public IList<Tuple<string, string>> AdditionalInformationList
		{
			get
			{
				if (additionalInformationList == null)
				{
					additionalInformationList = new List<Tuple<string, string>>();

					foreach (var additionalInfo in data.AdditionalInformationList)
					{
						additionalInformationList.Add(additionalInfo.Item1, additionalInfo.Item2);
					}
				}
				return additionalInformationList;
			}
		}
		IList<Tuple<string, string>> additionalInformationList;

		void CopyData<T>(IDictionary<string, T> source, IDictionary<string, T> destination, string dictionaryType)
		{
			if (source != null)
			{
				foreach (var pair in source)
				{
					if (destination.ContainsKey(pair.Key))
					{
						errorListener.Report(FormulaVisitErrorType.DuplicateKeyValue, Res.GetString("0d9f1724-c023-4156-920f-6fc343e517a8", "{0} already has a value for '{1}'; this value will be ignored '{2}'", dictionaryType, pair.Key, pair.Value));
					}
					else
					{
						destination.Add(pair.Key, pair.Value);
					}
				}
			}
		}
	}
}
