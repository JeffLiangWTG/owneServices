using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic
{
	public class GenericOriginCountryCodesEftaEU15Transformer : ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public GenericOriginCountryCodesEftaEU15Transformer(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValues>> parseResults)
		{
			var result = new List<RefCusCodeList>();
			if (parseResults.TryGetValue(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY, out var c0010))
			{
				foreach (var keyValues in c0010.List)
				{
					result.Add(CodeListsHelper.CreateRefListWithDirectionAttributes(keyValues, true, false));
				}
			}

			if (parseResults.TryGetValue(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES, out var a1300))
			{
				foreach (var keyValues in a1300.List)
				{
					var existingRefList = result.SingleOrDefault(x => x.ZZD_Code == keyValues.Code);
					if (existingRefList != null)
					{
						existingRefList.RefCusCodeListAttributes = CodeListsHelper.CreateDirectionAttributes(true, true);
					}
					else
					{
						result.Add(CodeListsHelper.CreateRefListWithDirectionAttributes(keyValues, false, true));
					}
				}
			}
			return result;
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY, CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES };

		protected override string CodeType => CodeListsConstants.Generic.CodeTypes.EU15_ORIGIN_COUNTRY_LIST;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Generic.CodeTypes.EU15_ORIGIN_COUNTRY_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
