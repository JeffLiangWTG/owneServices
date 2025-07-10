using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(SellRatesDecimalsCollection))]
	public class SellRatesDecimalsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SellRatesDecimalsCollection>
	{
		public void TestGetDefault()
		{
			var defaultValue = SellRatesDecimalsCollection.GetDefault();

			AssertEquals(1, defaultValue.Count);
			AssertNumberOfDecimalsAllowed(defaultValue[0], "ALL", "4");
		}

		void AssertNumberOfDecimalsAllowed(SellRatesDecimals numberOfDecimalsAllowedForRates, string code, string numberOfDecimalsAllowed)
		{
			AssertEquals("Code", code, numberOfDecimalsAllowedForRates.Code);
			AssertEquals("RoundingType", NumberOfDecimals.Four, numberOfDecimalsAllowedForRates.Decimals);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override SellRatesDecimalsCollection GetCollectionToTest()
		{
			return new SellRatesDecimalsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SellRatesDecimals();
		}

		#endregion
	}
}
