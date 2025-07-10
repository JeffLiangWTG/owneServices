namespace CargoWise.RefDbRepo.SEReferenceData.Services.FullExport
{
	/// <remarks/>
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
		[System.Xml.Serialization.XmlArrayItemAttribute("additionalCode", typeof(additionalCode), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("baseRegulation", typeof(baseRegulation), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("certificate", typeof(certificate), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("declarableGoodsNomenclature", typeof(declarableGoodsNomenclature), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("exportRefundNomenclature", typeof(exportRefundNomenclature), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("footnote", typeof(footnote), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("fullTemporaryStopRegulation", typeof(fullTemporaryStopRegulation), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("geographicalArea", typeof(geographicalArea), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("goodsNomenclature", typeof(goodsNomenclature), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("goodsNomenclatureGroup", typeof(goodsNomenclatureGroup), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("lookupTable", typeof(lookupTable), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measure", typeof(measure), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measureAction", typeof(measureAction), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measureConditionCode", typeof(measureConditionCode), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measureType", typeof(measureType1), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measurement", typeof(measurement), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measurementUnit", typeof(measurementUnit), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("measurementUnitQualifier", typeof(measurementUnitQualifier), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("meursingAdditionalCode", typeof(meursingAdditionalCode), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("meursingHeading", typeof(meursingHeading), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("meursingSubheading", typeof(meursingSubheading), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("meursingTablePlan", typeof(meursingTablePlan), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("modificationRegulation", typeof(modificationRegulation), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("monetaryExchangePeriod", typeof(monetaryExchangePeriod), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("preferenceCode", typeof(preferenceCode), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaBalanceEvent", typeof(quotaBalanceEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaClosedAndBalanceTransferredEvent", typeof(quotaClosedAndBalanceTransferredEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaCriticalEvent", typeof(quotaCriticalEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaDefinition", typeof(quotaDefinition), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaExhaustionEvent", typeof(quotaExhaustionEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaOrderNumber", typeof(quotaOrderNumber1), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaReopeningEvent", typeof(quotaReopeningEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaUnblockingEvent", typeof(quotaUnblockingEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("quotaUnsuspensionEvent", typeof(quotaUnsuspensionEvent), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("reliefCode", typeof(reliefCode), IsNullable = false)]
		[System.Xml.Serialization.XmlArrayItemAttribute("taxCode", typeof(taxCode), IsNullable = false)]
		public object[] items;

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string[] Text;
	}
}
