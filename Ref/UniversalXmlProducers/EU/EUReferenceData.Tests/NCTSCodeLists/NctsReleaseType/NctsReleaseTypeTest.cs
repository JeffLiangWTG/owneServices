using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsReleaseTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(guarantee.CodeType, Is.EqualTo("CL163"));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(guarantee.CodeListType, Is.EqualTo("ReleaseType"));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(guarantee.DataSource, Is.EqualTo("EUN Release Type"));
		}

		[SetUp]
		public void Setup()
		{
			guarantee = GetNctsCodeListDetails() as NctsReleaseType;
		}
		NctsReleaseType guarantee;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsReleaseType();
	}
}
