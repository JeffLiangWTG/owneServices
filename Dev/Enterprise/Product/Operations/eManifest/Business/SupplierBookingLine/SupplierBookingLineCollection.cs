
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingLineCollection : DependentBusinessObjectCollection<SupplierBookingLine, CommonShipment>
	{
		public SupplierBookingLineCollection(CommonShipment master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return SupplierBookingLineSchema.DL_JS_ApprovedShipment; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
