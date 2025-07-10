//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAccElectronicProcessingFeeLookups
//
//    This class should be used for overriding collections in AutoRefAccElectronicProcessingFeeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefAccElectronicProcessingFeeLookups : AutoRefAccElectronicProcessingFeeLookups
	{
		public RefAccElectronicProcessingFeeLookups(AutoRefAccElectronicProcessingFee parent) : base(parent)
		{
		}

		public static class SystemCodes
		{
			public const string CWN = "CWN";
		}

		public static class CategoryCodes
		{
			public const string STL = "STL";
		}

		public static class ElectronicProcessingFeeCodes
		{
			public const string SHD = "SHD";
			public const string BRD = "BRD";
		}
	}
}
