//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGAttributeZZValidation
//
//    This class should be used for overriding validation in AutoUNDGAttributeZZValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;

	public class UNDGAttributeZZValidation : AutoUNDGAttributeZZValidation
	{
		public UNDGAttributeZZValidation(AutoUNDGAttributeZZ parent) : base(parent)
		{
		}

		protected override void CheckDAZ_Language()
		{
			base.CheckDAZ_Language();
			ListValidation.ErrorIfInvalidCode(Parent.DAZ_LanguageInfo);
		}
	}
}
