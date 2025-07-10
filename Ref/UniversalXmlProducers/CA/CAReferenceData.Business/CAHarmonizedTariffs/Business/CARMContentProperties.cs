using System;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public abstract class CARMContentProperties
	{
		[XmlElement(ElementName = "UpdateOn", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
		public DateTime UpdateOn { get; set; }

		public virtual bool IsValid => true;
	}
}
