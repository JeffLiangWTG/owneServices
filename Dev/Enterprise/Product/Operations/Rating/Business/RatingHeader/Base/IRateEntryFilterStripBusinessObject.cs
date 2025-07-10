using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public interface IRateEntryFilterStripBusinessObject
	{
		ZQuery Filter { get; }
		void ClearRateEntryFilterStrips();

		ZQuery RateLineFilter { get; }
	}
}
