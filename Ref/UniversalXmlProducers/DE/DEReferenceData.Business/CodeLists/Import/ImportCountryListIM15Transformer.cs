using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import
{
	public class ImportCountryListIM15Transformer : ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValues>
	{
		public ImportCountryListIM15Transformer(string[] downLoadLinks)
			: base(downLoadLinks)
		{
		}

		protected override List<RefCusCodeList> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<IKeyValues>> parseResults)
		{
			var result = new List<RefCusCodeList>();

			if (parseResults.TryGetValue(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0300_COUNTRY_LIST, out var i0300))
			{
				var a1300List = parseResults.TryGetValue(CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES, out var a1300) ? a1300.List : new List<IKeyValues>();

				foreach (var keyValues in i0300.List)
				{
					if (!a1300List.Any(l => l.Code == keyValues.Code))
					{
						result.Add(CodeListsHelper.CreateRefList(keyValues));
					}
				}
			}
			
			return result;
		}

		protected override string[] CustomsCodeListIdentifiers => new[] { CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0300_COUNTRY_LIST, CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_A1300_EFTA_COUNTRIES};

		protected override string CodeType => CodeListsConstants.Generic.CodeTypes.IM15_ORIGIN_COUNTRY_LIST;

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Generic.CodeTypes.IM15_ORIGIN_COUNTRY_LIST);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override string XMLWriterDataSource => $"DE {CodeListsConstants.Import.CustomsCodeListIdentifiers.IMPORT_I0300_COUNTRY_LIST}";

		protected override IKeyValues GetKeyValues(XElement entry) => new BaseKeyValuesXML(entry, CodeListsConstants.XMLEntryElementNames.CODE);
	}
}
