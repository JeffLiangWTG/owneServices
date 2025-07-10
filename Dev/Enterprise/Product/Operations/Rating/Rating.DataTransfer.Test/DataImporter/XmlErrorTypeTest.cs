using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Test
{
	[TestedType(typeof(RateErrorType))]
	public class XmlErrorTypeTest : ErrorTypeTest
	{
		protected override void TestStaticErrorTypesCore()
		{
			base.TestStaticErrorTypesCore();

			AssertEquals("UnmatchOrgAddress", RateErrorType.UnmatchOrgAddress.Name);
			AssertEquals("", RateErrorType.UnmatchOrgAddress.Message);

			AssertEquals("InvalidOrgAddressType", RateErrorType.InvalidOrgAddressType.Name);
			AssertEquals("", RateErrorType.InvalidOrgAddressType.Message);

			AssertEquals("InvalidChargeCode", RateErrorType.InvalidChargeCode.Name);
			AssertEquals("Invalid Charge Code", RateErrorType.InvalidChargeCode.Message);

			AssertEquals("UnknownCartageZone", RateErrorType.UnknownCartageZone.Name);
			AssertEquals("Transport Zone Set Does Not Exist", RateErrorType.UnknownCartageZone.Message);
		}

		protected override ErrorType NewNotificationType(string name)
		{
			return new RateErrorType(name);
		}

		protected override ErrorType NewNotificationType(string name, string message)
		{
			return new RateErrorType(name, message);
		}
	}
}
