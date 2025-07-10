using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessageBuilders;

namespace Enterprise.Customs.Business
{
	public interface ISupportingDocSendingObjectParent
	{
		BusinessObjectFactory Factory { get; }
		IEnumerable<ISupportingDocumentMessageDataProvider> SendingObjects { get; }
	}
}
