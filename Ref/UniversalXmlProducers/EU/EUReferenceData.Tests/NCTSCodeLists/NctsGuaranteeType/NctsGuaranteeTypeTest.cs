using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsGuaranteeTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			const string guaranteeTypeCode = "CL251";
			Assert.That(guarantee.CodeType, Is.EqualTo(guaranteeTypeCode));
		}

		[Test]
		public void CodeListType()
		{
			const string guaranteeType = "GuaranteeType";
			Assert.That(guarantee.CodeListType, Is.EqualTo(guaranteeType));
		}

		[Test]
		public void DataSource()
		{
			const string guaranteeType = "EUN Guarantee Type";
			Assert.That(guarantee.DataSource, Is.EqualTo(guaranteeType));
		}

		[SetUp]
		public void Setup()
		{
			guarantee = GetNctsCodeListDetails() as NctsGuaranteeType;
		}

		NctsGuaranteeType guarantee;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsGuaranteeType();
	}
}
