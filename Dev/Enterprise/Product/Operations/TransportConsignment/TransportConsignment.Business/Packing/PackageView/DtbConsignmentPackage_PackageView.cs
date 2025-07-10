using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentPackage_PackageView : Package_PackageView
	{
		public DtbConsignmentPackage_PackageView(PkgPackage package, DtbBookingConsignment consignment)
			: base(package, consignment)
		{
		}

		/// <summary>
		/// Collection requires a new Package_PackageView for binding, so just return an empty instance.
		/// </summary>
		internal DtbConsignmentPackage_PackageView(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IDtbTransportConfirmationCollection GetConfirmations()
		{
			return new DtbConsignmentConfirmationCollection(Factory, new PackageConfirmationRelationship<DtbConsignmentConfirmation>(this));
		}

		protected override IDtbTransportInstructionPkgDivotCollection GetInstructionDivots()
		{
			return new DtbConsignmentInstructionPkgDivotCollection(this);
		}
	}
}
