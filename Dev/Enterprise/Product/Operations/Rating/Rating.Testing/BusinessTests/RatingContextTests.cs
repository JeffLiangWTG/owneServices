using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Rating.Services;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingContextTests : RatingTestCase
	{
		public void TestCreateForManualSelect_IDialogServiceShouldNotBeNull()
		{
			var mockedLogger = new Mock<ILogger>();

			AssertExceptionThrown(
				typeof(ArgumentNullException),
				() => RatingContext.CreateForManualSelect(mockedLogger.Object, null)
			);

			var mockedDialog = new Mock<IDialogService>();
			var ratingContext = RatingContext.CreateForManualSelect(mockedLogger.Object, mockedDialog.Object);
			AssertSame(mockedDialog.Object, ratingContext.DialogService);
		}
	}
}
