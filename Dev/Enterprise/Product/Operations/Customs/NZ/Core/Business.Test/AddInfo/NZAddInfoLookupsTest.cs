//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;

	internal class NZAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYesNoList()
		{
			AssertEquals(typeof(YesNoList), Lookups.YesNoList.GetType());
		}

		NZAddInfoLookups Lookups
		{
			get { return lookups ?? (lookups = new NZAddInfoLookups(new NZAddInfo(Factory.New<JobDeclaration>()))); }
		}

		NZAddInfoLookups lookups;
	}
}
