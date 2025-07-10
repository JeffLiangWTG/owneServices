using System.Collections.Generic;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs
{
	public class EmcsCnCodesEMCCNTransformer : CodeListsParserTSV<RefCusCodeList, IKeyValues>
	{
		public EmcsCnCodesEMCCNTransformer(Dictionary<string, string> downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0037_CN_CODES;

		protected override string CodeType => CodeListsConstants.Emcs.CodeTypes.EMCS_EMCCN_CN_CODES;

		protected override string HtmlElementIdentifier => "Codeliste 37 - CN Codes";

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCCN_CN_CODES);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);

		protected override IKeyValues GetKeyValues(string[] record) => new BaseKeyValuesTSV(record);

		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override string[] GetCodeList(HttpClient client, string downloadUrl) => DownloadEMCSAndExtractTSVCodeList(client, downloadUrl, CustomsCodeListIdentifier);
	}
}
