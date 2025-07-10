using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class OPYORCTransactionOutstandingAmountValidationTest : TestCaseWithFactory
	{
		public void TestCheckTransactionHeaderOutstandingAmount_WhenShouldNOTCheckTransactionHeaderOutstandingAmount()
		{
			var wrapperMock = new Mock<IOutstandingAmountValidationWrapper>();
			wrapperMock.Setup(x => x.ShouldCheckTransactionHeaderOutstandingAmount).Returns(false);

			AssertNullOrEmpty(new OPYORCTransactionOutstandingAmountValidation().Validate(wrapperMock.Object));

			wrapperMock.VerifyGet(x => x.ShouldCheckTransactionHeaderOutstandingAmount, Times.Once);
			wrapperMock.VerifyGet(x => x.OutstandingAmount, Times.Never);
			wrapperMock.VerifyGet(x => x.TotalAmount, Times.Never);
			wrapperMock.Verify(x => x.GetTransactionHeaderErrorMessage(), Times.Never);
		}

		public void TestCheckTransactionHeaderOutstandingAmount_WhenOutstandingAmountIsNotEqualToTotalAmount()
		{
			var wrapperMock = new Mock<IOutstandingAmountValidationWrapper>();
			wrapperMock.Setup(x => x.ShouldCheckTransactionHeaderOutstandingAmount).Returns(true);
			wrapperMock.Setup(x => x.OutstandingAmount).Returns(99m);
			wrapperMock.Setup(x => x.TotalAmount).Returns(100m);
			wrapperMock.Setup(x => x.GetTransactionHeaderErrorMessage()).Returns("Error");

			AssertEquals("Error", new OPYORCTransactionOutstandingAmountValidation().Validate(wrapperMock.Object));

			wrapperMock.VerifyGet(x => x.ShouldCheckTransactionHeaderOutstandingAmount, Times.Once);
			wrapperMock.VerifyGet(x => x.OutstandingAmount, Times.AtLeastOnce);
			wrapperMock.VerifyGet(x => x.TotalAmount, Times.AtLeastOnce);
			wrapperMock.Verify(x => x.GetTransactionHeaderErrorMessage(), Times.Once);
		}

		public void TestCheckTransactionHeaderOutstandingAmount_WhenOutstandingAmountIsEqualToTotalAmount()
		{
			var wrapperMock = new Mock<IOutstandingAmountValidationWrapper>();
			wrapperMock.Setup(x => x.ShouldCheckTransactionHeaderOutstandingAmount).Returns(true);
			wrapperMock.Setup(x => x.OutstandingAmount).Returns(100m);
			wrapperMock.Setup(x => x.TotalAmount).Returns(100m);
			wrapperMock.Setup(x => x.GetTransactionHeaderErrorMessage()).Returns("Error");

			AssertNullOrEmpty(new OPYORCTransactionOutstandingAmountValidation().Validate(wrapperMock.Object));

			wrapperMock.VerifyGet(x => x.ShouldCheckTransactionHeaderOutstandingAmount, Times.Once);
			wrapperMock.VerifyGet(x => x.OutstandingAmount, Times.AtLeastOnce);
			wrapperMock.VerifyGet(x => x.TotalAmount, Times.AtLeastOnce);
			wrapperMock.Verify(x => x.GetTransactionHeaderErrorMessage(), Times.Never);
		}
	}
}
