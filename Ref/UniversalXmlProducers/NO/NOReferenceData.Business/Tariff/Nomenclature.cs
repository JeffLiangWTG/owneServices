using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff
{
	public static class Nomenclature
	{
		public static string GetTranslatedString(List<RefCusNomenclatureGroup> nomenclature, string hsNumber)
		{
			var returnValue = string.Empty;

			var translated = from refCusNomenclatureGroup in nomenclature
							 where refCusNomenclatureGroup.ZZ5_CompositeKey == hsNumber
							 select new
							 {
								 trans = refCusNomenclatureGroup.ZZ5_Description
							 };

			if (translated.Count() == 1)
			{
				returnValue = translated.First().trans;
			}
			return returnValue;
		}

		public static string GetCompositeKey(List<RefCusNomenclatureGroup> nomenclature, string hsNumber, string tariffId)
		{
			var returnValue = "";
			var nomenclatures = from refCusNomenclatureGroup in nomenclature
								where refCusNomenclatureGroup.ZZ5_Value == hsNumber
								select new
								{
									comp = refCusNomenclatureGroup.ZZ5_CompositeKey
								};
			var maxStrLen = nomenclatures.Any() ? nomenclatures.Max(x => x.comp.Length) : 0;

			if (nomenclatures.Count() == 1)
			{
				returnValue = nomenclatures.First().comp;
			}
			else if (nomenclatures.Where(x => x.comp.GetLast(1) == tariffId.GetLast(1)).Count() == 1)
			{
				returnValue = nomenclatures.Where(x => x.comp.GetLast(1) == tariffId.GetLast(1)).First().comp;
			}
			else if (nomenclatures.Where(x => x.comp.GetLast(2) == tariffId.GetLast(2)).Count() == 1)
			{
				returnValue = nomenclatures.Where(x => x.comp.GetLast(2) == tariffId.GetLast(2)).First().comp;
			}
			else if (nomenclatures.Where(x => x.comp.GetLast(1) == tariffId.GetLast(1)).Where(x => x.comp.Length == maxStrLen).Count() == 1)
			{
				returnValue = nomenclatures.Where(x => x.comp.GetLast(1) == tariffId.GetLast(1)).Where(x => x.comp.Length == maxStrLen).First().comp;
			}
			else if (nomenclatures.Where(x => x.comp.GetLast(2) == tariffId.GetLast(2)).Where(x => x.comp.Length == maxStrLen).Count() == 1)
			{
				returnValue = nomenclatures.Where(x => x.comp.GetLast(2) == tariffId.GetLast(2)).Where(x => x.comp.Length == maxStrLen).First().comp;
			}
			else
			{
				TariffParser.ErrorBuilder.AppendLine("Unable to find correct nomenclature.");
				TariffParser.ErrorBuilder.AppendLine("DETAILS:");
				TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "HSNumber: {0}", hsNumber).AppendLine();
				TariffParser.ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Tariff: {0}", tariffId).AppendLine();
			}

			return returnValue;
		}

		public static string GetLast(this string source, int numLast)
		{
			return numLast >= source.Length ? source : source.Substring(source.Length - numLast);
		}
	}
}
