using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IRatingSupporterWithAdapter : IRatingSupporter
	{
		IAutoRating RatingAdapter { get; }
	}
}
