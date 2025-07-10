using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	[Serializable]
	[XmlType("tbl")]
	public class CodeBookTable
	{
		[XmlElement("tnr")]
		public string tableNumber { get; set; }
		[XmlElement("tnm")]
		public string tableName { get; set; }
		[XmlElement("elm")]
		public List<TableElement> elements { get; set; }
	}
}
