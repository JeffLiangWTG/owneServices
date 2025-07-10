using NUnit.Framework;
#if NETFRAMEWORK
using System;
using NUnit.Framework.TestHelper;
using Exceptions = Enterprise.MasterFiles.Business.OrgCreditLimitAndBalanceExceptions;
#endif

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCreditLimitAndBalanceExceptionsTest : TestCase
	{
		public void TestDbExceptionSerialisation()
		{
#if NETFRAMEWORK
			var expectedReason = Exceptions.DbException.ExceptionReason.NoDataInCache;
			var expectedInnerMessage = "Test Error";
			var testException = new Exceptions.DbException(expectedReason, new Exception(expectedInnerMessage));

			var testExceptionDesirialized = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(testException) as Exceptions.DbException;

			AssertEquals("Reason", expectedReason, testExceptionDesirialized.Reason);
			AssertEquals("Message", expectedReason.ToString(), testExceptionDesirialized.Message);
			AssertEquals("Inner exception message", expectedInnerMessage, testExceptionDesirialized.InnerException.Message);
#else
			Fail("This test has not been upgraded to support .NET Core yet");
#endif
		}

		public void TestWebServiceExceptionSerialisation()
		{
#if NETFRAMEWORK
			var expectedReason = Exceptions.WebServiceException.ExceptionReason.InvalidDataInResponse;
			var expectedInnerMessage = "Test Error";
			var testException = new Exceptions.WebServiceException(expectedReason, new Exception(expectedInnerMessage));

			var testExceptionDesirialized = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(testException) as Exceptions.WebServiceException;

			AssertEquals("Reason", expectedReason, testExceptionDesirialized.Reason);
			AssertEquals("Message", expectedReason.ToString(), testExceptionDesirialized.Message);
			AssertEquals("Inner exception message", expectedInnerMessage, testExceptionDesirialized.InnerException.Message);
#else
			Fail("This test has not been upgraded to support .NET Core yet");
#endif
		}
	}
}
