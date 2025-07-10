using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GeneratedOverLengthCodeExceptionTest : TestCase
	{
		public void TestIsUserVisible()
		{
			AssertEquals("Should be user visible.", ExceptionVisibility.User, ExceptionVisibilityAttribute.Evaluate(new GeneratedOverLengthCodeException()));
		}
	}
}
