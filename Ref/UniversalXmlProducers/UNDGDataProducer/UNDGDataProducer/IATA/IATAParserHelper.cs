using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class IATAParserHelper
	{
		public static UNDGAttribute[] LoadAttributes(IATARecord dgIataRecord)
		{
			var undgAttributes = new List<UNDGAttribute>();
			if (!string.IsNullOrEmpty(dgIataRecord.QualifyingDescriptiveText))
			{
				undgAttributes.Add(ParserHelper.CreateUNDGAttribute(dgIataRecord.QualifyingDescriptiveText, "0", Constants.AttributeTypes.QualifyingDescriptive));
			}
			if (!string.IsNullOrEmpty(dgIataRecord.OtherNames))
			{
				var splitOtherNames = dgIataRecord.OtherNames.Split(';');
				var otherNameIdx = 0;
				foreach (var otherName in splitOtherNames)
				{
					undgAttributes.Add(ParserHelper.CreateUNDGAttribute(otherName, otherNameIdx.ToString(CultureInfo.InvariantCulture), Constants.AttributeTypes.OtherNames));
					otherNameIdx++;
				}
			}
			if (!string.IsNullOrEmpty(dgIataRecord.SpecialProvisions))
			{
				var descriptor = dgIataRecord.SpecialProvisions.Replace(';', ' ').Trim();
				undgAttributes.Add(ParserHelper.CreateUNDGAttribute(descriptor, "0", Constants.AttributeTypes.SpecialProvisions));
			}

			return undgAttributes.ToArray();
		}

		public static void CreateIATAVariant(IEnumerable<UNDGSubstance> substances)
		{
			var variants = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "x", "w", "z" };

			var substancesGroupByCode = substances.GroupBy(x => x.DG_UNNO, (unno, records) => new { records });
			foreach (var subs in substancesGroupByCode)
			{
				var orderedRecords = subs.records.OrderBy(x => Convert.ToInt32(x.DG_UniqueRecordId, CultureInfo.InvariantCulture));
				if (orderedRecords.Count() == 1)
				{
					orderedRecords.ElementAt(0).DG_Variant = "";
				}
				else
				{
					for (var i = 0; i < orderedRecords.Count(); i++)
					{
						orderedRecords.ElementAt(i).DG_Variant = variants[i];
					}
				}
			}
		}
	}
}
