namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class JobDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		#region TestAutoRatingStatusInfo

		public void TestAutoRatingStatusInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_TotalAmountReturned = 100;
			entryHeader.CH_TotalPaid = 200;

			const string cannotAutorateMessage = "{0}\r\n\r\nAre you sure you want to continue with AutoRating?";
			var expectedMessage = string.Format(cannotAutorateMessage, JobDeclarationTest.GetEntryTotalAmountDoesNotMatchReturnedOneMessage(200, 100));
			AssertAutoRatingStatusInfo(declaration, expectedCanExecute: true, expectedMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertAutoRatingStatusInfo(declaration, expectedCanExecute: true, string.Empty);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_TotalAmountReturned = 0;
			AssertAutoRatingStatusInfo(declaration, expectedCanExecute: true, string.Empty);

			entryHeader.CH_TotalAmountReturned = 100;
			entryHeader.CH_TotalPaid = 0;
			expectedMessage = string.Format(cannotAutorateMessage, JobDeclarationTest.GetEntryTotalAmountDoesNotMatchReturnedOneMessage(0, 100));
			AssertAutoRatingStatusInfo(declaration, expectedCanExecute: true, expectedMessage);

			entryHeader.CH_TotalPaid = 100;
			AssertAutoRatingStatusInfo(declaration, expectedCanExecute: true, string.Empty);
		}

		static void AssertAutoRatingStatusInfo(JobDeclaration declaration, bool expectedCanExecute, string expectedMessage)
		{
			var autoRating = declaration.RatingAdapter;
			AssertEquals("CanExecute", expectedCanExecute, autoRating.StatusInformation.CanExecute);
			AssertEquals("Message", expectedMessage, autoRating.StatusInformation.Message);
		}

		#endregion
	}
}
