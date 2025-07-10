using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsNationalityTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(nctsNationality.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsNationalityCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(nctsNationality.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.Nationality));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(nctsNationality.DataSource, Is.EqualTo(Constants.UccDataSources.Nationality));
		}

		[SetUp]
		public void Setup()
		{
			nctsNationality = GetNctsCodeListDetails() as NctsNationality;
		}
		NctsNationality nctsNationality;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsNationality();
	}
}
