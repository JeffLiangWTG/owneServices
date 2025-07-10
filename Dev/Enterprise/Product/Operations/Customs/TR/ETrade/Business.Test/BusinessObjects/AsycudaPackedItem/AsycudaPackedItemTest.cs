using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	public class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStatisticalValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;

				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 7.7448m, ZDateTime.Today);
				helper.SetExchangeRate(helper.USDCurrency, 6.7344m, ZDateTime.Today);
				helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

				header.Branch.Company.GC_IsReciprocal = true;
				Factory.Save();
				var bill = header.Bills.AddNew();
				var pack1 = bill.Packs.AddNew();
				var pack2 = bill.Packs.AddNew();
				var pack3 = bill.Packs.AddNew();

				var packedItem1 = pack1.PackedItem;
				packedItem1.API_GoodsValue = 100;
				packedItem1.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				var packedItem2 = pack2.PackedItem;
				packedItem2.API_GoodsValue = 200;
				packedItem2.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Turkey;

				var packedItem3 = pack3.PackedItem;
				packedItem3.API_GoodsValue = 300;
				packedItem3.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;

				CombineAssertions("Statistical Value", () =>
				{
					AssertEquals("Pre condition", (ZDecimal)7.7448, helper.EURCurrency.CurrentCustomsRate);
					AssertEquals("Pre condition", (ZDecimal)6.7344, helper.USDCurrency.CurrentCustomsRate);
					AssertEquals("Pre condition", (ZDecimal)1, helper.TRYCurrency.CurrentCustomsRate);

					AssertEquals("Pre condition", Core.Constants.CurrencyCodes.UnitedStates, packedItem1.StatisticalValueCurrency);
					AssertEquals("EUR", (ZDecimal)115.00, packedItem1.StatisticalValue);

					AssertEquals("Pre condition", Core.Constants.CurrencyCodes.UnitedStates, packedItem2.StatisticalValueCurrency);
					AssertEquals("TRY", (ZDecimal)29.70, packedItem2.StatisticalValue);

					AssertEquals("Pre condition", Core.Constants.CurrencyCodes.UnitedStates, packedItem3.StatisticalValueCurrency);
					AssertEquals("USD", (ZDecimal)300.00, packedItem3.StatisticalValue);
				});
			}
		}

		public void TestBanderolTariff()
		{
			const string tariff = "5000";
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, packedItem.BanderolTariff);

				packedItem.BanderolTariff = tariff;
				AssertEquals(tariff, packedItem.BanderolTariff);
			});
		}

		public void TestDecimalPlaces()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(packedItem.GetType(), "API_GoodsValue", false, attr => attr.DecimalPlaces == 2);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.API_NetWeight = 10;
			packedItem.API_NetWeightUQ = Core.Constants.Weight.Kilograms;

			return packedItem;
		}

		public void TestAdditionalCodeAttributes()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<MaxLengthAttribute>(packedItem.GetType(), "API_ChemicalSubstanceCode", true, attr => attr.MaxLength == 9);
				AssertHasCustomAttribute<ResourceStringDataAttribute>(packedItem.GetType(), "API_ChemicalSubstanceCode", true, attr => attr.Caption == "Additional Code");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(packedItem.GetType(), "API_ChemicalSubstanceCode", true, attr => attr.ShortCaption == "Add.Code");
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	}
}
