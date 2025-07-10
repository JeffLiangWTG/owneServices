using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class PackageTypeRefCusCodeListParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.PackageType;

		protected override string GetZZD_Code(string[] columns) => columns[0].Normalize(NormalizationForm.FormKC);

		protected override string GetZZD_Description(string[] columns) => columns[1];

		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]{2}$");

		protected override RefCusCodeListLanguageParser GetRefCusCodeListLanguageParserCore() => new PackageTypeLanguageParser();

		public override bool HasAttribute => true;

		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new PackageTypeAttributeParser();

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			var needAddAttribute = !(string.IsNullOrEmpty(columns[3]) && string.IsNullOrEmpty(columns[4]));
			AddRefCusCodeList(refCusCodeLists, columns, needAddAttribute);
		}
	}
}
