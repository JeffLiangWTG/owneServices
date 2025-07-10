using Enterprise.Rating.Web.Configuration;
using Enterprise.Rating.Web.Controllers;
using Enterprise.Services.ServiceHost;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Configuration
{
	public class RatingWebControllerTypeResolverTest : TestCase
	{
		public void TestIsRatingControllerType()
		{
			Assert(RatingWebControllerTypeResolver.IsRatingControllerType(typeof(WTGStatusCheckController)));
			Assert(RatingWebControllerTypeResolver.IsRatingControllerType(typeof(RatesAPIController)));
			Assert(RatingWebControllerTypeResolver.IsRatingControllerType(typeof(RootController)));

			Assert(!RatingWebControllerTypeResolver.IsRatingControllerType(typeof(HomeController)));
		}
	}
}
