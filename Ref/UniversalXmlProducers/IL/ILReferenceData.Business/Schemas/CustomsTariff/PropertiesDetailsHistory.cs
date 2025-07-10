using System;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff
{
	public class PropertiesDetailsHistory
	{
		[XmlElement(ElementName = "ID")]
		public int ID { get; set; }

		[XmlElement(ElementName = "CreateDate")]
		public DateTime CreateDate { get; set; }

		[XmlElement(ElementName = "UpdateDate")]
		public DateTime UpdateDate { get; set; }

		[XmlElement(ElementName = "StartDate")]
		public DateTime StartDate { get; set; }

		[XmlElement(ElementName = "EndDate")]
		public DateTime EndDate { get; set; }

		[XmlElement(ElementName = "CustomsItemID")]
		public int CustomsItemID { get; set; }

		[XmlElement(ElementName = "EntityStatusID")]
		public int EntityStatusID { get; set; }

		[XmlElement(ElementName = "ChangeRequestTypePriority")]
		public int ChangeRequestTypePriority { get; set; }

		[XmlElement(ElementName = "IsCarItem")]
		public bool IsCarItem { get; set; }

		[XmlElement(ElementName = "IsElectronic")]
		public bool IsElectronic { get; set; }

		[XmlElement(ElementName = "VatDiscountRate")]
		public decimal VatDiscountRate { get; set; }

		[XmlElement(ElementName = "IsCarDiscount")]
		public bool IsCarDiscount { get; set; }
	}
}
