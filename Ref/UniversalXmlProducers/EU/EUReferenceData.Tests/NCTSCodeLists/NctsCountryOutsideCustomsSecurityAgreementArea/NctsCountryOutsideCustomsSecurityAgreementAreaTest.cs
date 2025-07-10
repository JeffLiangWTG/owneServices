using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.CountryCodesCTC.DocumentTypeExcise
{
	[TestFixture]
	sealed class NctsCountryOutsideCustomsSecurityAgreementAreaTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsCountryOutsideCustomsSecurityAgreementArea));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.NctsCountryOutsideCustomsSecurityAgreementArea));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.NctsCountryOutsideCustomsSecurityAgreementArea));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsCountryOutsideCustomsSecurityAgreementArea;
		}

		NctsCountryOutsideCustomsSecurityAgreementArea declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsCountryOutsideCustomsSecurityAgreementArea();
	}
}
