using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CommodityProvider))]
sealed class CommodityProviderTest : Customs.Business.Testing.DataProviderTestCase<CommodityProvider>
{
	public void TestDescriptionOfGoods()
	{
		item.BY_Description = "desc";
		AssertEquals("desc", Provider.DescriptionOfGoods);
	}

	public void TestCusCode()
	{
		item.BY_CusC4Number = "14";
		AssertEquals("14", Provider.CusCode);
	}

	public void TestHarmonizedSystemSubHeadingCode()
	{
		item.BY_HarmonisedTariff = "1234567890";
		AssertEquals("123456", Provider.HarmonizedSystemSubHeadingCode);
	}

	public void TestCombinedNomenclatureCode()
	{
		item.BY_HarmonisedTariff = "1234567890";
		AssertEquals("78", Provider.CombinedNomenclatureCode);
	}

	public void TestDangerousGoods()
	{
		var dangerousGood = Factory.New<UNDGDataItem>();
		dangerousGood.DI_ParentID = item.PK;
		dangerousGood.DI_ParentTableCode = item.TablePrefix;

		var dangerousGoods = Provider.DangerousGoods.Single();
		AssertEquals("SequenceNumeric", 1, dangerousGoods.SequenceNumeric);
	}

	public void TestGrossMass_InKilograms()
	{
		item.BY_GrossWeight = 1500m;
		CombineAssertions(() =>
		{
			item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("BY_GrossWeightUnit is Kilograms", 1500m, Provider.GrossMass);
			item.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("BY_GrossWeightUnit is Grams", 1.5m, Provider.GrossMass);
		});
	}

	public void TestGrossMass_Normalized()
	{
		item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		CombineAssertions(() =>
		{
			item.BY_GrossWeight = 10.0000m;
			AssertEquals("BY_GrossWeight = 10.0000", "10", Provider.GrossMass.ToString());
			item.BY_GrossWeight = 10.2000m;
			AssertEquals("BY_GrossWeight = 10.2000", "10.2", Provider.GrossMass.ToString());
			item.BY_GrossWeight = 10.2750m;
			AssertEquals("BY_GrossWeight = 10.2750", "10.275", Provider.GrossMass.ToString());
		});
	}

	public void TestNetMass_InKilograms()
	{
		item.BY_NetWeight = 1500m;
		CombineAssertions(() =>
		{
			item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("BY_NetWeightUnit is Kilograms", 1500m, Provider.NetMass);
			item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("BY_NetWeightUnit is Grams", 1.5m, Provider.NetMass);
		});
	}

	public void TestNetMass_Normalized()
	{
		item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
		CombineAssertions(() =>
		{
			item.BY_NetWeight = 10.0000m;
			AssertEquals("BY_NetWeight = 10.0000", "10", Provider.NetMass.ToString());
			item.BY_NetWeight = 10.2000m;
			AssertEquals("BY_NetWeight = 10.2000", "10.2", Provider.NetMass.ToString());
			item.BY_NetWeight = 10.2750m;
			AssertEquals("BY_NetWeight = 10.2750", "10.275", Provider.NetMass.ToString());
		});
	}

	public void TestNetMass_Null()
	{
		item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
		CombineAssertions(() =>
		{
			item.BY_NetWeight = 0;
			AssertNullOrEmpty("Empty", Provider.NetMass.ToString());
			item.BY_NetWeight = 100.0000m;
			AssertEquals("Not Empty", "0.1", Provider.NetMass.ToString());
		});
	}

	public void TestNetMass_Zero_PreviousDocumentsHeader()
	{
		AssertNetMass_Zero(item.Header.PreviousDocuments);
	}

	public void TestNetMass_Zero_PreviousDocumentsBill()
	{
		AssertNetMass_Zero(item.Bill.PreviousDocuments);
	}

	public void TestNetMass_Zero_PreviousDocumentsItem()
	{
		AssertNetMass_Zero(item.PreviousDocuments);
	}

	void AssertNetMass_Zero(Customs.Business.ICusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> previousDocuments)
	{
		var previousDoc = previousDocuments.AddNew();
		previousDoc.CSI_Code = "N830";
		item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
		CombineAssertions($"{previousDocuments.Master.GetType()} {previousDocuments.GetType()}", () =>
		{
			item.BY_NetWeight = 0;
			AssertEquals("Value 0", "0", Provider.NetMass.ToString());
			item.BY_NetWeight = 100.0000m;
			AssertEquals("Not Empty", "0.1", Provider.NetMass.ToString());
		});
	}

	public void TestSupplementaryQty_Normalized()
	{
		item.BY_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		CombineAssertions(() =>
		{
			item.BY_CustomsSecondQuantity = 10.0000m;
			AssertEquals("BY_CustomsSecondQuantity = 10.0000", "10", Provider.SupplementaryQty.ToString());
			item.BY_CustomsSecondQuantity = 10.2000m;
			AssertEquals("BY_CustomsSecondQuantity = 10.2000", "10.2", Provider.SupplementaryQty.ToString());
			item.BY_CustomsSecondQuantity = 10.2750m;
			AssertEquals("BY_CustomsSecondQuantity = 10.2750", "10.275", Provider.SupplementaryQty.ToString());
		});
	}

	public void TestGoodsMeasure()
	{
		AssertNotNull(Provider.GoodsMeasure);
	}

	public void TestInvoiceLine()
	{
		AssertEquals(0m, Provider.InvoiceLine);
	}

	public void TestQuotaOrderNumber()
	{
		AssertNull(Provider.QuotaOrderNumber);
	}

	public void TestTypeOfGoods()
	{
		AssertNull(Provider.TypeOfGoods);
	}

	protected override CommodityProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType("D");
		item = header.Bills.AddNew().GoodsItems.AddNew();
		provider = new CommodityProvider(item);
	}

	EU.NCTS.Business.NctsDepartureCargoDesc item;
	CommodityProvider provider;
}
