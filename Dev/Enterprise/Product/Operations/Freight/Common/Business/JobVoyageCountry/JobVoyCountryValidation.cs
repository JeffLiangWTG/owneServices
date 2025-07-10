//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyCountryValidation
//
//    This class should be used for overriding validation in AutoJobVoyCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobVoyCountryValidation : AutoJobVoyCountryValidation
	{
		public JobVoyCountryValidation(AutoJobVoyCountry parent)
			: base(parent)
		{
		}
	}
}
