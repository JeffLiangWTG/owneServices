using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class NameExtensionsTest : TestCase
	{
		public void TestGetFirstName()
		{
			var name = (ZString)"john smith";
			AssertEquals("john", name.GetFirstName());

			var name2 = (ZString)"john";
			AssertEquals("john", name2.GetFirstName());
		}

		public void TestGetLastName()
		{
			var name = (ZString)"john smith";
			AssertEquals("smith", name.GetLastName());

			var name2 = (ZString)"john";
			AssertEquals("", name2.GetLastName());
		}
	}
}
