using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentVariationCollection : ActiveBusinessObjectCollection<DtbConsignmentVariation>
	{
		public DtbConsignmentVariationCollection(DtbConsignment consignment)
			: base(consignment.Factory, consignment, null, DtbConsignmentVariationSchema.LTV_JobId)
		{
		}
	}
}
