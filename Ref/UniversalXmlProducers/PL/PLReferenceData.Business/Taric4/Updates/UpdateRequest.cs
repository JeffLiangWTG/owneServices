using System;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.Updates;

[XmlRoot]
[Serializable()]
public sealed class UpdateRequest
{
	public UpdateRequest()
	{
	}

	public UpdateRequest(DateTime startDate, DateTime endDate)
	{
		StartDate = startDate;
		EndDate = endDate;
	}

	[XmlAttribute]
	public DateTime StartDate { get; set; }

	[XmlAttribute]
	public DateTime EndDate { get; set; }
}
