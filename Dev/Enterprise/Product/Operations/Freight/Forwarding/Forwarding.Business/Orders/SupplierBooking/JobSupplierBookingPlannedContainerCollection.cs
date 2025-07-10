using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingPlannedContainerCollection : DependentBusinessObjectCollection<JobSupplierBookingPlannedContainer, JobSupplierBooking>
	{
		public JobSupplierBookingPlannedContainerCollection(JobSupplierBooking master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery(JobOrderContainerSchema.J1_ParentID, Master.PK);
			query.AddToFilter(JobOrderContainerSchema.J1_ParentTableCode, JobSupplierBookingSchema.Constants.Prefix);
			return query;
		}

		protected override string FkColumnName => JobOrderContainerSchema.J1_ParentID.Name;
	}
}
