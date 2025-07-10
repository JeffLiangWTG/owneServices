using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(SellRatesDecimals))]
	public class SellRatesDecimalsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateSellRateDecimals()
		{
			var numberOfDecimalsAllowed = new SellRatesDecimals();
			numberOfDecimalsAllowed.Code = "ONE";
			numberOfDecimalsAllowed.Decimals = "F";

			AssertHasError(numberOfDecimalsAllowed.CodeInfo, "Enter a valid Rate.");
			AssertHasError(numberOfDecimalsAllowed.DecimalsInfo, "Enter a valid Number of decimals.");

			numberOfDecimalsAllowed.Code = RatingConstants.RateCategory.LCL;
			numberOfDecimalsAllowed.Decimals = "4";

			AssertNoError(numberOfDecimalsAllowed.CodeInfo, "Enter a valid Rate.");
			AssertNoError(numberOfDecimalsAllowed.DecimalsInfo, "Enter a valid Number of decimals.");
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new SellRatesDecimals();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
