using CargoWise.Application;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class CustomsMessageStatusProviderFactoryTest : TestCase
	{
		public void TestProviderTypes()
		{
			AssertType(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusSeaManOBLHeaderMessageStatus>(), CustomsMessageStatusProviderFactory.New(Constants.CountryCodes.Australia));
			AssertType(typeof(NullCustomsMessageStatusProvider), CustomsMessageStatusProviderFactory.New("XX"));
			AssertType(typeof(NullCustomsMessageStatusProvider), CustomsMessageStatusProviderFactory.New(""));
		}

		[UseDummyCustomsMessageStatusProvider]
		public void TestProviderTypesForTesting()
		{
			AssertType(typeof(DummyCustomsMessageStatusProvider), CustomsMessageStatusProviderFactory.New(Constants.CountryCodes.Australia));
			AssertType(typeof(DummyCustomsMessageStatusProvider), CustomsMessageStatusProviderFactory.New("XX"));
			AssertType(typeof(DummyCustomsMessageStatusProvider), CustomsMessageStatusProviderFactory.New(""));
		}
	}
}
