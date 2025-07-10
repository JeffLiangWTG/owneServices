using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff
{
	[XmlRoot(ElementName = "CBC_NG_8362_MSG01_CustomsBookOut", Namespace = "http://malam.com/customs/CustomsBook/CBC_NG_8362_MSG01_CustomsBookOut")]
	public partial class CustomsBookOut
	{
		[XmlElement(ElementName = "CustomsBookGeneralTables")]
		public CustomsBookGeneralTables CustomsBookGeneralTables { get; set; }
	}
}
