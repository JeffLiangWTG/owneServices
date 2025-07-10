using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRunSheetCostSupporter : LinehaulAndRunSheetCostSupporter<DtbConsignmentRunSheet>
	{
		public DtbConsignmentRunSheetCostSupporter(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
		}

		protected override ZGuid CreditorPK
		{
			get { return Parent.TransportCo != null ? Parent.TransportCo.PK : ZGuid.Empty; }
		}

		protected override IEnumerable<IJobInvoicingPlugIn> Consignments
		{
			get { return Parent.HasNewConsignments ? Parent.RunSheetInstructions.SelectMany(i => i.Actions).Select(c => c.Consignment).Cast<IJobInvoicingPlugIn>() : Parent.RunSheetInstructions.SelectMany(i => i.Confirmations).Select(c => c.Instruction.Booking); }
		}
	}
}
