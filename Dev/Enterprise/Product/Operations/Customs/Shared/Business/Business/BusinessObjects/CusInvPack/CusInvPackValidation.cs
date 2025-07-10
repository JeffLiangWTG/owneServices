//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInvPackValidation
//
//    This class should be used for overriding validation in AutoCusInvPackValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusInvPackValidation : AutoCusInvPackValidation
	{
		public CusInvPackValidation(AutoCusInvPack parent)
			: base(parent)
		{
		}

		protected new CusInvPack Parent => (CusInvPack)base.Parent;

		protected override void CheckB5_TypeOfDifference()
		{
			base.CheckB5_TypeOfDifference();

			ListValidation.ErrorIfInvalidCode(Parent.B5_TypeOfDifferenceInfo, Parent.Lookups.TypeOfDifferenceList);
		}

		protected override void CheckB5_UnitCount()
		{
			MandatoryValidation.CheckNotNegative(Parent.B5_UnitCountInfo);
		}

		protected override void CheckB5_UnitsReleased()
		{
			MandatoryValidation.CheckNotNegative(Parent.B5_UnitsReleasedInfo);
		}
	}
}
