using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DummyLinehaulAndRunSheetCostSupporter : LinehaulAndRunSheetCostSupporter<DummyBusinessObject>
	{
		public DummyLinehaulAndRunSheetCostSupporter(DummyBusinessObject parent)
			: base(parent)
		{
		}

		protected override ZGuid CreditorPK
		{
			get { return CreditorToReturnPk; }
		}

		public ZGuid CreditorToReturnPk { get; set; }

		protected override IEnumerable<IJobInvoicingPlugIn> Consignments
		{
			get { return ConsignmentsToReturn; }
		}

		public IEnumerable<DtbBookingConsignment> ConsignmentsToReturn { get; set; }
	}
}
