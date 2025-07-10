using System;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ContainerAutomationExtensionsTest : TestCase
	{
		public void TestCapitalize_Null_Throws()
		{
			AssertExceptionThrown<Exception>(() => ((string)null).Capitalize());
		}

		public void TestCapitalize_Empty_ReturnsEmpty()
		{
			AssertEquals(string.Empty, string.Empty.Capitalize());
		}

		public void TestCapitalize_Length1Capital()
		{
			AssertEquals("A", "A".Capitalize());
		}

		public void TestCapitalize_Length1Lowercase()
		{
			AssertEquals("A", "a".Capitalize());
		}

		public void TestCapitalize_Length2Lowercase()
		{
			AssertEquals("Aa", "aa".Capitalize());
		}

		public void TestCapitalize_NonLetter()
		{
			AssertEquals(" ", " ".Capitalize());
		}

		public void TestBoolSerialize_True()
		{
			AssertEquals("True", true.Serialize());
		}

		public void TestBoolSerialize_False()
		{
			AssertEquals("False", false.Serialize());
		}
	}
}
