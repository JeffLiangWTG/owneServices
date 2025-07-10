using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MockEDIMessage : EDIMessage, IDocumentSupportable
	{
		public MockEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DocumentSupporter DocumentSupporter => ducumentSupporter ?? (ducumentSupporter = new EDIMessageDocumentSupporter(this));
		DocumentSupporter ducumentSupporter;
	}
}
