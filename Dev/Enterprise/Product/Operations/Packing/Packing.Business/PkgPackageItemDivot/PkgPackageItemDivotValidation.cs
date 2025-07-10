//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageItemDivotValidation
//
//    This class should be used for overriding validation in AutoPkgPackageItemDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Collections.Immutable;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPackageItemDivotValidation : AutoPkgPackageItemDivotValidation
	{
		public PkgPackageItemDivotValidation(AutoPkgPackageItemDivot parent)
			: base(parent)
		{
		}

		public new PkgPackageItemDivot Parent
		{
			get { return (PkgPackageItemDivot)base.Parent; }
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return !PropertiesToIgnoreCancelValidation.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);
		}

		static ImmutableArray<string> PropertiesToIgnoreCancelValidation { get; } = ImmutableArray
			.Create(nameof(PkgPackageItemDivot.KI_KP_Package), nameof(PkgPackageItemDivot.KI_ParentID));

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidataNetWeight();
			ValidataNetWeightUQ();
		}

		public void ValidataNetWeight()
		{
			ValidateCalculatedProperty(Parent.PkgNetWeightInfo);
		}

		public void ValidataNetWeightUQ()
		{
			ValidateCalculatedProperty(Parent.PkgNetWeightUQInfo);
		}

		protected void CheckPkgNetWeight()
		{
			MandatoryValidation.CheckNotNegative(Parent.PkgNetWeightInfo);
		}

		protected void CheckPkgNetWeightUQ()
		{
			ListValidation.WarnIfInvalidCode(Parent.PkgNetWeightUQInfo);
		}
	}
}
