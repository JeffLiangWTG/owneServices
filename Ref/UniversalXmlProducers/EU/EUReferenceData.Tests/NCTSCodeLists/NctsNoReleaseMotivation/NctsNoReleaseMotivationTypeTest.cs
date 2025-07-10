using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsNoReleaseMotivationTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsNoReleaseMotivationCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.NoReleaseMotivation));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.NoReleaseMotivation));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsNoReleaseMotivationType;
		}
		NctsNoReleaseMotivationType declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsNoReleaseMotivationType();
	}
}
