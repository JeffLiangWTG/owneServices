using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.CountryCodesCTC.CountryCodesCommonTransit
{
	[TestFixture]
	sealed class NctsCountryCodesCommonTransitTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsCountryCodesCommonTransit));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.CountryCodesCommonTransit));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.CountryCodesCommonTransit));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsCountryCodesCommonTransit;
		}

		NctsCountryCodesCommonTransit declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsCountryCodesCommonTransit();
	}
}
