using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsFunctionalErrorCodesIeCATest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(functionalErrorCode.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsFunctionalErrorCodesIeCA));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(functionalErrorCode.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.FunctionalErrorCodesIeCA));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(functionalErrorCode.DataSource, Is.EqualTo(Constants.UccDataSources.FunctionalErrorCodesIeCA));
		}

		[SetUp]
		public void Setup()
		{
			functionalErrorCode = GetNctsCodeListDetails() as NctsFunctionalErrorCodesIeCA;
		}

		NctsFunctionalErrorCodesIeCA functionalErrorCode;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsFunctionalErrorCodesIeCA();
	}
}
