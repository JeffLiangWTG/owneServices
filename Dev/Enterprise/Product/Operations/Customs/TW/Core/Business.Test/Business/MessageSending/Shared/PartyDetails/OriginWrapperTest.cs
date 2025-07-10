using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OriginWrapperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(wrapper.CountryCode, NUnit.Framework.Is.EqualTo("CountryCode").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocument()
		{
			NUnit.Framework.Assert.That(wrapper.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new OriginWrapper("CountryCode", new AdditionalDocumentWrapper("id"));
		}

		OriginWrapper wrapper;
	}
}
