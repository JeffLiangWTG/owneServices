//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsCartonGroupSizeLinkValidation
//
//    This class should be used for overriding validation in AutoWhsCartonGroupSizeLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsCartonGroupSizeLinkValidation : AutoWhsCartonGroupSizeLinkValidation
	{
		public WhsCartonGroupSizeLinkValidation(AutoWhsCartonGroupSizeLink parent)
			: base(parent)
		{
		}

		protected override void CheckWCV_OptimizationCost()
		{
			base.CheckWCV_OptimizationCost();
			CompareValidation.CheckNumberGreaterThanZero(Parent.WCV_OptimizationCostInfo);
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsCartonGroupSizeLinkSchema.Constants.WCV_WCG, WhsCartonGroupSizeLinkSchema.Constants.WCV_WCS);

		#endregion
	}
}
