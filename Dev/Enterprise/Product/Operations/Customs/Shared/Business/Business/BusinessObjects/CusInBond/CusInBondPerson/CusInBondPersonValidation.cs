//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondPersonValidation
//
//    This class should be used for overriding validation in AutoCusInBondPersonValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class CusInBondPersonValidation : AutoCusInBondPersonValidation
	{
		public CusInBondPersonValidation(AutoCusInBondPerson parent)
			: base(parent)
		{
		}

		protected override void CheckCP_DateOfBirthIsValidZDateTimeRange()
		{
			var dateOfBirth = Parent.CP_DateOfBirth;
			if (dateOfBirth.IsValid && !dateOfBirth.IsEmpty && dateOfBirth > ZDateTime.Today)
			{
				var info = Parent.CP_DateOfBirthInfo;
				if (!info.BizObj.IsInDatabase || !info.OriginalValue.Equals(info.Value))
				{
					info.AddError(Res.GetString("4d66fc86-c6ee-418e-812d-c4e727a94ac5", "Date Of Birth cannot be a future date."));
				}
			}
		}

		protected override void CheckCP_GS_NKStaff()
		{
			base.CheckCP_GS_NKStaff();
			ListValidation.ErrorIfInvalidCode(Parent.CP_GS_NKStaffInfo);
		}
	}
}
