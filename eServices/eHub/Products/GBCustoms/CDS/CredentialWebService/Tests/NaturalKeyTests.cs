using System;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.Tests
{
	[TestFixture]
	public class NaturalKeyTests
	{
		[Test]
		public void TestCtor_NaturalKeyIsNull_ThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new NaturalKey(null));
		}

		[Test]
		public void TestCtor_NaturalKeyHasNoBadge_ThrowArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new NaturalKey("HYECMT.GB123456789000"));
		}

		[Test]
		public void TestCtor_NaturalKeyHasAllValues_SplitAndAssignValues()
		{
			var naturalKey = new NaturalKey("HYECMT.GB123456789000.ABC");

			Assert.AreEqual("HYECMT", naturalKey.EnterpriseDbCode);
			Assert.AreEqual("HYE", naturalKey.EnterpriseCode);
			Assert.AreEqual("CMT", naturalKey.DBCode);
			Assert.AreEqual("GB123456789000.ABC", naturalKey.EoriBadge);
		}
	}
}
