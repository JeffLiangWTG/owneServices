//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGCountryReferenceValidation
//
//    This class should be used for overriding validation in AutoUNDGCountryReferenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCountryReferenceValidation : AutoUNDGCountryReferenceValidation
	{
		public UNDGCountryReferenceValidation(AutoUNDGCountryReference parent) : base(parent)
		{
		}
	}
}
