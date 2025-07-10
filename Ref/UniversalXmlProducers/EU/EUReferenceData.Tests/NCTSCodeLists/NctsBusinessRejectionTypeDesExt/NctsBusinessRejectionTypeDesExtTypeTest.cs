using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsBusinessRejectionTypeDesExtTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsBusinessRejectionTypeDesExt));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.BusinessRejectionTypeDesExt));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.BusinessRejectionTypeDesExt));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsBusinessRejectionTypeDesExtType;
		}
		NctsBusinessRejectionTypeDesExtType declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsBusinessRejectionTypeDesExtType();
	}
}
