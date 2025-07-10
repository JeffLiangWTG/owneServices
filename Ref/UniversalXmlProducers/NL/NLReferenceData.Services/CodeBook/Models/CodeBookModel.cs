using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	[Serializable]
	[XmlType("cbk")]
	public class CodeBookModel
	{
		[XmlElement("bnm")]
		public string bookName { get; set; }
		[XmlElement("tbl")]
		public List<CodeBookTable> codeBookTables { get; set; }
	}
}
