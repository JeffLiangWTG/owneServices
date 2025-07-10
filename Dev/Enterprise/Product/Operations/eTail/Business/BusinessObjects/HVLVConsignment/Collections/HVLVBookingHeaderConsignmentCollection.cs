using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderConsignmentCollection : DependentBusinessObjectCollection<HVLVConsignment, HVLVBookingHeader>,
		IHVLVConsignmentCollection
	{
		public HVLVBookingHeaderConsignmentCollection(HVLVBookingHeader bookingHeader)
			: base(bookingHeader)
		{ }

		IHVLVConsignment IHVLVConsignmentCollection.this[int i] => (IHVLVConsignment)Elements[i];

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.IgnoreActiveFilter = true;
			return query;
		}

		public override void Load()
		{
			base.Load();
			Master.ConsignmentIDCache.Clear();
			foreach (HVLVConsignment consignment in this)
			{
				Master.ConsignmentIDCache.Add(consignment.HVC_ConsignmentId);
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return HVLVConsignmentSchema.HVC_HVH_BookingHeader; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var baseQuery = base.CreateRelationshipFilter();
			if (!Master.HVH_IsProcessedAtOriginDepot)
			{
				return baseQuery.AddToFilter(HVLVConsignmentSchema.HVC_ClusterKey, Master.HVH_ClusterKey);
			}

			return baseQuery;
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			if (!IsLoading)
			{
				Master.MarkForWeightRecalculation();
				Master.MarkForVolumeRecalculation();
				Master.MarkForItemCountRecalculation();
			}
		}
	}
}
