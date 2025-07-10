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

using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyCountryValidation : JobVoyCountryValidation
	{
		public BaseJobVoyCountryValidation(AutoJobVoyCountry parent) : base(parent)
		{
		}
	}
}
