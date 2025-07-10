using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Urs.Api.Integration.DTOs;
using WiseRates.Api.Model;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.CarrierConnect.Test;

class ChargeCodeDtoTest : TestCaseWithFactory
{
	public void TestChargeCodeDto_FromUrsLine_HasChargeCodeProperties()
	{
		var code = Factory.New<AccChargeCode>();
		code.AC_Code = "XXX";
		code.AC_Desc = "XXX_Desc";
		code.AC_ChargeGroup = "XXX";

		var line = new UrsRateLine(
			Factory,
			new UrsCharge
			{
				ChargeDefinition = new ChargeDefinitionDto
				{
					UniversalCode = "ABC",
					Description = "ABC_Desc"
				},
				Code = "BCD",
				Name = "BCD_Desc"
			},
			ChargeType.Freight);
		line.TL_AC = code.PK;
		var dto = new ChargeCodeDto(line);
		var expectedDto = new ChargeCodeDto
		{
			UniversalChargeCode = "ABC",
			UniversalChargeCodeDescription = "ABC_Desc",
			CarrierChargeCode = "BCD",
			CarrierChargeCodeDescription = "BCD_Desc",
			ChargeCode = "XXX",
			ChargeCodeDescription = "XXX_Desc",
			ChargeCodeGroup = "XXX",
		};

		AssertEquals(expectedDto.ToString(), dto.ToString());
	}

	public void TestChargeCodeDto_SetsChargeApplicability_WhenApplicable()
	{
		var code = Factory.New<AccChargeCode>();
		code.AC_Code = "XXX";
		code.AC_Desc = "XXX_Desc";
		code.AC_ChargeGroup = "XXX";

		var rateEntry = new UniversalRateEntryDto { Applicable = RateApplicableCode.OnRequest };
		var rateCollection = new RateCollectionDto { PriceEntries = new[] { rateEntry } };
		var rateCollections = new RateCollectionDataDto { Items = new[] { rateCollection } };

		var line = new UrsRateLine(
			Factory,
			new UrsCharge
			{
				ChargeDefinition = new ChargeDefinitionDto
				{
					UniversalCode = "ABC",
					Description = "ABC_Desc"
				},
				Code = "BCD",
				Name = "BCD_Desc",
				RateCollections = rateCollections
			},
			ChargeType.Freight);
		line.TL_AC = code.PK;
		var dto = new ChargeCodeDto(line);

		AssertEquals(RateApplicableCode.GetDescription(RateApplicableCode.OnRequest), dto.ChargeApplicability);
	}
}
