#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect;

public class CarrierDto
{
	#region Construction

	public CarrierDto() { }

	public CarrierDto(OrgHeader orgHeader) => InitializeFromOrgHeader(orgHeader);

	public CarrierDto(UrsCarrier carrier)
	{
		if (carrier.OrgHeader is not null)
		{
			InitializeFromOrgHeader(carrier.OrgHeader);
			return;
		}

		OrgCode = carrier.OrgCode;
		C1cCode = carrier.C1cCode;
		ScacCode = carrier.ScacCode;
		IataCode = carrier.IataCode;
		OrgHeaderCodes = carrier.OrgHeaderCodes;
		ReferenceLines = !string.IsNullOrEmpty(ScacCode)
			? carrier.RefShippingLines.Select(line => new ReferenceLineDto(line)).ToList()
			: carrier.RefAirlines.Select(line => new ReferenceLineDto(line)).ToList();
		IsAirlineMultimapped = carrier.IsAirlineMultimapped;
	}

	void InitializeFromOrgHeader(OrgHeader orgHeader)
	{
		OrgCode = orgHeader.OH_Code;
		C1cCode = orgHeader.ShippingLine?.RSL_CargoWiseOneCode;
		Name = orgHeader.OH_FullName;
		PK = orgHeader.PK.ToGuid();
		ScacCode = orgHeader.ShippingLineSCAC;
		IataCode = UrsCarrier.GetIata(orgHeader);
		AirlinePrefix = orgHeader?.MiscServ?.Airline?.RM_AirlinePrefix;
	}

	#endregion

	public Guid? PK { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? OrgCode { get; set; }

	public string? C1cCode { get; set; }

	public string? ScacCode { get; set; }

	public string? IataCode { get; set; }

	public string? AirlinePrefix { get; set; }

	public string[] OrgHeaderCodes { get; set; } = [];

	public List<ReferenceLineDto> ReferenceLines { get; set; } = [];

	public bool IsAirlineMultimapped { get; set; }
}

public class ReferenceLineDto
{
	public ReferenceLineDto() { }

	public ReferenceLineDto(RefAirline airline)
	{
		Name = airline.RM_AirlineName1;
		Code = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
		Id = airline.PK.ToGuid();
	}

	public ReferenceLineDto(RefShippingLine shippingLine)
	{
		Name = shippingLine.RSL_CarrierName;
		Code = shippingLine.RSL_CargoWiseOneCode;
		Id = shippingLine.PK.ToGuid();
	}

	public string Name { get; set; } = null!;

	public string Code { get; set; } = null!;

	public Guid Id { get; set; }

	public override string ToString() => string.Join("|", Name, Code, Id);
}
