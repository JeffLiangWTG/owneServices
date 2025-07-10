using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingLineDependentCollection : DependentBusinessObjectCollection<SupplierBookingLine, SupplierBookingHeader>
	{
		public SupplierBookingLineDependentCollection(SupplierBookingHeader master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return SupplierBookingLineSchema.DL_DH_BookingHeader; }
		}
	}
}
