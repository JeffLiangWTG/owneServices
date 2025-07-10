using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSupportingDocumentCollection : DocumentWrapperCollection<NODocSupportingDocument>
{
	public NODocSupportingDocumentCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public NODocSupportingDocumentCollection(IEnumerable<SupportingDocument> collectionSource, BusinessObjectFactory factoryToWrap)
		: base(collectionSource, factoryToWrap)
	{
	}
}
