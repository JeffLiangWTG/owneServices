using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsRejectionCodeDestinationExitTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsRejectionCodeDestinationExitCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.RejectionCodeDestinationExit));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.RejectionCodeDestinationExit));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsRejectionCodeDestinationExitType;
		}
		NctsRejectionCodeDestinationExitType declarationType;
		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsRejectionCodeDestinationExitType();
	}
}
