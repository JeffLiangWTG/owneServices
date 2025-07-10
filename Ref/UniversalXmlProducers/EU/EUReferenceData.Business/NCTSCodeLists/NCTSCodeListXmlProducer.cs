using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public abstract class NCTSCodeListXmlProducer : NctsManyToOneCodeListXMLProducer
	{
		protected override IEnumerable<RefCusCodeList> UnifyAndMergeRefCusCodeLists(List<ExtractedCodeListProvider> extractedCodeLists) => extractedCodeLists.Single().ParsedXml;

		protected sealed override IUCCExportCodeListDetail[] CodeListDetails => new[] { CodeListDetail };

		protected abstract IUCCExportCodeListDetail CodeListDetail { get; }
	}
}
