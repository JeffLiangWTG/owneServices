using System;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff
{
	public class CustomsItemComputedData
	{
		[XmlElement(ElementName = "ID")]
		public int ID { get; set; }

		[XmlElement(ElementName = "CustomsItemID")]
		public int CustomsItemID { get; set; }

		[XmlElement(ElementName = "FullClassification")]
		public string FullClassification { get; set; }

		[XmlElement(ElementName = "IsLeaf")]
		public bool IsLeaf { get; set; }

		[XmlElement(ElementName = "Valid_CustomsItemDetailsHistoryID")]
		public int ValidCustomsItemDetailsHistoryID { get; set; }

		[XmlElement(ElementName = "Valid_PropertiesDetailsHistoryID")]
		public int ValidPropertiesDetailsHistoryID { get; set; }

		[XmlElement(ElementName = "IsHistoryExists")]
		public bool IsHistoryExists { get; set; }

		[XmlElement(ElementName = "IsRulesExists")]
		public bool IsRulesExists { get; set; }

		[XmlElement(ElementName = "StartDate")]
		public DateTime StartDate { get; set; }

		[XmlElement(ElementName = "EndDate")]
		public DateTime EndDate { get; set; }

		[XmlElement(ElementName = "CI_BaseFullClassification")]
		public string BaseFullClassification { get; set; }

		[XmlElement(ElementName = "CI_ComputedCheckDigit")]
		public int? ComputedCheckDigit { get; set; }

		[XmlElement(ElementName = "CI_CustomsBookTypeIDNum")]
		public int CustomsBookTypeIDNum { get; set; }

		[XmlElement(ElementName = "CI_CustomsItemCategoryIDNum")]
		public int CustomsItemCategoryIDNum { get; set; }

		[XmlElement(ElementName = "CIH_Title")]
		public string Title { get; set; }

		[XmlElement(ElementName = "CIH_GoodsDescription")]
		public string GoodsDescription { get; set; }

		[XmlElement(ElementName = "CIH_CustomsItemEntityStatusIDNum")]
		public int CustomsItemEntityStatusIDNum { get; set; }

		[XmlElement(ElementName = "FullGoodsDescription")]
		public string FullGoodsDescription { get; set; }

		[XmlElement(ElementName = "PH_MeasurementUnitID")]
		public int MeasurementUnitID { get; set; }
	}
}
