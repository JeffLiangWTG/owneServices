using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ErrorMessageHelperTest : TestCase
	{
		public void TestCodeToLocalizedMessage()
		{
			AssertCodeToLocalizedMessage(ErrorCode.ERR000, "Credit report service is unavailable, please try again later.");
			AssertCodeToLocalizedMessage(ErrorCode.ERR001, "The ComprehensiveReport report type that you have requested is not currently available for WiseTech Global. You may wish to order a different report type.\r\n\r\nIf you would like to request this report be made available.Please raise a CR9 service request.");
			AssertCodeToLocalizedMessage(ErrorCode.ERR002, "The ComprehensiveReport report type is unavailable for unincorporated entries. Please select a different report.");
			AssertCodeToLocalizedMessage(ErrorCode.ERR003, "The credit bureau is unable to find the provided registration code.\r\n\r\nPlease review if the number is valid. If it is and you would like to order a report, please raise a CR9 service request.");
			AssertCodeToLocalizedMessage(ErrorCode.ERR004, "The Main Address UNLOCO and DUNS number's country/region do not match. Please review and change accordingly.\r\n\r\nIf it is and you would like to order a report, please raise a CR9 service request.");
			AssertCodeToLocalizedMessage(ErrorCode.ERR005, "Credit report service is unavailable, please reload the form and try again later.");
			AssertCodeToLocalizedMessage(ErrorCode.ERR999, "An error has occurred while completing this request. Please raise a CR9 Service Request.");
		}

		void AssertCodeToLocalizedMessage(ErrorCode errorCode, string expectedMessage)
		{
			var errorInfo = new ErrorInfo(errorCode);

			var errorMessage = errorInfo.ToLocalizedMessage("ComprehensiveReport", "WiseTech Global");

			AssertEquals("Should show correct error message", expectedMessage, errorMessage);
		}
	}
}
