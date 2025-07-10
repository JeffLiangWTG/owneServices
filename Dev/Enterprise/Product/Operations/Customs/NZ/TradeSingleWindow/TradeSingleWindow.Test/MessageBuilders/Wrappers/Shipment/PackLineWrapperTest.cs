using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class PackLineWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new PackLineWrapper(null);
		}

		public void TestPackLineWrapper()
		{
			var testPackline = Factory.NewWithValidTestData<ForwardingPackLine>();
			testPackline.JL_DetailedDescription = "Test Packline";
			testPackline.JL_PackageCount = 100;
			testPackline.JL_F3_NKPackType = "UN";
			testPackline.JL_LinePrice = 1500m;
			testPackline.JL_ActualWeight = 170000m;
			testPackline.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			testPackline.JL_RN_NKOrigin = "AU";
			var dgItem = testPackline.UNDGs.AddNew();
			dgItem.DI_DGFlashPoint = -15m;
			var wrappedPackline = new PackLineWrapper(testPackline);
			AssertNotNull("PackLineWrapper", wrappedPackline);
			AssertEquals("GoodsDescription", "Test Packline", wrappedPackline.GoodsDescription);
			AssertEquals("ItemValue", 1500m, wrappedPackline.Value);
			AssertEquals("OriginOfGoods", "AU", wrappedPackline.GoodsOriginCountry);
			AssertEquals("PackageQty", 100, wrappedPackline.PackageQty);
			AssertEquals("PackageUnit", "UN", wrappedPackline.PackageType);
			AssertEquals("GrossWeightInKgm", 170m, wrappedPackline.GrossWeightInKg);
			AssertEquals("FlashpointTempInCelsius", -15m, wrappedPackline.FlashpointTempInCelsius);
		}
	}
}
