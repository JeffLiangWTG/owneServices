using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class SecureServiceBaseTestApproximateSuccessTest : TestCase
	{
		public void TestApproximateSecurityCodeEqualIgnoresDate()
		{
			var securityCodeOld = "8hihSbHKdQYURr7S9I1L9CTpfVjgHxxl3+LZETNx3IISWLCryiD19pVWCGninHEQ";
			var securityCodeCurrent = Utilities.GenerateSecurityToken("test", "test", "BR", "DP");
			SecureServiceBaseTestCase<SecureService>.AssertSecurityCodeApproximateEqual(securityCodeOld, securityCodeCurrent);
		}
	}
}
