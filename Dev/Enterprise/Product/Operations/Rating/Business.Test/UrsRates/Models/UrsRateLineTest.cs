using CargoWise.EntityFramework.Testing;
using Urs.Api.Integration.DTOs;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Test;

class UrsRateLineTest : TestCaseWithFactory
{
	public void TestUrsRateLine_HasChargeCodeProperties()
	{
		var line = new UrsRateLine(
			Factory,
			new UrsCharge(
				new BaseChargeDto
				{
					ChargeDefinition = new ChargeDefinitionDto
					{
						UniversalCode = "ABC",
						Description = "ABC_Desc"
					},
					Code = "BCD",
					Name = "BCD_Desc"
				},
				new TradeServiceDto()
			),
			ChargeType.Freight);

		CombineAssertions(() =>
		{
			AssertEquals("ABC", line.UniversalChargeCode);
			AssertEquals("ABC_Desc", line.UniversalChargeCodeDescription);
			AssertEquals("BCD", line.CarrierChargeCode);
			AssertEquals("BCD_Desc", line.CarrierChargeCodeDescription);
		});
	}
}
