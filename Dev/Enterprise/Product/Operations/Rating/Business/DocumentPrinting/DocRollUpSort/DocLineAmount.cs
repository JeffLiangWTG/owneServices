using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort
{
	public sealed class DocLineAmount
	{
		public DocLineAmount()
		{
			InnerDictionary = new Dictionary<DocLineAmountProperty, BaseDocLineAmountValue>();
		}

		Dictionary<DocLineAmountProperty, BaseDocLineAmountValue> InnerDictionary { get; }

		public QuotationLineList GetQuotationLineList(RateLine rateLine)
		{
			var result = new QuotationLineList();

			foreach (var dict in InnerDictionary)
			{
				result.AddRange(dict.Value.GetQuotationLineList(rateLine));
			}

			return result;
		}

		public void SetFlat(string currency, decimal amount, string applyTo = default, bool setZero = false)
		{
			if (setZero || amount != 0m)
			{
				var value = new FlatDocLineAmountValue(currency, amount, applyTo);
				InnerDictionary[value.Property] = value;
			}
		}

		public void SetUnit(string currency, string unit, decimal amount, string applyTo = default, bool setZero = false)
		{
			if (setZero || amount != 0m)
			{
				var value = new UnitDocLineAmountValue(currency, unit, amount, applyTo);
				InnerDictionary[value.Property] = value;
			}
		}

		public void SetFirstAddAdditional(string currency, string unit, decimal first, decimal additional, bool setZero = false)
		{
			if (setZero || first != 0m || additional != 0m)
			{
				var value = new FirstPlusAdditionalDocLineAmountValue(currency, unit, first, additional);
				InnerDictionary[value.Property] = value;
			}
		}

		public void SetHighest(string currency, string description, string weightUnit, decimal weightRate, decimal weightFlat, string volumeUnit, decimal volumeRate, decimal volumeFlat)
		{
			var value = new HighestDocLineAmountValue(currency, description, weightUnit, weightRate, weightFlat, volumeUnit, volumeRate, volumeFlat);
			InnerDictionary[value.Property] = value;
		}

		public void SetAgency(string currency, string description, decimal agencyRate, string agencyRateDescription, decimal additionalRate, string additionalRateDescription, decimal costPerAdditionalRate, string costPerAdditionalRateDescription, string unit)
		{
			var value = new AgencyDocLineAmountValue(currency, description, agencyRate, agencyRateDescription, additionalRate, additionalRateDescription, costPerAdditionalRate, costPerAdditionalRateDescription, unit);
			InnerDictionary[value.Property] = value;
		}

		public void SetMin(string currency, decimal amount, string applyTo = default, bool setZero = false)
		{
			if (setZero || amount != 0m)
			{
				var value = new MinDocLineAmountValue(currency, amount, applyTo);
				InnerDictionary[value.Property] = value;
			}
		}

		public void SetMax(string currency, decimal amount, string applyTo = default, bool setZero = false)
		{
			if (setZero || amount != 0)
			{
				var value = new MaxDocLineAmountValue(currency, applyTo, amount);
				InnerDictionary[value.Property] = value;
			}
		}

		public void SetPercentage(string currency, string applyTo, string unit, decimal percent, bool setZero = false)
		{
			if (setZero || percent != 0m)
			{
				var value = new PercentageDocLineAmountValue(currency, applyTo, unit, percent);
				InnerDictionary[value.Property] = value;
			}
		}

		public void SetSliding(string currency, string operatorBreak, string unit, decimal rate, decimal flat)
		{
			var value = new SlidingDocLineAmountValue(currency, operatorBreak, unit, rate, flat);
			InnerDictionary[value.Property] = value;
		}

		public static DocLineAmount operator +(DocLineAmount docLineAmount1, DocLineAmount docLineAmount2)
		{
			var result = new DocLineAmount();

			var keys = Merge(docLineAmount1.InnerDictionary.Keys.ToArray(), docLineAmount2.InnerDictionary.Keys.ToArray());
			foreach (var key in keys)
			{
				if (docLineAmount1.InnerDictionary.TryGetValue(key, out var value1))
				{
					AddOrUpdate(result.InnerDictionary, key, value1);
				}

				if (docLineAmount2.InnerDictionary.TryGetValue(key, out var value2))
				{
					AddOrUpdate(result.InnerDictionary, key, value2);
				}
			}

			return result;
		}

		static DocLineAmountProperty[] Merge(DocLineAmountProperty[] array1, DocLineAmountProperty[] array2)
		{
			var hashSet = new HashSet<DocLineAmountProperty>();
			hashSet.UnionWith(array1);
			hashSet.UnionWith(array2);

			return hashSet.ToArray();
		}

		static void AddOrUpdate(Dictionary<DocLineAmountProperty, BaseDocLineAmountValue> dictionary, DocLineAmountProperty key, BaseDocLineAmountValue value)
		{
			if (!dictionary.TryGetValue(key, out var _))
			{
				dictionary[key] = value;
			}
			else
			{
				dictionary[key] = dictionary[key].Add(value);
			}
		}
	}
}
