//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolLookups
//
//    This class should be used for overriding collections in AutoJobConsolLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class JobConsolLookups : AutoJobConsolLookups
	{
		public JobConsolLookups(AutoJobConsol parent)
			: base(parent)
		{
		}

		#region ScreeningStatusesList

		public CodeDescriptionPairList ScreeningStatusesList => Factory.GetCachedValue<ScreeningStatusesList>();

		#endregion
	}
}
