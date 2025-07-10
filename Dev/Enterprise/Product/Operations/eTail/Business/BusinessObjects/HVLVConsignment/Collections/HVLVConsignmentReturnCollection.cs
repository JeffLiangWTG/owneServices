using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentReturnCollection : BusinessObjectCollection<HVLVConsignment>, IHVLVConsignmentCollection
	{
		public enum ConsignmentType
		{
			Return,
			Former
		}

		public HVLVConsignmentReturnCollection(HVLVConsignment consignment, ConsignmentType flag)
			: base(consignment.Factory)
		{
			this.consignment = consignment;
			this.flag = flag;
		}

		IHVLVConsignment IHVLVConsignmentCollection.this[int i] => (IHVLVConsignment)Elements[i];
		readonly HVLVConsignment consignment;
		readonly ConsignmentType flag;

		public override void Load()
		{
			var query = new ZDBOnlyQuery(typeof(HVLVConsignment));
			if (flag == ConsignmentType.Former)
			{
				var pivotSubQuery = new ZDBOnlySubQuery(typeof(HVLVReturnPivot), HVLVReturnPivotSchema.HVP_HVC_Former);
				pivotSubQuery.AddToFilter(HVLVReturnPivotSchema.HVP_HVC_Return, consignment.PK);
				query.AddSubQuery(pivotSubQuery, JoinCondition.And);
			}
			else
			{
				var pivotSubQuery = new ZDBOnlySubQuery(typeof(HVLVReturnPivot), HVLVReturnPivotSchema.HVP_HVC_Return);
				pivotSubQuery.AddToFilter(HVLVReturnPivotSchema.HVP_HVC_Former, consignment.PK);
				query.AddSubQuery(pivotSubQuery, JoinCondition.And);
			}

			AddRange(Factory.Load<HVLVConsignment>(query));
		}
	}
}
