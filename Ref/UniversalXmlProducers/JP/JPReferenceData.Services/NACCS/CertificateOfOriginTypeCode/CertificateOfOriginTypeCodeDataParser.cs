using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class CertificateOfOriginTypeCodeDataParser : RefCusCodeListParser
	{
		const string TradeAgreementsCodeType = "COOT1";
		const string CertifiersCodeType = "COOT2";
		const string CargoTypesCodeType = "COOT3";

		public override string CurrentCodeType => currentCodeType;
		string currentCodeType;
		string code;

		protected override string GetZZD_Code(string[] columns) => columns[0];
		protected override string GetZZD_Description(string[] columns) => columns[1];
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z0-9]{1,2}$");

		public override bool HasAttribute => true;

		protected override void OnLineRead(string line)
		{
			if (line.Contains("１桁目"))
			{
				currentCodeType = TradeAgreementsCodeType;
			}
			else if (line.Contains("３桁目"))
			{
				currentCodeType = CertifiersCodeType;
			}
			else if (line.Contains("４桁目"))
			{
				currentCodeType = CargoTypesCodeType;
			}
		}

		protected override void AddRefCusCodeList(List<RefCusCodeList> refCusCodeLists, string[] columns)
		{
			var refCusCodeList = new RefCusCodeList()
			{
				ZZD_Code = GetZZD_Code(columns),
				ZZD_Description = GetZZD_Description(columns),
				ZZD_StartDate = GetZZD_StartDate(columns),
				ZZD_EndDate = GetZZD_EndDate(columns),
				ZZD_ZZK_NKCodeType = CurrentCodeType,
				ZZD_ZZZ_NKDataGrouping = ZZD_ZZZ_NKDataGrouping
			};

			var attributesAreValid = true;
			var languagesAreValid = true;

			code = columns[0];

			var attributeParser = GetRefCusCodeListAttributeParserCore();

			if (attributeParser != null && attributeParser.Configs != null)
			{
				refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[attributeParser.Configs.Length];
				attributesAreValid = attributeParser.TryAddRefCusCodeListAttribute(refCusCodeList, columns);
			}

			if (RefCusCodeListLanguageParser != null && RefCusCodeListLanguageParser.Configs.Length > 0)
			{
				refCusCodeList.RefCusCodeListLanguages = new RefCusCodeListLanguage[RefCusCodeListLanguageParser.Configs.Length];
				languagesAreValid = RefCusCodeListLanguageParser.TryAddRefCusCodeListLanguage(refCusCodeList, columns);
			}

			AddRefCusCodeOrAttributeTransportModeIfNeeded(refCusCodeList, columns);

			if (ValidateRefCusCodeList(refCusCodeLists, refCusCodeList) && attributesAreValid && languagesAreValid)
			{
				refCusCodeLists.Add(refCusCodeList);
			}
		}

		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore()
		{
			if (currentCodeType == TradeAgreementsCodeType)
			{
				return new Coot1CertificateOfOriginTypeRefCusCodeListAttributeParser(code);
			}

			if (currentCodeType == CargoTypesCodeType)
			{
				var parser = new Coot3CertificateOfOriginTypeRefCusCodeListAttributeParser(code);

				if (parser.Configs != null)
				{
					return parser;
				}
			}

			return null;
		}

		protected override bool ValidateUniqueness(IEnumerable<RefCusCodeList> refCusCodeLists, RefCusCodeList refCusCodeList)
		{
			return refCusCodeLists.All(c => !(c.ZZD_Code == refCusCodeList.ZZD_Code && c.ZZD_ZZK_NKCodeType == refCusCodeList.ZZD_ZZK_NKCodeType));
		}
	}
}
