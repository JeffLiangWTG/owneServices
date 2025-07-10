using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationValidation : AutoWhsCycleCountLocationValidation
	{
		public WhsCycleCountLocationValidation(AutoWhsCycleCountLocation parent) : base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(
				WhsCycleCountLocationSchema.Constants.WCL_WL_Location,
				WhsCycleCountLocationSchema.Constants.WCL_WCL_RejectedCycleCount,
				WhsCycleCountLocationSchema.Constants.WCL_P9_Task);

		#endregion
	}
}
