using System.Collections.Generic;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs
{
	public class EmcsMemberStatesEMCMSTransformer : CodeListsParserTSV<RefCusCodeList, IKeyValues>
	{
		public EmcsMemberStatesEMCMSTransformer(Dictionary<string, string> downloadLinks)
			: base(downloadLinks)
		{
		}

		protected override string CustomsCodeListIdentifier => CodeListsConstants.Emcs.CustomsCodeListIdentifiers.EMCS_CL0011_MEMBER_STATES;

		protected override string CodeType => CodeListsConstants.Emcs.CodeTypes.EMCS_EMCMS_MEMBER_STATES;

		protected override string HtmlElementIdentifier => "Codeliste 11 - Member States";

		protected override string OutputFileNameSuffix => nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCMS_MEMBER_STATES);

		protected override XmlWriterConfiguration XmlWriterConfiguration => CodeListsHelper.GetRefCusCodeListWriterConfiguration(CodeType);
		
		protected override RefCusCodeList CreateRefList(IKeyValues keyValues) => CodeListsHelper.CreateRefList(keyValues);

		protected override IKeyValues GetKeyValues(string[] record) => new BaseKeyValuesTSV(record);

		protected override string[] GetCodeList(HttpClient client, string downloadUrl) => DownloadEMCSAndExtractTSVCodeList(client, downloadUrl, CustomsCodeListIdentifier);
	}
}
