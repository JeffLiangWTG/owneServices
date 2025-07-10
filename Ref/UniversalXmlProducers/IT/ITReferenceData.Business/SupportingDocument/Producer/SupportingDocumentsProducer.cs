using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public class SupportingDocumentsProducer
	{
		public SupportingDocumentsProducer(IRawSupportingDocumentsLoader loader, IRawSupportingDocumentMapper mapper)
		{
			this.loader = Argument.NotNull(loader, nameof(loader));
			this.mapper = Argument.NotNull(mapper, nameof(mapper));
		}

		readonly IRawSupportingDocumentsLoader loader;
		readonly IRawSupportingDocumentMapper mapper;

		public IEnumerable<RefCusCodeList> ProduceEntities()
		{
			var supportingDocuments = loader.GetRawSupportingDocuments();
			return supportingDocuments.SelectMany(x => mapper.GetMappings(x)).ToArray();
		}
	}
}
