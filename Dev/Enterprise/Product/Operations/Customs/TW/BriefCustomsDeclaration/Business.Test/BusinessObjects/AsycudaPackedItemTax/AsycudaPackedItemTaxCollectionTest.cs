using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemTaxCollection))]
	sealed class AsycudaPackedItemTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFirstTobaccoTax()
		{
			var taxes = Taxes;
			var ttTax1 = taxes.AddNew();
			ttTax1.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			var ttTax2 = taxes.AddNew();
			ttTax2.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			AssertSame(ttTax1, taxes.FirstTobaccoTax);
		}

		public void TestRemoveAndDelete()
		{
			var taxes = Taxes;
			var ctTax = taxes.AddNew();
			ctTax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			var ttTax1 = taxes.AddNew();
			ttTax1.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			var ttTax2 = taxes.AddNew();
			ttTax2.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			var hwsTax = taxes.AddNew();
			hwsTax.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			CombineAssertions(() =>
			{
				AssertEquals("Create 4 taxes.", 4, taxes.Count);
				taxes.RemoveAndDelete(ctTax);
				AssertEquals("When deleting is not TT tax, HWS tax does not need to be deleted.", 3, taxes.Count);
				taxes.RemoveAndDelete(ttTax1);
				AssertEquals("When TT tax exists in the collection, HWS tax does not need to be deleted.", 2, taxes.Count);
				taxes.RemoveAndDelete(ttTax2);
				AssertEquals("When TT tax does not exist in the collection, HWS tax must be deleted together.", 0, taxes.Count);
			});
		}

		public void TestCreateHealthWelfareSurchargeIfNeed()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			var taxes = Taxes;
			var ttTax = taxes.AddNew();
			ttTax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			taxes.CreateHealthWelfareSurchargeIfNeed();
			var hwstax = taxes.Cast<AsycudaPackedItemTax>().FirstOrDefault(t => t.AET_ChargeType == ChargeTypeOtherList.Codes.HWS);
			CombineAssertions(() =>
			{
				AssertEquals(0m, hwstax.AET_Rate);
				AssertEquals(ZString.Empty, hwstax.AET_MethodOfCalculation);
			});

			taxes.RemoveAndDelete(hwstax);
			ttTax.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			ttTax.AET_Tariff = "TTTariff";
			taxes.CreateHealthWelfareSurchargeIfNeed();
			hwstax = taxes.Cast<AsycudaPackedItemTax>().FirstOrDefault(t => t.AET_ChargeType == ChargeTypeOtherList.Codes.HWS);
			CombineAssertions(() =>
			{
				AssertEquals(1000m, hwstax.AET_Rate);
				AssertEquals("KGM", hwstax.AET_MethodOfCalculation);
			});
		}

		public void TestRemoveHealthWelfareSurcharge()
		{
			var taxes = Taxes;
			var ctTax = taxes.AddNew();
			ctTax.AET_ChargeType = ChargeTypeOtherList.Codes.CT;
			var ttTax1 = taxes.AddNew();
			ttTax1.AET_ChargeType = ChargeTypeOtherList.Codes.TT;
			var hwsTax1 = taxes.AddNew();
			hwsTax1.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			var hwsTax2 = taxes.AddNew();
			hwsTax2.AET_ChargeType = ChargeTypeOtherList.Codes.HWS;
			taxes.RemoveHealthWelfareSurcharge();
			AssertEquals(false, taxes.Find(t => t.IsHealthWelfareSurcharge).Any());
		}

		AsycudaPackedItemTaxCollection Taxes => (AsycudaPackedItemTaxCollection)GetCollectionToTest();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew().PackedItems.AddNew().AsycudaTaxes;
		}
	}
}
