using System;
using static NUnit.Framework.Assertion;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	public static class ArgumentNullExceptionAssertUtil
	{
		public static void Assert(string parameterName, AnonymousMethod codeToRun)
		{
#if NETFRAMEWORK
			var expectedExceptionMessage = @"Value cannot be null.
Parameter name: " + parameterName;
#else
			var expectedExceptionMessage = @"Value cannot be null. (Parameter '" + parameterName + "')";
#endif

			AssertExceptionThrown(typeof(ArgumentNullException), expectedExceptionMessage, codeToRun);
		}
	}
}
