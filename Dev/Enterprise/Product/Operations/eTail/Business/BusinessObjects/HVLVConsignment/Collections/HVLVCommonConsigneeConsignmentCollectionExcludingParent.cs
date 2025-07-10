using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVCommonConsigneeConsignmentCollectionExcludingParent : HVLVCommonConsigneeConsignmentCollection
	{
		public HVLVCommonConsigneeConsignmentCollectionExcludingParent(HVLVConsignment consignment)
			: base(consignment)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(HVLVConsignmentSchema.PK, SQLComparisonOperator.NotEqual, ParentConsignment.PK);
			return result;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(HVLVConsignmentSchema.HVC_JE_ImportDeclaration, SQLComparisonOperator.Equal, null);
			result.AddToFilter(HVLVConsignmentSchema.HVC_JE_ExportDeclaration, SQLComparisonOperator.Equal, null);
			return result;
		}
	}
}
