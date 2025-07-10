using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLegCollection : ActiveBusinessObjectCollection<DtbConsignmentLeg>
	{
		public DtbConsignmentLegCollection(DtbConsignment consignment) : base(consignment.Factory, consignment, null, DtbConsignmentLegSchema.LTG_LTC_Consignment)
		{
		}

		public DtbConsignmentLegCollection(DtbConsignmentAction consignmentAction, SchemaColumn schemaColumn) : base(consignmentAction.Factory, consignmentAction, null, schemaColumn)
		{
		}
	}
}
