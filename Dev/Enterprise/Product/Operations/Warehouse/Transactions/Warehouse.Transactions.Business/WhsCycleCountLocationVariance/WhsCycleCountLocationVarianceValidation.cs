using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationVarianceValidation : AutoWhsCycleCountLocationVarianceValidation
	{
		public WhsCycleCountLocationVarianceValidation(AutoWhsCycleCountLocationVariance parent) : base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsCycleCountLocationVarianceSchema.Constants.WCC_WCL_CycleCountLocation, WhsCycleCountLocationVarianceSchema.Constants.WCC_WL_ExpectedStockLocation);

		#endregion
	}
}
