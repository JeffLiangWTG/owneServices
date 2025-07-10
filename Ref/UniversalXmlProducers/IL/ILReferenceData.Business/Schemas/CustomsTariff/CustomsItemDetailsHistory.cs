using System;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff
{
	public class CustomsItemDetailsHistory
	{
		[XmlElement(ElementName = "ID")]
		public int ID { get; set; }

		[XmlElement(ElementName = "Title")]
		public string Title { get; set; }

		[XmlElement(ElementName = "CreateDate")]
		public DateTime CreateDate { get; set; }

		[XmlElement(ElementName = "UpdateDate")]
		public DateTime UpdateDate { get; set; }

		[XmlElement(ElementName = "StartDate")]
		public DateTime StartDate { get; set; }

		[XmlElement(ElementName = "EndDate")]
		public DateTime EndDate { get; set; }

		[XmlElement(ElementName = "EntityStatusID")]
		public int EntityStatusID { get; set; }

		[XmlElement(ElementName = "EnglishGoodsDescription")]
		public string EnglishGoodsDescription { get; set; }

		[XmlElement(ElementName = "GoodsDescription")]
		public string GoodsDescription { get; set; }

		[XmlElement(ElementName = "CustomsItemID")]
		public int CustomsItemID { get; set; }

		[XmlElement(ElementName = "ChangeRequestTypePriority")]
		public int ChangeRequestTypePriority { get; set; }
	}
}
