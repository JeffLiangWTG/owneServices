//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobOrphanScanValidation
//
//    This class should be used for overriding validation in AutoJobOrphanScanValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class JobOrphanScanValidation : AutoJobOrphanScanValidation
	{
		public JobOrphanScanValidation(AutoJobOrphanScan parent)
			: base(parent)
		{
		}
	}
}

