using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsRejectionCodeDepartureExportTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsRejectionCodeDepartureExport));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.RejectionCodeDepartureExport));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.RejectionCodeDepartureExport));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsRejectionCodeDepartureExportType;
		}
		NctsRejectionCodeDepartureExportType declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsRejectionCodeDepartureExportType();
	}
}

