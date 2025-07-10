using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NCTSInvalidGuaranteeReasonTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(invalidGuaranteeReason.CodeType, Is.EqualTo("CL252"));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(invalidGuaranteeReason.CodeListType, Is.EqualTo("InvalidGuaranteeReason"));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(invalidGuaranteeReason.DataSource, Is.EqualTo("EUN Invalid Guarantee Reason"));
		}

		[SetUp]
		public void Setup()
		{
			invalidGuaranteeReason = GetNctsCodeListDetails() as NCTSInvalidGuaranteeReason;
		}
		NCTSInvalidGuaranteeReason invalidGuaranteeReason;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NCTSInvalidGuaranteeReason();
	}
}
