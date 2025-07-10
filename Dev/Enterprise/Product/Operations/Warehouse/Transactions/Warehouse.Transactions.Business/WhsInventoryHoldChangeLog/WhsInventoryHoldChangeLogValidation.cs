//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsInventoryHoldChangeLogValidation
//
//    This class should be used for overriding validation in AutoWhsInventoryHoldChangeLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryHoldChangeLogValidation : AutoWhsInventoryHoldChangeLogValidation
	{
		public WhsInventoryHoldChangeLogValidation(AutoWhsInventoryHoldChangeLog parent) : base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsInventoryHoldChangeLogSchema.Constants.WHL_WE_ParentDocketLine, WhsInventoryHoldChangeLogSchema.Constants.WHL_WHC_NKCode);

		#endregion
	}
}
