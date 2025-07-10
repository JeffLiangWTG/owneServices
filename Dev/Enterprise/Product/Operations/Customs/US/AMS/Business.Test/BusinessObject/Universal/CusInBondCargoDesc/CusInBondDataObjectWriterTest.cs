using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectWriterTest
	{
		void AssertInBondCommodityContents(PackingLine commodityData, ZInt? containerLink, ZString? tariff, ZDecimal? monetary, ZString? detailedDescription)
		{
			AssertInBondCommodityContents(commodityData, containerLink, tariff, monetary, 100m, "KG", 50, "BAG", "COMMODITY" + tariff, "MARKS" + tariff, "C" + tariff, "US", detailedDescription);
		}

		void AssertInBondCommodityContents(PackingLine commodityData, ZInt? containerLink, ZString? tariff, ZDecimal? monetary, ZDecimal? weight, ZString? weightUQ, ZLong? pieceCount, ZString? manifestUQ, ZString? description, ZString? marks, ZString? c4Number, ZString? originCountry, ZString? detailedDescription)
		{
			AssertNotNull("Precondition: commodityData", commodityData);
			CombineAssertions(() =>
			{
				AssertEquals("commodityData.ContainerLink", containerLink, commodityData.ContainerLink);
				AssertEquals("commodityData.HarmonisedCode", tariff, commodityData.HarmonisedCode);
				AssertEquals("commodityData.LinePrice", monetary, commodityData.LinePrice);
				AssertEquals("commodityData.Weight", weight, commodityData.Weight);
				AssertEquals("commodityData.WeightUnit", weightUQ.GetValueOrDefault(), commodityData.WeightUnit.GetCodeAsUpperCase());
				AssertEquals("commodityData.PackQty", pieceCount, commodityData.PackQty);
				AssertEquals("commodityData.PackType", manifestUQ.GetValueOrDefault(), commodityData.PackType.GetCodeAsUpperCase());
				AssertEquals("commodityData.GoodsDescription", description, commodityData.GoodsDescription);
				AssertEquals("commodityData.MarksAndNos", marks, commodityData.MarksAndNos);
				AssertEquals("commodityData.CountryOfOrigin", originCountry.GetValueOrDefault(), commodityData.CountryOfOrigin.GetCodeAsUpperCase());
				AssertEquals("commodityData.ReferenceNumber", c4Number, commodityData.ReferenceNumber);
				if (detailedDescription.HasValue)
				{
					AssertEquals("commodityData.DetailedDescription", detailedDescription, commodityData.DetailedDescription.Value);
				}
			});
		}

		void SetupCommodity(CusInBondCargoDesc commodity, ZString tariff, ZDecimal monetary, ZString? detailedDescription)
		{
			SetupCommodity(commodity, tariff, monetary, 100m, "KG", 50, "BAG", "COMMODITY" + tariff, "MARKS" + tariff, "C" + tariff, "US", detailedDescription);
		}

		void SetupCommodity(CusInBondCargoDesc commodity, ZString tariff, ZDecimal monetary, ZDecimal weight, ZString weightUQ, ZInt pieceCount, ZString manifestUQ, ZString description, ZString marks, ZString c4Number, ZString originCountry, ZString? detailedDescription)
		{
			commodity.BY_HarmonisedTariff = tariff;
			commodity.BY_MonetaryValue = monetary;
			commodity.BY_GrossWeight = weight;
			commodity.BY_GrossWeightUnit = weightUQ;
			commodity.BY_PieceCount = pieceCount;
			commodity.BY_ManifestUnitCode = manifestUQ;
			commodity.BY_Description = description;
			commodity.BY_MarksAndNumbers = marks;
			commodity.BY_CusC4Number = c4Number;
			commodity.BY_RN_NKCountryOfOrigin = originCountry;
			if (detailedDescription.HasValue)
			{
				commodity.DetailedDescription = detailedDescription.Value;
			}
		}
	}
}
