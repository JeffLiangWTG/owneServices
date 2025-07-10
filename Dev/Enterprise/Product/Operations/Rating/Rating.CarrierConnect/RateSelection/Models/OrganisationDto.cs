#nullable enable
using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class OrganisationDto
{
	public OrganisationDto() { }

	public OrganisationDto(OrgHeader orgHeader)
	{
		OrgCode = orgHeader.OH_Code;
		Name = orgHeader.OH_FullName;
		PK = orgHeader.PK.ToGuid();
	}

	public Guid PK { get; set; } = Guid.Empty;

	public string Name { get; set; } = string.Empty;

	public string OrgCode { get; set; } = string.Empty;
}
