using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadPkgPackagePivotValidation : AutoWhsLoadPkgPackagePivotValidation
	{
		public WhsLoadPkgPackagePivotValidation(AutoWhsLoadPkgPackagePivot parent)
			: base(parent)
		{
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsLoadPkgPackagePivotSchema.Constants.WLP_KP_Package, WhsLoadPkgPackagePivotSchema.Constants.WLP_WLO_Load);

		#endregion
	}
}
