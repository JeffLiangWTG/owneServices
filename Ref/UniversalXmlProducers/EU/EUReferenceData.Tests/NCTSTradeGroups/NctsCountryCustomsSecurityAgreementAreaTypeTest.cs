using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsCountryCustomsSecurityAgreementAreaTypeTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(declarationType.CodeType, Is.EqualTo(Constants.ZZRefCusTradeCountry.NctsCountryCustomsSecurityAgreementArea));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(declarationType.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.CountryCustomsSecurityAgreementArea));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(declarationType.DataSource, Is.EqualTo(Constants.UccDataSources.CountryCustomsSecurityAgreementArea));
		}

		[SetUp]
		public void Setup()
		{
			declarationType = GetNctsCodeListDetails() as NctsCountryCustomsSecurityAgreementAreaType;
		}
		NctsCountryCustomsSecurityAgreementAreaType declarationType;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsCountryCustomsSecurityAgreementAreaType();
	}
}
