using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class IMOParserHelper
	{
		const string RegexSeeSP = @"See SP([0-9]{0,3})";

		static void SetStowSegData(IEnumerable<StowSegRecord> stowSegRecords, string stowCode, string defaultType, List<UNDGAttribute> attributes)
		{
			var stowRecord = stowSegRecords.FirstOrDefault(x => x.DGLText.StartsWith(stowCode + " ", StringComparison.Ordinal));
			if (stowRecord != null)
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", stowRecord.DGLPhrase, defaultType));
			}
			else if (stowCode.StartsWith("SGG", StringComparison.Ordinal))
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", stowCode, "SGG"));
			}
			else
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", stowCode, "NVL"));
				Console.Error.WriteLine($"SpecProv index invalid. SpecProv index: {stowCode}");
			}
		}

		static void SetSpecProvDescriptionIndex(IEnumerable<SpecProvRecord> specProvRecords, string specProvIndex, string defaultType, List<UNDGAttribute> attributes)
		{
			var specProvRecord = specProvRecords.FirstOrDefault(x => x.SPNo == specProvIndex);
			if (specProvRecord != null)
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", specProvRecord.SPNo, defaultType));
			}
			else
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", specProvIndex, "NVL"));
				Console.Error.WriteLine($"SpecProv index invalid. SpecProv index: {specProvIndex}");
			}
		}

		public static void SetLQData(this UNDGSubstance result, string lqValue, List<UNDGAttribute> attributes)
		{
			if (lqValue.Contains(" or ", StringComparison.OrdinalIgnoreCase))
			{
				var lqSplit = lqValue.Split(new[] { "or" }, StringSplitOptions.RemoveEmptyEntries);
				var (lqMaxAmt, lqMaxAmtUq, lqMaxAmtType) = ParserHelper.SeparateAmtAndUqAndType(lqSplit[0]);
				var (lq2MaxAmt, lq2MaxAmtUq, lq2MaxAmtType) = ParserHelper.SeparateAmtAndUqAndType(lqSplit[1]);

				result.DG_LQMaxAmt = lqMaxAmt;
				result.DG_LQMaxAmtType = lqMaxAmtType;
				result.DG_LQMaxAmtUQ = lqMaxAmtUq;
				result.DG_LQ2OrPaxMaxAmt = lq2MaxAmt;
				result.DG_LQ2OrPaxMaxAmtType = lq2MaxAmtType;
				result.DG_LQ2OrPaxMaxAmtUQ = lq2MaxAmtUq;
			}
			else if (lqValue.StartsWith("see sp", StringComparison.OrdinalIgnoreCase))
			{
				var match = Regex.Match(lqValue, RegexSeeSP);
				if (match.Groups.Count > 1)
				{
					var index = match.Groups[1].Value;
					result.DG_LQSpecProvIndex = index;
					attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", index, "SPP"));
				}
				else
				{
					Console.Error.WriteLine($"LQ Column doesn't have the correct mapping. Value: {lqValue}");
				}
			}
			else
			{
				var (lqMaxAmt, lqMaxAmtUq, lqMaxAmtType) = ParserHelper.SeparateAmtAndUqAndType(lqValue);
				result.DG_LQMaxAmt = lqMaxAmt;
				result.DG_LQMaxAmtType = lqMaxAmtType;
				result.DG_LQMaxAmtUQ = lqMaxAmtUq;
			}
		}

		public static void SetEQData(this UNDGSubstance result, string eqValue, List<UNDGAttribute> attributes)
		{
			if (eqValue.StartsWith("see sp", StringComparison.OrdinalIgnoreCase))
			{
				var match = Regex.Match(eqValue, RegexSeeSP);
				if (match.Groups.Count > 1)
				{
					var index = match.Groups[1].Value;
					attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute("", index, "SPP"));
				}
				else
				{
					Console.Error.WriteLine($"LQ Column doesn't have the correct mapping. Value: {eqValue}");
				}
			}
			else
			{
				result.DG_ExceptedQuantityCode = eqValue;
			}
		}

		public static void CreateAttributesStowSegSpecProv(IEnumerable<StowSegRecord> stowSegRecords, IEnumerable<SpecProvRecord> specProvRecords, string stow, string seg, string specProv, List<UNDGAttribute> attributes)
		{
			if (stowSegRecords != null)
			{
				if (!string.IsNullOrEmpty(stow))
				{
					var splitStow = stow.Split(' ');
					foreach (var stowValue in splitStow)
					{
						SetStowSegData(stowSegRecords, stowValue, "CPV", attributes);
					}
				}
				if (!string.IsNullOrEmpty(seg))
				{
					var splitSeg = seg.Split(' ');
					foreach (var segValue in splitSeg)
					{
						SetStowSegData(stowSegRecords, segValue, "DLG", attributes);
					}
				}
			}

			if (specProvRecords != null && !string.IsNullOrEmpty(specProv))
			{
				var splitSpecProv = specProv.Split(' ');
				foreach (var specProvValue in splitSpecProv)
				{
					SetSpecProvDescriptionIndex(specProvRecords, specProvValue, "SPP", attributes);
				}
			}
		}

		public static void CreateAttributesProperty(IEnumerable<PropertyRecord> propertyRecords, string unno, string variant, List<UNDGAttribute> attributes)
		{
			if (propertyRecords == null || attributes == null)
			{
				return;
			}

			var propertyRecord = propertyRecords.FirstOrDefault(x => x.UNNO == unno && x.Variant == variant);
			if (propertyRecord != null)
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute(propertyRecord.PropObs, "0", "PRP"));
			}
		}

		public static void CreateAttributesQualifyingDescriptiveText(IEnumerable<QualifyingDescriptiveTextRecord> qualifyingDescriptiveTextRecords, string code, List<UNDGAttribute> attributes)
		{
			if (qualifyingDescriptiveTextRecords == null || attributes == null)
			{
				return;
			}

			var qualifyingDescriptiveTextRecord = qualifyingDescriptiveTextRecords.FirstOrDefault(x => x.UnId == code);
			if (qualifyingDescriptiveTextRecord != null)
			{
				attributes.AddIfNotExists(ParserHelper.CreateUNDGAttribute(qualifyingDescriptiveTextRecord.QualifyingDescriptiveText, "0", "QDT"));
			}
		}

		static void AddIfNotExists(this List<UNDGAttribute> attributes, UNDGAttribute attribute)
		{
			if (attributes == null || attribute == null || attributes.Any(x =>
				x.DA_Language == attribute.DA_Language &&
				x.DA_Index == attribute.DA_Index &&
				x.DA_Type == attribute.DA_Type))
			{
				return;
			}

			attributes.Add(attribute);
		}
	}
}
