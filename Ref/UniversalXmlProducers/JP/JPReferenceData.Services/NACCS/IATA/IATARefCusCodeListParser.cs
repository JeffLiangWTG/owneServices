using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class IATARefCusCodeListParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.IATA;

		protected override string GetZZD_Code(string[] columns) => columns[9].Normalize(NormalizationForm.FormKC);

		protected override string GetZZD_Description(string[] columns) => string.IsNullOrWhiteSpace(columns[13])? columns[5] : columns[13];

		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new IATAAttributeParser();

		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]{3}$");

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			var needAddAttribute = !string.IsNullOrWhiteSpace(columns[12]);
			AddRefCusCodeList(refCusCodeLists, columns, needAddAttribute);
		}
	}
}
