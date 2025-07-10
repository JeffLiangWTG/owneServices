using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	/// <summary>
	/// This collection was not converted to an ActiveBusinessObjectCollection as
	/// it is not possible to filter an Active Collection based on a DBOnlyQuery.
	/// An alternative solution is required for a future work item (WI00211489).
	/// WhsPickLineCollection might be a useful guide.
	/// </summary>
	public class WhsOrderCollectionForAttaching : BusinessObjectCollection<WhsOrder>, IWhsOrderCollection
	{
		public WhsOrderCollectionForAttaching(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketJobPivot), WhsDocketJobPivotSchema.WV_WD_Docket, notIn: true);
			var query = new ZDBOnlyQuery(typeof(WhsOrder));
			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return query;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(notifications, selectedBusinessObject);

			notifications.Add(Res.GetString("dbaf0028-eef9-4ad9-ae6d-fb325ff07fbd", "Cannot attach an Order already attached to another Shipment."));
		}

		protected override bool AllowNewCore => false;
	}
}
