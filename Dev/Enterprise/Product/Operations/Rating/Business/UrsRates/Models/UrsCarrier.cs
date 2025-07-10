#nullable enable
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business;

public class UrsCarrier
{
	BusinessObjectFactory Factory => new ();

	UrsCarrier() { }

	public static UrsCarrier FromOrg(OrgHeader org) =>
		new()
		{
			OrgHeader = org,
			OrgCode = org.OH_Code,
			C1cCode = org.ShippingLine?.RSL_CargoWiseOneCode,
			ScacCode = org.ShippingLineSCAC,
			IataCode = GetIata(org),
		};

	public static UrsCarrier FromSCAC(string scac) => new() { ScacCode = scac };

	public static UrsCarrier FromIATA(string iata, string[]? orgHeaderCodes = null, bool airlineMultimapped = false) =>
		new()
		{
			IataCode = iata,
			OrgHeaderCodes = orgHeaderCodes ?? [],
			IsAirlineMultimapped = airlineMultimapped,
		};

	public static string? GetIata(OrgHeader orgHeader) => orgHeader.MiscServ?.AirlineTwoCharacterCode;

	public OrgHeader? OrgHeader { get; init; }
	public string? OrgCode { get; init; }
	public string? C1cCode { get; init; }
	public string? ScacCode { get; init; }
	public string? IataCode { get; init; }
	public string[] OrgHeaderCodes { get; init; } = [];
	public bool IsAirlineMultimapped { get; init; }

	RefAirline[]? refAirlines;
	public RefAirline[] RefAirlines => refAirlines ??= GetRefAirlines();
	RefAirline[] GetRefAirlines()
	{
		var codeLength = IataCode?.Length;
		if (string.IsNullOrEmpty(IataCode) || codeLength > 3 || codeLength < 2)
		{
			return [];
		}

		var query = new ZDBOnlyQuery(typeof(RefAirline));
		query.AddToFilter(codeLength == 3 ? RefAirlineSchema.RM_ThreeLetterCode : RefAirlineSchema.RM_TwoCharacterCode, IataCode);
		query.AddToFilter(RefAirlineSchema.RM_IsActive, true);
		return Factory.Load<RefAirline>(query);
	}

	RefShippingLine[]? refShippingLines;
	public RefShippingLine[] RefShippingLines => refShippingLines ??= GetRefShippingLines();
	RefShippingLine[] GetRefShippingLines()
	{
		if (string.IsNullOrEmpty(ScacCode))
		{
			return [];
		}

		var query = new ZDBOnlyQuery(typeof(RefShippingLine));
		query.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, ScacCode);
		query.AddToFilter(RefShippingLineSchema.RSL_IsActive, true);
		return Factory.Load<RefShippingLine>(query);
	}

	public string GetKey() => string.Join("|", OrgCode, C1cCode, ScacCode, IataCode);
}
