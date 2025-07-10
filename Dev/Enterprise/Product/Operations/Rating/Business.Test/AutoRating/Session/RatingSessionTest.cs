using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test.AutoRating.Session
{
	public class RatingSessionTest : TestCase
	{
		public void TestStart_NestedCallWhenSuspendedThrowsRatingCancelled()
		{
			using (_Rating.Start(null))
			{
				Assert(!_Rating.IsSuspended);
				var repository = new MockRepository(MockBehavior.Loose);
				var mockSuspender = repository.Create<_Rating.ISuspendOwner>();
				using (_Rating.Suspend(mockSuspender.Object))
				{
					Assert(_Rating.IsSuspended);
					AssertExceptionThrown<AutoRater.RatingCancelledException>(() => { using (_Rating.Start(null)) { } });
					mockSuspender.Verify(x => x.HandleAttemptToCreateNestedSession(), Times.Once);
				}
				Assert(!_Rating.IsSuspended);
			}

			AssertNoExceptionThrown("can start another session if first is disposed", () => { using (_Rating.Start(null)) { } });
		}
	}
}
