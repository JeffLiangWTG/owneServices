using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DummyBusinessObjectWithDocuments : DummyBusinessObject, IDocumentSupportable
	{
		public DummyBusinessObjectWithDocuments(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new DummyDocumentSupporter(this); }
		}
	}
}
