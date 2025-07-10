//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCountriesAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSCountriesAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCountriesAddInfoValidation : AutoUSCountriesAddInfoValidation
	{
		public USCountriesAddInfoValidation(AutoUSCountriesAddInfo parent) : base(parent)
		{
		}

		new LaceyCountryAddInfo Parent
		{
			get { return (LaceyCountryAddInfo)base.Parent; }
		}

		protected override void CheckUS_CountryCode()
		{
			base.CheckUS_CountryCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_CountryCodeInfo, Parent.Lookups.USCountryList);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CountryCodeInfo);
		}
	}
}
