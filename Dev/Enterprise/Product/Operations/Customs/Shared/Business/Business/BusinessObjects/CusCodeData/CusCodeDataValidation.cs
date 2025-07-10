//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCodeDataValidation
//
//    This class should be used for overriding validation in AutoCusCodeDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusCodeDataValidation : AutoCusCodeDataValidation
	{
		public CusCodeDataValidation(AutoCusCodeData parent)
			: base(parent)
		{
		}

		new CusCodeData Parent
		{
			get { return (CusCodeData)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			CheckCY_CodeIsNotEmpty();
			CheckCY_CodeList();
		}

		protected virtual void CheckCY_CodeList()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo, Parent.Lookups.CY_CodeList);
		}

		protected virtual void CheckCY_CodeIsNotEmpty()
		{
			MandatoryValidation.CheckEntered(Parent.CY_CodeInfo);
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			if (Parent.CY_DataAllowWesternEuropeanCharactersOnly)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.CY_DataInfo);
			}
		}
	}
}
