using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public RatingHeaderFetchStrategy(RatingHeader header)
			: base(header)
		{
		}
	}
}
