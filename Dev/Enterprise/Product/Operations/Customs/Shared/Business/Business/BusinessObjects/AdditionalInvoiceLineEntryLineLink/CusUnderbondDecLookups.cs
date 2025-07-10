//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUnderbondDecLookups
//
//    This class should be used for overriding collections in AutoCusUnderbondDecLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusUnderbondDecLookups : AutoCusUnderbondDecLookups
	{
		public CusUnderbondDecLookups(AutoCusUnderbondDec parent) : base(parent)
		{
		}
	}
}
