using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.arcticgroup.se/tariff/arctictariff/export", IsNullable = false)]
	public partial class export
	{

		/// <remarks/>
		public string id;

		/// <remarks/>
		public string exportType;

		/// <remarks/>
		public parameters parameters;

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("measure", typeof(measure))]
		[System.Xml.Serialization.XmlArrayItemAttribute("goodsNomenclature", typeof(goodsNomenclature))]
		[System.Xml.Serialization.XmlArrayItemAttribute("declarableGoodsNomenclature", typeof(declarableGoodsNomenclature))]
		[System.Xml.Serialization.XmlArrayItemAttribute("measureType", typeof(measureType1))]
		[System.Xml.Serialization.XmlArrayItemAttribute("baseRegulation", typeof(baseRegulation))]
		[System.Xml.Serialization.XmlArrayItemAttribute("measureConditionCode", typeof(measureConditionCode))]
		[System.Xml.Serialization.XmlArrayItemAttribute("fullTemporaryStopRegulation", typeof(fullTemporaryStopRegulation))]
		[System.Xml.Serialization.XmlArrayItemAttribute("modificationRegulation", typeof(modificationRegulation))]
		[System.Xml.Serialization.XmlArrayItemAttribute("record", typeof(record))]
		public object[] items;
	}
}
