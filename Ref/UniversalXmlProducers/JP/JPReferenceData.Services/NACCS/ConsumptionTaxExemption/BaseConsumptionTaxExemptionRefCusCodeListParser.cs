using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class BaseConsumptionTaxExemptionRefCusCodeListParser : RefCusCodeListParser
	{
		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new ConsumptionTaxExemptionAttributeParser();

		protected override DateTime GetZZD_StartDate(string[] columns)
		{
			(var isFromNowOn, var result) = JapaneseLocalHelper.RetrieveTimeFromDescritpionWithJapaneseEras(columns[1]);
			return result == DateTime.MaxValue || !isFromNowOn ? StaticResources.DefaultZZD_StartDate : result;
		}

		protected override DateTime GetZZD_EndDate(string[] columns)
		{
			(var isFromNowOn, var result) = JapaneseLocalHelper.RetrieveTimeFromDescritpionWithJapaneseEras(columns[1]);
			return result == DateTime.MaxValue || isFromNowOn ? StaticResources.DefaultZZD_EndDate : result;
		}

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			var needAddAttribute = columns.Length > 2 && !string.IsNullOrWhiteSpace(columns[2]);
			AddRefCusCodeList(refCusCodeLists, columns, needAddAttribute);
		}
	}
}
