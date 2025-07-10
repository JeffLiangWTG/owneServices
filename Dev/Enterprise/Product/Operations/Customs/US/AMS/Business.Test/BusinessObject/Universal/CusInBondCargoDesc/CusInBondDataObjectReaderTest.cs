using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectReaderTest
	{
		PackingLine SetupPackingLine(ZLong pieceCount, ZString manifestUnitCode, ZString? harmonisedTariff, ZDecimal monetaryValue, ZString description, ZDecimal weight, ZString weightUnit, ZString marksAndNumbers, ZString countryOfOrigin, ZString c4Number, ZInt containerLink, ZString? detailedDescription)
		{
			var result = SetupPackingLine(pieceCount, manifestUnitCode, harmonisedTariff, monetaryValue, description, weight, weightUnit, marksAndNumbers, containerLink);
			result.CountryOfOrigin = new Country { Code = countryOfOrigin };
			result.ReferenceNumber = c4Number;
			result.DetailedDescription = detailedDescription;
			return result;
		}

		void AssertCusInBondCargoDescContents(CusInBondCargoDesc commodityBO, ZString harmonisedTariff, ZDecimal monetaryValue, ZDecimal weight, ZString weightUnit, ZInt pieceCount, ZString manifestUnit, ZString description, ZString marksAndNumbers, ZString countryOfOrigin, ZString c4Number, ZString? detailedDescription)
		{
			AssertEquals("commodityBO.BY_HarmonisedTariff", harmonisedTariff, commodityBO.BY_HarmonisedTariff);
			AssertEquals("commodityBO.BY_MonetaryValue", monetaryValue, commodityBO.BY_MonetaryValue);
			AssertEquals("commodityBO.BY_GrossWeight", weight, commodityBO.BY_GrossWeight);
			AssertEquals("commodityBO.BY_GrossWeightUnit", weightUnit, commodityBO.BY_GrossWeightUnit);
			AssertEquals("commodityBO.BY_PieceCount", pieceCount, commodityBO.BY_PieceCount);
			AssertEquals("commodityBO.BY_ManifestUnitCode", manifestUnit, commodityBO.BY_ManifestUnitCode);
			AssertEquals("commodityBO.BY_Description", description, commodityBO.BY_Description);
			AssertEquals("commodityBO.BY_MarksAndNumbers", marksAndNumbers, commodityBO.BY_MarksAndNumbers);
			AssertEquals("commodityBO.BY_RN_NKCountryOfOrigin", countryOfOrigin, commodityBO.BY_RN_NKCountryOfOrigin);
			AssertEquals("commodityBO.BY_CusC4Number", c4Number, commodityBO.BY_CusC4Number);
			if (!detailedDescription.HasValue)
			{
				var notes = commodityBO.Notes.FindByDescription("Detailed Goods Description");
				AssertEquals(1, notes.Length);
				AssertEquals("Commodity Detailed Description", detailedDescription, notes[0].ST_NoteDataAsText);
			}
		}
	}
}
