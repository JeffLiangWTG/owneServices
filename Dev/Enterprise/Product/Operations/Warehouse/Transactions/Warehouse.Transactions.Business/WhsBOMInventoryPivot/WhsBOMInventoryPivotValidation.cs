//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsBOMInventoryPivotValidation
//
//    This class should be used for overriding validation in AutoWhsBOMInventoryPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBOMInventoryPivotValidation : AutoWhsBOMInventoryPivotValidation
	{
		public WhsBOMInventoryPivotValidation(AutoWhsBOMInventoryPivot parent)
			: base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsBOMInventoryPivotSchema.Constants.WIP_WE_ComponentLine, WhsBOMInventoryPivotSchema.Constants.WIP_WE_InventoryLine);

		#endregion
	}
}
