using System;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	[Serializable]
	[XmlType("ResultsInfo")]
	public class ContentDetails
	{
		[XmlElement("totalRecords")]
		public int TotalRecords { get; set; }
		[XmlElement("executionDate")]
		public DateTime ExecutionDate { get; set; }
		[XmlElement("startDate")]
		public DateTime StartDate { get; set; }
		[XmlElement("endDate")]
		public DateTime EndDate { get; set; }
		[XmlElement("databaseDate")]
		public DateTime DatabaseDate { get; set; }
	}
}
