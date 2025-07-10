using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsGuaranteeTypeCTCTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(guarantee.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsGuaranteeTypeCTCCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(guarantee.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.GuaranteeTypeCTC));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(guarantee.DataSource, Is.EqualTo(Constants.UccDataSources.GuaranteeTypeCTC));
		}

		[SetUp]
		public void Setup()
		{
			guarantee = GetNctsCodeListDetails() as NctsGuaranteeTypeCTC;
		}

		NctsGuaranteeTypeCTC guarantee;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsGuaranteeTypeCTC();
	}
}
