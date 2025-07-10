using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	[TestFixture]
	sealed class EDILocationProviderTest
	{
		[Test]
		public void TestName()
		{
			Assert.AreEqual(ExpectedName, provider.Name);
		}

		[Test]
		public void TestCode()
		{
			Assert.AreEqual(ExpectedCode, provider.Code);
		}

		[Test]
		public void TestMailId()
		{
			Assert.AreEqual(ExpectedMailId, provider.MailId);
		}

		[SetUp]
		public void Setup()
		{
			provider = new EDILocationProvider(ExpectedName, ExpectedCode, ExpectedMailId);
		}
		EDILocationProvider provider;
		const string ExpectedName = "Name";
		const string ExpectedCode = "Code";
		const string ExpectedMailId = "MailId";
	}
}
