using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsOrder)]

	/**
	 * This collection was not converted to active as
	 * it is not possible to filter an active collection based on the DBOnlyQuery.
	 * An alternative solution is required for a future work item.
	 * WhsPickLineCollection might be a useful guide.
	 * This collection should inherit from WhsOrderCollection.
	 **/
	public class WhsOrderCollectionForPicking : BusinessObjectCollection<WhsOrder>
	{
		#region Constructors

		public WhsOrderCollectionForPicking(BusinessObjectFactory factory, WhsPick parent)
			: base(factory)
		{
			Parent = parent;
		}

		#endregion

		#region Filters
		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			if (Parent != null)
			{
				var query = new ZDBOnlyQuery(typeof(WhsDocket));
				query.AddToFilter(base.CreateAdditionalFilter());
				query.AddToFilter(WhsDocketSchema.WD_DocketStatus, DocketStatus.Codes.Entered);
				if (!Parent.WP_WW_Whs.IsEmpty)
				{
					query.AddToFilter(WhsDocketSchema.WD_WW_Whs, Parent.WP_WW_Whs);
				}

				var subQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				subQuery.AddToFilter(WhsDocketLineSchema.WE_WD, SQLComparisonOperator.Equal, WhsDocketSchema.PK);

				query.AddSubQuery(subQuery, JoinCondition.And);

				if (!Parent.WP_WA_DynamicPickAreaOverride.IsEmpty)
				{
					var subQueryEmptyPalletID = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, notIn: true);
					subQueryEmptyPalletID.AddToFilter(WhsDocketLineSchema.WE_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
					query.AddSubQuery(subQueryEmptyPalletID, JoinCondition.And);
				}

				return query;
			}
			return new ZQuery();
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			WhsOrder order = (WhsOrder)selectedBusinessObject;

			WhsPick.DocketPickabilityEventArgs pickability = order.GetPickability(Parent);
			if (pickability.MessageType == NotificationTypes.Error)
			{
				errors.Add("\n" + pickability.Message);
			}
		}

		#endregion

		protected override bool AllowNewCore => false;

		readonly WhsPick Parent;
	}
}
