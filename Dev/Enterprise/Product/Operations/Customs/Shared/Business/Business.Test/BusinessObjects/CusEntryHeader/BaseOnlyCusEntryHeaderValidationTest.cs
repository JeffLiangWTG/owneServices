using CargoWise.EntityFramework.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseOnlyCusEntryHeaderValidationTest : TestCaseWithFactory
	{
		public void TestDeactivatedEntryWithActiveCustomsTransactionError()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);

			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockDeclaration.Object.CustomsEntryHeaders.Add(mockEntry.Object);

			mockEntry.Setup(m => m.IsActive).Returns(false);
			mockEntry.Setup(m => m.HasBeenWithdrawn).Returns(false);
			mockEntry.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);

			mockEntry.Object.Validation.ValidateCH_BGMReference();
			AssertHasError(mockEntry.Object.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);

			mockEntry.Setup(m => m.HasBeenLodgedAtCustoms).Returns(false);
			mockEntry.Setup(m => m.IsWaitingForResponse).Returns(true);

			mockEntry.Object.Validation.ValidateCH_BGMReference();
			AssertHasError(mockEntry.Object.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);

			mockEntry.Reset();
			mockEntry.Setup(m => m.IsWaitingForResponse).Returns(false);
			mockEntry.Object.Validation.ValidateCH_BGMReference();
			AssertNoError(mockEntry.Object.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
		}

		public void TestNonAmendableChangesMessageError()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsCustomsHeaderAmendmentATotalReplacement).Returns(false);

			var mockEntry = Factory.NewMoq<CusEntryHeader>();
			mockDeclaration.Object.CustomsEntryHeaders.Add(mockEntry.Object);

			mockEntry.Setup(m => m.IsActive).Returns(true);
			mockEntry.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);
			mockEntry.Setup(m => m.HasBeenWithdrawn).Returns(false);
			mockEntry.Protected().Setup<bool>("HasNonAmendableChangesCore").Returns(true);

			mockEntry.Object.Validation.ValidateCH_BGMReference();
			AssertHasMessageError(mockEntry.Object.CH_BGMReferenceInfo, CusEntryHeaderValidation.HasNonAmendableChanges);

			mockEntry.Reset();
			mockEntry.Protected().Setup<bool>("HasNonAmendableChangesCore").Returns(false);

			mockEntry.Object.Validation.ValidateCH_BGMReference();
			AssertNoMessageError(mockEntry.Object.CH_BGMReferenceInfo, CusEntryHeaderValidation.HasNonAmendableChanges);
		}
	}
}
