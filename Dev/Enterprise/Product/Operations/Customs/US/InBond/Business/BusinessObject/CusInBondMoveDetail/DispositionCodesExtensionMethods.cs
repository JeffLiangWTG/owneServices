using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	static class DispositionCodesExtensionMethods
	{
		public static DispositionData GetLatestDispositionForInBondClosedDate(this DispositionDataCollection dispositions, string transportMode)
		{
			return dispositions.GetLatestDisposition(shouldIgnoreDispositionCode: (x) => DispositionCodeListLoader.IsInBondDispositionCode(transportMode, dispositions.Factory, x));
		}
	}
}
