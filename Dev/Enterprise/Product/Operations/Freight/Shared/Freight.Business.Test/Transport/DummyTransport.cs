using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	class DummyTransport : Transport
	{
		public DummyTransport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		protected override TypeDecider ParentTypeDecider => new DummyTransportParentTypeDecider();
	}
}
