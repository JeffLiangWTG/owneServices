//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconSnapshotValidation
//
//    This class should be used for overriding validation in AutoCusReconSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusReconSnapshotValidation : AutoCusReconSnapshotValidation
	{
		public CusReconSnapshotValidation(AutoCusReconSnapshot parent) : base(parent)
		{
		}
	}
}
