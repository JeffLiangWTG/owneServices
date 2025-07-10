using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionPkgDivotCollection : DtbTransportInstructionPkgDivotCollection<DtbConsignmentInstructionPkgDivot>
	{
		public DtbConsignmentInstructionPkgDivotCollection(DtbConsignmentInstruction instruction)
			: base(instruction)
		{
		}

		public DtbConsignmentInstructionPkgDivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DtbConsignmentInstructionPkgDivotCollection(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		public DtbConsignmentInstructionPkgDivotCollection(DtbConsignmentPackage_PackageView package_PackingView)
			: base(package_PackingView)
		{
		}
	}
}
