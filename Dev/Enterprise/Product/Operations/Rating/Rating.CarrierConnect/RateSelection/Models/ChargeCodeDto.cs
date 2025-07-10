#nullable enable
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.CarrierConnect;

public class ChargeCodeDto
{
	public ChargeCodeDto() { }

	public ChargeCodeDto(IRateLine line)
	{
		if (line is UrsRateLine ursLine)
		{
			UniversalChargeCode = ursLine.UniversalChargeCode;
			UniversalChargeCodeDescription = ursLine.UniversalChargeCodeDescription;
			CarrierChargeCode = ursLine.CarrierChargeCode;
			CarrierChargeCodeDescription = ursLine.CarrierChargeCodeDescription;

			var rawCharge = ursLine.BaseCharge;
			if (ursLine.BaseCharge is not UrsInclusiveCharge)
			{
				var rateItems = rawCharge.RateCollections?.Items?.First();
				var rate = rateItems?.PriceEntries?.First();
				if (rate != null && UrsRatesParseHelper.ShouldShowApplicabilityOnCarrierConnect(rate))
				{
					ChargeApplicability = RateApplicableCode.GetC3Description(rate.Applicable);
				}
			}
		}

		if (line.ChargeCode is not null)
		{
			InitializePropertiesFromChargeCode(line.ChargeCode);
		}
	}

	void InitializePropertiesFromChargeCode(AccChargeCode chargeCode)
	{
		ChargeCode = chargeCode.AC_Code;
		ChargeCodeGroup = chargeCode.AC_ChargeGroup;
		ChargeCodeDescription = chargeCode.AC_DescMultilingual;
	}

	public string? UniversalChargeCode { get; init; }

	public string? UniversalChargeCodeDescription { get; init; }

	public string? CarrierChargeCode { get; init; }

	public string? CarrierChargeCodeDescription { get; init; }

	public string? ChargeCode { get; set; }

	public string? ChargeCodeGroup { get; set; }

	public string? ChargeCodeDescription { get; set; }

	public string? ChargeApplicability { get; set; }

	public override string ToString() =>
		string.Join("|",
			UniversalChargeCode,
			UniversalChargeCodeDescription,
			CarrierChargeCode,
			CarrierChargeCodeDescription,
			ChargeCode,
			ChargeCodeGroup,
			ChargeCodeDescription);
}
