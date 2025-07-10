using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{

	[XmlRoot(ElementName = "feed", Namespace = "http://www.w3.org/2005/Atom")]
	public class CARMResponseFeed<T>
	{
		[XmlElement(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
		public List<CARMResponseEntry<T>> Entry { get; set; }
	}

	[XmlRoot(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
	public class CARMResponseEntry<T>
	{
		[XmlElement(ElementName = "content", Namespace = "http://www.w3.org/2005/Atom")]
		public CARMResponseContent<T> Content { get; set; }
	}

	[XmlRoot(ElementName = "content", Namespace = "http://www.w3.org/2005/Atom")]
	public class CARMResponseContent<T>
	{
		[XmlElement(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
		public T Properties { get; set; }
	}
}
