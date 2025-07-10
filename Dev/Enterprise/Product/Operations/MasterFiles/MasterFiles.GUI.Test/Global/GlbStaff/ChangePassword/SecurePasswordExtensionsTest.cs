using System.Net;
using System.Security;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test;

public class SecurePasswordExtensionsTest : TestCase
{
	public void TestToInsecureString()
	{
		var expected = "#password123";
		var secureString = new NetworkCredential("", expected).SecurePassword;

		AssertEquals("ToInsecureString", expected, secureString.ToInsecureString());
	}

	public void TestToInsecureString_WhenNull()
	{
		SecureString secureString = null;

		AssertEquals("ToInsecureString", string.Empty, secureString.ToInsecureString());
	}
}
