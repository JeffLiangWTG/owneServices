//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNMFSHarvestingDetailAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNMFSHarvestingDetailAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USNMFSHarvestingDetailAddInfoValidation : AutoUSNMFSHarvestingDetailAddInfoValidation
	{
		public USNMFSHarvestingDetailAddInfoValidation(AutoUSNMFSHarvestingDetailAddInfo parent)
			: base(parent)
		{
		}
	}
}
