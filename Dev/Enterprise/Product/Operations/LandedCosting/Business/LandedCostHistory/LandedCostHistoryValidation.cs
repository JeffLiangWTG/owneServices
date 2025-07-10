//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLandedCostHistoryValidation
//
//    This class should be used for overriding validation in AutoLandedCostHistoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostHistoryValidation : AutoLandedCostHistoryValidation
	{
		public LandedCostHistoryValidation(AutoLandedCostHistory parent) : base(parent)
		{
		}

		protected override void CheckLH_OP()
		{
			base.CheckLH_OP();
			var info = Parent.LH_OPInfo;
			if (!info.Value.IsEmpty && info.Value.IsValid && Parent.SupplierPart != null && Parent.SupplierPart.IsCancelled)
			{
				info.AddWarning(Res.GetString("12345678-c51f-4414-bf4e-f2075ed30252", "This Product is inactive."));
			}
		}

		protected override void CheckLH_LandedCostHistoryLineType()
		{
			base.CheckLH_LandedCostHistoryLineType();
			if (Parent.SupportsNoCostApportionmentItem)
			{
				MandatoryValidation.CheckEntered(Parent.LH_LandedCostHistoryLineTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.LH_LandedCostHistoryLineTypeInfo);
				if (Parent.IsNoCostApportionmentItem)
				{
					var lcHeader = Parent.LCHeader;
					if (lcHeader != null
						&& lcHeader.Histories.All(x => x.IsNoCostApportionmentItem)
						&& lcHeader.CostInputs.Cast<LandCostInput>().Any())
					{
						Parent.LH_LandedCostHistoryLineTypeInfo.AddWarning(Res.GetString("59775a7b-2d11-465a-a665-c33444330578", "All lines are marked as No Cost Apportionment but there are Transport Logistics Costs captured."));
					}
				}
			}
		}

		protected new LandedCostHistory Parent => (LandedCostHistory)base.Parent;
	}
}
