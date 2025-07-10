using Moq;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class ARAPOutstandingAmountValidationTest : OutstandingAmountValidationTest
	{
		public void TestCheckMatchLinkOutstandingAmount_WhenShouldNOTCheckMatchLinkOutstandingAmount()
		{
			var wrapperMock = new Mock<IOutstandingAmountValidationWrapper>();
			wrapperMock.Setup(x => x.ShouldCheckMatchLinkOutstandingAmount).Returns(false);

			AssertNullOrEmpty(new ARAPOutstandingAmountValidation().Validate(wrapperMock.Object));

			wrapperMock.VerifyGet(x => x.ShouldCheckMatchLinkOutstandingAmount, Times.Once);
			wrapperMock.VerifyGet(x => x.OutstandingAmount, Times.Never);
			wrapperMock.VerifyGet(x => x.TotalAmount, Times.Never);
			wrapperMock.VerifyGet(x => x.MatchLinkAmountSum, Times.Never);
			wrapperMock.Verify(x => x.GetMatchLinkErrorMessage(), Times.Never);
		}

		public void TestCheckMatchLinkOutstandingAmount_WhenOutstandingAmountIsNotEqualToTotalAmountMinusSumMatchLinkAmount()
		{
			var wrapperMock = new Mock<IOutstandingAmountValidationWrapper>();
			wrapperMock.Setup(x => x.ShouldCheckMatchLinkOutstandingAmount).Returns(true);
			wrapperMock.Setup(x => x.OutstandingAmount).Returns(99m);
			wrapperMock.Setup(x => x.TotalAmount).Returns(100m);
			wrapperMock.Setup(x => x.MatchLinkAmountSum).Returns(2m);
			wrapperMock.Setup(x => x.GetMatchLinkErrorMessage()).Returns("Error");

			AssertEquals("Error", new ARAPOutstandingAmountValidation().Validate(wrapperMock.Object));

			wrapperMock.VerifyGet(x => x.ShouldCheckMatchLinkOutstandingAmount, Times.Once);
			wrapperMock.VerifyGet(x => x.OutstandingAmount, Times.AtLeastOnce);
			wrapperMock.VerifyGet(x => x.TotalAmount, Times.AtLeastOnce);
			wrapperMock.VerifyGet(x => x.MatchLinkAmountSum, Times.Once);
			wrapperMock.Verify(x => x.GetMatchLinkErrorMessage(), Times.Once);
		}

		public void TestCheckMatchLinkOutstandingAmount_WhenOutstandingAmountIsEqualToTotalAmountMinusSumMatchLinkAmount()
		{
			var wrapperMock = new Mock<IOutstandingAmountValidationWrapper>();
			wrapperMock.Setup(x => x.ShouldCheckMatchLinkOutstandingAmount).Returns(true);
			wrapperMock.Setup(x => x.OutstandingAmount).Returns(99m);
			wrapperMock.Setup(x => x.TotalAmount).Returns(100m);
			wrapperMock.Setup(x => x.MatchLinkAmountSum).Returns(1m);
			wrapperMock.Setup(x => x.GetMatchLinkErrorMessage()).Returns("Error");

			AssertNullOrEmpty(new ARAPOutstandingAmountValidation().Validate(wrapperMock.Object));

			wrapperMock.VerifyGet(x => x.ShouldCheckMatchLinkOutstandingAmount, Times.Once);
			wrapperMock.VerifyGet(x => x.OutstandingAmount, Times.AtLeastOnce);
			wrapperMock.VerifyGet(x => x.TotalAmount, Times.AtLeastOnce);
			wrapperMock.VerifyGet(x => x.MatchLinkAmountSum, Times.Once);
			wrapperMock.Verify(x => x.GetMatchLinkErrorMessage(), Times.Never);
		}
	}
}
