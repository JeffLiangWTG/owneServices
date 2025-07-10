using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class ICRPackWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ICRPackWrapper(null);
		}

		public void TestIsEmptyContainer()
		{
			Assert(!wrappedPack.IsEmptyContainer);
		}

		public void TestGoodsDescription()
		{
			Assert(wrappedPack.GoodsDescription.IsEmpty);
			testPack.APA_GoodsDescription = "TEST DESCRIPTION";
			AssertEquals("TEST DESCRIPTION", wrappedPack.GoodsDescription);
		}

		public void TestIdentityNumber()
		{
			Assert(wrappedPack.IdentityNumber.IsEmpty);
		}

		public void TestValue()
		{
			Assert(wrappedPack.Value.IsEmpty);
			testPack.LinePrice = 123m;
			AssertEquals(123m, wrappedPack.Value);
		}

		public void TestCurrency()
		{
			Assert(wrappedPack.Currency.IsEmpty);
			testPack.LinePriceCurrency = Core.Constants.CurrencyCodes.FalklandIslands;
			AssertEquals(Core.Constants.CurrencyCodes.FalklandIslands, wrappedPack.Currency);
		}

		public void TestIdentityType()
		{
			Assert(wrappedPack.IdentityType.IsEmpty);
		}

		public void TestFlashpointTempInCelsius()
		{
			Assert(wrappedPack.FlashpointTempInCelsius.IsEmpty);
			var undg = testPack.UNDGs.AddNew();
			undg.DI_DGFlashPoint = -25m;
			AssertEquals("Flashpoint temp should not be sent (or simply defaulted in Asycuda)", 0m, wrappedPack.FlashpointTempInCelsius);
		}

		public void TestTemperatures()
		{
			AssertNull(wrappedPack.Temperatures);
		}

		public void TestGrossWeightInKg()
		{
			AssertEquals(ZDecimal.Zero, wrappedPack.GrossWeightInKg);
			testPack.APA_Weight = 123.99m;
			AssertEquals(ZDecimal.Zero, wrappedPack.GrossWeightInKg);
			testPack.APA_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(123.99m, wrappedPack.GrossWeightInKg);
			testPack.APA_Weight = 1m;
			testPack.APA_WeightUQ = Core.Constants.Weight.ShortTons;
			AssertEquals(907.185m, wrappedPack.GrossWeightInKg);
			testPack.APA_WeightUQ = "ZZ";
			AssertEquals(ZDecimal.Zero, wrappedPack.GrossWeightInKg);
		}

		public void TestGoodsOriginCountry()
		{
			Assert(wrappedPack.GoodsOriginCountry.IsEmpty);
			var testBill = Factory.New<AsycudaBill>();
			testBill.ABL_RL_NKOrigin = ZString.Empty;
			var pack = testBill.Packs.AddNew();
			IICRConsignmentItem packWrapper = new ICRPackWrapper(pack);
			Assert(packWrapper.GoodsOriginCountry.IsEmpty);
			testBill.ABL_RL_NKOrigin = "NZAKL";
			AssertEquals(Core.Constants.CountryCodes.NewZealand, packWrapper.GoodsOriginCountry);
		}

		public void TestPackageQty()
		{
			Assert(wrappedPack.PackageQty.IsEmpty);
			testPack.APA_PackQty = 9;
			AssertEquals(9, wrappedPack.PackageQty);
		}

		public void TestPackageUnit()
		{
			Assert(wrappedPack.PackageType.IsEmpty);
			testPack.APA_PackUQ = "BOX";
			AssertEquals("BOX", wrappedPack.PackageType);
		}

		public void TestContainerNumber()
		{
			Assert(wrappedPack.ContainerNumber.IsEmpty);
			var testContainer = Factory.New<AsycudaContainer>();
			testContainer.ACN_ContainerNumber = "CN1234567";
			testPack.ContainerPK = testContainer.PK;
			AssertEquals("CN1234567", wrappedPack.ContainerNumber);
		}

		public void TestClassifications()
		{
			Assert(!wrappedPack.Classifications.Any());
			var undg = testPack.UNDGs.AddNew();
			undg.DI_IMOClass = "1.6N";
			AssertEquals(1, wrappedPack.Classifications.Count());
			var classification = wrappedPack.Classifications.First();
			AssertEquals("16N", classification.Classification);
			AssertEquals(ClassificationTypeList.Codes.SSO, classification.ClassificationTypeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testPack = Factory.New<AsycudaPack>();
			wrappedPack = new ICRPackWrapper(testPack);
		}

		AsycudaPack testPack;
		IICRConsignmentItem wrappedPack;
	}
}
