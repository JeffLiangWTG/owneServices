using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic
{
	public class GenericOriginCountryCodesCO15Transformer : ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public GenericOriginCountryCodesCO15Transformer(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValues>> parseResults)
		{
			var result = new List<RefCusCodeList>();
			if (parseResults.TryGetValue(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY, out var c1000))
			{
				foreach (var keyValues in c1000.List)
				{
					result.Add(CodeListsHelper.CreateRefListWithDirectionAttributes(keyValues, true, false));
				}
			}

			if (parseResults.TryGetValue(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES, out var a1314))
			{
				foreach (var keyValues in a1314.List)
				{
					var existing = result.SingleOrDefault(x => x.ZZD_Code == keyValues.Code);
					if (existing != null)
					{
						existing.RefCusCodeListAttributes = CodeListsHelper.CreateDirectionAttributes(true, true);
					}
					else
					{
						result.Add(CodeListsHelper.CreateRefListWithDirectionAttributes(keyValues, false, true));
					}
				}
			}
			return result;
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_C0010_COUNTRY_CODES_COMMUNITY, CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES };

		protected override string CodeType => CodeListsConstants.Generic.CodeTypes.CO15_ORIGIN_COUNTRY_LIST;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Generic.CodeTypes.CO15_ORIGIN_COUNTRY_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
