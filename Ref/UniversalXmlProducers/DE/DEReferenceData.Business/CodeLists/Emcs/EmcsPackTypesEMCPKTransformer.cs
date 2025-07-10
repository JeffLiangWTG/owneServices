using System.Collections.Generic;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs
{
	public class EmcsPackTypesEMCPKTransformer : CodeListsParserTSV<RefCusCodeList, IKeyValues>
	{
		public EmcsPackTypesEMCPKTransformer(Dictionary<string, string> downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0017_PACK_TYPES;

		protected override string CodeType => CodeListsConstants.Emcs.CodeTypes.EMCS_EMCPK_PACK_TYPES;

		protected override string HtmlElementIdentifier => "Codeliste 17 - Packaging Codes";

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCPK_PACK_TYPES);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListLanguageWriterConfiguration(CodeType, "EUN");

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefListLanguage(keyValues);

		protected override IKeyValues GetKeyValues(string[] record) => new BaseKeyValuesTSV(record);

		protected override string[] GetCodeList(HttpClient client, string downloadUrl) => DownloadEMCSAndExtractTSVCodeList(client, downloadUrl, CustomsCodeListIdentifier);
	}
}
