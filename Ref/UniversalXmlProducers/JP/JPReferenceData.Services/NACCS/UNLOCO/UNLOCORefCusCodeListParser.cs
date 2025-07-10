using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services;

public class UNLOCORefCusCodeListParser : RefCusCodeListParser
{
	public override string CurrentCodeType => Constants.CodeType.PORT;

	protected override string GetZZD_Code(string[] columns) => columns[2];

	protected override string GetZZD_Description(string[] columns) => columns[5];

	protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new UNLOCOAttributeParser();

	protected override RefCusCodeListLanguageParser GetRefCusCodeListLanguageParserCore() => new UNLOCOLanguageParser();

	protected override Regex ZZD_CodeRegex => new Regex("^[0-9A-Z]{5}$");

	protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns, bool needAddAttribute, bool needAddLanguage)
	{
		needAddLanguage = !string.IsNullOrEmpty(columns[7]);
		base.AddRefCusCodeList(refCusCodeLists, columns, needAddAttribute, needAddLanguage);
	}
}
