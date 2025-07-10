using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class ConditionGeneratorHelper
	{
		public static IEnumerable<RefCusConditionValue> GetRatioConditionValues(IGroupedMeasureConditionKey groupedMeasureConditionKey, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, string supplementaryUnit)
		{
			Argument.NotNull(groupedMeasureConditionKey, nameof(groupedMeasureConditionKey));
			Argument.NotNull(measureConditionRecords, nameof(measureConditionRecords));

			var hasMoreThanThreeRatios = measureConditionRecords.Count() > 3;
			var refCusConditionValues = new List<RefCusConditionValue>();
			supplementaryUnit = string.IsNullOrEmpty(supplementaryUnit) ? "NAR" : supplementaryUnit.Trim();
			if (hasMoreThanThreeRatios)
			{
				refCusConditionValues.AddRange(
					from measureConditionData in measureConditionRecords
					where measureConditionData.MeasureAction == "28"
					select new RefCusConditionValue
					{
						ZX3_Value = measureConditionData.MeasureConditionCode == "M" ? $"VFD/[{measureConditionData.MeasureUnit}] = {measureConditionData.ConditionAmount}" :
					measureConditionData.MeasureConditionCode == "U" ? $"VFD/[{supplementaryUnit}] = {measureConditionData.ConditionAmount}" : $"[{measureConditionData.MeasureUnit}]/[{supplementaryUnit}] = {measureConditionData.ConditionAmount}",
						ZX3_ZX4_NKValueType = ConditionValueTypeFormula
					});
			}
			else
			{
				var conditions = new List<string>();

				foreach (var measureConditionData in measureConditionRecords)
				{
					var conditionValue = GetConditionValue(groupedMeasureConditionKey.MeasureConditionCode, measureConditionData.MeasureUnit, measureConditionData.ConditionAmount, measureConditionData.MeasureAction, supplementaryUnit);

					if (!string.IsNullOrEmpty(conditionValue))
					{
						conditions.Add(conditionValue);
					}
				}

				if (conditions.Any())
				{
					var zx3Value = string.Join(" & ", conditions);

					var refCusConditionValue = new RefCusConditionValue
					{
						ZX3_Value = zx3Value,
						ZX3_ZX4_NKValueType = ConditionValueTypeFormula
					};
					refCusConditionValues.Add(refCusConditionValue);
				}
			}

			return refCusConditionValues;
		}

		static string GetConditionValue(string measureConditionCode, string measureUnit, string conditionAmount, string measureAction, string supplementaryUnit)
		{
			var unitConditionFormat = measureConditionCode == "M" ? $"VFD/[{measureUnit}]" :
				 measureConditionCode == "U" ? $"VFD/[{supplementaryUnit}]" : $"[{measureUnit}]/[{supplementaryUnit}]";

			if (measureAction == "10" && conditionAmount != "0.000")
			{
				return $"{unitConditionFormat} < {conditionAmount}";
			}

			var greaterThanEquation = new[] { "R", "M" }.Contains(measureConditionCode) ? ">=" : ">";

			if (conditionAmount == "0.000" && greaterThanEquation == ">=")
			{
				return null;
			}
			return measureAction == "28" ? $"{unitConditionFormat} {greaterThanEquation} {conditionAmount}" : null;
		}

		public static IEnumerable<RefCusConditionValue> GetNonRatioConditionValues(IGroupedMeasureConditionKey groupedMeasureConditionKey, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords)
		{
			Argument.NotNull(groupedMeasureConditionKey, nameof(groupedMeasureConditionKey));
			Argument.NotNull(measureConditionRecords, nameof(measureConditionRecords));

			var refCusConditionValues = new List<RefCusConditionValue>();

			foreach (var measureConditionData in measureConditionRecords)
			{
				var refCusConditionValue = new RefCusConditionValue();

				if (string.IsNullOrEmpty(measureConditionData.CertificateTypeCode))
				{
					if (measureConditionData.MeasureConditionCode == "F")
					{
						continue;
					}
					else if ((!string.IsNullOrEmpty(measureConditionData.ConditionAmount)
						&& measureConditionData.MeasureConditionCode == "A" && measureConditionData.MeasureAction == "01")
						|| (measureConditionRecords.Any(x => x.MeasureAction == "07" && !string.IsNullOrEmpty(x.CertificateTypeCode))))
					{
						refCusConditionValue.ZX3_Value = string.IsNullOrEmpty(measureConditionData.ConditionAmount) ? "Not presented" : measureConditionData.ConditionAmount;
						refCusConditionValue.ZX3_ZX4_NKValueType = ConditionValueInformational;
					}
					else if (string.IsNullOrEmpty(measureConditionData.MeasureUnit)
						|| string.IsNullOrEmpty(measureConditionData.ConditionAmount))
					{
						continue;
					}
					else
					{
						refCusConditionValue.ZX3_Value = $"[{measureConditionData.MeasureUnit}] <= {measureConditionData.ConditionAmount}";
						refCusConditionValue.ZX3_ZX4_NKValueType = ConditionValueTypeFormula;
					}
				}
				else
				{
					if ((supCertificateStartsWithInclusionArray.Contains(measureConditionData.CertificateTypeCode.Substring(0,1))
						&& !supCertificateExclusionArray.Contains(measureConditionData.CertificateTypeCode))
						|| supCertificateInclusionArray.Contains(measureConditionData.CertificateTypeCode))
					{
						refCusConditionValue.ZX3_ZX4_NKValueType = ConditionValueTypeSupportingDocument;
					}
					else
					{
						refCusConditionValue.ZX3_ZX4_NKValueType = ConditionValueSupportingDocumentNoReferenceRequired;
					}

					refCusConditionValue.ZX3_Value = measureConditionData.CertificateTypeCode;
				}
				refCusConditionValues.Add(refCusConditionValue);
			}

			return refCusConditionValues;
		}

		static readonly string[] supCertificateInclusionArray = new[] { "Y022", "Y023", "Y024", "Y025", "Y026", "Y027", "Y028", "Y029", "Y031", "Y040", "Y041", "Y042", "Y915", "Y919" };
		static readonly string[] supCertificateStartsWithInclusionArray = new[] { "A", "B", "C", "E", "N", "H", "L", "Q", "Z" };
		static readonly string[] supCertificateExclusionArray = new[] { "L136", "N380", "N235", "N271", "N325", "N750", "N934", "N935", "N787", "N864" };
		const string ConditionValueTypeSupportingDocument = "SUP";
		const string ConditionValueTypeFormula = "FRM";
		const string ConditionValueSupportingDocumentNoReferenceRequired = "SNR";
		const string ConditionValueInformational = "INF";
	}
}
