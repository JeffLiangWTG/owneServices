using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsQueryIdentifierTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(nctsQueryIdentifier.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsQueryIdentifierCodeType));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(nctsQueryIdentifier.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.QueryIdentifier));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(nctsQueryIdentifier.DataSource, Is.EqualTo(Constants.UccDataSources.QueryIdentifier));
		}

		[SetUp]
		public void Setup()
		{
			nctsQueryIdentifier = GetNctsCodeListDetails() as NctsQueryIdentifierType;
		}

		NctsQueryIdentifierType nctsQueryIdentifier;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsQueryIdentifierType();
	}
}

