using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export", IsNullable = false)]
#pragma warning disable CS8860 // Types and aliases should not be named 'record'.
	public partial class record
#pragma warning restore CS8860 // Types and aliases should not be named 'record'.
	{
		[System.Xml.Serialization.XmlElementAttribute("measure", typeof(measure))]
		[System.Xml.Serialization.XmlElementAttribute("goodsNomenclature", typeof(goodsNomenclature))]
		[System.Xml.Serialization.XmlElementAttribute("baseRegulation", typeof(baseRegulation))]
		[System.Xml.Serialization.XmlElementAttribute("modificationRegulation", typeof(modificationRegulation))]
		public object Item;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public long recordId;

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool recordIdSpecified;
	}
}
