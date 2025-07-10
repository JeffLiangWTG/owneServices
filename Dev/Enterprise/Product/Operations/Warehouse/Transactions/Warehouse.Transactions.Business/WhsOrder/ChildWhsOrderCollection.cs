using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ChildWhsOrderCollection : ActiveBusinessObjectCollection<WhsDocket>, IChildWhsOrderCollection
	{
		public ChildWhsOrderCollection(BusinessObject master)
			: base(master, typeof(WhsDocketJobPivot), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order), WhsDocketJobPivotSchema.WV_ParentId, WhsDocketJobPivotSchema.WV_WD_Docket)
		{
			if (Relationship is ManyToManyRelationship relationship)
			{
				relationship.AdditionalDivotFilter = new ZQuery(WhsDocketJobPivotSchema.WV_DocketType, DocketType.Codes.Order);
			}
		}

		IWhsDocket IChildWhsOrderCollection.this[int index] => this[index];

		protected override void SetRelationshipDefaultsForElementCore(WhsDocket newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);

			var relationShip = (ManyToManyRelationship)Relationship;
			var pivot = (WhsDocketJobPivot)relationShip.GetPivotObject(newElement);
			pivot.WV_DocketType = DocketType.Codes.Order;
			pivot.WV_ParentTableCode = relationShip.Master.TablePrefix;
		}
	}
}
