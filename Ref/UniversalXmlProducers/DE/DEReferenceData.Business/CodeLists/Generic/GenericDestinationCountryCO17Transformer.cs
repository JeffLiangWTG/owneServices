using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic
{
	public class GenericDestinationCountryCO17Transformer : ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public GenericDestinationCountryCO17Transformer(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValues>> parseResults)
		{
			var result = new List<RefCusCodeList>();
			if (parseResults.TryGetValue(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0806_COUNTRY_LIST_CO, out var i0806))
			{
				foreach (var keyValues in i0806.List)
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

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0806_COUNTRY_LIST_CO, CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1314_EU_MEMBER_STATES };

		protected override string CodeType => CodeListsConstants.Generic.CodeTypes.CO17_DESTINATION_COUNTRY_LIST;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Generic.CodeTypes.CO17_DESTINATION_COUNTRY_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfigurationWithAttributes(true, CodeType);

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
