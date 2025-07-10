//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZMAFFilesAddInfoValidation
//
//    This class should be used for overriding validation in AutoNZMAFFilesAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public class NZMAFFilesAddInfoValidation : AutoNZMAFFilesAddInfoValidation
	{
		public NZMAFFilesAddInfoValidation(AutoNZMAFFilesAddInfo parent) : base(parent)
		{
		}
	}
}
