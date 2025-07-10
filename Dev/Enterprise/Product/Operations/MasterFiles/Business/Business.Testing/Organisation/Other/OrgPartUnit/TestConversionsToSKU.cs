using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestConversionsToSKU : TestCaseWithFactory
	{
		#region TestEmptyConstructor

		public void TestEmptyConstructor()
		{
			var conversion = new ConversionToSKU("UNT", "");

			AssertEquals(1m, conversion.QtySKU);
			AssertEquals("UNT", conversion.PackType);
			AssertEquals("", conversion.UOMType);
			AssertEquals("UNT", conversion.ConversionPath);
		}

		public void TestEmptyConstructor_UpperCase()
		{
			var conversion = new ConversionToSKU("unt", "");

			AssertEquals("UNT", conversion.PackType);
			AssertEquals("UNT", conversion.ConversionPath);
		}

		public void TestEmptyConstructor_WithUOMType()
		{
			var conversion = new ConversionToSKU("UNT", "SPC");

			AssertEquals(1m, conversion.QtySKU);
			AssertEquals("UNT", conversion.PackType);
			AssertEquals("SPC", conversion.UOMType);
			AssertEquals("UNT", conversion.ConversionPath);
		}

		#endregion

		#region TestComplexConstructor

		public void TestComplexConstructor()
		{
			var conversion = new ConversionToSKU("BOX", "", 15, "UNT");

			AssertEquals(15m, conversion.QtySKU);
			AssertEquals("BOX", conversion.PackType);
			AssertEquals("", conversion.UOMType);
			AssertEquals("BOX->UNT", conversion.ConversionPath);
		}

		public void TestComplexConstructor_UpperCase()
		{
			var conversion = new ConversionToSKU("box", "", 15, "UNT");

			AssertEquals("BOX", conversion.PackType);
			AssertEquals("BOX->UNT", conversion.ConversionPath);
		}

		public void TestComplexConstructor_WithUOM()
		{
			var conversion = new ConversionToSKU("BOX", "CAS", 15, "UNT");

			AssertEquals(15m, conversion.QtySKU);
			AssertEquals("BOX", conversion.PackType);
			AssertEquals("CAS", conversion.UOMType);
			AssertEquals("BOX->UNT", conversion.ConversionPath);

			var conversionAsIPackType = (IPackTypeConversion)conversion;
			AssertEquals(15m, conversionAsIPackType.QtySKU);
			AssertEquals(1m, conversionAsIPackType.PackQty);
			AssertEquals("CAS", conversionAsIPackType.UOMType);
		}

		#endregion

		#region TestPackTypesFromConversionPath

		public void TestPackTypesFromConversionPath()
		{
			var conversion = new ConversionToSKU("CAS", "", 15, "PCK->UNT");

			AssertContainsExactElementsInAnyOrder(new[] { "CAS", "PCK", "UNT" }, conversion.PackTypesFromConversionPath);
		}

		#endregion
	}
}
