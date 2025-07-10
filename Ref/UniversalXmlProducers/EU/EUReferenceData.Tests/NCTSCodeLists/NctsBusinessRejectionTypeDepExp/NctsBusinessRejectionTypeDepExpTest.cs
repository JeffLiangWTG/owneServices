using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsBusinessRejectionTypeDepExpTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(rejectionType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsBusinessRejectionTypeDepExpCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(rejectionType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.BusinessRejectionTypeDepExp));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(rejectionType.DataSource, Is.EqualTo(Constants.UccDataSources.BusinessRejectionTypeDepExp));
		}

		[SetUp]
		public void Setup()
		{
			rejectionType = GetNctsCodeListDetails() as NctsBusinessRejectionTypeDepExp;
		}

		NctsBusinessRejectionTypeDepExp rejectionType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsBusinessRejectionTypeDepExp();

	}
}
