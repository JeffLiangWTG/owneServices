using System;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public class Tariff
{
	public Tariff(string code, string description, DateTime startDate, string uOM)
	{
		Code = code;
		Description = description;
		StartDate = startDate;
		UOM = uOM;
	}

	public string Code { get; }

	public string Description { get; }

	public DateTime StartDate { get; }

	public string UOM {  get; }
}
