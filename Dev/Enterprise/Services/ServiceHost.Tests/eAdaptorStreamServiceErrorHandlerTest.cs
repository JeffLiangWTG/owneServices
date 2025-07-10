using System;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class eAdaptorStreamServiceErrorHandlerTest : TestCase
	{
		public void TestProvideFault()
		{
			var exception = GenerateExceptionWithStackTrace();
			Message fault = null;

			var handler = new eAdaptorStreamServiceErrorHandler();
			handler.ProvideFault(exception, MessageVersion.Default, ref fault);

			AssertNotNull(fault);
			var faultException = MessageFault.CreateFault(fault, int.MaxValue);
			var faultReason = faultException.Reason.ToString();
			AssertContains("Test Exception", faultReason);
			Assert(!ContainsStackTrace(faultReason));
		}

		bool ContainsStackTrace(string input)
		{
			var pattern = @"\bat\s+\w+.*\s+in\s+.*:\s*line\s+\d+";
			return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
		}

		Exception GenerateExceptionWithStackTrace()
		{
			try
			{
				ThrowException();
			}
			catch (Exception ex)
			{
				return new Exception("Test Exception", ex);
			}
			return null;
		}

		void ThrowException()
		{
			throw new InvalidOperationException("Original exception");
		}
	}
}
