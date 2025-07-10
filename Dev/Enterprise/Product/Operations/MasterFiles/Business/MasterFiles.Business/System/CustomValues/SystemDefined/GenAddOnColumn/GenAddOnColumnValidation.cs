//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenAddOnColumnValidation
//
//    This class should be used for overriding validation in AutoGenAddOnColumnValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenAddOnColumnValidation : AutoGenAddOnColumnValidation
	{
		public GenAddOnColumnValidation(AutoGenAddOnColumn parent)
			: base(parent)
		{
		}
	}
}
