using System;
using CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Models;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Tests
{
	[TestFixture]
	public class NaturalKeyModelTests
	{
		[Test]
		public void TestCtor_StateIsNull_ThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new NaturalKey(null));
		}

		[Test]
		public void TestCtor_StateHasNoBadge_ThrowArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new NaturalKey("HYECMT.GB123456789000"));
		}

		[Test]
		public void TestCtor_StateHasAllValues_SplitAndAssignValues()
		{
			var naturalKey = new NaturalKey("HYECMT.GB123456789000.ABC");

			Assert.AreEqual("HYECMT", naturalKey.EnterpriseDbCode);
			Assert.AreEqual("GB123456789000", naturalKey.Eori);
			Assert.AreEqual("ABC", naturalKey.Profile);
		}
	}
}
