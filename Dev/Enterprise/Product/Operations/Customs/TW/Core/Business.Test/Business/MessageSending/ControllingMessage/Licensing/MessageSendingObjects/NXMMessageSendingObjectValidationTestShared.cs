using static CargoWise.EntityFramework.Testing.TestCaseWithFactory;
using static NUnit.Framework.AssertionWithHtml;

namespace Enterprise.Customs.TW.Business.Testing
{
	static class NXMMessageSendingObjectValidationTestShared
	{
		public static void TestValidateShouldSend_LineNumber(SupportingDocument document)
		{
			CombineAssertions("document.LineNumber", () =>
			{
				const string errorMessage = "Please enter a 'Document Line Number' greater than 0.";
				document.LineNumber = 0;
				AssertHasMessageErrorContaining(document.LineNumberInfo, errorMessage);
				document.LineNumber = -1;
				AssertHasMessageErrorContaining(document.LineNumberInfo, errorMessage);
				document.LineNumber = 1;
				AssertNoError(document.LineNumberInfo, errorMessage);
			});
		}
	}
}
