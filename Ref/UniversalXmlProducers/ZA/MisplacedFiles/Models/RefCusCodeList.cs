using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ZAReferenceData.Models
{
	public class RefCusCodeList
	{
		public string ZZD_ZZK_NKCodeType { get; set; }

		public string ZZD_Code { get; set; }

		public string ZZD_Description { get; set; }

		public DateTime ZZD_StartDate { get; set; }

		public DateTime ZZD_EndDate { get; set; }

		public string ZZD_ZZZ_NKDataGrouping { get; set; }

		[XmlElement("RefCusCodeListAttribute")]
		public List<RefCusCodeListAttribute> RefCusCodeListAttributes { get; } = new List<RefCusCodeListAttribute>();
	}
}
