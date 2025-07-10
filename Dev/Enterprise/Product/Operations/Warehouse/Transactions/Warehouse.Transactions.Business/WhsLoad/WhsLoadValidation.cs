using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadValidation : AutoWhsLoadValidation
	{
		public WhsLoadValidation(AutoWhsLoad parent)
			: base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsLoadSchema.Constants.WLO_WL_PlannedDockDoor, WhsLoadSchema.Constants.WLO_PL_NKCarrierServiceLevel);

		#endregion
	}
}
