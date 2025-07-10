using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	partial class AsycudaBillTest
	{
		public void TestABL_GoodsValue()
		{
			AssertEquals("0", bill.ABL_GoodsValue.ToString());
			bill.ABL_GoodsValue = 123.456m;
			AssertEquals("123", bill.ABL_GoodsValue.ToString());
		}

		public void TestFDAIndicator()
		{
			bill.FDAIndicator = ZBool.True;
			AssertEquals("billCountryUS.FDAIndicator", ZBool.True, bill.FDAIndicator);
		}

		public void TestCustomsEntryNumberType()
		{
			AssertEquals(2, bill.CustomsEntryNumberTypeInfo.MaxLength);
			AssertEquals(false, header.IsExpressCourier);
			AssertEquals(true, bill.CustomsEntryNumberTypeInfo.ReadOnly);

			header.IsExpressCourier = true;
			AssertEquals(false, bill.CustomsEntryNumberTypeInfo.ReadOnly);
		}

		public void TestCustomsEntryNumber()
		{
			AssertEquals(11, bill.CustomsEntryNumberInfo.MaxLength);
			AssertEquals(false, header.IsExpressCourier);
			AssertEquals(true, bill.CustomsEntryNumberInfo.ReadOnly);

			header.IsExpressCourier = true;
			AssertEquals(false, bill.CustomsEntryNumberInfo.ReadOnly);
		}

		public void TestDeleteCustomsNumbersNoLongerApplicable()
		{
			bill.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.Gifts;
			Factory.Save();

			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Gifts, bill.CustomsEntryNumberType);
		}

		public void TestABL_Tariff()
		{
			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_Tariff = "0101210001";
			var info = bill.ABL_TariffInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(info);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_Tariff Caption", "Tariff", resourceStringData.Caption);
				AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaBill), bill.ABL_TariffInfo.Name, true, attribute => attribute.ListDataSourceMember == "Lookups.TariffCollection");
				AssertEquals("ABL_Tariff MaxLength", 15, bill.ABL_TariffInfo.MaxLength);
				AssertEquals("0101.21.0001", bill.ABL_Tariff);
			});
		}

		public void TestITariffFormatProvider()
		{
			AssertType<US.Business.TariffFormatter>(((ITariffFormatProvider)bill).TariffFormatter);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
