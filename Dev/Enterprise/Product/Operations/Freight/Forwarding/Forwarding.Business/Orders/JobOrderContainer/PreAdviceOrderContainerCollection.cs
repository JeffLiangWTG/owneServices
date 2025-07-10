using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class PreAdviceOrderContainerCollection : DependentBusinessObjectCollection<OrderContainer, JobShipmentPreplanning>
	{
		public PreAdviceOrderContainerCollection(JobShipmentPreplanning preAdvice)
			: base(preAdvice)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery(JobOrderContainerSchema.J1_ParentID, Master.PK);
			query.AddToFilter(JobOrderContainerSchema.J1_ParentTableCode, JobShipmentPreplanningSchema.Constants.Prefix);
			return query;
		}

		protected override string FkColumnName => JobOrderContainerSchema.J1_ParentID.Name;
	}
}
