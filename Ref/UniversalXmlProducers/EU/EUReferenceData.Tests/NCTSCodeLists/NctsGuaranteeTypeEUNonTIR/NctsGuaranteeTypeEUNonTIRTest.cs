using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsGuaranteeTypeEUNonTIRTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(guarantee.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsGuaranteeTypeEUNonTIRCode));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(guarantee.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.GuaranteeTypeEUNonTIR));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(guarantee.DataSource, Is.EqualTo(Constants.UccDataSources.GuaranteeTypeEUNonTIR));
		}

		[SetUp]
		public void Setup()
		{
			guarantee = GetNctsCodeListDetails() as NctsGuaranteeTypeEUNonTIR;
		}

		NctsGuaranteeTypeEUNonTIR guarantee;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsGuaranteeTypeEUNonTIR();
	}
}
