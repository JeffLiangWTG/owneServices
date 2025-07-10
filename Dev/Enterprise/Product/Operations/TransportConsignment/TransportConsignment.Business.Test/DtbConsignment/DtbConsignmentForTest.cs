using System.Data;
using CargoWise.EntityFramework;
using Enterprise.TransportConsignment.Integration;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentForTest : DtbConsignment
	{
		public DtbConsignmentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new bool IsAutoLogged => base.IsAutoLogged;

		protected override IDtbConsignmentService GetConsignmentService()
		{
			return ConsignmentServiceOverride;
		}

		public IDtbConsignmentService ConsignmentServiceOverride { get; set; }
	}
}
