//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTestXmlAddInfoValidation
//
//    This class should be used for overriding validation in AutoTestXmlAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class TestXmlAddInfoValidation : AutoTestXmlAddInfoValidation
	{
		public TestXmlAddInfoValidation(AutoTestXmlAddInfo parent) : base(parent)
		{
		}
	}
}
